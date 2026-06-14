using Dapper;
using ScopelyBattles.Shared.DataAccess;

namespace ScopelyBattles.Shared.Leaderboard;

public sealed class LeaderboardStore(PostgresConnectionFactory connectionFactory)
{
    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT
                id,
                name,
                score
            FROM players
            ORDER BY score DESC, id ASC
            LIMIT @Limit OFFSET @Offset;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<LeaderboardEntry>(
            new CommandDefinition(sql, new { Limit = limit, Offset = offset }, cancellationToken: cancellationToken)
        );

        return rows.AsList();
    }
}
