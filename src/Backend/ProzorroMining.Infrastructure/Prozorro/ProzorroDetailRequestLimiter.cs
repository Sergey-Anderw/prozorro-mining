using System.Threading.RateLimiting;

namespace ProzorroMining.Infrastructure.Prozorro;
internal sealed class ProzorroDetailRequestLimiter : IAsyncDisposable
{
    private readonly TokenBucketRateLimiter _limiter;

    public ProzorroDetailRequestLimiter(int requestsPerSecond, int queueLimit)
    {
        _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = requestsPerSecond,
            TokensPerPeriod = requestsPerSecond,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = queueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        using var lease = await _limiter.AcquireAsync(1, cancellationToken);
        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("Failed to acquire Prozorro detail request rate limiter lease.");
        }
    }

    public ValueTask DisposeAsync() => _limiter.DisposeAsync();
}
