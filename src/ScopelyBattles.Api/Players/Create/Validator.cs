using FastEndpoints;
using FluentValidation;

namespace ScopelyBattles.Api.Players.Create;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(20);
        RuleFor(request => request.Description).MaximumLength(1000);
        RuleFor(request => request.Gold).InclusiveBetween(0, 1_000_000_000);
        RuleFor(request => request.Silver).InclusiveBetween(0, 1_000_000_000);
        RuleFor(request => request.AttackValue).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DefenseValue).GreaterThanOrEqualTo(0);
        RuleFor(request => request.HitPoints).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Score).GreaterThanOrEqualTo(0);
    }
}
