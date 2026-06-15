namespace ScopelyBattles.Shared;

public static class GameRules
{
    public const int MaxBattleTurns = 10_000;
    public const int MaxResourceValue = 1_000_000_000;
    public const long MaxScore = long.MaxValue;
    public const int MinAttackRoll = 1;
    public const int MaxAttackRoll = 100;
    public const int MinDefenseMissChance = 0;
    public const int MaxDefenseMissChance = 95;
    public const decimal MinAttackMultiplier = 0.5m;
    public const int MinStolenResourcePercent = 5;
    public const int MaxStolenResourcePercent = 10;
}
