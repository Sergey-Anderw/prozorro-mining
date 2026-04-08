using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProzorroMining.Infrastructure.Configuration;

namespace ProzorroMining.Infrastructure.Prozorro;

/// <summary>
/// Typed HTTP transport for the external Prozorro API.
/// </summary>
internal sealed class ProzorroApiTransport : IProzorroApiTransport
{
    private readonly HttpClient _httpClient;
    private readonly ProzorroApiOptions _options;
    private readonly ILogger<ProzorroApiTransport> _logger;

    public ProzorroApiTransport(
        HttpClient httpClient,
        IOptions<ProzorroApiOptions> options,
        ILogger<ProzorroApiTransport> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetTendersPageAsync(
        string? offsetToken,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                $"Prozorro API base URL is not configured. Set {ProzorroApiOptions.SectionName}:ApiUrl.");
        }

        var effectivePageSize = pageSize ?? _options.DefaultPageSize;
        var path = BuildTendersPath(offsetToken, effectivePageSize);

        _logger.LogInformation(
            "Requesting Prozorro tenders page. OffsetToken present: {HasOffsetToken}, PageSize: {PageSize}.",
            !string.IsNullOrWhiteSpace(offsetToken),
            effectivePageSize);

        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Prozorro API returned non-success status code {StatusCode} for {Path}.",
                (int)response.StatusCode,
                path);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string BuildTendersPath(string? offsetToken, int pageSize)
    {
        var query = new List<string> { $"limit={pageSize}" };
        if (!string.IsNullOrWhiteSpace(offsetToken))
        {
            query.Add($"offset={Uri.EscapeDataString(offsetToken)}");
        }

        return $"tenders?{string.Join("&", query)}";
    }
}
