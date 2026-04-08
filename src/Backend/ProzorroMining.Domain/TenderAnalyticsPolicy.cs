namespace ProzorroMining.Domain;

/// <summary>
/// Defines policies for analyzing tender data.
/// </summary>
public static class TenderAnalyticsPolicy
{
    /// <summary>
    /// Minimum expected budget to include in savings calculations.
    /// </summary>
    public const decimal MinimumBudgetThreshold = 10000;

    /// <summary>
    /// Number of top procurers to return in analytics.
    /// </summary>
    public const int TopProcurersCount = 10;

    /// <summary>
    /// Determines if a tender should be included in savings analytics.
    /// </summary>
    /// <param name="expectedAmount">The tender's expected budget.</param>
    /// <returns>True if the tender meets the minimum threshold, false otherwise.</returns>
    public static bool IsTenderSignificantForAnalytics(decimal expectedAmount)
    {
        return expectedAmount >= MinimumBudgetThreshold;
    }
}
