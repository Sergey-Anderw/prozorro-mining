using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;

/// <summary>
/// Dapper-backed persistence for tenders.
/// </summary>
public sealed class TenderRepository : ITenderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<TenderRepository> _logger;

    public TenderRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<TenderRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<long> UpsertAsync(TenderPersistenceModel tender, CancellationToken cancellationToken)
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
            RETURNING id;
            """;

        _logger.LogInformation(
            "Upserting tender for ProzorroTenderId {ProzorroTenderId}.",
            tender.ProzorroTenderId);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        try
        {
            return await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    sql,
                    tender,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upsert tender for ProzorroTenderId {ProzorroTenderId}.",
                tender.ProzorroTenderId);
            throw;
        }
    }
}
