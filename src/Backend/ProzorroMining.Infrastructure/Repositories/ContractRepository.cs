using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;

/// <summary>
/// Dapper-backed persistence for contracts.
/// </summary>
public sealed class ContractRepository : IContractRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<ContractRepository> _logger;

    public ContractRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<ContractRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<long> CreateAsync(ContractPersistenceModel contract, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO contracts (tender_id, contract_amount)
            VALUES (@TenderId, @ContractAmount)
            RETURNING id;
            """;

        _logger.LogDebug("Creating contract for tender id {TenderId}.", contract.TenderId);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        try
        {
            return await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    sql,
                    contract,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contract for tender id {TenderId}.", contract.TenderId);
            throw;
        }
    }
}
