using FluentAssertions;
using ProzorroMining.App.Abstractions.Prozorro;
using ProzorroMining.App.Features.Imports;
using Xunit;

namespace ProzorroMining.UnitTests.Features.Imports;

public sealed class ImportTenderEligibilityPolicyTests
{
    private readonly ImportTenderEligibilityPolicy _sut = new();

    [Fact]
    public void ShouldFetchDetails_AllowsRecentSummaryWithoutStatusOrCpv()
    {
        var filterFloor = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        var summary = CreateTender(
            status: string.Empty,
            cpvCode: null,
            dateModified: new DateTime(2026, 03, 15, 0, 0, 0, DateTimeKind.Utc));

        var result = _sut.ShouldFetchDetails(summary, filterFloor);

        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ReturnsImportableDecision_ForMatchingTender()
    {
        var filterFloor = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        var tender = CreateTender(
            status: "complete",
            cpvCode: "09310000-5",
            dateModified: new DateTime(2026, 03, 20, 0, 0, 0, DateTimeKind.Utc));

        var decision = _sut.Evaluate(tender, filterFloor);

        decision.ShouldImport.Should().BeTrue();
        decision.SkipReason.Should().BeNull();
        decision.FilterDate.Should().Be(tender.DateModified);
    }

    [Fact]
    public void Evaluate_ReturnsCpvSkip_WhenTenderHasDifferentCpv()
    {
        var filterFloor = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
        var tender = CreateTender(
            status: "complete",
            cpvCode: "11111111-1",
            dateModified: new DateTime(2026, 03, 20, 0, 0, 0, DateTimeKind.Utc));

        var decision = _sut.Evaluate(tender, filterFloor);

        decision.ShouldImport.Should().BeFalse();
        decision.SkipReason.Should().Be("cpv");
    }

    private static ProzorroTenderData CreateTender(
        string status,
        string? cpvCode,
        DateTime dateModified,
        DateTime? dateCreated = null) =>
        new(
            ProzorroTenderId: "tender-1",
            Status: status,
            CpvCode: cpvCode,
            ExpectedAmount: 100m,
            ProcuringEntityName: "Entity",
            TenderDate: dateModified,
            DateCreated: dateCreated ?? dateModified,
            DateModified: dateModified,
            SupplierNames: Array.Empty<string>(),
            ContractAmounts: Array.Empty<decimal>(),
            RawPayload: "{}");
}
