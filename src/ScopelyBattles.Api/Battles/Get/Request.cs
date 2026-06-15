using Microsoft.AspNetCore.Mvc;

namespace ScopelyBattles.Api.Battles.Get;

public sealed record Request
{
    [FromRoute]
    public int Id { get; init; }
}
