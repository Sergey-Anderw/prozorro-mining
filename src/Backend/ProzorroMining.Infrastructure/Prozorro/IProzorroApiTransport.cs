namespace ProzorroMining.Infrastructure.Prozorro;

/// <summary>
/// Performs raw HTTP transport to the external Prozorro API.
/// </summary>
internal interface IProzorroApiTransport
{
    /// <summary>
    /// Reads a raw tenders page payload using the external cursor token.
    /// </summary>
    Task<string> GetTendersPageAsync(string? offsetToken, int? pageSize, CancellationToken cancellationToken);
}
