using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.Infrastructure.Db;
using ProzorroMining.Infrastructure.Repositories;

namespace ProzorroMining.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.Name)]
public sealed class ImportedTenderPersistenceTests
{
    private readonly PostgresContainerFixture _postgres;

    public ImportedTenderPersistenceTests(PostgresContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task PersistAsync_UpsertsTenderAndRefreshesChildCollections()
    {
        await _postgres.ResetDatabaseAsync();
        var sut = CreateSut();

        var initialResult = await sut.PersistAsync(CreateTender(
            procuringEntityName: "Entity A",
            supplierNames: ["Supplier 1", "Supplier 2"],
            contractAmounts: [100m, 50m],
            expectedAmount: 180m), CancellationToken.None);

        var updateResult = await sut.PersistAsync(CreateTender(
            procuringEntityName: "Entity B",
            supplierNames: ["Supplier 2", "Supplier 3", "Supplier 3"],
            contractAmounts: [75m],
            expectedAmount: 200m), CancellationToken.None);

        initialResult.Inserted.Should().BeTrue();
        updateResult.Inserted.Should().BeFalse();
        updateResult.TenderId.Should().Be(initialResult.TenderId);

        var tender = await DatabaseTestHelper.QuerySingleAsync<TenderRow>(
            _postgres.ConnectionString,
            """
            SELECT
                procuring_entity_name AS ProcuringEntityName,
                expected_amount AS ExpectedAmount
            FROM tenders
            WHERE prozorro_tender_id = 'tender-1';
            """);

        tender.ProcuringEntityName.Should().Be("Entity B");
        tender.ExpectedAmount.Should().Be(200m);

        var contracts = await DatabaseTestHelper.QueryAsync<decimal>(
            _postgres.ConnectionString,
            """
            SELECT contract_amount
            FROM contracts
            WHERE tender_id = @TenderId
            ORDER BY contract_amount;
            """,
            new { TenderId = initialResult.TenderId });

        contracts.Should().Equal(75m);

        var suppliers = await DatabaseTestHelper.QueryAsync<string>(
            _postgres.ConnectionString,
            """
            SELECT s.name
            FROM tender_suppliers ts
            INNER JOIN suppliers s ON s.id = ts.supplier_id
            WHERE ts.tender_id = @TenderId
            ORDER BY s.name;
            """,
            new { TenderId = initialResult.TenderId });

        suppliers.Should().Equal("Supplier 2", "Supplier 3");
    }

    private ImportedTenderPersistence CreateSut()
    {
        var connectionFactory = new NpgsqlConnectionFactory(new Npgsql.NpgsqlDataSourceBuilder(_postgres.ConnectionString).Build());
        var commandSettings = new PostgresCommandSettings(30);
        return new ImportedTenderPersistence(connectionFactory, commandSettings, NullLogger<ImportedTenderPersistence>.Instance);
    }

    private static ImportedTenderPersistenceModel CreateTender(
        string procuringEntityName,
        IReadOnlyList<string> supplierNames,
        IReadOnlyList<decimal> contractAmounts,
        decimal expectedAmount) =>
        new(
            new TenderPersistenceModel(
                ProzorroTenderId: "tender-1",
                Status: "complete",
                CpvCode: "09310000-5",
                ExpectedAmount: expectedAmount,
                ProcuringEntityName: procuringEntityName,
                TenderDate: new DateTime(2026, 04, 10, 0, 0, 0, DateTimeKind.Utc),
                DateCreated: new DateTime(2026, 04, 09, 0, 0, 0, DateTimeKind.Utc),
                DateModified: new DateTime(2026, 04, 10, 0, 0, 0, DateTimeKind.Utc),
                RawPayload: "{}"),
            supplierNames,
            contractAmounts);

    private sealed class TenderRow
    {
        public required string ProcuringEntityName { get; init; }
        public decimal ExpectedAmount { get; init; }
    }
}
