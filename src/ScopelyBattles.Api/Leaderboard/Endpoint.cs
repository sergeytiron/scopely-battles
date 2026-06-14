using FastEndpoints;
using ScopelyBattles.Shared.Leaderboard;

namespace ScopelyBattles.Api.Leaderboard;

public sealed class Endpoint(LeaderboardStore leaderboardStore) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/leaderboard");
        AllowAnonymous();
        Description(b =>
            b.Produces<Response>(StatusCodes.Status200OK).ProducesProblemFE(StatusCodes.Status400BadRequest)
        );
        Summary(s =>
        {
            s.Responses[StatusCodes.Status200OK] = "Players ranked by score (descending).";
            s.Responses[StatusCodes.Status400BadRequest] = "Invalid paging parameters.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var rows = await leaderboardStore.GetLeaderboardAsync(request.Limit, request.Offset, cancellationToken);

        await Send.ResponseAsync(Response.From(rows, request.Offset), StatusCodes.Status200OK, cancellationToken);
    }
}
