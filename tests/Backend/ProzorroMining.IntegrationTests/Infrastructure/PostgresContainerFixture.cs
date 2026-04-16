using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

namespace ProzorroMining.IntegrationTests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString => _container?.GetConnectionString()
        ?? throw new InvalidOperationException("PostgreSQL test container is not initialized.");

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("prozorromining_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            await ResetDatabaseAsync();
        }
        catch (ArgumentException ex)
        {
            throw SkipException.ForSkip($"Integration tests require Docker. {ex.Message}");
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        const string dropSql = """
            DROP SCHEMA IF EXISTS public CASCADE;
            CREATE SCHEMA public;
            """;

        await connection.ExecuteAsync(dropSql);

        var migrationPath = Path.Combine(
            RepositoryRoot.Get(),
            "src",
            "Backend",
            "ProzorroMining.DbMigrator",
            "Migrations",
            "0001__initial_schema.sql");

        var migrationSql = await File.ReadAllTextAsync(migrationPath);
        await connection.ExecuteAsync(migrationSql);
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "postgres";
}
