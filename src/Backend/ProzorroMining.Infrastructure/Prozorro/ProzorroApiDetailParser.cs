using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

internal sealed class ProzorroApiDetailParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<ProzorroApiDetailParser> _logger;

    public ProzorroApiDetailParser(ILogger<ProzorroApiDetailParser> logger)
    {
        _logger = logger;
    }

    public ProzorroTenderData? ParseTender(string rawPayload)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<ProzorroTenderDetailEnvelopeDto>(rawPayload, SerializerOptions);
            return ProzorroMappings.ToModel(envelope, rawPayload);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Prozorro tender detail payload.");
            throw;
        }
    }
}
