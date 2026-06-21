using Bloomtown.Shared.Crafting;
using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Tests;

public sealed class CraftingRecipeDatabaseTests
{
    [Fact]
    public void PlankRecipe_ScalesInputsAndOutput()
    {
        Assert.True(CraftingRecipeDatabase.TryGet(CraftingRecipeId.Plank, out var recipe));

        var inputs = CraftingRecipeDatabase.GetScaledInputs(recipe, 10);

        Assert.Equal(10, inputs[ItemType.Wood]);
        Assert.Equal(10, CraftingRecipeDatabase.GetScaledOutputQuantity(recipe, 10));
        Assert.Equal(ItemType.Plank, recipe.OutputItem);
    }

    [Fact]
    public void BasicToolRecipe_RequiresWoodAndStone()
    {
        Assert.True(CraftingRecipeDatabase.TryGet(CraftingRecipeId.BasicTool, out var recipe));

        var inputs = CraftingRecipeDatabase.GetScaledInputs(recipe, 2);

        Assert.Equal(4, inputs[ItemType.Wood]);
        Assert.Equal(2, inputs[ItemType.Stone]);
        Assert.Equal(2, CraftingRecipeDatabase.GetScaledOutputQuantity(recipe, 2));
        Assert.Equal(ItemType.Tool, recipe.OutputItem);
    }

    [Theory]
    [InlineData("plank", CraftingRecipeId.Plank)]
    [InlineData("tool", CraftingRecipeId.BasicTool)]
    [InlineData("basic tool", CraftingRecipeId.BasicTool)]
    public void RecipeNameLookup_ResolvesKnownNames(string name, CraftingRecipeId expected)
    {
        Assert.True(CraftingRecipeNameLookup.TryResolve(name, out var recipeId));
        Assert.Equal(expected, recipeId);
    }
}