using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Gathering;

/// <summary>
/// Validates gather requests, runs timed sessions, and applies rewards with node cooldowns.
/// </summary>
public sealed class ResourceGatheringHandler
{
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly PlayerNpcRelationshipService? _relationshipService;
    private readonly WorldTimeSystem? _worldTime;
    private readonly Action<uint, GatheringResponse> _sendResponse;
    private readonly Dictionary<uint, GatheringSession> _activeSessions = new();
    private readonly Dictionary<int, double> _nodeCooldownGameMinutes = new();

    public ResourceGatheringHandler(
        AoiSystem aoiSystem,
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        Action<uint, GatheringResponse> sendResponse,
        PlayerNpcRelationshipService? relationshipService = null,
        WorldTimeSystem? worldTime = null)
    {
        _aoiSystem = aoiSystem;
        _economyService = economyService;
        _needsService = needsService;
        _relationshipService = relationshipService;
        _worldTime = worldTime;
        _sendResponse = sendResponse;
    }

    public GatheringResponse TryStartGather(uint playerEntityId, float playerX, float playerZ, GatheringRequest request)
    {
        if (request.ResourceType == 0)
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.UnknownResource,
                "Unknown resource type.");
        }

        if (_activeSessions.ContainsKey(playerEntityId))
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.AlreadyGathering,
                "You are already gathering. Wait for the current action to finish.");
        }

        if (!_economyService.TryGetState(playerEntityId, out _))
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.EconomyUnavailable,
                "Player inventory is unavailable.");
        }

        var node = FindNearestNode(playerX, playerZ, request.ResourceType);
        if (node is null)
        {
            var itemName = ItemDatabase.GetDisplayName(request.ResourceType);
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.NoNodeNearby,
                $"No {itemName} resource node nearby. Move closer to a gathering area.");
        }

        var distance = GetDistance(playerX, playerZ, node.WorldX, node.WorldZ);
        if (distance > GatheringConfig.GatherRadiusMeters)
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.NotInRange,
                $"{node.Name} is too far away ({distance:F1}m). Move within {GatheringConfig.GatherRadiusMeters:F0}m.");
        }

        if (!_aoiSystem.IsPositionVisibleToPlayer(playerEntityId, node.WorldX, node.WorldZ))
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.NotInAoi,
                $"{node.Name} is not in your area. Walk closer until the node is within your AOI.");
        }

        if (IsNodeOnCooldown(node.NodeId, out var remainingMinutes))
        {
            return Fail(
                playerEntityId,
                request.ResourceType,
                GatheringFailureReason.OnCooldown,
                $"{node.Name} is on cooldown ({remainingMinutes:F0} game minutes remaining).");
        }

        var gatherDuration = GatheringConfig.GatherDurationRealSeconds;
        PlayerEconomyState? economyState = null;
        if (_economyService.TryGetState(playerEntityId, out var economy))
        {
            economyState = economy;
            gatherDuration *= PlayerEnergyConfig.GetGatherDurationMultiplier(economy.Energy);
            gatherDuration *= _needsService.GetGatherDurationMultiplier(economy);
            _needsService.ApplyGatherStart(economy);
        }

        _activeSessions[playerEntityId] = new GatheringSession
        {
            PlayerEntityId = playerEntityId,
            NodeId = node.NodeId,
            RemainingRealSeconds = gatherDuration,
        };

        var itemLabel = ItemDatabase.GetDisplayName(request.ResourceType);
        var penaltyNote = string.Empty;
        if (economyState is not null)
        {
            if (PlayerEnergyConfig.IsLowEnergy(economyState.Energy))
                penaltyNote += " (low energy — gathering is slower)";
            if (PlayerNeedsConfig.IsExhausted(economyState.Fatigue))
                penaltyNote += " (exhausted — gathering is slower)";
            if (PlayerNeedsConfig.IsLowMood(economyState.Mood))
                penaltyNote += " (low mood — gathering is slower)";
        }
        var message =
            $"Started gathering {itemLabel} at {node.Name}. Please wait {gatherDuration:F1} seconds...{penaltyNote}";

        Log.Information(
            "Player {PlayerId} started gathering {Item} at node {NodeName} ({NodeId}).",
            playerEntityId,
            itemLabel,
            node.Name,
            node.NodeId);

        return new GatheringResponse(
            GatheringResponseKind.Started,
            request.ResourceType,
            0,
            node.NodeId,
            GatheringFailureReason.None,
            message);
    }

    public void CancelPlayerSession(uint playerEntityId)
    {
        _activeSessions.Remove(playerEntityId);
    }

    public double GetNodeCooldownGameMinutes(int nodeId)
    {
        return _nodeCooldownGameMinutes.GetValueOrDefault(nodeId, 0);
    }

    public void Update(double deltaTimeSeconds, double deltaGameMinutes)
    {
        DecayCooldowns(deltaGameMinutes);

        if (_activeSessions.Count == 0)
            return;

        var completedPlayers = new List<uint>();

        foreach (var (playerEntityId, session) in _activeSessions)
        {
            session.RemainingRealSeconds -= deltaTimeSeconds;
            if (session.RemainingRealSeconds > 0)
                continue;

            completedPlayers.Add(playerEntityId);
            CompleteGather(playerEntityId, session);
        }

        foreach (var playerEntityId in completedPlayers)
            _activeSessions.Remove(playerEntityId);
    }

    private void CompleteGather(uint playerEntityId, GatheringSession session)
    {
        var node = ResourceNodeRegistry.GetById(session.NodeId);
        if (node is null)
        {
            _sendResponse(
                playerEntityId,
                Fail(playerEntityId, ItemType.Wood, GatheringFailureReason.NoNodeNearby, "Resource node no longer exists."));
            return;
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            _sendResponse(
                playerEntityId,
                Fail(
                    playerEntityId,
                    node.OutputItem,
                    GatheringFailureReason.EconomyUnavailable,
                    "Could not deliver gathered items to inventory."));
            return;
        }

        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var bonusYield = 0;
        if (VillageSocialStandingMechanicalConfig.IsTomWoodGathering(node.OutputItem)
            && VillageSocialStandingMechanicalConfig.ShouldGrantTomWoodBonusYield(
                playerEntityId,
                node.NodeId,
                standingTier))
        {
            bonusYield = 1;
        }

        var totalYield = node.YieldAmount + bonusYield;
        economy.Inventory.AddItem(node.OutputItem, totalYield);
        _needsService.ApplyGatherComplete(economy);

        var rainyDayActive = _worldTime is not null && VillageEventConfig.IsRainyDay(_worldTime.GameDay);
        if (rainyDayActive)
        {
            economy.Fatigue = PlayerNeedsConfig.Clamp(
                economy.Fatigue + VillageEventConfig.RainyDayGatheringFatiguePenalty);
        }

        _nodeCooldownGameMinutes[node.NodeId] = GatheringConfig.NodeCooldownGameMinutes;
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var itemLabel = ItemDatabase.GetDisplayName(node.OutputItem);
        var message = $"Successfully gathered {totalYield} {itemLabel} from {node.Name}.";
        message += VillageSocialStandingMechanicalConfig.FormatTomWoodGatherFeedback(standingTier, bonusYield);
        if (rainyDayActive)
            message += $" {VillageEventConfig.FormatRainyDayGatheringFeedback()}";

        Log.Information(
            "Player {PlayerId} successfully gathered {Quantity} {Item} from node {NodeName} ({NodeId}).",
            playerEntityId,
            totalYield,
            itemLabel,
            node.Name,
            node.NodeId);

        _sendResponse(
            playerEntityId,
            new GatheringResponse(
                GatheringResponseKind.Completed,
                node.OutputItem,
                totalYield,
                node.NodeId,
                GatheringFailureReason.None,
                message));
    }

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId)
    {
        if (_relationshipService is null)
            return VillageSocialStandingTier.Stranger;

        return VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
    }

    private void DecayCooldowns(double deltaGameMinutes)
    {
        if (deltaGameMinutes <= 0 || _nodeCooldownGameMinutes.Count == 0)
            return;

        var expired = new List<int>();

        foreach (var (nodeId, remaining) in _nodeCooldownGameMinutes)
        {
            var updated = remaining - deltaGameMinutes;
            if (updated <= 0)
                expired.Add(nodeId);
            else
                _nodeCooldownGameMinutes[nodeId] = updated;
        }

        foreach (var nodeId in expired)
            _nodeCooldownGameMinutes.Remove(nodeId);
    }

    private bool IsNodeOnCooldown(int nodeId, out double remainingGameMinutes)
    {
        if (_nodeCooldownGameMinutes.TryGetValue(nodeId, out remainingGameMinutes))
            return true;

        remainingGameMinutes = 0;
        return false;
    }

    private static ResourceNode? FindNearestNode(float playerX, float playerZ, ItemType resourceType)
    {
        ResourceNode? nearest = null;
        var bestDistance = float.MaxValue;

        foreach (var node in ResourceNodeRegistry.All)
        {
            if (node.OutputItem != resourceType)
                continue;

            var distance = GetDistance(playerX, playerZ, node.WorldX, node.WorldZ);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            nearest = node;
        }

        return nearest;
    }

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private GatheringResponse Fail(
        uint playerEntityId,
        ItemType resourceType,
        GatheringFailureReason reason,
        string message)
    {
        Log.Information(
            "Player {PlayerId} gather {Item} failed ({Reason}): {Message}",
            playerEntityId,
            resourceType == 0 ? "unknown" : ItemDatabase.GetDisplayName(resourceType),
            reason,
            message);

        return new GatheringResponse(
            GatheringResponseKind.Failed,
            resourceType,
            0,
            0,
            reason,
            message);
    }
}