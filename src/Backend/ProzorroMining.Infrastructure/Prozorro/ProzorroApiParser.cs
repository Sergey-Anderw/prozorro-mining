using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

internal sealed class ProzorroApiParser : IProzorroApiParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<ProzorroApiParser> _logger;

    public ProzorroApiParser(ILogger<ProzorroApiParser> logger)
    {
        _logger = logger;
    }

    public ProzorroTendersPage ParseTendersPage(string rawPayload)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<ProzorroApiEnvelopeDto>(rawPayload, SerializerOptions);
            var page = ProzorroMappings.ToModel(envelope, rawPayload);

            _logger.LogDebug(
                "Parsed Prozorro tenders page. Items: {ItemCount}, NextPath present: {HasNextPath}.",
                page.Items.Count,
                !string.IsNullOrWhiteSpace(page.NextPath));

            return page;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Prozorro tenders payload.");
            throw;
        }
    }
}
