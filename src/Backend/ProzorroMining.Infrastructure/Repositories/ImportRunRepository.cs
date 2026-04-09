using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class ImportRunRepository : IImportRunRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;
    private readonly ILogger<ImportRunRepository> _logger;

    public ImportRunRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings,
        ILogger<ImportRunRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
        _logger = logger;
    }

    public async Task<ImportRunStatusSnapshot?> GetLatestAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS ImportRunId,
                status AS Status,
                started_at AS StartedAt,
                finished_at AS FinishedAt,
                processed_count AS ProcessedCount,
                inserted_count AS InsertedCount,
                updated_count AS UpdatedCount,
                failed_count AS FailedCount,
                error_message AS ErrorMessage
            FROM import_runs
            ORDER BY started_at DESC NULLS LAST, id DESC
            LIMIT 1;
            """;

        _logger.LogDebug("Reading latest import run snapshot from PostgreSQL.");
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
            new CommandDefinition(
                sql,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<ImportRunStatusSnapshot?> GetByIdAsync(long importRunId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS ImportRunId,
                status AS Status,
                started_at AS StartedAt,
                finished_at AS FinishedAt,
                processed_count AS ProcessedCount,
                inserted_count AS InsertedCount,
                updated_count AS UpdatedCount,
                failed_count AS FailedCount,
                error_message AS ErrorMessage
            FROM import_runs
            WHERE id = @ImportRunId
            LIMIT 1;
            """;

        _logger.LogDebug("Reading import run {ImportRunId} snapshot from PostgreSQL.", importRunId);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
            new CommandDefinition(
                sql,
                new { ImportRunId = importRunId },
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<long> CreateAsync(CreateImportRunRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO import_runs (
                started_at,
                finished_at,
                status,
                processed_count,
                inserted_count,
                updated_count,
                failed_count,
                error_message)
            VALUES (
                @StartedAt,
                @FinishedAt,
                @Status,
                @ProcessedCount,
                @InsertedCount,
                @UpdatedCount,
                @FailedCount,
                @ErrorMessage)
            RETURNING id;
            """;

        _logger.LogInformation(
            "Creating import run record with status {Status} and started at {StartedAt}.",
            record.Status,
            record.StartedAt);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                sql,
                record,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task UpdateCompletionAsync(long importRunId, CompleteImportRunRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE import_runs
            SET
                finished_at = @FinishedAt,
                status = @Status,
                processed_count = @ProcessedCount,
                inserted_count = @InsertedCount,
                updated_count = @UpdatedCount,
                failed_count = @FailedCount,
                error_message = @ErrorMessage
            WHERE id = @ImportRunId;
            """;

        _logger.LogDebug(
            "Completing import run {ImportRunId} with status {Status}. Processed: {ProcessedCount}, Inserted: {InsertedCount}, Updated: {UpdatedCount}, Failed: {FailedCount}.",
            importRunId,
            record.Status,
            record.ProcessedCount,
            record.InsertedCount,
            record.UpdatedCount,
            record.FailedCount);
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    ImportRunId = importRunId,
                    record.FinishedAt,
                    record.Status,
                    record.ProcessedCount,
                    record.InsertedCount,
                    record.UpdatedCount,
                    record.FailedCount,
                    record.ErrorMessage
                },
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }
}
