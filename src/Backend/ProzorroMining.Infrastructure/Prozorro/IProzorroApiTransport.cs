namespace ProzorroMining.Infrastructure.Prozorro;

internal interface IProzorroApiTransport
{
    
    Task<string> GetTendersPageAsync(string? nextPagePath, bool descending, CancellationToken cancellationToken);

    Task<string> GetTenderAsync(string tenderId, CancellationToken cancellationToken);
}
