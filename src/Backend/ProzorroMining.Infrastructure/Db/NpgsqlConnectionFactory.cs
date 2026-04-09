using System.Data.Common;
using Npgsql;

namespace ProzorroMining.Infrastructure.Db;

public sealed class NpgsqlConnectionFactory(
    NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return connection;
    }
}
