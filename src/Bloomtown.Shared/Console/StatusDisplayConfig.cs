using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Console;

/// <summary>
/// Shared formatting helpers for the player status command and light onboarding hints.
/// </summary>
public static class StatusDisplayConfig
{
    public const int NewcomerHintMaxGameDay = 5;
    public const int SocialFabricMinGameDay = 2;
    public const int InterpersonalStatusMinGameDay = 3;
    public const int SocialCircleMinGameDay = 4;
    public const int MaxInventoryLinesInStatus = 5;

    public static string FormatSectionHeader(string title) => $"── {title} ──";

    public static string? FormatNewcomerHint(int gameDay, int contributionScore)
    {
        if (gameDay > NewcomerHintMaxGameDay)
            return null;

        if (gameDay == 1)
            return "Tip: greet elsie · sit bench · daily · rhythm · help";

        if (gameDay == 2)
            return "Tip: chat locals · nap at home · community · status shows what's nearby";

        if (gameDay <= 4 && contributionScore < 3)
            return "Tip: help garden · gather wood · contribute to village projects";

        return "Tip: rhythm for daily agency · greet NPCs to grow social standing";
    }

    public static bool ShouldShowSocialFabric(int gameDay) => gameDay >= SocialFabricMinGameDay;

    public static bool ShouldShowInterpersonalStatus(int gameDay) => gameDay >= InterpersonalStatusMinGameDay;

    public static bool ShouldShowSocialCircle(int gameDay) => gameDay >= SocialCircleMinGameDay;

    public static string FormatCompactInventory(IReadOnlyList<ItemStack> stacks)
    {
        if (stacks.Count == 0)
            return "Inventory: (empty)";

        if (stacks.Count <= MaxInventoryLinesInStatus)
        {
            var lines = stacks
                .Select(stack => $"  - {ItemDatabase.GetDisplayName(stack.ItemType)} x{stack.Quantity}");
            return "Inventory:" + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        var shown = stacks
            .Take(MaxInventoryLinesInStatus)
            .Select(stack => $"{ItemDatabase.GetDisplayName(stack.ItemType)} x{stack.Quantity}");
        var remaining = stacks.Count - MaxInventoryLinesInStatus;
        return $"Inventory ({stacks.Count} stacks): {string.Join(", ", shown)} · +{remaining} more — type 'inventory' for full list";
    }

    public static string FormatCompactFurniture(IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        if (placedFurniture.Count == 0)
            return "Furniture: (none)";

        var parts = placedFurniture
            .OrderBy(pair => pair.Key)
            .Select(pair => $"{FurnitureTypeDisplay.GetName(pair.Key)} x{pair.Value}");
        var comfort = placedFurniture.Sum(pair => FurnitureCatalog.GetComfortValue(pair.Key) * pair.Value);
        return $"Furniture: {string.Join(", ", parts)} (+{comfort} comfort)";
    }
}