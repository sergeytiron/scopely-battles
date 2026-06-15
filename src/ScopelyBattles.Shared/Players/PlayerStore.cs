using Dapper;
using Npgsql;
using ScopelyBattles.Shared.DataAccess;

namespace ScopelyBattles.Shared.Players;

public sealed class PlayerStore(PostgresConnectionFactory connectionFactory)
{
    public async Task<CreateResult> CreateAsync(Player player, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO players
                (name, description, gold, silver, attack_value, defense_value, hit_points, score)
            VALUES
                (@Name, @Description, @Gold, @Silver, @AttackValue, @DefenseValue, @HitPoints, @Score)
            RETURNING
                id,
                name,
                description,
                gold,
                silver,
                attack_value,
                defense_value,
                hit_points,
                score;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        try
        {
            var createdPlayer = await connection.QuerySingleAsync<Player>(
                new CommandDefinition(sql, player, cancellationToken: cancellationToken)
            );

            return CreateResult.Success(createdPlayer);
        }
        catch (PostgresException exception)
            when (exception is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "uq_players_name" })
        {
            return CreateResult.DuplicateName();
        }
    }

    public async Task<Player?> GetAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id,
                name,
                description,
                gold,
                silver,
                attack_value,
                defense_value,
                hit_points,
                score
            FROM players
            WHERE id = @Id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Player>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken)
        );
    }
}
