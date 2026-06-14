using FastEndpoints;
using ScopelyBattles.Shared.Battles;

namespace ScopelyBattles.Api.Battles.Create;

public sealed class Endpoint(BattleStore battleStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/battles");
        Description(b =>
            b.Produces<Response>(StatusCodes.Status202Accepted)
                .ProducesProblemFE(StatusCodes.Status400BadRequest)
                .ProducesProblemFE(StatusCodes.Status401Unauthorized)
                .ProducesProblemFE(StatusCodes.Status404NotFound)
                .ProducesProblemFE(StatusCodes.Status409Conflict)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status202Accepted] = "Battle queued.";
            s.Responses[StatusCodes.Status400BadRequest] = "Validation failed.";
            s.Responses[StatusCodes.Status401Unauthorized] = "API key is missing or invalid.";
            s.Responses[StatusCodes.Status404NotFound] = "Attacker or defender does not exist.";
            s.Responses[StatusCodes.Status409Conflict] = "Idempotency key was already used for a different battle.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var battle = new Battle
        {
            IdempotencyKey = request.IdempotencyKey,
            AttackerId = request.AttackerId,
            DefenderId = request.DefenderId,
        };
        var result = await battleStore.CreateAsync(battle, cancellationToken);

        if (result.IsMissingPlayer)
        {
            ThrowError(
                request => request.AttackerId,
                "Attacker or defender does not exist.",
                StatusCodes.Status404NotFound
            );
        }

        if (result.IsIdempotencyConflict)
        {
            ThrowError(
                request => request.IdempotencyKey,
                "Idempotency key was already used for a different battle.",
                StatusCodes.Status409Conflict
            );
        }

        await Send.ResponseAsync(Response.FromBattle(result.Battle!), StatusCodes.Status202Accepted, cancellationToken);
    }
}
