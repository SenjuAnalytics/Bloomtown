using Bloomtown.Shared.Crafting;

namespace Bloomtown.Shared.Protocol;

public readonly record struct CraftingRequest(CraftingRecipeId RecipeId, byte Quantity);