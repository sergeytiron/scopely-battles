namespace ScopelyBattles.Shared.Battles;

public sealed record StolenResources
{
    public required int GoldPercent { get; init; }
    public required int SilverPercent { get; init; }
    public required int Gold { get; init; }
    public required int Silver { get; init; }

    public int Total => Gold + Silver;
}
