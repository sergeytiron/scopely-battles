using ScopelyBattles.Api.Players.Create;
using ScopelyBattles.Shared;

namespace ScopelyBattles.UnitTests.Players.Create;

public sealed class ValidationTests
{
    [Fact]
    public void Validate_ReturnsErrorsForInvalidRequest()
    {
        var request = new Request
        {
            Name = string.Empty,
            Description = new string('x', 1001),
            Gold = -1,
            Silver = GameRules.MaxResourceValue + 1,
            AttackValue = -1,
            DefenseValue = -1,
            HitPoints = -1,
        };

        var result = new Validator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Description));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Gold));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Silver));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.AttackValue));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.DefenseValue));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.HitPoints));
    }

    [Fact]
    public void Validate_ReturnsNoErrorsForValidRequest()
    {
        var request = new Request
        {
            Name = "alice",
            Description = "fighter",
            Gold = 100,
            Silver = 200,
            AttackValue = 10,
            DefenseValue = 12,
            HitPoints = 50,
        };

        var result = new Validator().Validate(request);

        Assert.True(result.IsValid);
    }
}
