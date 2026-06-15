using ScopelyBattles.Shared.Battles;
using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.UnitTests.Battles;

public sealed class SimulationTests
{
    [Fact]
    public void Simulate_AttacksFirstWithAttackingPlayerAndAlternatesTurns()
    {
        var outcome = CreateBattle(attackerId: 1, defenderId: 2)
            .Simulate(
                CreatePlayer(id: 1, hitPoints: 20, attackValue: 10),
                CreatePlayer(id: 2, hitPoints: 20, attackValue: 10),
                CreateRandom(100, 100, 100, 5, 5)
            );

        Assert.Equal(1, outcome.Report.Turns[0].AttackerId);
        Assert.Equal(2, outcome.Report.Turns[0].DefenderId);
        Assert.Equal(2, outcome.Report.Turns[1].AttackerId);
        Assert.Equal(1, outcome.Report.Turns[1].DefenderId);
        Assert.Equal(1, outcome.Winner.Id);
    }

    [Fact]
    public void Simulate_UsesDefenderDefenseAsMissChance()
    {
        var outcome = CreateBattle(attackerId: 1, defenderId: 2)
            .Simulate(
                CreatePlayer(id: 1, hitPoints: 20, attackValue: 10),
                CreatePlayer(id: 2, hitPoints: 20, attackValue: 10, defenseValue: 25),
                CreateRandom(18, 100, 100, 100, 5, 5)
            );

        Assert.True(outcome.Report.Turns[0].Missed);
        Assert.Equal(0, outcome.Report.Turns[0].Damage);
        Assert.False(outcome.Report.Turns[1].Missed);
    }

    [Fact]
    public void Simulate_ReducesAttackValueAsHitPointsDropWithMinimumOfHalfBaseAttack()
    {
        var outcome = CreateBattle(attackerId: 1, defenderId: 2)
            .Simulate(
                CreatePlayer(id: 1, hitPoints: 100, attackValue: 70),
                CreatePlayer(id: 2, hitPoints: 1_000, attackValue: 10),
                CreateRandom()
            );

        var attackingPlayerTurns = outcome.Report.Turns.Where(turn => turn.AttackerId == 1).ToArray();

        Assert.Equal(70, attackingPlayerTurns[0].AttackValue);
        Assert.Equal(63, attackingPlayerTurns[1].AttackValue);
        Assert.Equal(35, attackingPlayerTurns[^1].AttackValue);
    }

    [Fact]
    public void Simulate_StealsRandomInclusivePercentagesAndRoundsUp()
    {
        var outcome = CreateBattle(attackerId: 1, defenderId: 2)
            .Simulate(
                CreatePlayer(id: 1, hitPoints: 10, attackValue: 100),
                CreatePlayer(id: 2, hitPoints: 1, attackValue: 0, gold: 11, silver: 20),
                CreateRandom(100, 5, 10)
            );

        Assert.NotNull(outcome.Report.StolenResources);
        Assert.Equal(5, outcome.Report.StolenResources.GoldPercent);
        Assert.Equal(10, outcome.Report.StolenResources.SilverPercent);
        Assert.Equal(1, outcome.Report.StolenResources.Gold);
        Assert.Equal(2, outcome.Report.StolenResources.Silver);
        Assert.Equal(1, outcome.Winner.Gold);
        Assert.Equal(2, outcome.Winner.Silver);
        Assert.Equal(10, outcome.Loser.Gold);
        Assert.Equal(18, outcome.Loser.Silver);
        Assert.Equal(3, outcome.Winner.Score);
    }

    private static TestRandomProvider CreateRandom(params int[] rolls) => new(rolls);

    private static Battle CreateBattle(int attackerId, int defenderId) =>
        new()
        {
            Id = 1,
            IdempotencyKey = Guid.NewGuid(),
            AttackerId = attackerId,
            DefenderId = defenderId,
        };

    private static Player CreatePlayer(
        int id,
        int hitPoints,
        int attackValue,
        int defenseValue = 0,
        int gold = 0,
        int silver = 0,
        int score = 0
    ) => new Player($"player-{id}", "test", gold, silver, attackValue, defenseValue, hitPoints, score) { Id = id };

    private sealed class TestRandomProvider(IEnumerable<int> rolls) : IRandomProvider
    {
        private readonly Queue<int> _rolls = new(rolls);

        public int NextInclusive(int minValue, int maxValue)
        {
            var roll = _rolls.Count > 0 ? _rolls.Dequeue() : maxValue;

            Assert.InRange(roll, minValue, maxValue);

            return roll;
        }
    }
}
