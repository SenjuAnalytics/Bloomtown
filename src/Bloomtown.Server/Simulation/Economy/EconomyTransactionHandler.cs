using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Handles inventory queries and NPC shop buy/sell transactions.
/// </summary>
public sealed class EconomyTransactionHandler
{
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerEconomyService _economyService;
    private readonly NpcMemoryService? _memoryService;
    private readonly WorldTimeSystem? _worldTime;

    public EconomyTransactionHandler(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        PlayerNpcRelationshipService relationshipService,
        PlayerEconomyService economyService,
        NpcMemoryService? memoryService = null,
        WorldTimeSystem? worldTime = null)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _relationshipService = relationshipService;
        _economyService = economyService;
        _memoryService = memoryService;
        _worldTime = worldTime;
    }

    public EconomyResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EconomyRequest request)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(request.Kind, 0, EconomyFailureReason.UnknownRequest, "Player economy state is unavailable.");
        }

        return request.Kind switch
        {
            EconomyRequestKind.Inventory => HandleInventory(economy),
            EconomyRequestKind.Buy => HandleBuy(playerEntityId, playerX, playerZ, economy, request),
            EconomyRequestKind.Sell => HandleSell(playerEntityId, playerX, playerZ, economy, request),
            _ => Fail(request.Kind, request.NpcEntityId, EconomyFailureReason.UnknownRequest, "Unknown economy request."),
        };
    }

    private EconomyResponse HandleInventory(PlayerEconomyState economy)
    {
        var message = _economyService.FormatInventory(economy);
        Log.Information("Player {PlayerId} viewed inventory.", economy.PlayerEntityId);

        return new EconomyResponse(
            Success: true,
            EconomyRequestKind.Inventory,
            0,
            EconomyFailureReason.None,
            message);
    }

    private EconomyResponse HandleBuy(
        uint playerEntityId,
        float playerX,
        float playerZ,
        PlayerEconomyState economy,
        EconomyRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, request.NpcEntityId, EconomyFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (TryValidateShopAccess(playerEntityId, playerX, playerZ, request.Kind, request.NpcEntityId, out var accessFailure))
            return accessFailure;

        if (!NpcShopCatalog.NpcSellsItem(request.NpcEntityId, request.ItemType))
        {
            var npcName = NpcNameLookup.GetDisplayNameOrDefault(request.NpcEntityId);
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.ItemNotSoldByNpc,
                $"{npcName} does not sell {itemName}.");
        }

        var affinity = _relationshipService.GetAffinity(playerEntityId, request.NpcEntityId);
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var unitPrice = ShopPriceCalculator.GetBuyPrice(
            ItemDatabase.GetBaseBuyPrice(request.ItemType),
            affinity,
            economy.VillageTitle,
            request.NpcEntityId,
            standingTier);
        var tradePrivilegeApplied = TryApplyMiraTradePrivilegeBuy(playerEntityId, request.NpcEntityId, ref unitPrice);
        var marketDayApplied = TryApplyMarketDayBuy(request.NpcEntityId, ref unitPrice);
        var totalCost = ShopPriceCalculator.GetTotalBuyCost(unitPrice, request.Quantity);

        if (economy.Coins < totalCost)
        {
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.NotEnoughCoins,
                $"Not enough coins. Need {totalCost}, you have {economy.Coins}.");
        }

        economy.Coins -= totalCost;
        economy.Inventory.AddItem(request.ItemType, request.Quantity);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(request.NpcEntityId);
        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        var titleLabel = VillageTitleDisplay.GetName(economy.VillageTitle);
        var message =
            $"Bought {request.Quantity} {itemLabel} from {npcLabel} for {totalCost} coins ({unitPrice}/each, affinity={affinity}, title={titleLabel}). Coins remaining: {economy.Coins}.";

        if (request.NpcEntityId == NpcEntityIds.Mira
            && VillageSocialStandingMechanicalConfig.IsEligibleForMiraTradeBonus(standingTier))
        {
            message += $" {VillageSocialStandingMechanicalConfig.FormatMiraBuyTradeFeedback(standingTier)}";
        }

        if (tradePrivilegeApplied)
        {
            var remainingUses = _memoryService?.GetMiraSocialInfluenceTradePrivilegeRemainingUses(playerEntityId) ?? 0;
            message += $" {SocialInfluenceActionConfig.FormatTradePrivilegeAppliedFeedback(buyPrivilege: true, remainingUses)}";
        }

        if (marketDayApplied)
            message += $" {VillageEventConfig.FormatMarketDayTradeFeedback(isBuy: true)}";

        Log.Information(
            "Player {PlayerId} bought {Quantity} {Item} from {NpcName} for {Cost} coins (unit {UnitPrice}, affinity {Affinity}).",
            playerEntityId,
            request.Quantity,
            itemLabel,
            npcLabel,
            totalCost,
            unitPrice,
            affinity);

        return new EconomyResponse(true, request.Kind, request.NpcEntityId, EconomyFailureReason.None, message);
    }

    private EconomyResponse HandleSell(
        uint playerEntityId,
        float playerX,
        float playerZ,
        PlayerEconomyState economy,
        EconomyRequest request)
    {
        if (request.ItemType == 0)
            return Fail(request.Kind, request.NpcEntityId, EconomyFailureReason.UnknownItem, "Unknown item type.");

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (TryValidateShopAccess(playerEntityId, playerX, playerZ, request.Kind, request.NpcEntityId, out var accessFailure))
            return accessFailure;

        if (!NpcShopCatalog.NpcBuysItem(request.NpcEntityId, request.ItemType))
        {
            var npcName = NpcNameLookup.GetDisplayNameOrDefault(request.NpcEntityId);
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.ItemNotBoughtByNpc,
                $"{npcName} does not buy {itemName}.");
        }

        if (!economy.Inventory.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var owned = economy.Inventory.GetItemCount(request.ItemType);
            return Fail(
                request.Kind,
                request.NpcEntityId,
                EconomyFailureReason.NotEnoughItems,
                $"Not enough {itemName}. You have {owned}, tried to sell {request.Quantity}.");
        }

        var affinity = _relationshipService.GetAffinity(playerEntityId, request.NpcEntityId);
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var unitPrice = ShopPriceCalculator.GetSellPrice(
            ItemDatabase.GetBaseSellPrice(request.ItemType),
            affinity,
            request.NpcEntityId,
            standingTier);
        var tradePrivilegeApplied = TryApplyMiraTradePrivilegeSell(playerEntityId, request.NpcEntityId, ref unitPrice);
        var marketDayApplied = TryApplyMarketDaySell(request.NpcEntityId, ref unitPrice);
        var totalPayout = ShopPriceCalculator.GetTotalSellPayout(unitPrice, request.Quantity);

        economy.Inventory.RemoveItem(request.ItemType, request.Quantity);
        economy.Coins += totalPayout;
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(request.NpcEntityId);
        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        var message =
            $"Sold {request.Quantity} {itemLabel} to {npcLabel} for {totalPayout} coins ({unitPrice}/each, affinity={affinity}). Coins: {economy.Coins}.";

        if (request.NpcEntityId == NpcEntityIds.Mira
            && VillageSocialStandingMechanicalConfig.IsEligibleForMiraTradeBonus(standingTier))
        {
            message += $" {VillageSocialStandingMechanicalConfig.FormatMiraSellTradeFeedback(standingTier)}";
        }

        if (tradePrivilegeApplied)
        {
            var remainingUses = _memoryService?.GetMiraSocialInfluenceTradePrivilegeRemainingUses(playerEntityId) ?? 0;
            message += $" {SocialInfluenceActionConfig.FormatTradePrivilegeAppliedFeedback(buyPrivilege: false, remainingUses)}";
        }

        if (marketDayApplied)
            message += $" {VillageEventConfig.FormatMarketDayTradeFeedback(isBuy: false)}";

        Log.Information(
            "Player {PlayerId} sold {Quantity} {Item} to {NpcName} for {Payout} coins (unit {UnitPrice}, affinity {Affinity}).",
            playerEntityId,
            request.Quantity,
            itemLabel,
            npcLabel,
            totalPayout,
            unitPrice,
            affinity);

        return new EconomyResponse(true, request.Kind, request.NpcEntityId, EconomyFailureReason.None, message);
    }

    private bool TryValidateShopAccess(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EconomyRequestKind kind,
        uint npcEntityId,
        out EconomyResponse failure)
    {
        if (npcEntityId == 0)
        {
            failure = Fail(
                kind,
                0,
                EconomyFailureReason.UnknownNpc,
                $"Unknown NPC. Known NPCs: {NpcNameLookup.KnownNamesList}.");
            return true;
        }

        if (!NpcShopCatalog.TryGetShop(npcEntityId, out _))
        {
            failure = Fail(
                kind,
                npcEntityId,
                EconomyFailureReason.UnknownNpc,
                $"{NpcNameLookup.GetDisplayNameOrDefault(npcEntityId)} does not run a shop.");
            return true;
        }

        var simulation = _npcManager.GetState(npcEntityId);
        if (simulation is null)
        {
            failure = Fail(
                kind,
                npcEntityId,
                EconomyFailureReason.UnknownNpc,
                $"NPC '{NpcNameLookup.GetDisplayNameOrDefault(npcEntityId)}' was not found.");
            return true;
        }

        var npcName = simulation.Npc.Name;
        var distance = NpcProximityDetector.GetDistance(
            playerX,
            playerZ,
            simulation.Npc.PositionX,
            simulation.Npc.PositionZ);

        if (!NpcProximityDetector.IsWithinRange(
                playerX,
                playerZ,
                simulation,
                InteractionConfig.InteractionRadiusMeters))
        {
            failure = Fail(
                kind,
                npcEntityId,
                EconomyFailureReason.NotInRange,
                $"{npcName} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m to trade.");
            return true;
        }

        if (!_aoiSystem.IsEntityVisibleToPlayer(npcEntityId, playerEntityId))
        {
            failure = Fail(
                kind,
                npcEntityId,
                EconomyFailureReason.NotInAoi,
                $"{npcName} is not in your area. Walk closer until they appear in your AOI.");
            return true;
        }

        failure = default;
        return false;
    }

    private bool TryApplyMiraTradePrivilegeBuy(uint playerEntityId, uint npcEntityId, ref int unitPrice)
    {
        if (npcEntityId != NpcEntityIds.Mira
            || _memoryService is null
            || !_memoryService.TryGetMiraSocialInfluenceTradePrivilegeIsBuy(playerEntityId, out var buyPrivilege)
            || !buyPrivilege
            || !_memoryService.TryConsumeMiraSocialInfluenceTradePrivilege(playerEntityId, out _))
        {
            return false;
        }

        unitPrice = Math.Max(
            1,
            (int)MathF.Round(
                unitPrice * (1f - SocialInfluenceActionConfig.MiraTradePrivilegeBuyDiscountPercent / 100f)));
        return true;
    }

    private bool TryApplyMiraTradePrivilegeSell(uint playerEntityId, uint npcEntityId, ref int unitPrice)
    {
        if (npcEntityId != NpcEntityIds.Mira
            || _memoryService is null
            || !_memoryService.TryGetMiraSocialInfluenceTradePrivilegeIsBuy(playerEntityId, out var buyPrivilege)
            || buyPrivilege
            || !_memoryService.TryConsumeMiraSocialInfluenceTradePrivilege(playerEntityId, out _))
        {
            return false;
        }

        unitPrice = Math.Max(
            1,
            (int)MathF.Round(
                unitPrice * (1f + SocialInfluenceActionConfig.MiraTradePrivilegeSellBonusPercent / 100f)));
        return true;
    }

    private bool TryApplyMarketDayBuy(uint npcEntityId, ref int unitPrice)
    {
        if (npcEntityId != NpcEntityIds.Mira
            || _worldTime is null
            || !VillageEventConfig.IsMarketDay(_worldTime.GameDay))
        {
            return false;
        }

        unitPrice = VillageEventConfig.ApplyMarketDayBuyPrice(unitPrice);
        return true;
    }

    private bool TryApplyMarketDaySell(uint npcEntityId, ref int unitPrice)
    {
        if (npcEntityId != NpcEntityIds.Mira
            || _worldTime is null
            || !VillageEventConfig.IsMarketDay(_worldTime.GameDay))
        {
            return false;
        }

        unitPrice = VillageEventConfig.ApplyMarketDaySellPrice(unitPrice);
        return true;
    }

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId) =>
        VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));

    private static EconomyResponse Fail(
        EconomyRequestKind kind,
        uint npcEntityId,
        EconomyFailureReason reason,
        string message)
    {
        Log.Information("Economy request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new EconomyResponse(false, kind, npcEntityId, reason, message);
    }
}