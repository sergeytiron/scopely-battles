namespace ScopelyBattles.Api.Players.Create;

public sealed record Request
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int Gold { get; init; }
    public int Silver { get; init; }
    public int AttackValue { get; init; }
    public int DefenseValue { get; init; }
    public int HitPoints { get; init; }
}
