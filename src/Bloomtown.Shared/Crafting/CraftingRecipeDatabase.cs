using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Crafting;

/// <summary>
/// Hardcoded crafting recipes for the basic crafting spike.
/// </summary>
public static class CraftingRecipeDatabase
{
    private static readonly Dictionary<CraftingRecipeId, CraftingRecipe> Recipes = new()
    {
        [CraftingRecipeId.Plank] = new CraftingRecipe
        {
            Id = CraftingRecipeId.Plank,
            Name = "Plank",
            InputsPerCraft = new Dictionary<ItemType, int> { [ItemType.Wood] = 1 },
            OutputItem = ItemType.Plank,
            OutputQuantityPerCraft = 1,
        },
        [CraftingRecipeId.BasicTool] = new CraftingRecipe
        {
            Id = CraftingRecipeId.BasicTool,
            Name = "Basic Tool",
            InputsPerCraft = new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 2,
                [ItemType.Stone] = 1,
            },
            OutputItem = ItemType.Tool,
            OutputQuantityPerCraft = 1,
        },
    };

    public static IReadOnlyCollection<CraftingRecipe> All => Recipes.Values;

    public static bool TryGet(CraftingRecipeId recipeId, out CraftingRecipe recipe)
    {
        return Recipes.TryGetValue(recipeId, out recipe!);
    }

    /// <summary>
    /// Scales per-craft inputs to the requested batch size.
    /// </summary>
    public static Dictionary<ItemType, int> GetScaledInputs(CraftingRecipe recipe, int craftCount)
    {
        var scaled = new Dictionary<ItemType, int>();
        foreach (var (itemType, perCraft) in recipe.InputsPerCraft)
            scaled[itemType] = perCraft * craftCount;

        return scaled;
    }

    public static int GetScaledOutputQuantity(CraftingRecipe recipe, int craftCount)
    {
        return recipe.OutputQuantityPerCraft * craftCount;
    }
}