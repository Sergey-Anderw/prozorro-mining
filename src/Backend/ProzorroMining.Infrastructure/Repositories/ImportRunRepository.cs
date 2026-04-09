using Dapper;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class ImportRunRepository(
    IDbConnectionFactory connectionFactory,
    PostgresCommandSettings commandSettings)
    : IImportRunRepository
{
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

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
            new CommandDefinition(
                sql,
                commandTimeout: commandSettings.CommandTimeoutSeconds,
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

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
            new CommandDefinition(
                sql,
                new { ImportRunId = importRunId },
                commandTimeout: commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<ImportRunStatusSnapshot?> GetRunningAsync(CancellationToken cancellationToken)
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
            WHERE status = @Status
            ORDER BY started_at DESC NULLS LAST, id DESC
            LIMIT 1;
            """;

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ImportRunStatusSnapshot>(
            new CommandDefinition(
                sql,
                new { Status = "Running" },
                commandTimeout: commandSettings.CommandTimeoutSeconds,
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

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                sql,
                record,
                commandTimeout: commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task<int> FailRunningAsync(DateTime finishedAt, string errorMessage, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE import_runs
            SET
                finished_at = @FinishedAt,
                status = @Status,
                error_message = @ErrorMessage
            WHERE status = @RunningStatus;
            """;

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    FinishedAt = finishedAt,
                    Status = "Failed",
                    ErrorMessage = errorMessage,
                    RunningStatus = "Running"
                },
                commandTimeout: commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task UpdateProgressAsync(long importRunId, UpdateImportRunProgressRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE import_runs
            SET
                status = @Status,
                processed_count = @ProcessedCount,
                inserted_count = @InsertedCount,
                updated_count = @UpdatedCount,
                failed_count = @FailedCount
            WHERE id = @ImportRunId;
            """;

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    ImportRunId = importRunId,
                    record.Status,
                    record.ProcessedCount,
                    record.InsertedCount,
                    record.UpdatedCount,
                    record.FailedCount
                },
                commandTimeout: commandSettings.CommandTimeoutSeconds,
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

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);

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
                commandTimeout: commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }
}
