namespace ProzorroMining.Infrastructure.Models;

/// <summary>
/// Lightweight summary of the latest import run status.
/// </summary>
public sealed record ImportRunSummary(
    string Status,
    DateTime StartedAt,
    int ProcessedCount,
    int FailedCount);
