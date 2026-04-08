using Dapper;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;

/// <summary>
/// Dapper-backed persistence for import run data.
/// </summary>
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
                status AS Status,
                started_at AS StartedAt,
                processed_count AS ProcessedCount,
                failed_count AS FailedCount
            FROM import_runs
            ORDER BY started_at DESC NULLS LAST, id DESC
            LIMIT 1;
            """;

        _logger.LogDebug("Reading latest import run snapshot from PostgreSQL.");
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        try
        {
            return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
                new CommandDefinition(
                    sql,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read latest import run snapshot from PostgreSQL.");
            throw;
        }
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

        try
        {
            return await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    sql,
                    record,
                    commandTimeout: _commandSettings.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create import run record in PostgreSQL.");
            throw;
        }
    }
}
