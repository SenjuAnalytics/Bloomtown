using Bloomtown.Shared.Crafting;

namespace Bloomtown.Shared.Protocol;

public readonly record struct CraftingResponse(
    bool Success,
    CraftingRecipeId RecipeId,
    byte Quantity,
    CraftingFailureReason FailureReason,
    string Message);