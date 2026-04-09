using Dapper;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
   

    public DashboardRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
    }

    public async Task<decimal> GetTotalSavingsAsync(CancellationToken cancellationToken)
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

      
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<decimal>(
            new CommandDefinition(
                totalSavingsSql,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<DashboardProcurerData>> GetTopProcurersAsync(CancellationToken cancellationToken)
    {
        const string topProcurersSql = """
            WITH contract_totals AS (
                SELECT tender_id, COALESCE(SUM(contract_amount), 0) AS total_contract_amount
                FROM contracts
                GROUP BY tender_id
            )
            SELECT
                COALESCE(NULLIF(t.procuring_entity_name, ''), 'Unknown') AS Name,
                COALESCE(SUM(COALESCE(ct.total_contract_amount, 0)), 0) AS TotalContractValue
            FROM tenders t
            LEFT JOIN contract_totals ct ON ct.tender_id = t.id
            GROUP BY COALESCE(NULLIF(t.procuring_entity_name, ''), 'Unknown')
            ORDER BY TotalContractValue DESC, Name ASC
            LIMIT 5;
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return (await connection.QueryAsync<DashboardProcurerData>(
            new CommandDefinition(
                topProcurersSql,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken)))
            .ToList();
    }

    public async Task<IReadOnlyList<DashboardSupplierData>> GetTopSuppliersAsync(CancellationToken cancellationToken)
    {
        const string topSuppliersSql = """
            SELECT
                s.name AS Name,
                COALESCE(SUM(c.contract_amount), 0) AS TotalContractValue
            FROM suppliers s
            INNER JOIN tender_suppliers ts ON ts.supplier_id = s.id
            INNER JOIN contracts c ON c.tender_id = ts.tender_id
            GROUP BY s.name
            ORDER BY TotalContractValue DESC, Name ASC
            LIMIT 5;
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return (await connection.QueryAsync<DashboardSupplierData>(
            new CommandDefinition(
                topSuppliersSql,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken)))
            .ToList();
    }
}
