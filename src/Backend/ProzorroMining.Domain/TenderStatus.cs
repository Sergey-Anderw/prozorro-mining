namespace ProzorroMining.Domain;

/// <summary>
/// Represents the status of a tender in the Prozorro system.
/// </summary>
public enum TenderStatus
{
    /// <summary>
    /// Tender is being prepared.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Tender is active and accepting bids.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tender evaluation is in progress.
    /// </summary>
    Evaluation = 2,

    /// <summary>
    /// Tender has been awarded.
    /// </summary>
    Awarded = 3,

    /// <summary>
    /// Tender is completed.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Tender has been cancelled.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Tender status is unknown.
    /// </summary>
    Unknown = 99
}
