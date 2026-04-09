using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class ImportedTenderPersistence : IImportedTenderPersistence
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<ImportedTenderPersistence> _logger;

    public ImportedTenderPersistence(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<ImportedTenderPersistence> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<ImportedTenderPersistenceResult> PersistAsync(
        ImportedTenderPersistenceModel importedTender,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var upsertResult = await UpsertTenderAsync(connection, transaction, importedTender.Tender, cancellationToken);
            await RefreshContractsAsync(connection, transaction, upsertResult.TenderId, importedTender.ContractAmounts, cancellationToken);
            await RefreshSuppliersAsync(connection, transaction, upsertResult.TenderId, importedTender.SupplierNames, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new ImportedTenderPersistenceResult(upsertResult.TenderId, upsertResult.Inserted);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to persist tender aggregate {ProzorroTenderId}.", importedTender.Tender.ProzorroTenderId);
            throw;
        }
    }

    private async Task<(long TenderId, bool Inserted)> UpsertTenderAsync(
        DbConnection connection,
        DbTransaction transaction,
        TenderPersistenceModel tender,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO tenders (
                prozorro_tender_id,
                status,
                cpv_code,
                expected_amount,
                procuring_entity_name,
                tender_date,
                date_created,
                date_modified,
                raw_payload)
            VALUES (
                @ProzorroTenderId,
                @Status,
                @CpvCode,
                @ExpectedAmount,
                @ProcuringEntityName,
                @TenderDate,
                @DateCreated,
                @DateModified,
                CAST(@RawPayload AS jsonb))
            ON CONFLICT (prozorro_tender_id) DO UPDATE SET
                status = EXCLUDED.status,
                cpv_code = EXCLUDED.cpv_code,
                expected_amount = EXCLUDED.expected_amount,
                procuring_entity_name = EXCLUDED.procuring_entity_name,
                tender_date = EXCLUDED.tender_date,
                date_created = EXCLUDED.date_created,
                date_modified = EXCLUDED.date_modified,
                raw_payload = EXCLUDED.raw_payload,
                updated_at = NOW()
            RETURNING id AS TenderId, (xmax = 0) AS Inserted;
            """;

        return await connection.QuerySingleAsync<(long TenderId, bool Inserted)>(
            new CommandDefinition(
                sql,
                tender,
                transaction,
                _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    private async Task RefreshContractsAsync(
        DbConnection connection,
        DbTransaction transaction,
        long tenderId,
        IReadOnlyList<decimal> contractAmounts,
        CancellationToken cancellationToken)
    {
        const string deleteSql = "DELETE FROM contracts WHERE tender_id = @TenderId;";
        const string insertSql = """
            INSERT INTO contracts (tender_id, contract_amount)
            VALUES (@TenderId, @ContractAmount);
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                deleteSql,
                new { TenderId = tenderId },
                transaction,
                _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

        foreach (var contractAmount in contractAmounts.Where(x => x > 0))
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertSql,
                    new { TenderId = tenderId, ContractAmount = contractAmount },
                    transaction,
                    _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
    }

    private async Task RefreshSuppliersAsync(
        DbConnection connection,
        DbTransaction transaction,
        long tenderId,
        IReadOnlyList<string> supplierNames,
        CancellationToken cancellationToken)
    {
        const string deleteLinksSql = "DELETE FROM tender_suppliers WHERE tender_id = @TenderId;";
        const string upsertSupplierSql = """
            INSERT INTO suppliers (name)
            VALUES (@Name)
            ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
            RETURNING id;
            """;
        const string insertLinkSql = """
            INSERT INTO tender_suppliers (tender_id, supplier_id)
            VALUES (@TenderId, @SupplierId)
            ON CONFLICT (tender_id, supplier_id) DO NOTHING;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                deleteLinksSql,
                new { TenderId = tenderId },
                transaction,
                _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

        foreach (var supplierName in supplierNames
                     .Where(name => !string.IsNullOrWhiteSpace(name))
                     .Select(name => name.Trim())
                     .Distinct(StringComparer.Ordinal))
        {
            var supplierId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    upsertSupplierSql,
                    new { Name = supplierName },
                    transaction,
                    _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertLinkSql,
                    new { TenderId = tenderId, SupplierId = supplierId },
                    transaction,
                    _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
    }
}
