using System.ComponentModel.DataAnnotations;

namespace ProzorroMining.Infrastructure.Configuration;

/// <summary>
/// External Prozorro API settings.
/// </summary>
public sealed class ProzorroApiOptions
{
    public const string SectionName = "Prozorro";

    public string? ApiUrl { get; init; }

    public string? ApiKey { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    [Range(1, 1000)]
    public int DefaultPageSize { get; init; } = 100;

    public RetryOptions Retry { get; init; } = new();

    public sealed class RetryOptions
    {
        [Range(1, 10)]
        public int MaxAttempts { get; init; } = 3;

        [Range(1, 60)]
        public int BaseDelaySeconds { get; init; } = 2;
    }
}
