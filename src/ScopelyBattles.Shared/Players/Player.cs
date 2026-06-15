using System.Diagnostics.CodeAnalysis;
using ScopelyBattles.Shared.Battles;

namespace ScopelyBattles.Shared.Players;

public sealed record Player
{
    private int _damageTaken;

    // ctor for dapper
    private Player() { }

    [SetsRequiredMembers]
    public Player(
        string name,
        string? description,
        int gold,
        int silver,
        int attackValue,
        int defenseValue,
        int hitPoints,
        int score
    )
    {
        Name = name;
        Description = description;
        Gold = gold;
        Silver = silver;
        AttackValue = attackValue;
        DefenseValue = defenseValue;
        HitPoints = hitPoints;
        Score = score;
    }

    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }

    public int Gold
    {
        get;
        internal set => field = WithinBounds(value, GameRules.MaxResourceValue, nameof(Gold));
    }

    public int Silver
    {
        get;
        internal set => field = WithinBounds(value, GameRules.MaxResourceValue, nameof(Silver));
    }

    public int AttackValue { get; internal set; }
    public int DefenseValue { get; internal set; }
    public required int HitPoints { get; init; }

    public int Score
    {
        get;
        internal set => field = WithinBounds(value, GameRules.MaxScore, nameof(Score));
    }

    private int CurrentHitPoints => HitPoints - _damageTaken;

    internal bool IsDefeated => CurrentHitPoints <= 0;

    internal void ResetBattleState() => _damageTaken = 0;

    internal BattleTurn Attack(Player defender, int turn, int roll)
    {
        var attackValue = CurrentAttackValue();
        var missed = defender.AttackMisses(roll);
        var damage = missed ? 0 : attackValue;

        if (!missed)
        {
            defender.TakeDamage(damage);
        }

        return new BattleTurn
        {
            Turn = turn,
            AttackerId = Id,
            DefenderId = defender.Id,
            Roll = roll,
            Missed = missed,
            AttackValue = attackValue,
            Damage = damage,
            DefenderHitPoints = defender.CurrentHitPoints,
        };
    }

    internal StolenResources Steal(Player loser, IRandomProvider random)
    {
        var goldPercent = random.NextInclusive(GameRules.MinStolenResourcePercent, GameRules.MaxStolenResourcePercent);
        var silverPercent = random.NextInclusive(
            GameRules.MinStolenResourcePercent,
            GameRules.MaxStolenResourcePercent
        );

        var gold = StolenAmount(loser.Gold, goldPercent);
        var silver = StolenAmount(loser.Silver, silverPercent);

        loser.Gold -= gold;
        loser.Silver -= silver;

        Gold += gold;
        Silver += silver;
        Score += gold + silver;

        return new StolenResources
        {
            GoldPercent = goldPercent,
            SilverPercent = silverPercent,
            Gold = gold,
            Silver = silver,
        };
    }

    private int CurrentAttackValue()
    {
        var scaledAttack = (int)Math.Ceiling(AttackValue * (decimal)CurrentHitPoints / HitPoints);
        var minimumAttack = (int)Math.Ceiling(AttackValue * GameRules.MinAttackMultiplier);

        return Math.Max(scaledAttack, minimumAttack);
    }

    private bool AttackMisses(int roll) =>
        roll <= Math.Clamp(DefenseValue, GameRules.MinDefenseMissChance, GameRules.MaxDefenseMissChance);

    private void TakeDamage(int damage)
    {
        _damageTaken += damage;
    }

    private static int StolenAmount(int balance, int percent) => (int)Math.Ceiling((decimal)balance * percent / 100);

    private int WithinBounds(int value, int max, string resourceName)
    {
        if (value < 0 || value > max)
        {
            throw new BattleSimulationException(
                $"Player {Id} {resourceName.ToLowerInvariant()} must be between 0 and {max}."
            );
        }

        return value;
    }
}
