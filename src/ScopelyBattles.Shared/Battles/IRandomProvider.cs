namespace ScopelyBattles.Shared.Battles;

public interface IRandomProvider
{
    int NextInclusive(int minValue, int maxValue);
}
