using System.Data;
using Npgsql;
using Serilog;

namespace ProzorroMining.DbMigrator;

internal sealed record Migration(int Id, string Name, string FilePath);

internal sealed class MigrationRunner(string connectionString, string migrationsPath, ILogger logger)
{
    public async Task MigrateAsync()
    {
        logger.Information("Starting database migration using scripts from {MigrationsPath}", migrationsPath);

        if (!Directory.Exists(migrationsPath))
        {
            throw new DirectoryNotFoundException($"Migration directory not found: {migrationsPath}");
        }

        var migrations = LoadMigrations(migrationsPath).ToList();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await EnsureMigrationHistoryTableAsync(connection);

        var appliedMigrations = await GetAppliedMigrationIdsAsync(connection);

        foreach (var migration in migrations)
        {
            if (appliedMigrations.Contains(migration.Id))
            {
                logger.Information("Skipping already applied migration {MigrationId}: {MigrationName}", migration.Id, migration.Name);
                continue;
            }

            logger.Information("Applying migration {MigrationId}: {MigrationName}", migration.Id, migration.Name);
            await ApplyMigrationAsync(connection, migration);
            logger.Information("Successfully applied migration {MigrationId}: {MigrationName}", migration.Id, migration.Name);
        }
    }

    private static IEnumerable<Migration> LoadMigrations(string migrationsPath)
    {
        return Directory.EnumerateFiles(migrationsPath, "*.sql", SearchOption.TopDirectoryOnly)
            .Select(filePath =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var parts = fileName.Split(new[] { "__" }, 2, StringSplitOptions.None);

                if (parts.Length != 2 || !int.TryParse(parts[0], out var id))
                {
                    throw new InvalidOperationException($"Invalid migration filename: {fileName}. Expected format '0001__name.sql'.");
                }

                return new Migration(id, parts[1].Replace('_', ' '), filePath);
            })
            .OrderBy(m => m.Id);
    }

    private async Task EnsureMigrationHistoryTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS migration_history (
    id BIGSERIAL PRIMARY KEY,
    migration_id INTEGER NOT NULL UNIQUE,
    name TEXT NOT NULL,
    applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);";

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<HashSet<int>> GetAppliedMigrationIdsAsync(NpgsqlConnection connection)
    {
        const string sql = "SELECT migration_id FROM migration_history";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var appliedIds = new HashSet<int>();
        while (await reader.ReadAsync())
        {
            if (reader.IsDBNull(0))
            {
                continue;
            }

            appliedIds.Add(reader.GetInt32(0));
        }

        return appliedIds;
    }

    private async Task ApplyMigrationAsync(NpgsqlConnection connection, Migration migration)
    {
        var script = await File.ReadAllTextAsync(migration.FilePath);

        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            await using var command = new NpgsqlCommand(script, connection, transaction);
            await command.ExecuteNonQueryAsync();

            await using var insertCommand = new NpgsqlCommand(
                "INSERT INTO migration_history (migration_id, name) VALUES (@migrationId, @name)",
                connection,
                transaction);

            insertCommand.Parameters.AddWithValue("@migrationId", migration.Id);
            insertCommand.Parameters.AddWithValue("@name", migration.Name);

            await insertCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Migration {MigrationId} failed: {MigrationName}", migration.Id, migration.Name);
            await transaction.RollbackAsync();
            throw;
        }
    }
}
