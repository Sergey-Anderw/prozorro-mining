using System.Data.Common;

namespace ProzorroMining.Infrastructure.Db;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
