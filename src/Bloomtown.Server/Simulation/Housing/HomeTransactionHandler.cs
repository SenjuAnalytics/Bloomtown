using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Handles home storage view, deposit, and withdraw requests (must be at home).
/// </summary>
public sealed class HomeTransactionHandler
{
    private readonly PlayerHousingService _housingService;
    private readonly PlayerEconomyService _economyService;
    private readonly HomeActivityService _activityService;
    private readonly PlayerMilestoneService? _milestoneService;

    public HomeTransactionHandler(
        PlayerHousingService housingService,
        PlayerEconomyService economyService,
        HomeActivityService activityService,
        PlayerMilestoneService? milestoneService = null)
    {
        _housingService = housingService;
        _economyService = economyService;
        _activityService = activityService;
        _milestoneService = milestoneService;
    }

    public HomeResponse Handle(uint playerEntityId, float playerX, float playerZ, HomeRequest request)
    {
        if (!_housingService.TryGetState(playerEntityId, out var house))
        {
            return Fail(request.Kind, HomeFailureReason.HomeUnavailable, "Home storage is unavailable.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(request.Kind, HomeFailureReason.HomeUnavailable, "Player inventory is unavailable.");
        }

        if (request.Kind == HomeRequestKind.Activity)
        {
            return _activityService.TryPerformActivity(
                playerEntityId,
                playerX,
                playerZ,
                request.ActivityType);
        }

        if (!PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ))
        {
            var distance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            return Fail(
                request.Kind,
                HomeFailureReason.NotAtHome,
                $"You must be at your home ({house.HouseX:F0}, {house.HouseZ:F0}) to use home storage ({distance:F1}m away). " +
                $"Move within {PlayerHousingConfig.AccessRadiusMeters:F0}m.");
        }

        return request.Kind switch
        {
            HomeRequestKind.View => HandleView(house),
            HomeRequestKind.Deposit => HandleDeposit(playerEntityId, house, economy, request),
            HomeRequestKind.Withdraw => HandleWithdraw(playerEntityId, house, economy, request),
            HomeRequestKind.Upgrade => _housingService.TryUpgradeHouse(playerEntityId, playerX, playerZ),
            HomeRequestKind.PlaceFurniture => HandlePlaceFurniture(playerEntityId, playerX, playerZ, request.FurnitureType),
            _ => Fail(request.Kind, HomeFailureReason.UnknownRequest, "Unknown home request."),
        };
    }

    private HomeResponse HandlePlaceFurniture(
        uint playerEntityId,
        float playerX,
        float playerZ,
        FurnitureType furnitureType)
    {
        var response = _housingService.TryPlaceFurniture(playerEntityId, playerX, playerZ, furnitureType);
        if (!response.Success || _milestoneService is null)
            return response;

        _milestoneService.ReconcileAsync(playerEntityId).GetAwaiter().GetResult();
        if (_milestoneService.TryConsumePendingFeedback(playerEntityId, out var feedback)
            && !string.IsNullOrWhiteSpace(feedback))
        {
            return response with
            {
                Message = $"{response.Message}{Environment.NewLine}{feedback}",
            };
        }

        return response;
    }

    private HomeResponse HandleView(PlayerHouseState house)
    {
        var message = _housingService.FormatHomeStorage(house);
        Log.Information("Player {PlayerId} viewed home storage at ({HouseX:F0}, {HouseZ:F0}).", house.PlayerEntityId, house.HouseX, house.HouseZ);
        return new HomeResponse(true, HomeRequestKind.View, HomeFailureReason.None, message);
    }

    private HomeResponse HandleDeposit(
        uint playerEntityId,
        PlayerHouseState house,
        PlayerEconomyState economy,
        HomeRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, HomeFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                HomeFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (!economy.Inventory.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var owned = economy.Inventory.GetItemCount(request.ItemType);
            return Fail(
                request.Kind,
                HomeFailureReason.NotEnoughItems,
                $"Not enough {itemName} in inventory. You have {owned}, tried to deposit {request.Quantity}.");
        }

        economy.Inventory.RemoveItem(request.ItemType, request.Quantity);
        house.HomeStorage.AddItem(request.ItemType, request.Quantity);
        PersistBothAsync(playerEntityId).GetAwaiter().GetResult();

        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        Log.Information(
            "Player {PlayerId} deposited {Quantity} {Item} into home storage at ({HouseX:F0}, {HouseZ:F0}).",
            playerEntityId,
            request.Quantity,
            itemLabel,
            house.HouseX,
            house.HouseZ);

        return new HomeResponse(
            true,
            request.Kind,
            HomeFailureReason.None,
            $"Deposited {request.Quantity} {itemLabel} into your home storage.");
    }

    private HomeResponse HandleWithdraw(
        uint playerEntityId,
        PlayerHouseState house,
        PlayerEconomyState economy,
        HomeRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, HomeFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                HomeFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (!house.HomeStorage.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var stored = house.HomeStorage.GetItemCount(request.ItemType);
            return Fail(
                request.Kind,
                HomeFailureReason.NotEnoughItems,
                $"Not enough {itemName} in home storage. Stored {stored}, tried to withdraw {request.Quantity}.");
        }

        house.HomeStorage.RemoveItem(request.ItemType, request.Quantity);
        economy.Inventory.AddItem(request.ItemType, request.Quantity);
        PersistBothAsync(playerEntityId).GetAwaiter().GetResult();

        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        Log.Information(
            "Player {PlayerId} withdrew {Quantity} {Item} from home storage at ({HouseX:F0}, {HouseZ:F0}).",
            playerEntityId,
            request.Quantity,
            itemLabel,
            house.HouseX,
            house.HouseZ);

        return new HomeResponse(
            true,
            request.Kind,
            HomeFailureReason.None,
            $"Withdrew {request.Quantity} {itemLabel} from your home storage.");
    }

    private async Task PersistBothAsync(uint playerEntityId)
    {
        await _economyService.SavePlayerAsync(playerEntityId);
        await _housingService.SavePlayerAsync(playerEntityId);
    }

    private static HomeResponse Fail(HomeRequestKind kind, HomeFailureReason reason, string message)
    {
        Log.Information("Home request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new HomeResponse(false, kind, reason, message);
    }
}