namespace ProzorroMining.Domain;

/// <summary>
/// Defines policies for aggregating supplier data.
/// </summary>
public static class SupplierAggregationPolicy
{
    /// <summary>
    /// Minimum contract count to include a supplier in top rankings.
    /// </summary>
    public const int MinimumContractCountForRanking = 3;

    /// <summary>
    /// Number of top suppliers to return in analytics.
    /// </summary>
    public const int TopSuppliersCount = 10;

    /// <summary>
    /// Determines if a supplier should be included in analytics based on contract count.
    /// </summary>
    /// <param name="contractCount">The number of contracts for the supplier.</param>
    /// <returns>True if the supplier meets the minimum threshold, false otherwise.</returns>
    public static bool IsSupplierSignificant(int contractCount)
    {
        return contractCount >= MinimumContractCountForRanking;
    }
}
