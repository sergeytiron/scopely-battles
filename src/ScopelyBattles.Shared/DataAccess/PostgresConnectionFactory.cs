using Microsoft.Extensions.Options;
using Npgsql;

namespace ScopelyBattles.Shared.DataAccess;

public sealed class PostgresConnectionFactory(IOptions<ConnectionStrings> options)
{
    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(options.Value.Postgres);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
