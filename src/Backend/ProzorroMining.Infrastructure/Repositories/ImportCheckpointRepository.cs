using Dapper;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;

namespace ProzorroMining.Infrastructure.Repositories;
public sealed class ImportCheckpointRepository : IImportCheckpointRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly PostgresCommandSettings _commandSettings;


    public ImportCheckpointRepository(
        IDbConnectionFactory connectionFactory,
        PostgresCommandSettings commandSettings)
    {
        _connectionFactory = connectionFactory;
        _commandSettings = commandSettings;
   
    }

    public async Task<ImportCheckpointSnapshot?> GetAsync(string sourceName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                source_name AS SourceName,
                last_seen_date AS LastSeenDate,
                last_run_at AS LastRunAt
            FROM import_checkpoint
            WHERE source_name = @SourceName
            LIMIT 1;
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<ImportCheckpointSnapshot>(
            new CommandDefinition(
                sql,
                new { SourceName = sourceName },
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }

    public async Task UpsertAsync(UpdateImportCheckpointRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO import_checkpoint (source_name, last_seen_date, last_run_at)
            VALUES (@SourceName, @LastSeenDate, @LastRunAt)
            ON CONFLICT (source_name) DO UPDATE SET
                last_seen_date = EXCLUDED.last_seen_date,
                last_run_at = EXCLUDED.last_run_at,
                updated_at = NOW();
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                record,
                commandTimeout: _commandSettings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));
    }
}
