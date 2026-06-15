using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Shared.Battles;

public sealed record Battle
{
    public int Id { get; init; }
    public required Guid IdempotencyKey { get; init; }
    public required int AttackerId { get; init; }
    public required int DefenderId { get; init; }
    public string Status { get; init; } = "queued";

    public bool HasSamePlayers(Battle battle) => AttackerId == battle.AttackerId && DefenderId == battle.DefenderId;

    public BattleSimulationOutcome Simulate(Player attacker, Player defender, IRandomProvider random)
    {
        attacker.ResetBattleState();
        defender.ResetBattleState();

        var turns = new List<BattleTurn>();

        for (var turn = 1; turn <= GameRules.MaxBattleTurns; turn++)
        {
            var roll = random.NextInclusive(GameRules.MinAttackRoll, GameRules.MaxAttackRoll);
            turns.Add(attacker.Attack(defender, turn, roll));

            if (defender.IsDefeated)
            {
                return Complete(attacker, defender, turns, random);
            }

            (attacker, defender) = (defender, attacker);
        }

        throw new BattleSimulationException($"Battle exceeded {GameRules.MaxBattleTurns} turns.");
    }

    public BattleReport CreateFailureReport(string error) =>
        new()
        {
            BattleId = Id,
            AttackerId = AttackerId,
            DefenderId = DefenderId,
            Error = error,
        };

    private BattleSimulationOutcome Complete(
        Player winner,
        Player loser,
        IReadOnlyList<BattleTurn> turns,
        IRandomProvider random
    )
    {
        var stolenResources = winner.Steal(loser, random);

        return new BattleSimulationOutcome
        {
            Winner = winner,
            Loser = loser,
            Report = new BattleReport
            {
                BattleId = Id,
                AttackerId = AttackerId,
                DefenderId = DefenderId,
                WinnerId = winner.Id,
                LoserId = loser.Id,
                StolenResources = stolenResources,
                Turns = turns,
            },
        };
    }
}
