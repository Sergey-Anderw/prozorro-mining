namespace ProzorroMining.Domain;

/// <summary>
/// Represents the status of an import run.
/// </summary>
public enum ImportRunStatus
{
    /// <summary>
    /// Import run is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Import is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Import completed successfully.
    /// </summary>
    Success = 2,

    /// <summary>
    /// Import failed with errors.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Import was cancelled.
    /// </summary>
    Cancelled = 4
}
