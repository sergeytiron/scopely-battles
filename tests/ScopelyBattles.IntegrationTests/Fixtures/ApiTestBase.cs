using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using ScopelyBattles.Api.Authentication;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.IntegrationTests.Fixtures;

[Collection<ApiCollection>]
public abstract class ApiTestBase(ApiFixture app) : TestBase, IAsyncLifetime
{
    protected ApiFixture App { get; } = app;

    public async ValueTask InitializeAsync()
    {
        App.Client.DefaultRequestHeaders.Remove(ApiKeyAuthenticationHandler.HeaderName);
        App.Client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, ApiFixture.ApiKey);
        await App.ResetDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await App.ResetDatabaseAsync();
    }

    protected async Task<int> SeedPlayerAsync(int score = 0)
    {
        var player = new Player
        {
            Name = $"pl-{Guid.NewGuid():N}"[..20],
            Description = "test player",
            Gold = 0,
            Silver = 0,
            AttackValue = 0,
            DefenseValue = 0,
            HitPoints = 0,
            Score = score,
        };

        var store = App.Services.GetRequiredService<PlayerStore>();
        var result = await store.CreateAsync(player, TestContext.Current.CancellationToken);

        Assert.NotNull(result.Player);

        return result.Player.Id;
    }

    protected async Task SeedPlayersAsync(params int[] scores)
    {
        foreach (var score in scores)
        {
            await SeedPlayerAsync(score);
        }
    }
}
