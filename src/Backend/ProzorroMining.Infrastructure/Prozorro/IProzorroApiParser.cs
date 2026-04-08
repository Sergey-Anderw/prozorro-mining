using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

/// <summary>
/// Parses raw Prozorro API payloads into normalized application models.
/// </summary>
internal interface IProzorroApiParser
{
    ProzorroTendersPage ParseTendersPage(string rawPayload);
}
