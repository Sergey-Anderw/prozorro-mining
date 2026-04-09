using System.Threading.Channels;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.App.Features.Imports;

namespace ProzorroMining.Api;

internal sealed class ImportExecutionQueue(
    IServiceScopeFactory scopeFactory,
    ILogger<ImportExecutionQueue> logger)
    : BackgroundService, IImportExecutionQueue
{
    private readonly Channel<ImportExecutionRequest> _channel = Channel.CreateUnbounded<ImportExecutionRequest>();

    public ValueTask EnqueueAsync(long importRunId, DateTime startedAt, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(new ImportExecutionRequest(importRunId, startedAt), cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverInterruptedImportsAsync(stoppingToken);

        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<RunImportOrchestrator>();
                await orchestrator.ExecuteStartedAsync(request.ImportRunId, request.StartedAt, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background import run {ImportRunId} terminated unexpectedly.", request.ImportRunId);
            }
        }
    }

    private async Task RecoverInterruptedImportsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var importRunRepository = scope.ServiceProvider.GetRequiredService<IImportRunRepository>();
        var recoveredCount = await importRunRepository.FailRunningAsync(
            DateTime.UtcNow,
            "Import was interrupted because the application stopped before completion.",
            cancellationToken);

        if (recoveredCount > 0)
        {
            logger.LogWarning(
                "Marked {RecoveredCount} interrupted import run(s) as failed during startup recovery.",
                recoveredCount);
        }
    }

    private sealed record ImportExecutionRequest(long ImportRunId, DateTime StartedAt);
}
