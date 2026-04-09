using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<DashboardRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<DashboardOverviewData> GetOverviewAsync(CancellationToken cancellationToken)
    {
        const string totalSavingsSql = """
            WITH contract_totals AS (
                SELECT tender_id, COALESCE(SUM(contract_amount), 0) AS total_contract_amount
                FROM contracts
                GROUP BY tender_id
            )
            SELECT COALESCE(
                SUM(
                    COALESCE(t.expected_amount, 0) - COALESCE(ct.total_contract_amount, 0)),
                0)
            FROM tenders t
            LEFT JOIN contract_totals ct ON ct.tender_id = t.id;
            """;

        const string topProcurersSql = """
            WITH contract_totals AS (
                SELECT tender_id, COALESCE(SUM(contract_amount), 0) AS total_contract_amount
                FROM contracts
                GROUP BY tender_id
            )
            SELECT
                COALESCE(NULLIF(t.procuring_entity_name, ''), 'Unknown') AS Name,
                COALESCE(SUM(COALESCE(ct.total_contract_amount, 0)), 0) AS TotalContractValue,
                COALESCE(SUM(COALESCE(t.expected_amount, 0) - COALESCE(ct.total_contract_amount, 0)), 0) AS TotalSavings
            FROM tenders t
            LEFT JOIN contract_totals ct ON ct.tender_id = t.id
            GROUP BY COALESCE(NULLIF(t.procuring_entity_name, ''), 'Unknown')
            ORDER BY TotalContractValue DESC, Name ASC
            LIMIT 5;
            """;

        const string topSuppliersSql = """
            SELECT
                s.name AS Name,
                COUNT(c.id)::integer AS ContractCount,
                COALESCE(SUM(c.contract_amount), 0) AS TotalValue
            FROM suppliers s
            INNER JOIN tender_suppliers ts ON ts.supplier_id = s.id
            INNER JOIN contracts c ON c.tender_id = ts.tender_id
            GROUP BY s.name
            ORDER BY TotalValue DESC, Name ASC
            LIMIT 5;
            """;

        _logger.LogDebug("Reading dashboard overview from PostgreSQL.");
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        try
        {
            var totalSavings = await connection.ExecuteScalarAsync<decimal>(
                new CommandDefinition(
                    totalSavingsSql,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            var topProcurers = (await connection.QueryAsync<DashboardProcurerData>(
                new CommandDefinition(
                    topProcurersSql,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)))
                .ToList();

            var topSuppliers = (await connection.QueryAsync<DashboardSupplierData>(
                new CommandDefinition(
                    topSuppliersSql,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)))
                .ToList();

            _logger.LogDebug(
                "Dashboard overview read completed. Procurers: {ProcurerCount}, Suppliers: {SupplierCount}.",
                topProcurers.Count,
                topSuppliers.Count);

            return new DashboardOverviewData(totalSavings, topProcurers, topSuppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read dashboard overview from PostgreSQL.");
            throw;
        }
    }
}
