using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using ProzorroMining.IntegrationTests.Infrastructure;

namespace ProzorroMining.IntegrationTests.Api;

[Collection(PostgresCollection.Name)]
public sealed class AnalyticsEndpointsTests
{
    private readonly PostgresContainerFixture _postgres;

    public AnalyticsEndpointsTests(PostgresContainerFixture postgres)
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

    [Fact]
    public async Task GetTopProcurers_ReturnsTopFiveOrderedByContractValue()
    {
        await _postgres.ResetDatabaseAsync();
        await SeedAnalyticsDataAsync();

        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/analytics/top-procurers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<RankedPartyResponse>>();
        payload.Should().NotBeNull();
        payload!.Select(x => (x.Name, x.TotalContractValue)).Should().Equal(
            ("Procurer B", 260m),
            ("Procurer A", 210m),
            ("Unknown", 70m),
            ("Procurer C", 60m),
            ("Procurer D", 50m));
    }

    [Fact]
    public async Task GetTopSuppliers_ReturnsTopFiveOrderedByContractValue()
    {
        await _postgres.ResetDatabaseAsync();
        await SeedAnalyticsDataAsync();

        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/analytics/top-suppliers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<RankedPartyResponse>>();
        payload.Should().NotBeNull();
        payload!.Select(x => (x.Name, x.TotalContractValue)).Should().Equal(
            ("Supplier 3", 260m),
            ("Supplier 2", 210m),
            ("Supplier 1", 180m),
            ("Supplier 4", 60m),
            ("Supplier 5", 50m));
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
                (3, 'tender-3', 'complete', '09310000-5', 150, '', TIMESTAMPTZ '2026-04-05 00:00:00+00', TIMESTAMPTZ '2026-04-05 00:00:00+00', TIMESTAMPTZ '2026-04-06 00:00:00+00', '{}'::jsonb),
                (4, 'tender-4', 'complete', '09310000-5', 120, 'Procurer C', TIMESTAMPTZ '2026-04-07 00:00:00+00', TIMESTAMPTZ '2026-04-07 00:00:00+00', TIMESTAMPTZ '2026-04-08 00:00:00+00', '{}'::jsonb),
                (5, 'tender-5', 'complete', '09310000-5', 90, 'Procurer D', TIMESTAMPTZ '2026-04-09 00:00:00+00', TIMESTAMPTZ '2026-04-09 00:00:00+00', TIMESTAMPTZ '2026-04-10 00:00:00+00', '{}'::jsonb),
                (6, 'tender-6', 'complete', '09310000-5', 40, 'Procurer E', TIMESTAMPTZ '2026-04-11 00:00:00+00', TIMESTAMPTZ '2026-04-11 00:00:00+00', TIMESTAMPTZ '2026-04-12 00:00:00+00', '{}'::jsonb);

            INSERT INTO contracts (tender_id, contract_amount)
            VALUES
                (1, 150),
                (1, 60),
                (2, 260),
                (3, 70),
                (4, 60),
                (5, 50);

            INSERT INTO suppliers (id, name)
            VALUES
                (1, 'Supplier 1'),
                (2, 'Supplier 2'),
                (3, 'Supplier 3'),
                (4, 'Supplier 4'),
                (5, 'Supplier 5'),
                (6, 'Supplier 6');

            INSERT INTO tender_suppliers (tender_id, supplier_id)
            VALUES
                (1, 1),
                (1, 2),
                (2, 3),
                (3, 1),
                (4, 4),
                (5, 5),
                (6, 6);
            """;

        await DatabaseTestHelper.ExecuteAsync(_postgres.ConnectionString, sql);
    }

    private sealed class SavingsResponse
    {
        public decimal TotalSavings { get; init; }
    }

    private sealed class RankedPartyResponse
    {
        public required string Name { get; init; }
        public decimal TotalContractValue { get; init; }
    }
}
