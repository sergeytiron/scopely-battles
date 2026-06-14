using ScopelyBattles.Shared.Battles;

namespace ScopelyBattles.Api.Battles.Create;

public sealed record Response
{
    public required int Id { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public required string Status { get; init; }

    public static Response FromBattle(Battle battle) =>
        new()
        {
            Id = battle.Id,
            AttackerId = battle.AttackerId,
            DefenderId = battle.DefenderId,
            Status = battle.Status,
        };
}
