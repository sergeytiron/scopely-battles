using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using ScopelyBattles.Shared.Battles;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.IntegrationTests.Fixtures;

[Collection<ApiCollection>]
public abstract class IntegrationTestBase(ApiFixture app) : TestBase, IAsyncLifetime
{
    protected ApiFixture App { get; } = app;

    public virtual async ValueTask InitializeAsync()
    {
        await App.ResetDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await App.ResetDatabaseAsync();
    }

    protected static Player CreatePlayer(
        int hitPoints = 1,
        int attackValue = 0,
        int defenseValue = 0,
        int gold = 0,
        int silver = 0,
        int score = 0
    ) => new($"player-{Guid.NewGuid():N}"[..20], "test", gold, silver, attackValue, defenseValue, hitPoints, score);

    protected async Task<int> SeedPlayerAsync(int score = 0) => await SeedPlayerAsync(CreatePlayer(score: score));

    protected async Task<int> SeedPlayerAsync(Player player)
    {
        var result = await Store<PlayerStore>().CreateAsync(player, TestContext.Current.CancellationToken);

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

    protected async Task<Battle> SubmitBattleAsync(int attackerId, int defenderId)
    {
        var result = await Store<BattleStore>()
            .CreateAsync(
                new Battle
                {
                    IdempotencyKey = Guid.NewGuid(),
                    AttackerId = attackerId,
                    DefenderId = defenderId,
                },
                TestContext.Current.CancellationToken
            );

        Assert.NotNull(result.Battle);

        return result.Battle;
    }

    protected async Task<Player> GetPlayerAsync(int id)
    {
        var player = await Store<PlayerStore>().GetAsync(id, TestContext.Current.CancellationToken);

        Assert.NotNull(player);

        return player;
    }

    protected async Task<Battle> GetBattleAsync(int id)
    {
        var battle = await Store<BattleStore>().GetAsync(id, TestContext.Current.CancellationToken);

        Assert.NotNull(battle);

        return battle;
    }

    protected async Task<BattleReport?> GetBattleReportAsync(int id) =>
        await Store<BattleStore>().GetReportAsync(id, TestContext.Current.CancellationToken);

    private TStore Store<TStore>()
        where TStore : notnull => App.Services.GetRequiredService<TStore>();
}
