using FastEndpoints;
using FluentValidation;

namespace ScopelyBattles.Api.Leaderboard;

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Limit).InclusiveBetween(1, 100);
        RuleFor(request => request.Offset).GreaterThanOrEqualTo(0);
    }
}
