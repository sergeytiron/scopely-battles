using Dapper;
using Npgsql;
using ScopelyBattles.Shared.DataAccess;

namespace ScopelyBattles.Shared.Battles;

public sealed class BattleStore(PostgresConnectionFactory connectionFactory)
{
    public async Task<CreateResult> CreateAsync(Battle battle, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO battles
                (idempotency_key, attacker_id, defender_id)
            VALUES
                (@IdempotencyKey, @AttackerId, @DefenderId)
            RETURNING
                id,
                idempotency_key,
                attacker_id,
                defender_id,
                status::text;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        try
        {
            var submittedBattle = await connection.QuerySingleAsync<Battle>(
                new CommandDefinition(sql, battle, cancellationToken: cancellationToken)
            );

            return CreateResult.Success(submittedBattle);
        }
        catch (PostgresException exception) when (IsMissingPlayer(exception))
        {
            return CreateResult.MissingPlayer();
        }
        catch (PostgresException exception) when (IsDuplicateIdempotencyKey(exception))
        {
            var existingBattle = await GetByIdempotencyKeyAsync(connection, battle.IdempotencyKey, cancellationToken);

            return existingBattle.HasSamePlayers(battle)
                ? CreateResult.Success(existingBattle)
                : CreateResult.IdempotencyConflict();
        }
    }

    private static async Task<Battle> GetByIdempotencyKeyAsync(
        NpgsqlConnection connection,
        Guid idempotencyKey,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT
                id,
                idempotency_key,
                attacker_id,
                defender_id,
                status::text
            FROM battles
            WHERE idempotency_key = @IdempotencyKey;
            """;

        return await connection.QuerySingleAsync<Battle>(
            new CommandDefinition(sql, new { IdempotencyKey = idempotencyKey }, cancellationToken: cancellationToken)
        );
    }

    private static bool IsMissingPlayer(PostgresException exception) =>
        exception
            is {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: "fk_battles_attacker" or "fk_battles_defender"
            };

    private static bool IsDuplicateIdempotencyKey(PostgresException exception) =>
        exception is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "uq_battles_idempotency_key" };
}
