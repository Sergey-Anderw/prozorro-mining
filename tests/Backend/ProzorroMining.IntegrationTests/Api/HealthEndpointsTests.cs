using FluentAssertions;
using System.Net;
using ProzorroMining.IntegrationTests.Infrastructure;

namespace ProzorroMining.IntegrationTests.Api;

[Collection(PostgresCollection.Name)]
public sealed class HealthEndpointsTests
{
    private readonly PostgresContainerFixture _postgres;

    public HealthEndpointsTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task GetLive_ReturnsHealthyText()
    {
        await _postgres.ResetDatabaseAsync();
        await using var factory = new CustomWebApplicationFactory(_postgres.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }
}
