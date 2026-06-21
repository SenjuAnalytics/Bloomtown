using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Gifting;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Gifting;

/// <summary>
/// Validates proximity and inventory, then gifts items to NPCs for affinity gains.
/// </summary>
public sealed class NpcGiftingService
{
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly NpcMemoryService _memoryService;

    public NpcGiftingService(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        PlayerNpcRelationshipService relationshipService,
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        NpcMemoryService memoryService)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _relationshipService = relationshipService;
        _economyService = economyService;
        _needsService = needsService;
        _memoryService = memoryService;
    }

    public GiftResponse Handle(uint playerEntityId, float playerX, float playerZ, GiftRequest request)
    {
        if (request.ItemType == 0)
        {
            return Fail(request, GiftFailureReason.UnknownItem, "Unknown item type.");
        }

        if (request.Quantity is < 1 or > EconomyConfig.MaxTransactionQuantity)
        {
            return Fail(
                request,
                GiftFailureReason.InvalidQuantity,
                $"Quantity must be between 1 and {EconomyConfig.MaxTransactionQuantity}.");
        }

        if (request.NpcEntityId == 0)
        {
            return Fail(
                request,
                GiftFailureReason.UnknownNpc,
                $"Unknown NPC. Known NPCs: {NpcNameLookup.KnownNamesList}.");
        }

        var simulation = _npcManager.GetState(request.NpcEntityId);
        if (simulation is null)
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(request.NpcEntityId);
            return Fail(
                request,
                GiftFailureReason.UnknownNpc,
                $"NPC '{npcLabel}' was not found. Known NPCs: {NpcNameLookup.KnownNamesList}.");
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
            return Fail(
                request,
                GiftFailureReason.NotInRange,
                $"{npcName} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m to give a gift.");
        }

        if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
        {
            return Fail(
                request,
                GiftFailureReason.NotInAoi,
                $"{npcName} is not in your area. Walk closer until they appear in your AOI.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(request, GiftFailureReason.PlayerUnavailable, "Player inventory is unavailable.");
        }

        if (!economy.Inventory.HasItem(request.ItemType, request.Quantity))
        {
            var itemName = ItemDatabase.GetDisplayName(request.ItemType);
            var owned = economy.Inventory.GetItemCount(request.ItemType);
            return Fail(
                request,
                GiftFailureReason.NotEnoughItems,
                $"Not enough {itemName} to gift. You have {owned}, tried to give {request.Quantity}.");
        }

        var isPreferred = NpcGiftPreference.IsPreferred(request.NpcEntityId, request.ItemType);
        var affinityGain = GiftValueConfig.CalculateAffinityGain(
            request.NpcEntityId,
            request.ItemType,
            request.Quantity);
        economy.Inventory.RemoveItem(request.ItemType, request.Quantity);
        _needsService.ApplyGift(economy);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var affinityChange = _relationshipService
            .ApplyAffinityGainAsync(playerEntityId, request.NpcEntityId, affinityGain)
            .GetAwaiter()
            .GetResult();

        TryRecordVillageBondRecognitionMemory(playerEntityId);

        var newlyRecorded = _memoryService
            .OnGiftGivenAsync(playerEntityId, request.NpcEntityId, isPreferred)
            .GetAwaiter()
            .GetResult();
        var memories = _memoryService.GetMemoriesForNpc(playerEntityId, request.NpcEntityId);
        var justRecordedFirstPreferredGift =
            newlyRecorded == NpcMemoryType.FirstPreferredGiftReceived;
        var justRecordedBondGiftMemory = newlyRecorded is not null
            && NpcEmotionalBondGiftConfig.IsFavoriteGiftBondingMemory(newlyRecorded.Value);

        var itemLabel = ItemDatabase.GetDisplayName(request.ItemType);
        var variationSeed = playerEntityId + request.NpcEntityId + (uint)request.ItemType;
        string dialogue;

        if (isPreferred
            && NpcEmotionalBondConfig.IsFocusNpc(request.NpcEntityId)
            && NpcEmotionalBondGiftConfig.TryGetFavoriteGiftAcceptanceLine(
                request.NpcEntityId,
                request.ItemType,
                affinityChange.NewTier,
                justRecordedBondGiftMemory,
                variationSeed) is { } bondGiftLine
            && !string.IsNullOrWhiteSpace(bondGiftLine))
        {
            dialogue = bondGiftLine;
        }
        else
        {
            dialogue = NpcResponseGenerator.GenerateGiftResponse(
                simulation,
                request.ItemType,
                isPreferred,
                affinityChange.NewTier,
                memories,
                justRecordedFirstPreferredGift);
        }

        var tierLabel = RelationshipTierDisplay.GetName(affinityChange.NewTier);
        var preferenceNote = isPreferred ? " (favorite gift!)" : string.Empty;
        var bondGiftNote = isPreferred && NpcEmotionalBondConfig.IsFocusNpc(request.NpcEntityId)
            ? NpcEmotionalBondGiftConfig.FormatFavoriteGiftBondFeedback(npcName, justRecordedBondGiftMemory)
            : string.Empty;
        var message =
            $"{bondGiftNote}Affinity with {npcName} +{affinityChange.NewAffinity - affinityChange.PreviousAffinity}{preferenceNote} " +
            $"(now {affinityChange.NewAffinity}, {tierLabel}).";

        Log.Information(
            "Player {PlayerId} gifted {Quantity} {Item} to {NpcName} ({Preferred}). Affinity {Old}->{New} ({OldTier}->{NewTier}).",
            playerEntityId,
            request.Quantity,
            itemLabel,
            npcName,
            isPreferred ? "preferred" : "neutral",
            affinityChange.PreviousAffinity,
            affinityChange.NewAffinity,
            RelationshipTierDisplay.GetName(affinityChange.PreviousTier),
            tierLabel);

        return new GiftResponse(
            Success: true,
            request.NpcEntityId,
            request.ItemType,
            request.Quantity,
            affinityChange.NewAffinity - affinityChange.PreviousAffinity,
            affinityChange.NewAffinity,
            GiftFailureReason.None,
            dialogue,
            message);
    }

    private void TryRecordVillageBondRecognitionMemory(uint playerEntityId)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));

        if (_memoryService.TryRecordVillageNoticedBondsIfEligibleAsync(
                playerEntityId,
                focusCloseFriendCount)
            .GetAwaiter()
            .GetResult())
        {
            Log.Information(
                "Recorded village bond recognition memory for player {PlayerId} with {CloseFriendCount} focus close friend(s).",
                playerEntityId,
                focusCloseFriendCount);
        }
    }

    private static GiftResponse Fail(GiftRequest request, GiftFailureReason reason, string message)
    {
        Log.Information("Gift to NPC {NpcId} failed ({Reason}): {Message}", request.NpcEntityId, reason, message);
        return new GiftResponse(
            false,
            request.NpcEntityId,
            request.ItemType,
            request.Quantity,
            0,
            0,
            reason,
            string.Empty,
            message);
    }
}