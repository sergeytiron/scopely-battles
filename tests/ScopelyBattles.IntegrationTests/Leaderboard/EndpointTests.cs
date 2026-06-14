using System.Net;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using ScopelyBattles.Api.Leaderboard;
using ScopelyBattles.IntegrationTests.Fixtures;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.IntegrationTests.Leaderboard;

public sealed class EndpointTests(ApiFixture app) : ApiTestBase(app)
{
    [Fact]
    public async Task GetLeaderboard_RanksPlayersByScoreDescending()
    {
        int[] expectedScores = [3, 2, 1];
        await SeedPlayersAsync(expectedScores);

        var leaderboard = await GetLeaderboardAsync();
        var actualScores = leaderboard.Entries.Select(entry => entry.Score).ToArray();

        Assert.Equal(expectedScores, actualScores);
    }

    [Fact]
    public async Task GetLeaderboard_AppliesLimitAndOffset()
    {
        await SeedPlayersAsync(2, 1);

        var fullBoard = await GetLeaderboardAsync();
        var secondRowPage = await GetLeaderboardAsync(limit: 1, offset: 1);

        var secondRow = Assert.Single(secondRowPage.Entries);
        Assert.Equal(2, secondRow.Rank);
        Assert.Equal(fullBoard.Entries[1].Id, secondRow.Id);
    }

    private async Task<Response> GetLeaderboardAsync(int limit = 100, int offset = 0)
    {
        var response = await App.Client.GETAsync<Endpoint, Request, Response>(
            new Request { Limit = limit, Offset = offset }
        );

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.NotNull(response.Result);

        return response.Result;
    }

    private async Task SeedPlayersAsync(params int[] scores)
    {
        foreach (var score in scores)
        {
            await SeedPlayerAsync(score);
        }
    }

    private async Task<int> SeedPlayerAsync(int score)
    {
        var player = new Player
        {
            Name = $"lb-{Guid.NewGuid():N}"[..20],
            Description = "leaderboard",
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
}
