using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;

/// <summary>
/// Dapper-backed persistence for suppliers.
/// </summary>
public sealed class SupplierRepository : ISupplierRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<SupplierRepository> _logger;

    public SupplierRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<SupplierRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<long> GetOrCreateAsync(SupplierPersistenceModel supplier, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO suppliers (name)
            VALUES (@Name)
            ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
            RETURNING id;
            """;

        _logger.LogDebug("Getting or creating supplier {SupplierName}.", supplier.Name);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        try
        {
            return await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    sql,
                    supplier,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get or create supplier {SupplierName}.", supplier.Name);
            throw;
        }
    }
}
