namespace ScopelyBattles.Shared.Battles;

public sealed class RandomProvider : IRandomProvider
{
    public int NextInclusive(int minValue, int maxValue) => Random.Shared.Next(minValue, maxValue + 1);
}
