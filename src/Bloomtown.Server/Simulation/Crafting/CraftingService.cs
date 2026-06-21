using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Crafting;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Crafting;

/// <summary>
/// Validates crafting requests, consumes materials, and adds crafted items to player inventory.
/// </summary>
public sealed class CraftingService
{
    private readonly PlayerEconomyService _economyService;

    public CraftingService(PlayerEconomyService economyService)
    {
        _economyService = economyService;
    }

    public CraftingResponse Handle(uint playerEntityId, CraftingRequest request)
    {
        if (!CraftingRecipeDatabase.TryGet(request.RecipeId, out var recipe))
        {
            return Fail(
                request.RecipeId,
                request.Quantity,
                CraftingFailureReason.UnknownRecipe,
                "Unknown crafting recipe.");
        }

        if (request.Quantity is < 1 or > CraftingConfig.MaxCraftQuantity)
        {
            return Fail(
                request.RecipeId,
                request.Quantity,
                CraftingFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {CraftingConfig.MaxCraftQuantity}.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                request.RecipeId,
                request.Quantity,
                CraftingFailureReason.PlayerUnavailable,
                "Player inventory is unavailable.");
        }

        var scaledInputs = CraftingRecipeDatabase.GetScaledInputs(recipe, request.Quantity);
        var missingMaterial = FindFirstMissingMaterial(economy.Inventory, scaledInputs);
        if (missingMaterial is not null)
        {
            var (itemType, required, owned) = missingMaterial.Value;
            var itemName = ItemDatabase.GetDisplayName(itemType);
            var outputName = ItemDatabase.GetDisplayName(recipe.OutputItem);
            var outputCount = CraftingRecipeDatabase.GetScaledOutputQuantity(recipe, request.Quantity);
            var outputLabel = outputCount == 1 ? outputName : $"{outputCount} {outputName}s";

            Log.Information(
                "Player {PlayerId} crafting {Recipe} x{Quantity} failed: need {Required} {Item}, have {Owned}.",
                playerEntityId,
                recipe.Name,
                request.Quantity,
                required,
                itemName,
                owned);

            return Fail(
                request.RecipeId,
                request.Quantity,
                CraftingFailureReason.InsufficientMaterials,
                $"You don't have enough {itemName} to craft {outputLabel} (need {required}, have {owned}).");
        }

        foreach (var (itemType, quantity) in scaledInputs)
            economy.Inventory.RemoveItem(itemType, quantity);

        var craftedQuantity = CraftingRecipeDatabase.GetScaledOutputQuantity(recipe, request.Quantity);
        economy.Inventory.AddItem(recipe.OutputItem, craftedQuantity);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var craftedName = ItemDatabase.GetDisplayName(recipe.OutputItem);
        var craftedLabel = craftedQuantity == 1 ? craftedName : $"{craftedQuantity} {craftedName}s";
        var materialsSummary = FormatMaterialsSummary(scaledInputs);
        var message = $"Crafted {craftedLabel} using {materialsSummary}.";

        Log.Information(
            "Player {PlayerId} crafted {Recipe} x{BatchCount}: produced {OutputQuantity} {OutputItem} using {Materials}.",
            playerEntityId,
            recipe.Name,
            request.Quantity,
            craftedQuantity,
            craftedName,
            materialsSummary);

        return new CraftingResponse(
            Success: true,
            request.RecipeId,
            request.Quantity,
            CraftingFailureReason.None,
            message);
    }

    /// <summary>
    /// Returns the first input material the inventory cannot cover for this batch.
    /// </summary>
    private static (ItemType ItemType, int Required, int Owned)? FindFirstMissingMaterial(
        Inventory inventory,
        IReadOnlyDictionary<ItemType, int> scaledInputs)
    {
        foreach (var (itemType, required) in scaledInputs.OrderBy(pair => pair.Key))
        {
            var owned = inventory.GetItemCount(itemType);
            if (owned < required)
                return (itemType, required, owned);
        }

        return null;
    }

    private static string FormatMaterialsSummary(IReadOnlyDictionary<ItemType, int> scaledInputs)
    {
        return string.Join(
            ", ",
            scaledInputs
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Value} {ItemDatabase.GetDisplayName(pair.Key)}"));
    }

    private static CraftingResponse Fail(
        CraftingRecipeId recipeId,
        byte quantity,
        CraftingFailureReason reason,
        string message)
    {
        Log.Information("Crafting request failed ({Reason}): {Message}", reason, message);
        return new CraftingResponse(false, recipeId, quantity, reason, message);
    }
}