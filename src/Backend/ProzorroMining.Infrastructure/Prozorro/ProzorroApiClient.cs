using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

/// <summary>
/// Typed HTTP client for the external Prozorro API.
/// </summary>
public sealed class ProzorroApiClient : IProzorroApiClient
{
    private readonly IProzorroApiTransport _transport;
    private readonly IProzorroApiParser _parser;
    private readonly ILogger<ProzorroApiClient> _logger;

    internal ProzorroApiClient(
        IProzorroApiTransport transport,
        IProzorroApiParser parser,
        ILogger<ProzorroApiClient> logger)
    {
        _transport = transport;
        _parser = parser;
        _logger = logger;
    }

    public async Task<ProzorroTendersPage> GetTendersPageAsync(
        string? offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        var rawPayload = await _transport.GetTendersPageAsync(offset, limit, cancellationToken);
        var page = _parser.ParseTendersPage(rawPayload);

        _logger.LogInformation(
            "Prozorro tenders page retrieved. ItemCount: {ItemCount}, NextOffset present: {HasNextOffset}.",
            page.Items.Count,
            !string.IsNullOrWhiteSpace(page.NextOffset));

        return page;
    }
}
