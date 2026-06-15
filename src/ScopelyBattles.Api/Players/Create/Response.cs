using ScopelyBattles.Shared.Players;

namespace ScopelyBattles.Api.Players.Create;

public sealed record Response
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int Gold { get; init; }
    public required int Silver { get; init; }
    public required int AttackValue { get; init; }
    public required int DefenseValue { get; init; }
    public required int HitPoints { get; init; }
    public required long Score { get; init; }

    public static Response FromPlayer(Player player) =>
        new()
        {
            Id = player.Id,
            Name = player.Name,
            Description = player.Description,
            Gold = player.Gold,
            Silver = player.Silver,
            AttackValue = player.AttackValue,
            DefenseValue = player.DefenseValue,
            HitPoints = player.HitPoints,
            Score = player.Score,
        };
}
