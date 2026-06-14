using ScopelyBattles.Api.Leaderboard;

namespace ScopelyBattles.UnitTests.Leaderboard;

public sealed class ValidationTests
{
    [Fact]
    public void Validate_ReturnsErrorsForInvalidRequest()
    {
        var request = new Request { Limit = 0, Offset = -1 };

        var result = new Validator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Limit));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Request.Offset));
    }

    [Fact]
    public void Validate_ReturnsNoErrorsForValidRequest()
    {
        var request = new Request { Limit = 50, Offset = 0 };

        var result = new Validator().Validate(request);

        Assert.True(result.IsValid);
    }
}
