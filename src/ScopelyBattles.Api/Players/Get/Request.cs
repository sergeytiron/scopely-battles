using Microsoft.AspNetCore.Mvc;

namespace ScopelyBattles.Api.Players.Get;

public sealed record Request
{
    [FromRoute]
    public int Id { get; init; }
}
