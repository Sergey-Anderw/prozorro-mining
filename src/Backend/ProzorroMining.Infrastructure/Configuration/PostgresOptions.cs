using System.ComponentModel.DataAnnotations;

namespace ProzorroMining.Infrastructure.Configuration;

/// <summary>
/// PostgreSQL settings used by the infrastructure layer.
/// </summary>
public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    [Range(1, 120)]
    public int ConnectionTimeoutSeconds { get; init; } = 15;

    [Range(1, 600)]
    public int CommandTimeoutSeconds { get; init; } = 30;

    [Range(0, 1024)]
    public int MinimumPoolSize { get; init; } = 0;

    [Range(1, 4096)]
    public int MaximumPoolSize { get; init; } = 100;

    [Range(0, 3600)]
    public int KeepAliveSeconds { get; init; } = 30;
}
