using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.App.Abstractions.Prozorro;
using ProzorroMining.App.Features.Imports;
using ProzorroMining.Domain;
using Xunit;

namespace ProzorroMining.UnitTests.Features.Imports;

public sealed class RunImportOrchestratorTests
{
    [Fact]
    public async Task ExecuteStartedAsync_PersistsEligibleTender_AndCompletesRunSuccessfully()
    {
        var importRuns = new Mock<IImportRunRepository>();
        var checkpoints = new Mock<IImportCheckpointRepository>();
        var persistence = new Mock<IImportedTenderPersistence>();
        var apiClient = new Mock<IProzorroApiClient>();

        CompleteImportRunRecord? completionRecord = null;
        UpdateImportCheckpointRecord? checkpointRecord = null;

        importRuns
            .Setup(x => x.GetRunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportRunStatusSnapshot?)null);
        importRuns
            .Setup(x => x.CreateAsync(It.IsAny<CreateImportRunRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42L);
        importRuns
            .Setup(x => x.UpdateCompletionAsync(42L, It.IsAny<CompleteImportRunRecord>(), It.IsAny<CancellationToken>()))
            .Callback<long, CompleteImportRunRecord, CancellationToken>((_, record, _) => completionRecord = record)
            .Returns(Task.CompletedTask);
        checkpoints
            .Setup(x => x.GetAsync("prozorro", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportCheckpointSnapshot?)null);
        checkpoints
            .Setup(x => x.UpsertAsync(It.IsAny<UpdateImportCheckpointRecord>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateImportCheckpointRecord, CancellationToken>((record, _) => checkpointRecord = record)
            .Returns(Task.CompletedTask);

        apiClient
            .Setup(x => x.GetTendersPageAsync(It.IsAny<string?>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProzorroTendersPage(
                null,
                new[]
                {
                    CreateTenderSummary("tender-1", DateTime.UtcNow.AddDays(-2))
                }));
        apiClient
            .Setup(x => x.GetTenderAsync("tender-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenderDetail("tender-1", "complete", "09310000-5", DateTime.UtcNow.AddDays(-2)));

        persistence
            .Setup(x => x.PersistAsync(It.IsAny<ImportedTenderPersistenceModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportedTenderPersistenceResult(101L, true));

        var sut = CreateSut(importRuns, checkpoints, persistence, apiClient);

        var start = await sut.StartAsync(CancellationToken.None);
        var result = await sut.ExecuteStartedAsync(start.Data!.ImportRunId, start.Data.StartedAt, CancellationToken.None);

        result.Status.Should().Be(ImportRunStatus.Success.ToString());
        result.ProcessedCount.Should().Be(1);
        result.InsertedCount.Should().Be(1);
        result.UpdatedCount.Should().Be(0);
        result.FailedCount.Should().Be(0);

        persistence.Verify(x => x.PersistAsync(It.IsAny<ImportedTenderPersistenceModel>(), It.IsAny<CancellationToken>()), Times.Once);
        completionRecord.Should().NotBeNull();
        completionRecord!.Status.Should().Be(ImportRunStatus.Success.ToString());
        completionRecord.ProcessedCount.Should().Be(1);
        completionRecord.InsertedCount.Should().Be(1);
        checkpointRecord.Should().NotBeNull();
        checkpointRecord!.SourceName.Should().Be("prozorro");
        checkpointRecord.LastSeenDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteStartedAsync_DoesNotPersistIneligibleTender_AndStillCompletesRunSuccessfully()
    {
        var importRuns = new Mock<IImportRunRepository>();
        var checkpoints = new Mock<IImportCheckpointRepository>();
        var persistence = new Mock<IImportedTenderPersistence>();
        var apiClient = new Mock<IProzorroApiClient>();

        CompleteImportRunRecord? completionRecord = null;

        importRuns
            .Setup(x => x.GetRunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportRunStatusSnapshot?)null);
        importRuns
            .Setup(x => x.CreateAsync(It.IsAny<CreateImportRunRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(77L);
        importRuns
            .Setup(x => x.UpdateCompletionAsync(77L, It.IsAny<CompleteImportRunRecord>(), It.IsAny<CancellationToken>()))
            .Callback<long, CompleteImportRunRecord, CancellationToken>((_, record, _) => completionRecord = record)
            .Returns(Task.CompletedTask);
        checkpoints
            .Setup(x => x.GetAsync("prozorro", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportCheckpointSnapshot?)null);
        checkpoints
            .Setup(x => x.UpsertAsync(It.IsAny<UpdateImportCheckpointRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        apiClient
            .Setup(x => x.GetTendersPageAsync(It.IsAny<string?>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProzorroTendersPage(
                null,
                new[]
                {
                    CreateTenderSummary("tender-2", DateTime.UtcNow.AddDays(-2))
                }));
        apiClient
            .Setup(x => x.GetTenderAsync("tender-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenderDetail("tender-2", "complete", "11111111-1", DateTime.UtcNow.AddDays(-2)));

        var sut = CreateSut(importRuns, checkpoints, persistence, apiClient);

        var start = await sut.StartAsync(CancellationToken.None);
        var result = await sut.ExecuteStartedAsync(start.Data!.ImportRunId, start.Data.StartedAt, CancellationToken.None);

        result.Status.Should().Be(ImportRunStatus.Success.ToString());
        result.ProcessedCount.Should().Be(1);
        result.InsertedCount.Should().Be(0);
        result.UpdatedCount.Should().Be(0);
        result.FailedCount.Should().Be(0);

        persistence.Verify(x => x.PersistAsync(It.IsAny<ImportedTenderPersistenceModel>(), It.IsAny<CancellationToken>()), Times.Never);
        completionRecord.Should().NotBeNull();
        completionRecord!.ProcessedCount.Should().Be(1);
        completionRecord.InsertedCount.Should().Be(0);
        completionRecord.UpdatedCount.Should().Be(0);
        completionRecord.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteStartedAsync_StopsTraversal_WhenDescendingPageFallsOutsideWindow()
    {
        var importRuns = new Mock<IImportRunRepository>();
        var checkpoints = new Mock<IImportCheckpointRepository>();
        var persistence = new Mock<IImportedTenderPersistence>();
        var apiClient = new Mock<IProzorroApiClient>();

        importRuns
            .Setup(x => x.GetRunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportRunStatusSnapshot?)null);
        importRuns
            .Setup(x => x.CreateAsync(It.IsAny<CreateImportRunRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(88L);
        importRuns
            .Setup(x => x.UpdateCompletionAsync(88L, It.IsAny<CompleteImportRunRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        checkpoints
            .Setup(x => x.GetAsync("prozorro", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportCheckpointSnapshot?)null);
        checkpoints
            .Setup(x => x.UpsertAsync(It.IsAny<UpdateImportCheckpointRecord>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        apiClient
            .SetupSequence(x => x.GetTendersPageAsync(It.IsAny<string?>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProzorroTendersPage(
                "/api/2.5/tenders?descending=1&offset=next-token",
                new[]
                {
                    CreateTenderSummary("recent-tender", DateTime.UtcNow.AddDays(-3)),
                    CreateTenderSummary("old-tender", DateTime.UtcNow.AddDays(-40))
                }))
            .ReturnsAsync(new ProzorroTendersPage(
                null,
                new[]
                {
                    CreateTenderSummary("should-not-be-requested", DateTime.UtcNow.AddDays(-2))
                }));
        apiClient
            .Setup(x => x.GetTenderAsync("recent-tender", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenderDetail("recent-tender", "complete", "09310000-5", DateTime.UtcNow.AddDays(-3)));

        persistence
            .Setup(x => x.PersistAsync(It.IsAny<ImportedTenderPersistenceModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportedTenderPersistenceResult(500L, true));

        var sut = CreateSut(importRuns, checkpoints, persistence, apiClient);

        var start = await sut.StartAsync(CancellationToken.None);
        var result = await sut.ExecuteStartedAsync(start.Data!.ImportRunId, start.Data.StartedAt, CancellationToken.None);

        result.PagesTraversed.Should().Be(1);
        result.InsertedCount.Should().Be(1);
        apiClient.Verify(x => x.GetTenderAsync("recent-tender", It.IsAny<CancellationToken>()), Times.Once);
        apiClient.Verify(x => x.GetTenderAsync("old-tender", It.IsAny<CancellationToken>()), Times.Never);
        apiClient.Verify(x => x.GetTendersPageAsync("/api/2.5/tenders?descending=1&offset=next-token", true, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_ReturnsConflict_WhenAnotherImportIsAlreadyRunning()
    {
        var importRuns = new Mock<IImportRunRepository>();
        var checkpoints = new Mock<IImportCheckpointRepository>();
        var persistence = new Mock<IImportedTenderPersistence>();
        var apiClient = new Mock<IProzorroApiClient>();

        importRuns
            .Setup(x => x.GetRunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportRunStatusSnapshot(
                15L,
                ImportRunStatus.Running.ToString(),
                DateTime.UtcNow,
                null,
                10,
                2,
                1,
                0,
                null));

        var sut = CreateSut(importRuns, checkpoints, persistence, apiClient);

        var result = await sut.StartAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("CONFLICT");
        importRuns.Verify(x => x.CreateAsync(It.IsAny<CreateImportRunRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static RunImportOrchestrator CreateSut(
        Mock<IImportRunRepository> importRuns,
        Mock<IImportCheckpointRepository> checkpoints,
        Mock<IImportedTenderPersistence> persistence,
        Mock<IProzorroApiClient> apiClient) =>
        new(
            importRuns.Object,
            checkpoints.Object,
            persistence.Object,
            apiClient.Object,
            new ImportTenderEligibilityPolicy(),
            NullLogger<RunImportOrchestrator>.Instance);

    private static ProzorroTenderData CreateTenderSummary(string tenderId, DateTime modifiedAtUtc) =>
        new(
            ProzorroTenderId: tenderId,
            Status: string.Empty,
            CpvCode: null,
            ExpectedAmount: null,
            ProcuringEntityName: null,
            TenderDate: modifiedAtUtc,
            DateCreated: modifiedAtUtc,
            DateModified: modifiedAtUtc,
            SupplierNames: Array.Empty<string>(),
            ContractAmounts: Array.Empty<decimal>(),
            RawPayload: "{}");

    private static ProzorroTenderData CreateTenderDetail(
        string tenderId,
        string status,
        string cpvCode,
        DateTime modifiedAtUtc) =>
        new(
            ProzorroTenderId: tenderId,
            Status: status,
            CpvCode: cpvCode,
            ExpectedAmount: 100m,
            ProcuringEntityName: "Entity",
            TenderDate: modifiedAtUtc,
            DateCreated: modifiedAtUtc.AddDays(-1),
            DateModified: modifiedAtUtc,
            SupplierNames: new[] { "Supplier 1" },
            ContractAmounts: new[] { 90m },
            RawPayload: "{}");
}
