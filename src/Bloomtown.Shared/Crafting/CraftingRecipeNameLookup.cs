namespace Bloomtown.Shared.Crafting;

/// <summary>
/// Case-insensitive recipe name parsing for console commands.
/// </summary>
public static class CraftingRecipeNameLookup
{
    private static readonly Dictionary<string, CraftingRecipeId> NamesByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["plank"] = CraftingRecipeId.Plank,
        ["planks"] = CraftingRecipeId.Plank,
        ["tool"] = CraftingRecipeId.BasicTool,
        ["basic tool"] = CraftingRecipeId.BasicTool,
        ["basictool"] = CraftingRecipeId.BasicTool,
    };

    public static string KnownRecipesList => string.Join(", ", NamesByKey.Keys.OrderBy(name => name));

    public static bool TryResolve(string name, out CraftingRecipeId recipeId)
    {
        return NamesByKey.TryGetValue(name.Trim(), out recipeId);
    }
}