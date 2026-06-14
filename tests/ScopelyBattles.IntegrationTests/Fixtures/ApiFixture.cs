using FastEndpoints.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Respawn;
using ScopelyBattles.Api.Authentication;
using ScopelyBattles.Shared.DataAccess;
using Testcontainers.PostgreSql;

namespace ScopelyBattles.IntegrationTests.Fixtures;

public sealed class ApiFixture : AppFixture<Program>
{
    public const string ApiKey = "test-api-key";

    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:18.4")
        .WithDatabase("scopelybattles_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner? _respawner;

    public string ConnectionString => _postgresContainer.GetConnectionString();

    protected override IHost ConfigureAppHost(IHostBuilder host)
    {
        host.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { [ApiKeyAuthenticationHandler.ConfigurationKey] = ApiKey }
            )
        );

        return base.ConfigureAppHost(host);
    }

    protected override async ValueTask PreSetupAsync()
    {
        await _postgresContainer.StartAsync();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<PostgresConnectionFactory>();
        services.Configure<ConnectionStrings>(options => options.Postgres = ConnectionString);
        services.AddSingleton<PostgresConnectionFactory>();
    }

    protected override async ValueTask SetupAsync()
    {
        var sqlFiles = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Sql", "001-players.sql"),
            Path.Combine(AppContext.BaseDirectory, "Sql", "002-battles.sql"),
        };

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        foreach (var sqlFile in sqlFiles)
        {
            var sql = await File.ReadAllTextAsync(sqlFile);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["public"] }
        );
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("Database reset cannot run before the API fixture is initialized.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    protected override async ValueTask TearDownAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
