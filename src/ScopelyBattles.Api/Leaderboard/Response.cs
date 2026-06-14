using ScopelyBattles.Shared.Leaderboard;

namespace ScopelyBattles.Api.Leaderboard;

public sealed record Response
{
    public required IReadOnlyList<Entry> Entries { get; init; }

    public sealed record Entry
    {
        public required int Rank { get; init; }
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required int Score { get; init; }
    }

    public static Response From(IReadOnlyList<LeaderboardEntry> rows, int offset) =>
        new()
        {
            Entries =
            [
                .. rows.Select(
                    (row, index) =>
                        new Entry
                        {
                            Rank = offset + index + 1,
                            Id = row.Id,
                            Name = row.Name,
                            Score = row.Score,
                        }
                ),
            ],
        };
}
