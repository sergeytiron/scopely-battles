namespace ScopelyBattles.Shared.Battles;

public sealed record BattleTurn
{
    public required int Turn { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public required int Roll { get; init; }
    public required bool Missed { get; init; }
    public required int AttackValue { get; init; }
    public required int Damage { get; init; }
    public required int DefenderHitPoints { get; init; }
}
