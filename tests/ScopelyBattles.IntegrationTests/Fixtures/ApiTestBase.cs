using FastEndpoints.Testing;

namespace ScopelyBattles.IntegrationTests.Fixtures;

[Collection<ApiCollection>]
public abstract class ApiTestBase(ApiFixture app) : TestBase, IAsyncLifetime
{
    protected ApiFixture App { get; } = app;

    public async ValueTask InitializeAsync()
    {
        await App.ResetDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await App.ResetDatabaseAsync();
    }
}
