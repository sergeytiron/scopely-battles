using FastEndpoints;
using FluentValidation;
using ScopelyBattles.Shared;

namespace ScopelyBattles.Api.Players.Create;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(20);
        RuleFor(request => request.Description).MaximumLength(1000);
        RuleFor(request => request.Gold).InclusiveBetween(0, GameRules.MaxResourceValue);
        RuleFor(request => request.Silver).InclusiveBetween(0, GameRules.MaxResourceValue);
        RuleFor(request => request.AttackValue).GreaterThan(0);
        RuleFor(request => request.DefenseValue).GreaterThanOrEqualTo(0);
        RuleFor(request => request.HitPoints).GreaterThan(0);
    }
}
