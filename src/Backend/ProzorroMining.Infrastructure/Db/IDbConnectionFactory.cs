using System.Data.Common;

namespace ProzorroMining.Infrastructure.Db;

/// <summary>
/// Creates open database connections for infrastructure data access.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An open <see cref="DbConnection"/>.</returns>
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
