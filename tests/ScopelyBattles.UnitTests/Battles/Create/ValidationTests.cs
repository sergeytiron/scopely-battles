using ScopelyBattles.Api.Battles.Create;

namespace ScopelyBattles.UnitTests.Battles.Create;

public sealed class ValidationTests
{
    [Fact]
    public void Validate_ReturnsErrorsForInvalidRequest()
    {
        var request = new Request
        {
            IdempotencyKey = Guid.Empty,
            AttackerId = 0,
            DefenderId = 0,
        };

        var result = new Validator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.IdempotencyKey));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.AttackerId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.DefenderId));
    }

    [Fact]
    public void Validate_ReturnsErrorWhenPlayersAreTheSame()
    {
        var request = new Request
        {
            IdempotencyKey = Guid.NewGuid(),
            AttackerId = 1,
            DefenderId = 1,
        };

        var result = new Validator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.DefenderId));
    }

    [Fact]
    public void Validate_ReturnsNoErrorsForValidRequest()
    {
        var request = new Request
        {
            IdempotencyKey = Guid.NewGuid(),
            AttackerId = 1,
            DefenderId = 2,
        };

        var result = new Validator().Validate(request);

        Assert.True(result.IsValid);
    }
}
