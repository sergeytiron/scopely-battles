using ScopelyBattles.Api.Authentication;

namespace ScopelyBattles.IntegrationTests.Fixtures;

public abstract class ApiTestBase(ApiFixture app) : IntegrationTestBase(app)
{
    public override async ValueTask InitializeAsync()
    {
        App.Client.DefaultRequestHeaders.Remove(ApiKeyAuthenticationHandler.HeaderName);
        App.Client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, ApiFixture.ApiKey);
        await base.InitializeAsync();
    }
}
