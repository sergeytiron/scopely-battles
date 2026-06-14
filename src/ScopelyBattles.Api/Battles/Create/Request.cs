using FastEndpoints;

namespace ScopelyBattles.Api.Battles.Create;

public sealed record Request
{
    [FromHeader("Idempotency-Key")]
    public required Guid IdempotencyKey { get; init; }

    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
}
