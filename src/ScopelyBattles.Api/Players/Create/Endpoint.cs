using FastEndpoints;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Api.Players.Create;

public sealed class Endpoint(PlayerStore playerStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/players");
        AllowAnonymous();
        Description(b =>
            b.Produces<Response>(StatusCodes.Status201Created)
                .ProducesProblemFE(StatusCodes.Status409Conflict)
                .ProducesProblemFE(StatusCodes.Status400BadRequest)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status201Created] = "Player created.";
            s.Responses[StatusCodes.Status400BadRequest] = "Validation failed.";
            s.Responses[StatusCodes.Status409Conflict] = "Player name already exists.";
        });
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
