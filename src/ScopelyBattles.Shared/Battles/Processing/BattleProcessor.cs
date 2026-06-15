using System.Data.Common;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using ScopelyBattles.Shared.DataAccess;
using ScopelyBattles.Shared.Players;
using BattleClaim = (
    ScopelyBattles.Shared.Battles.Battle Battle,
    ScopelyBattles.Shared.Players.Player Attacker,
    ScopelyBattles.Shared.Players.Player Defender
);

namespace ScopelyBattles.Shared.Battles.Processing;

public sealed class BattleProcessor(
    PostgresConnectionFactory connectionFactory,
    IRandomProvider random,
    ILogger<BattleProcessor> logger
)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var claim = await ClaimNextBattleAsync(connection, transaction, cancellationToken);

        if (claim is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var (battle, attacker, defender) = claim.Value;
        logger.LogInformation(
            "Processing battle {BattleId}: attacker {AttackerId}, defender {DefenderId}.",
            battle.Id,
            battle.AttackerId,
            battle.DefenderId
        );

        try
        {
            var outcome = battle.Simulate(attacker, defender, random);
            await CompleteBattleAsync(connection, transaction, outcome, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation(
                "Completed battle {BattleId}: winner {WinnerId}, loser {LoserId}, stolen {StolenResources}.",
                outcome.Report.BattleId,
                outcome.Report.WinnerId,
                outcome.Report.LoserId,
                outcome.Report.StolenResources?.Total ?? 0
            );
            return true;
        }
        catch (BattleSimulationException exception)
        {
            await FailBattleAsync(connection, transaction, battle, exception.Message, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning(
                "Failed battle {BattleId}: attacker {AttackerId}, defender {DefenderId}. {Error}",
                battle.Id,
                battle.AttackerId,
                battle.DefenderId,
                exception.Message
            );
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<BattleClaim?> ClaimNextBattleAsync(
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT
                battle.id,
                battle.idempotency_key,
                battle.attacker_id,
                battle.defender_id,
                battle.status::text AS status,
                attacker.id AS attacker_split,
                attacker.id,
                attacker.name,
                attacker.description,
                attacker.gold,
                attacker.silver,
                attacker.attack_value,
                attacker.defense_value,
                attacker.hit_points,
                attacker.score,
                defender.id AS defender_split,
                defender.id,
                defender.name,
                defender.description,
                defender.gold,
                defender.silver,
                defender.attack_value,
                defender.defense_value,
                defender.hit_points,
                defender.score
            FROM battles battle
            JOIN players attacker ON attacker.id = battle.attacker_id
            JOIN players defender ON defender.id = battle.defender_id
            WHERE battle.status = 'queued'
            ORDER BY battle.id
            FOR UPDATE OF battle, attacker, defender SKIP LOCKED
            LIMIT 1;
            """;

        var rows = await connection.QueryAsync<Battle, Player, Player, BattleClaim?>(
            new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken),
            static (battle, attacker, defender) => (battle, attacker, defender),
            splitOn: "attacker_split,defender_split"
        );

        return rows.Select(row => row).SingleOrDefault();
    }

    private static async Task CompleteBattleAsync(
        DbConnection connection,
        DbTransaction transaction,
        BattleSimulationOutcome outcome,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            UPDATE players
            SET
                gold = @WinnerGold,
                silver = @WinnerSilver,
                score = @WinnerScore
            WHERE id = @WinnerId;

            UPDATE players
            SET
                gold = @LoserGold,
                silver = @LoserSilver
            WHERE id = @LoserId;

            UPDATE battles
            SET
                status = 'completed',
                report = @Report::jsonb
            WHERE id = @BattleId AND status = 'queued';
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    WinnerId = outcome.Winner.Id,
                    LoserId = outcome.Loser.Id,
                    WinnerGold = outcome.Winner.Gold,
                    WinnerSilver = outcome.Winner.Silver,
                    WinnerScore = outcome.Winner.Score,
                    LoserGold = outcome.Loser.Gold,
                    LoserSilver = outcome.Loser.Silver,
                    outcome.Report.BattleId,
                    Report = JsonSerializer.Serialize(outcome.Report, JsonOptions),
                },
                transaction,
                cancellationToken: cancellationToken
            )
        );
    }

    private static async Task FailBattleAsync(
        DbConnection connection,
        DbTransaction transaction,
        Battle battle,
        string error,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            UPDATE battles
            SET
                status = 'failed',
                report = @Report::jsonb
            WHERE id = @BattleId AND status = 'queued';
            """;

        var report = battle.CreateFailureReport(error);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { BattleId = battle.Id, Report = JsonSerializer.Serialize(report, JsonOptions) },
                transaction,
                cancellationToken: cancellationToken
            )
        );
    }
}
