namespace ScopelyBattles.Shared.Battles;

public sealed record CreateResult
{
    private CreateResult(Battle? battle = null, bool isMissingPlayer = false, bool isIdempotencyConflict = false) =>
        (Battle, IsMissingPlayer, IsIdempotencyConflict) = (battle, isMissingPlayer, isIdempotencyConflict);

    public Battle? Battle { get; }
    public bool IsMissingPlayer { get; }
    public bool IsIdempotencyConflict { get; }

    public static CreateResult Success(Battle battle) => new(battle: battle);

    public static CreateResult MissingPlayer() => new(isMissingPlayer: true);

    public static CreateResult IdempotencyConflict() => new(isIdempotencyConflict: true);
}
