using System.Text.Json;
using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

internal static class ProzorroMappings
{
    public static ProzorroTendersPage ToModel(
        ProzorroApiEnvelopeDto? envelope,
        string rawPayload)
    {
        var items = envelope?.Data?
            .Select(tender => tender.ToModel(rawPayload))
            .Where(tender => tender is not null)
            .Cast<ProzorroTenderData>()
            .ToList()
            ?? [];

        return new ProzorroTendersPage(envelope?.NextPage?.Offset, items);
    }

    private static ProzorroTenderData? ToModel(this ProzorroTenderDto dto, string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            return null;
        }

        return new ProzorroTenderData(
            dto.Id,
            dto.Status ?? "unknown",
            dto.Items?
                .Select(x => x.Classification?.Id)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
            dto.Value?.Amount,
            dto.ProcuringEntity?.Name,
            dto.Date,
            dto.DateCreated ?? DateTime.UtcNow,
            dto.DateModified ?? dto.DateCreated ?? DateTime.UtcNow,
            dto.Awards?
                .SelectMany(x => x.Suppliers ?? [])
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .ToList()
                ?? [],
            dto.Contracts?
                .Select(x => x.Value?.Amount)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList()
                ?? [],
            SerializeRawTender(dto, rawPayload));
    }

    private static string SerializeRawTender(ProzorroTenderDto dto, string rawPayload)
    {
        try
        {
            return JsonSerializer.Serialize(dto);
        }
        catch
        {
            return rawPayload;
        }
    }
}
