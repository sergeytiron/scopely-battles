using FastEndpoints;
using ScopelyBattles.Shared.Battles;

namespace ScopelyBattles.Api.Battles.Get;

public sealed class Endpoint(BattleStore battleStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/battles/{id}");
        Description(b =>
            b.Produces<Response>(StatusCodes.Status200OK)
                .ProducesProblemFE(StatusCodes.Status401Unauthorized)
                .ProducesProblemFE(StatusCodes.Status404NotFound)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status200OK] = "Battle found.";
            s.Responses[StatusCodes.Status401Unauthorized] = "API key is missing or invalid.";
            s.Responses[StatusCodes.Status404NotFound] = "Battle does not exist.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var battle = await battleStore.GetAsync(request.Id, cancellationToken);

        if (battle is null)
        {
            await Send.NotFoundAsync(cancellationToken);
            return;
        }

        var report = await battleStore.GetReportAsync(request.Id, cancellationToken);

        await Send.ResponseAsync(Response.FromBattle(battle, report), StatusCodes.Status200OK, cancellationToken);
    }
}
