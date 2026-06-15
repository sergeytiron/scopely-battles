namespace ScopelyBattles.Shared.Leaderboard;

public sealed record LeaderboardEntry
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required long Score { get; init; }
}
