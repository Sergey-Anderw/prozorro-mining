namespace ProzorroMining.Infrastructure.Models;

/// <summary>
/// A dashboard overview result derived from persisted procurement data.
/// </summary>
public sealed record DashboardOverviewData(
    decimal TotalSavings,
    IReadOnlyList<DashboardProcurer> TopProcurers,
    IReadOnlyList<DashboardSupplier> TopSuppliers);

/// <summary>
/// A top procurer summary row.
/// </summary>
public sealed record DashboardProcurer(
    string Name,
    decimal BudgetAmount,
    decimal TotalSavings);

/// <summary>
/// A top supplier summary row.
/// </summary>
public sealed record DashboardSupplier(
    string Name,
    int ContractCount,
    decimal TotalValue);
