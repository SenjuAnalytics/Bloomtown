using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Client;

/// <summary>
/// Parses village area commands: areas, browse market, chat locals, relax garden, tend plants, stroll river, reflect river.
/// </summary>
public static class VillageAreaCommandParser
{
    public static bool TryParse(string commandLine, out VillageAreaRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var normalized = commandLine.Trim().ToLowerInvariant();
        if (normalized.StartsWith('/'))
            normalized = normalized[1..];

        if (string.IsNullOrWhiteSpace(normalized))
        {
            errorMessage = "Empty command.";
            return false;
        }

        if (normalized is "areas" or "area" or "village areas")
        {
            request = new VillageAreaRequest(VillageAreaRequestKind.List, VillageAreaInteractionKind.None);
            return true;
        }

        var interaction = normalized switch
        {
            "browse market" or "visit market" => VillageAreaInteractionKind.BrowseMarket,
            "chat locals" or "chat local" => VillageAreaInteractionKind.ChatLocals,
            "relax garden" or "garden relax" => VillageAreaInteractionKind.RelaxGarden,
            "tend plants" or "tend garden" => VillageAreaInteractionKind.TendPlants,
            "stroll river" or "walk river" => VillageAreaInteractionKind.StrollRiver,
            "reflect river" or "river reflect" => VillageAreaInteractionKind.ReflectRiver,
            _ => VillageAreaInteractionKind.None,
        };

        if (interaction == VillageAreaInteractionKind.None)
            return false;

        request = new VillageAreaRequest(VillageAreaRequestKind.Interact, interaction);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Village areas: areas | Market (18,6): browse market, chat locals | Garden (20,14): relax garden, tend plants | River (14,18): stroll river, reflect river";
    }
}