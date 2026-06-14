using FastEndpoints;
using FluentValidation;

namespace ScopelyBattles.Api.Battles.Create;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.IdempotencyKey).NotEqual(Guid.Empty);
        RuleFor(request => request.AttackerId).GreaterThan(0);
        RuleFor(request => request.DefenderId).GreaterThan(0).NotEqual(request => request.AttackerId);
    }
}
