using ScopelyBattles.Shared.Leaderboard;

namespace ScopelyBattles.Api.Leaderboard;

public sealed record Response
{
    public required int Rank { get; init; }
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required long Score { get; init; }

    public static IReadOnlyList<Response> From(IReadOnlyList<LeaderboardEntry> rows, int offset) =>
        [
            .. rows.Select(
                (row, index) =>
                    new Response
                    {
                        Rank = offset + index + 1,
                        Id = row.Id,
                        Name = row.Name,
                        Score = row.Score,
                    }
            ),
        ];
}
