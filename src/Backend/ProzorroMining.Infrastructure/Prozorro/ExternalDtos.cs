using System.Text.Json.Serialization;

namespace ProzorroMining.Infrastructure.Prozorro;

internal sealed class ProzorroApiEnvelopeDto
{
    [JsonPropertyName("data")]
    public List<ProzorroTenderDto>? Data { get; init; }

    [JsonPropertyName("next_page")]
    public ProzorroNextPageDto? NextPage { get; init; }
}

internal sealed class ProzorroNextPageDto
{
    [JsonPropertyName("offset")]
    public string? Offset { get; init; }
}

internal sealed class ProzorroTenderDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("mainProcurementCategory")]
    public string? MainProcurementCategory { get; init; }

    [JsonPropertyName("value")]
    public ProzorroValueDto? Value { get; init; }

    [JsonPropertyName("procuringEntity")]
    public ProzorroProcuringEntityDto? ProcuringEntity { get; init; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; init; }

    [JsonPropertyName("dateCreated")]
    public DateTime? DateCreated { get; init; }

    [JsonPropertyName("dateModified")]
    public DateTime? DateModified { get; init; }

    [JsonPropertyName("items")]
    public List<ProzorroItemDto>? Items { get; init; }

    [JsonPropertyName("awards")]
    public List<ProzorroAwardDto>? Awards { get; init; }

    [JsonPropertyName("contracts")]
    public List<ProzorroContractDto>? Contracts { get; init; }
}

internal sealed class ProzorroValueDto
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }
}

internal sealed class ProzorroProcuringEntityDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

internal sealed class ProzorroItemDto
{
    [JsonPropertyName("classification")]
    public ProzorroClassificationDto? Classification { get; init; }
}

internal sealed class ProzorroClassificationDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
}

internal sealed class ProzorroAwardDto
{
    [JsonPropertyName("suppliers")]
    public List<ProzorroSupplierDto>? Suppliers { get; init; }
}

internal sealed class ProzorroSupplierDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

internal sealed class ProzorroContractDto
{
    [JsonPropertyName("value")]
    public ProzorroValueDto? Value { get; init; }
}
