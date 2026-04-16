using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using ProzorroMining.IntegrationTests.Infrastructure;

namespace ProzorroMining.IntegrationTests.Api;

[Collection(PostgresCollection.Name)]
public sealed class AnalyticsSavingsEndpointsTests
{
    private readonly PostgresContainerFixture _postgres;

    public AnalyticsSavingsEndpointsTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task GetSavings_ReturnsAggregatedBudgetSavings()
    {
        await _postgres.ResetDatabaseAsync();
        await SeedAnalyticsDataAsync();

        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/analytics/savings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<SavingsResponse>();
        payload.Should().NotBeNull();
        payload!.TotalSavings.Should().Be(190m);
    }

    private async Task SeedAnalyticsDataAsync()
    {
        const string sql = """
            INSERT INTO tenders (
                id,
                prozorro_tender_id,
                status,
                cpv_code,
                expected_amount,
                procuring_entity_name,
                tender_date,
                date_created,
                date_modified,
                raw_payload)
            VALUES
                (1, 'tender-1', 'complete', '09310000-5', 200, 'Procurer A', TIMESTAMPTZ '2026-04-01 00:00:00+00', TIMESTAMPTZ '2026-04-01 00:00:00+00', TIMESTAMPTZ '2026-04-02 00:00:00+00', '{}'::jsonb),
                (2, 'tender-2', 'complete', '09310000-5', 300, 'Procurer B', TIMESTAMPTZ '2026-04-03 00:00:00+00', TIMESTAMPTZ '2026-04-03 00:00:00+00', TIMESTAMPTZ '2026-04-04 00:00:00+00', '{}'::jsonb),
                (3, 'tender-3', 'complete', '09310000-5', 150, '', TIMESTAMPTZ '2026-04-05 00:00:00+00', TIMESTAMPTZ '2026-04-05 00:00:00+00', TIMESTAMPTZ '2026-04-06 00:00:00+00', '{}'::jsonb);

            INSERT INTO contracts (tender_id, contract_amount)
            VALUES
                (1, 150),
                (1, 60),
                (2, 260),
                (3, 70);
            """;

        await DatabaseTestHelper.ExecuteAsync(_postgres.ConnectionString, sql);
    }

    private sealed class SavingsResponse
    {
        public decimal TotalSavings { get; init; }
    }
}
