using ScopelyBattles.Shared.Battles;

namespace ScopelyBattles.Api.Battles.Get;

public sealed record Response
{
    public required int Id { get; init; }
    public required Guid IdempotencyKey { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public required string Status { get; init; }
    public BattleReport? Report { get; init; }

    public static Response FromBattle(Battle battle, BattleReport? report) =>
        new()
        {
            Id = battle.Id,
            IdempotencyKey = battle.IdempotencyKey,
            AttackerId = battle.AttackerId,
            DefenderId = battle.DefenderId,
            Status = battle.Status,
            Report = report,
        };
}
