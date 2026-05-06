using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PulseQueue.Infrastructure.Data;

public sealed class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PulseQueue")
            ?? throw new InvalidOperationException("Connection string 'PulseQueue' is not configured.");
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
