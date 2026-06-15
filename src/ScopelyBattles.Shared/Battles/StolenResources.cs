namespace ScopelyBattles.Shared.Battles;

public sealed record StolenResources
{
    public required int GoldPercent { get; init; }
    public required int SilverPercent { get; init; }
    public required long Gold { get; init; }
    public required long Silver { get; init; }

    public long Total => Gold + Silver;
}
