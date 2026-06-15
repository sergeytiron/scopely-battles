using FastEndpoints;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Api.Players.Create;

public sealed class Endpoint(PlayerStore playerStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/players");
        Description(b =>
            b.Produces<Response>(StatusCodes.Status201Created)
                .ProducesProblemFE(StatusCodes.Status401Unauthorized)
                .ProducesProblemFE(StatusCodes.Status409Conflict)
                .ProducesProblemFE(StatusCodes.Status400BadRequest)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status201Created] = "Player created.";
            s.Responses[StatusCodes.Status400BadRequest] = "Validation failed.";
            s.Responses[StatusCodes.Status401Unauthorized] = "API key is missing or invalid.";
            s.Responses[StatusCodes.Status409Conflict] = "Player name already exists.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var player = new Player(
            request.Name,
            request.Description,
            request.Gold,
            request.Silver,
            request.AttackValue,
            request.DefenseValue,
            request.HitPoints,
            request.Score
        );
        var result = await playerStore.CreateAsync(player, cancellationToken);

        if (result.IsDuplicateName)
        {
            ThrowError(request => request.Name, "Player name already exists.", StatusCodes.Status409Conflict);
        }

        await Send.ResponseAsync(Response.FromPlayer(result.Player), StatusCodes.Status201Created, cancellationToken);
    }
}
