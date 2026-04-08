using System.Data.Common;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ProzorroMining.Infrastructure.Db;

/// <summary>
/// Creates PostgreSQL connections using Npgsql.
/// </summary>
public sealed class NpgsqlConnectionFactory(
    NpgsqlDataSource dataSource,
    ILogger<NpgsqlConnectionFactory> logger) : IDbConnectionFactory
{
    /// <inheritdoc />
    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Opening PostgreSQL connection for infrastructure operation.");
        var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return connection;
    }
}
