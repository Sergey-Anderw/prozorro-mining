using System.Threading.Channels;
using ProzorroMining.App.Features.Imports;

namespace ProzorroMining.Api;

internal sealed class ImportExecutionQueue : BackgroundService, IImportExecutionQueue
{
    private readonly Channel<ImportExecutionRequest> _channel = Channel.CreateUnbounded<ImportExecutionRequest>();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportExecutionQueue> _logger;

    public ImportExecutionQueue(
        IServiceScopeFactory scopeFactory,
        ILogger<ImportExecutionQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public ValueTask EnqueueAsync(long importRunId, DateTime startedAt, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(new ImportExecutionRequest(importRunId, startedAt), cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<RunImportOrchestrator>();
                await orchestrator.ExecuteStartedAsync(request.ImportRunId, request.StartedAt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background import run {ImportRunId} terminated unexpectedly.", request.ImportRunId);
            }
        }
    }

    private sealed record ImportExecutionRequest(long ImportRunId, DateTime StartedAt);
}
