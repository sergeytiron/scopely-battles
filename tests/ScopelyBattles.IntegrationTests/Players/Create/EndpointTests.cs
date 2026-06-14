using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using ScopelyBattles.Api.Players.Create;
using ScopelyBattles.IntegrationTests.Fixtures;

namespace ScopelyBattles.IntegrationTests.Players.Create;

public sealed class EndpointTests(ApiFixture app) : TestBase<ApiFixture>
{
    [Fact]
    public async Task PostPlayers_CreatesPlayer()
    {
        var name = $"alice-{Guid.NewGuid():N}"[..20];

        var request = new Request
        {
            Name = name,
            Description = "fighter",
            Gold = 100,
            Silver = 200,
            AttackValue = 10,
            DefenseValue = 12,
            HitPoints = 50,
            Score = 500,
        };

        var response = await app.Client.POSTAsync<Endpoint, Request, Response>(request);

        Assert.Equal(HttpStatusCode.Created, response.Response.StatusCode);

        var body = response.Result;

        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(request.Name, body.Name);
        Assert.Equal(request.Description, body.Description);
        Assert.Equal(request.Gold, body.Gold);
        Assert.Equal(request.Silver, body.Silver);
        Assert.Equal(request.AttackValue, body.AttackValue);
        Assert.Equal(request.DefenseValue, body.DefenseValue);
        Assert.Equal(request.HitPoints, body.HitPoints);
        Assert.Equal(request.Score, body.Score);
    }

    [Fact]
    public async Task PostPlayers_ReturnsConflictForDuplicateName()
    {
        var name = $"alice-{Guid.NewGuid():N}"[..20];

        var request = new Request
        {
            Name = name,
            Description = "fighter",
            Gold = 100,
            Silver = 200,
            AttackValue = 10,
            DefenseValue = 12,
            HitPoints = 50,
            Score = 500,
        };

        await app.Client.POSTAsync<Endpoint, Request, Response>(request);
        var response = await app.Client.POSTAsync<Endpoint, Request, Response>(request);

        Assert.Equal(HttpStatusCode.Conflict, response.Response.StatusCode);
        Assert.NotNull(response.ErrorContent);
    }
}
