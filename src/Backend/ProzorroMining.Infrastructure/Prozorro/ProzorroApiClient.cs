using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;
public sealed class ProzorroApiClient : IProzorroApiClient
{
    private readonly IProzorroApiTransport _transport;
    private readonly IProzorroApiParser _parser;
    private readonly ProzorroApiDetailParser _detailParser;

    internal ProzorroApiClient(
        IProzorroApiTransport transport,
        IProzorroApiParser parser,
        ProzorroApiDetailParser detailParser,
        ILogger<ProzorroApiClient> logger)
    {
        _transport = transport;
        _parser = parser;
        _detailParser = detailParser;
    }

    public async Task<ProzorroTendersPage> GetTendersPageAsync(
        string? nextPagePath,
        bool descending,
        CancellationToken cancellationToken)
    {
        var rawPayload = await _transport.GetTendersPageAsync(nextPagePath, descending, cancellationToken);
        var page = _parser.ParseTendersPage(rawPayload);

        return page;
    }

    public async Task<ProzorroTenderData?> GetTenderAsync(string tenderId, CancellationToken cancellationToken)
    {
        var rawPayload = await _transport.GetTenderAsync(tenderId, cancellationToken);
        var tender = _detailParser.ParseTender(rawPayload);

        return tender;
    }
}
