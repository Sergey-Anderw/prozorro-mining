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
    private readonly object _stateLock = new();
    private CancellationTokenSource? _currentExecutionCancellation;
    private long? _currentImportRunId;

    public ValueTask EnqueueAsync(long importRunId, DateTime startedAt, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(new ImportExecutionRequest(importRunId, startedAt), cancellationToken);

    public ValueTask<long?> CancelCurrentAsync(CancellationToken cancellationToken)
    {
        lock (_stateLock)
        {
            if (_currentExecutionCancellation is null || _currentImportRunId is null)
            {
                return ValueTask.FromResult<long?>(null);
            }

            _currentExecutionCancellation.Cancel();
            return ValueTask.FromResult<long?>(_currentImportRunId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverInterruptedImportsAsync(stoppingToken);

        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            SetCurrentExecution(request.ImportRunId, executionCancellation);

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<RunImportOrchestrator>();
                await orchestrator.ExecuteStartedAsync(request.ImportRunId, request.StartedAt, executionCancellation.Token);
            }
            catch (OperationCanceledException) when (executionCancellation.IsCancellationRequested)
            {
                logger.LogInformation("Background import run {ImportRunId} was canceled.", request.ImportRunId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background import run {ImportRunId} terminated unexpectedly.", request.ImportRunId);
            }
            finally
            {
                ClearCurrentExecution(executionCancellation);
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

    private void SetCurrentExecution(long importRunId, CancellationTokenSource cancellation)
    {
        lock (_stateLock)
        {
            _currentImportRunId = importRunId;
            _currentExecutionCancellation = cancellation;
        }
    }

    private void ClearCurrentExecution(CancellationTokenSource cancellation)
    {
        lock (_stateLock)
        {
            if (!ReferenceEquals(_currentExecutionCancellation, cancellation))
            {
                return;
            }

            _currentImportRunId = null;
            _currentExecutionCancellation = null;
        }
    }

    private sealed record ImportExecutionRequest(long ImportRunId, DateTime StartedAt);
}
