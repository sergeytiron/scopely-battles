using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using ScopelyBattles.IntegrationTests.Fixtures;
using ScopelyBattles.Shared;
using ScopelyBattles.Shared.Battles;
using ScopelyBattles.Shared.Battles.Processing;
using ScopelyBattles.Shared.DataAccess;

namespace ScopelyBattles.IntegrationTests.Battles.Process;

public sealed class ProcessorTests(ApiFixture app) : IntegrationTestBase(app)
{
    [Fact]
    public async Task ProcessNext_CompletesBattleAndUpdatesPlayers()
    {
        var attackerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var defenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 1, gold: 11, silver: 20));
        var battle = await SubmitBattleAsync(attackerId, defenderId);
        var processor = CreateProcessor(100, 10);

        var processedBattle = await processor.ProcessNextAsync(TestContext.Current.CancellationToken);

        Assert.True(processedBattle);

        var attacker = await GetPlayerAsync(attackerId);
        var defender = await GetPlayerAsync(defenderId);
        var completedBattle = await GetBattleAsync(battle.Id);
        var report = await GetBattleReportAsync(battle.Id);

        Assert.Equal("completed", completedBattle.Status);
        Assert.NotNull(report);
        Assert.Equal(attackerId, report.WinnerId);
        Assert.Equal(defenderId, report.LoserId);
        Assert.Equal(2, attacker.Gold);
        Assert.Equal(2, attacker.Silver);
        Assert.Equal(4, attacker.Score);
        Assert.Equal(9, defender.Gold);
        Assert.Equal(18, defender.Silver);
    }

    [Fact]
    public async Task ProcessNext_DoesNotApplyCompletedBattleMoreThanOnce()
    {
        var attackerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var defenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 1, gold: 11, silver: 20));
        await SubmitBattleAsync(attackerId, defenderId);
        var processor = CreateProcessor(100, 10);

        Assert.True(await processor.ProcessNextAsync(TestContext.Current.CancellationToken));
        Assert.False(await processor.ProcessNextAsync(TestContext.Current.CancellationToken));

        var attacker = await GetPlayerAsync(attackerId);
        var defender = await GetPlayerAsync(defenderId);

        Assert.Equal(2, attacker.Gold);
        Assert.Equal(2, attacker.Silver);
        Assert.Equal(4, attacker.Score);
        Assert.Equal(9, defender.Gold);
        Assert.Equal(18, defender.Silver);
    }

    [Fact]
    public async Task ProcessNext_PicksQueuedBattlesInSubmissionOrderWhenPlayersAreAvailable()
    {
        var firstAttackerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var firstDefenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 0));
        var secondAttackerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var secondDefenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 0));
        var firstBattle = await SubmitBattleAsync(firstAttackerId, firstDefenderId);
        var secondBattle = await SubmitBattleAsync(secondAttackerId, secondDefenderId);
        var processor = CreateProcessor(100, 5, 5);

        Assert.True(await processor.ProcessNextAsync(TestContext.Current.CancellationToken));

        Assert.Equal("completed", (await GetBattleAsync(firstBattle.Id)).Status);
        Assert.Equal("queued", (await GetBattleAsync(secondBattle.Id)).Status);
    }

    [Fact]
    public async Task ProcessNext_CanProcessLaterBattleWhenOldestBattleSharesLockedPlayer()
    {
        var lockedPlayerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var firstDefenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 0));
        var secondAttackerId = await SeedPlayerAsync(CreatePlayer(hitPoints: 10, attackValue: 100));
        var secondDefenderId = await SeedPlayerAsync(CreatePlayer(hitPoints: 1, attackValue: 0));
        var firstBattle = await SubmitBattleAsync(lockedPlayerId, firstDefenderId);
        var secondBattle = await SubmitBattleAsync(secondAttackerId, secondDefenderId);

        await using var playerLock = await LockPlayerAsync(lockedPlayerId);

        var processor = CreateProcessor(100, 5, 5);

        Assert.True(await processor.ProcessNextAsync(TestContext.Current.CancellationToken));

        Assert.Equal("queued", (await GetBattleAsync(firstBattle.Id)).Status);
        Assert.Equal("completed", (await GetBattleAsync(secondBattle.Id)).Status);
    }

    [Fact]
    public async Task ProcessNext_MarksDomainFailuresAsFailedWithoutUpdatingPlayers()
    {
        var attackerId = await SeedPlayerAsync(
            CreatePlayer(hitPoints: 10, attackValue: 100, gold: GameRules.MaxResourceValue)
        );
        var defenderId = await SeedPlayerAsync(
            CreatePlayer(hitPoints: 1, attackValue: 0, gold: GameRules.MaxResourceValue)
        );
        var battle = await SubmitBattleAsync(attackerId, defenderId);
        var processor = CreateProcessor(100, 5, 5);

        Assert.True(await processor.ProcessNextAsync(TestContext.Current.CancellationToken));

        var attacker = await GetPlayerAsync(attackerId);
        var defender = await GetPlayerAsync(defenderId);
        var failedBattle = await GetBattleAsync(battle.Id);
        var report = await GetBattleReportAsync(battle.Id);

        Assert.Equal("failed", failedBattle.Status);
        Assert.NotNull(report);
        Assert.Contains("gold must be between", report.Error);
        Assert.Equal(GameRules.MaxResourceValue, attacker.Gold);
        Assert.Equal(GameRules.MaxResourceValue, defender.Gold);
    }

    private BattleProcessor CreateProcessor(params int[] rolls)
    {
        var connectionFactory = App.Services.GetRequiredService<PostgresConnectionFactory>();
        return new BattleProcessor(
            connectionFactory,
            new TestRandomProvider(rolls),
            NullLogger<BattleProcessor>.Instance
        );
    }

    private async Task<IAsyncDisposable> LockPlayerAsync(int id)
    {
        var connection = new NpgsqlConnection(App.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                "SELECT id FROM players WHERE id = @Id FOR UPDATE;",
                new { Id = id },
                transaction,
                cancellationToken: TestContext.Current.CancellationToken
            )
        );

        return new PlayerLock(connection, transaction);
    }

    private sealed class PlayerLock(NpgsqlConnection connection, NpgsqlTransaction transaction) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestRandomProvider(IEnumerable<int> rolls) : IRandomProvider
    {
        private readonly Queue<int> _rolls = new(rolls);

        public int NextInclusive(int minValue, int maxValue)
        {
            var roll = _rolls.Dequeue();

            Assert.InRange(roll, minValue, maxValue);

            return roll;
        }
    }
}
