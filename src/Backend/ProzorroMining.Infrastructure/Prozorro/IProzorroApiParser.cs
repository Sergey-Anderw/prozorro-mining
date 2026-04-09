using ProzorroMining.App.Abstractions.Prozorro;

namespace ProzorroMining.Infrastructure.Prozorro;

internal interface IProzorroApiParser
{
    ProzorroTendersPage ParseTendersPage(string rawPayload);
}
