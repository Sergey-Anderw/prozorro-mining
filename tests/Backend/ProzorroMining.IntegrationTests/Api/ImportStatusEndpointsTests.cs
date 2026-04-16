using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using ProzorroMining.IntegrationTests.Infrastructure;

namespace ProzorroMining.IntegrationTests.Api;

[Collection(PostgresCollection.Name)]
public sealed class ImportStatusEndpointsTests
{
    private readonly PostgresContainerFixture _postgres;

    public ImportStatusEndpointsTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task GetStatus_ReturnsRunningImport_WhenOneExists()
    {
        await _postgres.ResetDatabaseAsync();
        await SeedImportRunsAsync();

        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/import/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ImportStatusResponse>();
        payload.Should().NotBeNull();
        payload!.ImportRunId.Should().Be(2);
        payload.Status.Should().Be("Running");
        payload.ProcessedCount.Should().Be(11);
        payload.InsertedCount.Should().Be(7);
        payload.UpdatedCount.Should().Be(3);
        payload.FailedCount.Should().Be(1);
        payload.FinishedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetStatus_ReturnsPending_WhenNoImportsExist()
    {
        await _postgres.ResetDatabaseAsync();

        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/import/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ImportStatusResponse>();
        payload.Should().NotBeNull();
        payload!.ImportRunId.Should().BeNull();
        payload.Status.Should().Be("Pending");
        payload.ProcessedCount.Should().Be(0);
    }

    private async Task SeedImportRunsAsync()
    {
        const string sql = """
            INSERT INTO import_runs (
                id,
                started_at,
                finished_at,
                status,
                processed_count,
                inserted_count,
                updated_count,
                failed_count,
                error_message)
            VALUES
                (1, TIMESTAMPTZ '2026-04-10 09:00:00+00', TIMESTAMPTZ '2026-04-10 09:05:00+00', 'Success', 5, 4, 1, 0, NULL),
                (2, TIMESTAMPTZ '2026-04-11 12:00:00+00', NULL, 'Running', 11, 7, 3, 1, NULL);
            """;

        await DatabaseTestHelper.ExecuteAsync(_postgres.ConnectionString, sql);
    }

    private sealed class ImportStatusResponse
    {
        public long? ImportRunId { get; init; }
        public required string Status { get; init; }
        public DateTime? FinishedAt { get; init; }
        public int ProcessedCount { get; init; }
        public int InsertedCount { get; init; }
        public int UpdatedCount { get; init; }
        public int FailedCount { get; init; }
    }
}
