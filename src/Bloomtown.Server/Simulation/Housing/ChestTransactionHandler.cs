using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Handles personal chest view, deposit, and withdraw requests.
/// </summary>
public sealed class ChestTransactionHandler
{
    private readonly PlayerChestService _chestService;
    private readonly PlayerEconomyService _economyService;

    public ChestTransactionHandler(PlayerChestService chestService, PlayerEconomyService economyService)
    {
        _chestService = chestService;
        _economyService = economyService;
    }

    public ChestResponse Handle(uint playerEntityId, float playerX, float playerZ, ChestRequest request)
    {
        if (!_chestService.TryGetState(playerEntityId, out var chest))
        {
            return Fail(request.Kind, ChestFailureReason.ChestUnavailable, "Personal chest is unavailable.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(request.Kind, ChestFailureReason.ChestUnavailable, "Player inventory is unavailable.");
        }

        if (!PersonalChestConfig.IsWithinRange(playerX, playerZ))
        {
            var distance = PersonalChestConfig.GetDistance(playerX, playerZ);
            return Fail(
                request.Kind,
                ChestFailureReason.NotInRange,
                $"Personal chest is too far away ({distance:F1}m). Move within {PersonalChestConfig.AccessRadiusMeters:F0}m of ({PersonalChestConfig.WorldX:F0}, {PersonalChestConfig.WorldZ:F0}).");
        }

        return request.Kind switch
        {
            ChestRequestKind.View => HandleView(chest),
            ChestRequestKind.Deposit => HandleDeposit(playerEntityId, chest, economy, request),
            ChestRequestKind.Withdraw => HandleWithdraw(playerEntityId, chest, economy, request),
            _ => Fail(request.Kind, ChestFailureReason.UnknownRequest, "Unknown chest request."),
        };
    }

    private ChestResponse HandleView(PlayerChestState chest)
    {
        var message = _chestService.FormatChest(chest);
        Log.Information("Player {PlayerId} viewed personal chest.", chest.PlayerEntityId);
        return new ChestResponse(true, ChestRequestKind.View, ChestFailureReason.None, message);
    }

    private ChestResponse HandleDeposit(
        uint playerEntityId,
        PlayerChestState chest,
        PlayerEconomyState economy,
        ChestRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, ChestFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                ChestFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (!economy.Inventory.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var owned = economy.Inventory.GetItemCount(request.ItemType);
            return Fail(
                request.Kind,
                ChestFailureReason.NotEnoughItems,
                $"Not enough {itemName} in inventory. You have {owned}, tried to deposit {request.Quantity}.");
        }

        economy.Inventory.RemoveItem(request.ItemType, request.Quantity);
        chest.Storage.AddItem(request.ItemType, request.Quantity);
        PersistBothAsync(playerEntityId).GetAwaiter().GetResult();

        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        var message = $"Deposited {request.Quantity} {itemLabel} into your personal chest.";
        Log.Information(
            "Player {PlayerId} deposited {Quantity} {Item} into personal chest.",
            playerEntityId,
            request.Quantity,
            itemLabel);

        return new ChestResponse(true, request.Kind, ChestFailureReason.None, message);
    }

    private ChestResponse HandleWithdraw(
        uint playerEntityId,
        PlayerChestState chest,
        PlayerEconomyState economy,
        ChestRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, ChestFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                ChestFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (!chest.Storage.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var stored = chest.Storage.GetItemCount(request.ItemType);
            return Fail(
                request.Kind,
                ChestFailureReason.NotEnoughItems,
                $"Not enough {itemName} in chest. Stored {stored}, tried to withdraw {request.Quantity}.");
        }

        chest.Storage.RemoveItem(request.ItemType, request.Quantity);
        economy.Inventory.AddItem(request.ItemType, request.Quantity);
        PersistBothAsync(playerEntityId).GetAwaiter().GetResult();

        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        var message = $"Withdrew {request.Quantity} {itemLabel} from your personal chest.";
        Log.Information(
            "Player {PlayerId} withdrew {Quantity} {Item} from personal chest.",
            playerEntityId,
            request.Quantity,
            itemLabel);

        return new ChestResponse(true, request.Kind, ChestFailureReason.None, message);
    }

    private async Task PersistBothAsync(uint playerEntityId)
    {
        await _economyService.SavePlayerAsync(playerEntityId);
        await _chestService.SavePlayerAsync(playerEntityId);
    }

    private static ChestResponse Fail(ChestRequestKind kind, ChestFailureReason reason, string message)
    {
        Log.Information("Chest request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new ChestResponse(false, kind, reason, message);
    }
}