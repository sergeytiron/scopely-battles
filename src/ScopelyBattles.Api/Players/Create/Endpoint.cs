using FastEndpoints;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Api.Players.Create;

public sealed class Endpoint(PlayerStore playerStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/players");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var player = new Player
        {
            Name = request.Name,
            Description = request.Description,
            Gold = request.Gold,
            Silver = request.Silver,
            AttackValue = request.AttackValue,
            DefenseValue = request.DefenseValue,
            HitPoints = request.HitPoints,
            Score = request.Score,
        };
        var result = await playerStore.CreateAsync(player, cancellationToken);

        if (result.IsDuplicateName)
        {
            ThrowError(request => request.Name, "Player name already exists.", StatusCodes.Status409Conflict);
        }

        await Send.ResponseAsync(Response.FromPlayer(result.Player), StatusCodes.Status201Created, cancellationToken);
    }
}
