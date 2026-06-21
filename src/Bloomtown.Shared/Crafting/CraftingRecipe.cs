using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Crafting;

/// <summary>
/// Defines one crafting recipe: input materials per craft and output item quantity.
/// </summary>
public sealed class CraftingRecipe
{
    public required CraftingRecipeId Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyDictionary<ItemType, int> InputsPerCraft { get; init; }
    public required ItemType OutputItem { get; init; }
    public required int OutputQuantityPerCraft { get; init; }
}