namespace ScopelyBattles.Shared.Battles;

public sealed record BattleReport
{
    public required int BattleId { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public int? WinnerId { get; init; }
    public int? LoserId { get; init; }
    public StolenResources? StolenResources { get; init; }
    public IReadOnlyList<BattleTurn> Turns { get; init; } = [];
    public string? Error { get; init; }
}
