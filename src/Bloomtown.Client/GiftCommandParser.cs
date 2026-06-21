using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses gift console commands: gift apple to elsie, give wood to tom.
/// </summary>
public static class GiftCommandParser
{
    public static bool TryParse(string commandLine, out GiftRequest request, out string errorMessage)
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

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts[0] is not ("gift" or "give"))
            return false;

        if (parts.Length < 4)
        {
            errorMessage = "Usage: gift <item> to <npc>  or  gift <qty> <item> to <npc>";
            return false;
        }

        var connectorIndex = Array.IndexOf(parts, "to");
        if (connectorIndex < 0)
        {
            errorMessage = "Gift command must include 'to', e.g. gift apple to elsie.";
            return false;
        }

        byte quantity = 1;
        string itemName;
        if (connectorIndex == 2)
        {
            itemName = parts[1];
        }
        else if (connectorIndex == 3)
        {
            if (!byte.TryParse(parts[1], out quantity) || quantity is < 1 or > 99)
            {
                errorMessage = "Quantity must be a number between 1 and 99.";
                return false;
            }

            itemName = parts[2];
        }
        else
        {
            errorMessage = "Usage: gift <item> to <npc>  or  gift <qty> <item> to <npc>";
            return false;
        }

        if (connectorIndex + 1 >= parts.Length)
        {
            errorMessage = "Missing NPC name after 'to'.";
            return false;
        }

        var npcName = parts[connectorIndex + 1];

        if (!ItemNameLookup.TryResolve(itemName, out var itemType))
        {
            errorMessage = $"Unknown item '{itemName}'. Known items: {ItemNameLookup.KnownItemsList}.";
            return false;
        }

        if (!NpcNameLookup.TryResolveEntityId(npcName, out var npcEntityId))
        {
            errorMessage = $"Unknown NPC '{npcName}'. Known NPCs: {NpcNameLookup.KnownNamesList}.";
            return false;
        }

        request = new GiftRequest(itemType, quantity, npcEntityId);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Gift commands: gift apple to elsie, give 3 wood to tom";
    }
}