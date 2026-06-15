using FastEndpoints;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Api.Players.Get;

public sealed class Endpoint(PlayerStore playerStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/players/{id}");
        Description(b =>
            b.Produces<Response>(StatusCodes.Status200OK)
                .ProducesProblemFE(StatusCodes.Status401Unauthorized)
                .ProducesProblemFE(StatusCodes.Status404NotFound)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status200OK] = "Player found.";
            s.Responses[StatusCodes.Status401Unauthorized] = "API key is missing or invalid.";
            s.Responses[StatusCodes.Status404NotFound] = "Player does not exist.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var player = await playerStore.GetAsync(request.Id, cancellationToken);

        if (player is null)
        {
            await Send.ResponseAsync(default!, StatusCodes.Status404NotFound, cancellationToken);
            return;
        }

        await Send.ResponseAsync(Response.FromPlayer(player), StatusCodes.Status200OK, cancellationToken);
    }
}
