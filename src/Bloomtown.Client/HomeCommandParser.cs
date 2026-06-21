using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses home storage commands: home, home storage, house deposit, house withdraw.
/// </summary>
public static class HomeCommandParser
{
    public static bool TryParse(string commandLine, out HomeRequest request, out string errorMessage)
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

        if (normalized is "home" or "home storage")
        {
            request = new HomeRequest(HomeRequestKind.View, 0, 0);
            return true;
        }

        if (normalized is "upgrade house" or "improve home" or "upgrade home")
        {
            request = new HomeRequest(HomeRequestKind.Upgrade, 0, 0);
            return true;
        }

        if (normalized.StartsWith("place ") || normalized.StartsWith("furnish "))
        {
            var prefix = normalized.StartsWith("place ") ? "place " : "furnish ";
            var furnitureName = normalized[prefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(furnitureName))
            {
                errorMessage = $"Usage: place <furniture>. Known furniture: {FurnitureNameLookup.KnownFurnitureList}.";
                return false;
            }

            if (!FurnitureNameLookup.TryResolve(furnitureName, out var furnitureType))
            {
                errorMessage = $"Unknown furniture '{furnitureName}'. Known furniture: {FurnitureNameLookup.KnownFurnitureList}.";
                return false;
            }

            request = new HomeRequest(HomeRequestKind.PlaceFurniture, 0, 0, furnitureType);
            return true;
        }

        if (normalized.StartsWith("home deposit ") || normalized.StartsWith("house deposit "))
        {
            var prefix = normalized.StartsWith("home deposit ") ? "home deposit " : "house deposit ";
            return TryParseTransfer(normalized[prefix.Length..], HomeRequestKind.Deposit, out request, out errorMessage);
        }

        if (normalized.StartsWith("home withdraw ") || normalized.StartsWith("house withdraw "))
        {
            var prefix = normalized.StartsWith("home withdraw ") ? "home withdraw " : "house withdraw ";
            return TryParseTransfer(normalized[prefix.Length..], HomeRequestKind.Withdraw, out request, out errorMessage);
        }

        return false;
    }

    private static bool TryParseTransfer(string remainder, HomeRequestKind kind, out HomeRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 2)
        {
            errorMessage = kind == HomeRequestKind.Deposit
                ? "Usage: house deposit <item>  or  house deposit <qty> <item>"
                : "Usage: house withdraw <item>  or  house withdraw <qty> <item>";
            return false;
        }

        byte quantity = 1;
        string itemName;

        if (parts.Length == 1)
        {
            itemName = parts[0];
        }
        else
        {
            if (!byte.TryParse(parts[0], out quantity) || quantity is < 1 or > 99)
            {
                errorMessage = "Quantity must be a number between 1 and 99.";
                return false;
            }

            itemName = parts[1];
        }

        if (!ItemNameLookup.TryResolve(itemName, out var itemType))
        {
            errorMessage = $"Unknown item '{itemName}'. Known items: {ItemNameLookup.KnownItemsList}.";
            return false;
        }

        request = new HomeRequest(kind, itemType, quantity);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Home: home storage, place bed, upgrade house, house deposit wood (must be at your home)";
    }
}