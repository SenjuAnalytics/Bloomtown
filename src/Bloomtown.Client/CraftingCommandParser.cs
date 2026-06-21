using Bloomtown.Shared.Crafting;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses crafting console commands: craft plank 10, craft tool.
/// </summary>
public static class CraftingCommandParser
{
    public readonly record struct ParsedCommand(CraftingRecipeId RecipeId, byte Quantity);

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
        if (parts[0] != "craft")
            return false;

        if (parts.Length < 2)
        {
            errorMessage = $"Usage: craft <recipe> [quantity]. Known recipes: {CraftingRecipeNameLookup.KnownRecipesList}.";
            return false;
        }

        if (!CraftingRecipeNameLookup.TryResolve(parts[1], out var recipeId))
        {
            errorMessage = $"Unknown recipe '{parts[1]}'. Known recipes: {CraftingRecipeNameLookup.KnownRecipesList}.";
            return false;
        }

        byte quantity = 1;
        if (parts.Length >= 3)
        {
            if (!byte.TryParse(parts[2], out quantity) || quantity is < 1 or > CraftingConfig.MaxCraftQuantity)
            {
                errorMessage = $"Quantity must be a number between 1 and {CraftingConfig.MaxCraftQuantity}.";
                return false;
            }
        }

        command = new ParsedCommand(recipeId, quantity);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Crafting commands: craft plank 10, craft tool";
    }
}