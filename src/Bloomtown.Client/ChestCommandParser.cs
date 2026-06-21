using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses personal chest commands: chest, deposit, withdraw.
/// </summary>
public static class ChestCommandParser
{
    public static bool TryParse(string commandLine, out ChestRequest request, out string errorMessage)
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

        if (parts[0] == "chest")
        {
            request = new ChestRequest(ChestRequestKind.View, 0, 0);
            return true;
        }

        if (parts[0] is "deposit" or "withdraw")
        {
            return TryParseTransfer(parts, out request, out errorMessage);
        }

        return false;
    }

    private static bool TryParseTransfer(string[] parts, out ChestRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var kind = parts[0] == "deposit" ? ChestRequestKind.Deposit : ChestRequestKind.Withdraw;

        if (parts.Length < 2)
        {
            errorMessage = kind == ChestRequestKind.Deposit
                ? "Usage: deposit <item>  or  deposit <qty> <item>"
                : "Usage: withdraw <item>  or  withdraw <qty> <item>";
            return false;
        }

        byte quantity = 1;
        string itemName;

        if (parts.Length == 2)
        {
            itemName = parts[1];
        }
        else if (parts.Length == 3)
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
            errorMessage = kind == ChestRequestKind.Deposit
                ? "Usage: deposit <item>  or  deposit <qty> <item>"
                : "Usage: withdraw <item>  or  withdraw <qty> <item>";
            return false;
        }

        if (!ItemNameLookup.TryResolve(itemName, out var itemType))
        {
            errorMessage = $"Unknown item '{itemName}'. Known items: {ItemNameLookup.KnownItemsList}.";
            return false;
        }

        request = new ChestRequest(kind, itemType, quantity);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Chest: chest, deposit wood, withdraw 5 stone (near personal chest at 5,5)";
    }
}