using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses economy console commands: inventory, buy, sell.
/// </summary>
public static class EconomyCommandParser
{
    public readonly record struct ParsedCommand(
        EconomyRequestKind Kind,
        ItemType ItemType,
        byte Quantity,
        uint NpcEntityId);

    public static bool TryParse(string commandLine, out ParsedCommand command, out string errorMessage)
    {
        command = default;
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

        if (parts[0] == "inventory")
        {
            command = new ParsedCommand(EconomyRequestKind.Inventory, 0, 0, 0);
            return true;
        }

        if (parts[0] == "buy")
            return TryParseTrade(parts, isBuy: true, out command, out errorMessage);

        if (parts[0] == "sell")
            return TryParseTrade(parts, isBuy: false, out command, out errorMessage);

        return false;
    }

    private static bool TryParseTrade(
        string[] parts,
        bool isBuy,
        out ParsedCommand command,
        out string errorMessage)
    {
        command = default;
        errorMessage = string.Empty;

        var connector = isBuy ? "from" : "to";
        var kind = isBuy ? EconomyRequestKind.Buy : EconomyRequestKind.Sell;

        if (parts.Length < 4)
        {
            errorMessage = isBuy
                ? "Usage: buy <item> from <npc>  or  buy <qty> <item> from <npc>"
                : "Usage: sell <item> to <npc>  or  sell <qty> <item> to <npc>";
            return false;
        }

        byte quantity = 1;
        string itemName;
        string npcName;
        var connectorIndex = Array.IndexOf(parts, connector);

        if (connectorIndex < 0)
        {
            errorMessage = isBuy
                ? "Buy command must include 'from', e.g. buy wood from elsie."
                : "Sell command must include 'to', e.g. sell stone to tom.";
            return false;
        }

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
            errorMessage = isBuy
                ? "Usage: buy <item> from <npc>  or  buy <qty> <item> from <npc>"
                : "Usage: sell <item> to <npc>  or  sell <qty> <item> to <npc>";
            return false;
        }

        if (connectorIndex + 1 >= parts.Length)
        {
            errorMessage = $"Missing NPC name after '{connector}'.";
            return false;
        }

        npcName = parts[connectorIndex + 1];

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

        command = new ParsedCommand(kind, itemType, quantity, npcEntityId);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Economy commands: inventory, buy wood from elsie, sell stone to tom, buy apple from mira, sell wood to mira";
    }
}