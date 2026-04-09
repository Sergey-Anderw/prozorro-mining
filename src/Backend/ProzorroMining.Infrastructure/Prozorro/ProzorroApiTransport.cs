using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProzorroMining.Infrastructure.Configuration;

namespace ProzorroMining.Infrastructure.Prozorro;
internal sealed class ProzorroApiTransport : IProzorroApiTransport
{
    private readonly HttpClient _httpClient;
    private readonly ProzorroApiOptions _options;
    private readonly ProzorroDetailRequestLimiter _detailRequestLimiter;
    private readonly ILogger<ProzorroApiTransport> _logger;

    public ProzorroApiTransport(
        HttpClient httpClient,
        IOptions<ProzorroApiOptions> options,
        ProzorroDetailRequestLimiter detailRequestLimiter,
        ILogger<ProzorroApiTransport> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _detailRequestLimiter = detailRequestLimiter;
        _logger = logger;
    }

    public async Task<string> GetTendersPageAsync(
        string? nextPagePath,
        bool descending,
        CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                $"Prozorro API base URL is not configured. Set {ProzorroApiOptions.SectionName}:ApiUrl.");
        }

        var requestUri = BuildTendersUri(_httpClient.BaseAddress, nextPagePath, descending);

        _logger.LogDebug(
            "Requesting Prozorro tenders page. HasNextPagePath: {HasNextPagePath}, Descending: {Descending}.",
            !string.IsNullOrWhiteSpace(nextPagePath),
            descending);

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Prozorro API returned non-success status code {StatusCode} for {RequestUri}.",
                (int)response.StatusCode,
                requestUri);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetTenderAsync(string tenderId, CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                $"Prozorro API base URL is not configured. Set {ProzorroApiOptions.SectionName}:ApiUrl.");
        }

        var path = $"tenders/{Uri.EscapeDataString(tenderId)}";
        await _detailRequestLimiter.WaitAsync(cancellationToken);
        _logger.LogDebug("Requesting Prozorro tender detail for {TenderId}.", tenderId);

        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Prozorro API returned non-success status code {StatusCode} for tender {TenderId}.",
                (int)response.StatusCode,
                tenderId);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static Uri BuildTendersUri(Uri baseAddress, string? nextPagePath, bool descending)
    {
        if (!string.IsNullOrWhiteSpace(nextPagePath))
        {
            if (Uri.TryCreate(nextPagePath, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }

            return new Uri(baseAddress, nextPagePath);
        }

        var query = new List<string>();
        if (descending)
        {
            query.Add("descending=1");
        }

        var relativePath = query.Count == 0
            ? "tenders"
            : $"tenders?{string.Join("&", query)}";

        return new Uri(baseAddress, relativePath);
    }
}
