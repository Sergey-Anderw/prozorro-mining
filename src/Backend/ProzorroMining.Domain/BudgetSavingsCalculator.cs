namespace ProzorroMining.Domain;

/// <summary>
/// Calculates budget savings and metrics from tender data.
/// </summary>
public static class BudgetSavingsCalculator
{
    /// <summary>
    /// Calculates the savings amount as the difference between expected and actual contract amounts.
    /// </summary>
    /// <param name="expectedAmount">The initial expected budget amount.</param>
    /// <param name="contractAmount">The final contract amount.</param>
    /// <returns>The savings amount (expected - actual), or 0 if negative.</returns>
    public static decimal CalculateSavings(decimal expectedAmount, decimal contractAmount)
    {
        if (expectedAmount <= 0 || contractAmount <= 0)
            return 0;

        var savings = expectedAmount - contractAmount;
        return savings > 0 ? savings : 0;
    }

    /// <summary>
    /// Calculates the savings percentage.
    /// </summary>
    /// <param name="expectedAmount">The initial expected budget amount.</param>
    /// <param name="contractAmount">The final contract amount.</param>
    /// <returns>The savings percentage (0-100), or 0 if calculation is not possible.</returns>
    public static decimal CalculateSavingsPercentage(decimal expectedAmount, decimal contractAmount)
    {
        if (expectedAmount <= 0)
            return 0;

        var savings = CalculateSavings(expectedAmount, contractAmount);
        return (savings / expectedAmount) * 100;
    }
}
