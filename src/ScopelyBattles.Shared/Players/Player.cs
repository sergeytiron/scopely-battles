namespace ScopelyBattles.Shared.Players;

public sealed record Player
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required int Gold { get; init; }
    public required int Silver { get; init; }
    public required int AttackValue { get; init; }
    public required int DefenseValue { get; init; }
    public required int HitPoints { get; init; }
    public required int Score { get; init; }
}
