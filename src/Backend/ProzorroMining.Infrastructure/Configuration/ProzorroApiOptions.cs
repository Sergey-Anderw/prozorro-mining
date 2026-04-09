using System.ComponentModel.DataAnnotations;

namespace ProzorroMining.Infrastructure.Configuration;

public sealed class ProzorroApiOptions
{
    public const string SectionName = "Prozorro";

    public string? ApiUrl { get; init; }

    public string? ApiKey { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    public RetryOptions Retry { get; init; } = new();

    public DetailRateLimitOptions DetailRateLimit { get; init; } = new();

    public sealed class RetryOptions
    {
        [Range(1, 10)]
        public int MaxAttempts { get; init; } = 3;

        [Range(1, 60)]
        public int BaseDelaySeconds { get; init; } = 2;
    }

    public sealed class DetailRateLimitOptions
    {
        [Range(1, 100)]
        public int RequestsPerSecond { get; init; } = 8;

        [Range(0, 1000)]
        public int QueueLimit { get; init; } = 32;
    }
}
