using System.Net;
using FastEndpoints;
using ScopelyBattles.Api.Battles.Create;
using ScopelyBattles.IntegrationTests.Fixtures;

namespace ScopelyBattles.IntegrationTests.Battles.Create;

public sealed class EndpointTests(ApiFixture app) : ApiTestBase(app)
{
    [Fact]
    public async Task PostBattles_QueuesBattle()
    {
        var attackerId = await SeedPlayerAsync();
        var defenderId = await SeedPlayerAsync();
        var request = CreateRequest(attackerId, defenderId);

        var response = await App.Client.POSTAsync<Endpoint, Request, Response>(request);

        Assert.Equal(HttpStatusCode.Accepted, response.Response.StatusCode);

        var body = response.Result;
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(request.AttackerId, body.AttackerId);
        Assert.Equal(request.DefenderId, body.DefenderId);
        Assert.Equal("queued", body.Status);
    }

    [Fact]
    public async Task PostBattles_ReturnsSameBattleForSameIdempotencyKey()
    {
        var attackerId = await SeedPlayerAsync();
        var defenderId = await SeedPlayerAsync();
        var request = CreateRequest(attackerId, defenderId);

        var firstResponse = await App.Client.POSTAsync<Endpoint, Request, Response>(request);
        var secondResponse = await App.Client.POSTAsync<Endpoint, Request, Response>(request);

        Assert.Equal(HttpStatusCode.Accepted, secondResponse.Response.StatusCode);
        Assert.NotNull(firstResponse.Result);
        Assert.NotNull(secondResponse.Result);
        Assert.Equal(firstResponse.Result.Id, secondResponse.Result.Id);
    }

    [Fact]
    public async Task PostBattles_ReturnsConflictWhenIdempotencyKeyIsReusedForDifferentBattle()
    {
        var attackerId = await SeedPlayerAsync();
        var firstDefenderId = await SeedPlayerAsync();
        var secondDefenderId = await SeedPlayerAsync();
        var idempotencyKey = Guid.NewGuid();

        await App.Client.POSTAsync<Endpoint, Request, Response>(
            CreateRequest(attackerId, firstDefenderId, idempotencyKey)
        );
        var response = await App.Client.POSTAsync<Endpoint, Request, Response>(
            CreateRequest(attackerId, secondDefenderId, idempotencyKey)
        );

        Assert.Equal(HttpStatusCode.Conflict, response.Response.StatusCode);
        Assert.NotNull(response.ErrorContent);
    }

    [Fact]
    public async Task PostBattles_ReturnsNotFoundWhenPlayerDoesNotExist()
    {
        var request = CreateRequest(attackerId: 1, defenderId: 2);

        var response = await App.Client.POSTAsync<Endpoint, Request, Response>(request);

        Assert.Equal(HttpStatusCode.NotFound, response.Response.StatusCode);
        Assert.NotNull(response.ErrorContent);
    }

    private static Request CreateRequest(int attackerId, int defenderId, Guid? idempotencyKey = null) =>
        new()
        {
            IdempotencyKey = idempotencyKey ?? Guid.NewGuid(),
            AttackerId = attackerId,
            DefenderId = defenderId,
        };
}
