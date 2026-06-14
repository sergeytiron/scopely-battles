using System.Net;
using System.Net.Http.Json;
using ScopelyBattles.Api.Authentication;
using ScopelyBattles.IntegrationTests.Fixtures;
using CreateBattleRequest = ScopelyBattles.Api.Battles.Create.Request;
using CreatePlayerRequest = ScopelyBattles.Api.Players.Create.Request;

namespace ScopelyBattles.IntegrationTests.Authentication;

public sealed class EndpointTests(ApiFixture app) : ApiTestBase(app)
{
    [Theory]
    [InlineData("/players")]
    [InlineData("/leaderboard")]
    [InlineData("/battles")]
    public async Task Endpoint_ReturnsUnauthorizedWhenApiKeyIsMissing(string path)
    {
        using var client = App.CreateClient();

        var response = await SendRequestAsync(client, path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/players")]
    [InlineData("/leaderboard")]
    [InlineData("/battles")]
    public async Task Endpoint_ReturnsUnauthorizedWhenApiKeyIsInvalid(string path)
    {
        using var client = CreateClientWithInvalidApiKey();

        var response = await SendRequestAsync(client, path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateClientWithInvalidApiKey() =>
        App.CreateClient(c => c.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, "not-the-key"));

    private static Task<HttpResponseMessage> SendRequestAsync(HttpClient client, string path) =>
        path switch
        {
            "/players" => client.PostAsJsonAsync(path, CreatePlayerRequest(), TestContext.Current.CancellationToken),
            "/leaderboard" => client.GetAsync(path, TestContext.Current.CancellationToken),
            "/battles" => client.PostAsJsonAsync(path, CreateBattleRequest(), TestContext.Current.CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(path), path, "Unknown endpoint path."),
        };

    private static CreatePlayerRequest CreatePlayerRequest() =>
        new()
        {
            Name = $"alice-{Guid.NewGuid():N}"[..20],
            Description = "fighter",
            Gold = 100,
            Silver = 200,
            AttackValue = 10,
            DefenseValue = 12,
            HitPoints = 50,
            Score = 500,
        };

    private static CreateBattleRequest CreateBattleRequest() =>
        new()
        {
            IdempotencyKey = Guid.NewGuid(),
            AttackerId = 1,
            DefenderId = 2,
        };
}
