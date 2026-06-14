using System.Diagnostics.CodeAnalysis;

namespace ScopelyBattles.Shared.Players;

public sealed record CreateResult
{
    private CreateResult(bool isDuplicateName, Player? player) => (IsDuplicateName, Player) = (isDuplicateName, player);

    [MemberNotNullWhen(false, nameof(Player))]
    public bool IsDuplicateName { get; init; }

    public Player? Player { get; init; }

    public static CreateResult Success(Player player) => new(false, player);

    public static CreateResult DuplicateName() => new(true, null);
}
