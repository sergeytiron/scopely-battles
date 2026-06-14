using FastEndpoints.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using ScopelyBattles.Shared.DataAccess;
using Testcontainers.PostgreSql;

namespace ScopelyBattles.IntegrationTests.Fixtures;

public sealed class ApiFixture : AppFixture<Program>
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:18.4")
        .WithDatabase("scopelybattles_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();

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
    }

    protected override async ValueTask TearDownAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
