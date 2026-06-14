namespace ScopelyBattles.Shared.Battles;

public sealed record Battle
{
    public int Id { get; init; }
    public required Guid IdempotencyKey { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public string Status { get; init; } = "queued";

    public bool HasSamePlayers(Battle battle) => AttackerId == battle.AttackerId && DefenderId == battle.DefenderId;
}
