using System.ComponentModel;

namespace ScopelyBattles.Api.Leaderboard;

public sealed record Request
{
    [DefaultValue(50)]
    public int Limit { get; init; } = 50;

    [DefaultValue(0)]
    public int Offset { get; init; } = 0;
}
