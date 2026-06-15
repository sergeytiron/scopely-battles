using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Shared.Battles;

public sealed record BattleSimulationOutcome
{
    public required BattleReport Report { get; init; }
    public required Player Winner { get; init; }
    public required Player Loser { get; init; }
}
