using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProzorroMining.App.Abstractions.Prozorro;
using ProzorroMining.Infrastructure.Configuration;

namespace ProzorroMining.Infrastructure.Prozorro;
public sealed class ProzorroApiClient : IProzorroApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ProzorroDetailRequestLimiter _detailRequestLimiter;
    private readonly ILogger<ProzorroApiClient> _logger;

    public ProzorroApiClient(
        HttpClient httpClient,
        IOptions<ProzorroApiOptions> options,
        ProzorroDetailRequestLimiter detailRequestLimiter,
        ILogger<ProzorroApiClient> logger)
    {
        _httpClient = httpClient;
        _detailRequestLimiter = detailRequestLimiter;
        _logger = logger;

        var apiOptions = options.Value;
        if (Uri.TryCreate(apiOptions.ApiUrl, UriKind.Absolute, out var baseAddress))
        {
            _httpClient.BaseAddress = baseAddress;
        }
        else
        {
            _logger.LogWarning(
                "Prozorro API base URL is not configured. External calls will fail until {Section}:ApiUrl is set.",
                ProzorroApiOptions.SectionName);
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<ProzorroTendersPage> GetTendersPageAsync(
        string? nextPagePath,
        bool descending,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildTendersUri(nextPagePath, descending);

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Prozorro API returned non-success status code {StatusCode} for {RequestUri}.",
                (int)response.StatusCode,
                requestUri);
        }

        response.EnsureSuccessStatusCode();
        var rawPayload = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var envelope = JsonSerializer.Deserialize<ProzorroApiEnvelopeDto>(rawPayload, SerializerOptions);
            var page = ProzorroMappings.ToModel(envelope, rawPayload);

            return page;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Prozorro tenders payload.");
            throw;
        }
    }

    public async Task<ProzorroTenderData?> GetTenderAsync(string tenderId, CancellationToken cancellationToken)
    {
        EnsureBaseAddress();

        await _detailRequestLimiter.WaitAsync(cancellationToken);
        
        using var response = await _httpClient.GetAsync(
            $"tenders/{Uri.EscapeDataString(tenderId)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Prozorro API returned non-success status code {StatusCode} for tender {TenderId}.",
                (int)response.StatusCode,
                tenderId);
        }

        response.EnsureSuccessStatusCode();
        var rawPayload = await response.Content.ReadAsStringAsync(cancellationToken);

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

    private Uri BuildTendersUri(string? nextPagePath, bool descending)
    {
        EnsureBaseAddress();

        if (!string.IsNullOrWhiteSpace(nextPagePath))
        {
            if (Uri.TryCreate(nextPagePath, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }

            return new Uri(_httpClient.BaseAddress!, nextPagePath);
        }

        return descending
            ? new Uri(_httpClient.BaseAddress!, "tenders?descending=1")
            : new Uri(_httpClient.BaseAddress!, "tenders");
    }

    private void EnsureBaseAddress()
    {
        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException(
                $"Prozorro API base URL is not configured. Set {ProzorroApiOptions.SectionName}:ApiUrl.");
        }
    }
}
