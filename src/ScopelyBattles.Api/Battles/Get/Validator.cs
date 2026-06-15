using FastEndpoints;
using FluentValidation;

namespace ScopelyBattles.Api.Battles.Get;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
    }
}
