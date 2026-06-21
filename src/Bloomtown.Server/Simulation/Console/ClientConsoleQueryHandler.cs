using System.Text;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Legacy;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Gathering;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Console;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Village;

namespace Bloomtown.Server.Simulation.Console;

/// <summary>
/// Answers client console queries for status, nearby entities, and resource nodes.
/// </summary>
public sealed class ClientConsoleQueryHandler
{
    private const float NearbyRadiusMeters = 24f;

    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerEnergySystem _energySystem;
    private readonly PlayerHousingService _housingService;
    private readonly ResourceGatheringHandler _gatheringHandler;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly VillageProjectStateService? _projectStateService;
    private readonly VillageAreaService? _areaService;
    private readonly WorldTimeSystem? _worldTime;
    private readonly PlayerRoutineService? _routineService;
    private readonly CommunityActivityService? _communityActivityService;
    private readonly DailyVillageActivityService? _dailyVillageActivityService;
    private readonly DailyRhythmService? _dailyRhythmService;
    private readonly NpcMemoryService? _memoryService;
    private readonly PlayerLegacyService? _legacyService;
    private readonly NpcInterpersonalRelationshipService? _interpersonalRelationshipService;
    private readonly CommunityReputationService? _communityReputationService;
    private readonly PlayerLongTermGoalService? _longTermGoalService;
    private readonly PlayerMilestoneService? _milestoneService;

    public ClientConsoleQueryHandler(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        PlayerEconomyService economyService,
        PlayerEnergySystem energyService,
        PlayerHousingService housingService,
        ResourceGatheringHandler gatheringHandler,
        PlayerNpcRelationshipService relationshipService,
        VillageProjectStateService? projectStateService = null,
        VillageAreaService? areaService = null,
        WorldTimeSystem? worldTime = null,
        PlayerRoutineService? routineService = null,
        CommunityActivityService? communityActivityService = null,
        DailyVillageActivityService? dailyVillageActivityService = null,
        DailyRhythmService? dailyRhythmService = null,
        NpcMemoryService? memoryService = null,
        PlayerLegacyService? legacyService = null,
        NpcInterpersonalRelationshipService? interpersonalRelationshipService = null,
        CommunityReputationService? communityReputationService = null,
        PlayerLongTermGoalService? longTermGoalService = null,
        PlayerMilestoneService? milestoneService = null)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _economyService = economyService;
        _energySystem = energyService;
        _housingService = housingService;
        _gatheringHandler = gatheringHandler;
        _relationshipService = relationshipService;
        _projectStateService = projectStateService;
        _areaService = areaService;
        _worldTime = worldTime;
        _routineService = routineService;
        _communityActivityService = communityActivityService;
        _dailyVillageActivityService = dailyVillageActivityService;
        _dailyRhythmService = dailyRhythmService;
        _memoryService = memoryService;
        _legacyService = legacyService;
        _interpersonalRelationshipService = interpersonalRelationshipService;
        _communityReputationService = communityReputationService;
        _longTermGoalService = longTermGoalService;
        _milestoneService = milestoneService;
    }

    public ClientQueryResponse Handle(uint playerEntityId, float playerX, float playerZ, ClientQueryRequest request)
    {
        return request.Kind switch
        {
            ClientQueryKind.Status => BuildStatus(playerEntityId, playerX, playerZ),
            ClientQueryKind.Goal => BuildGoal(playerEntityId),
            ClientQueryKind.Nearby => BuildNearby(playerEntityId, playerX, playerZ),
            ClientQueryKind.Nodes => BuildNodes(playerEntityId, playerX, playerZ),
            ClientQueryKind.Rest => _energySystem.Rest(playerEntityId),
            ClientQueryKind.Sleep => _housingService.SleepAtHome(playerEntityId, playerX, playerZ),
            _ => new ClientQueryResponse(request.Kind, false, "Unknown query type."),
        };
    }

    private ClientQueryResponse BuildStatus(uint playerEntityId, float playerX, float playerZ)
    {
        var builder = new StringBuilder();
        var gameDay = _worldTime?.GameDay ?? 1;

        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Overview"));
        builder.AppendLine($"Position: ({playerX:F1}, {playerZ:F1})");

        if (_worldTime is not null)
        {
            builder.AppendLine(VillageLifeConfig.FormatGameTimeStatus(
                _worldTime.GameDay,
                _worldTime.GameHour,
                _worldTime.MinuteOfHour));
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            builder.AppendLine("Coins: unknown");
            builder.AppendLine("Inventory: unavailable");
            return new ClientQueryResponse(ClientQueryKind.Status, true, builder.ToString().TrimEnd());
        }

        var newcomerHint = StatusDisplayConfig.FormatNewcomerHint(gameDay, economy.VillageContributionScore);
        if (!string.IsNullOrWhiteSpace(newcomerHint))
            builder.AppendLine(newcomerHint);

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Needs"));
        builder.AppendLine(
            $"Energy {economy.Energy:F0}/{PlayerEnergyConfig.MaxValue:F0} ({PlayerEnergyConfig.FormatStatusLabel(economy.Energy)}) · "
            + $"Hunger {economy.Hunger:F0}/{PlayerHungerConfig.MaxValue:F0} ({PlayerHungerConfig.FormatStatusLabel(economy.Hunger)})");
        builder.AppendLine(
            $"Mood {economy.Mood:F0}/{PlayerNeedsConfig.MaxValue:F0} ({PlayerNeedsConfig.FormatMoodLabel(economy.Mood)}) · "
            + $"Fatigue {economy.Fatigue:F0}/{PlayerNeedsConfig.MaxValue:F0} ({PlayerNeedsConfig.FormatFatigueLabel(economy.Fatigue)}) · "
            + $"Social {economy.SocialNeed:F0}/{PlayerNeedsConfig.MaxValue:F0} ({PlayerNeedsConfig.FormatSocialLabel(economy.SocialNeed)})");

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Village Standing"));
        builder.AppendLine(
            $"Coins {economy.Coins} · Reputation {economy.VillageReputation} · "
            + $"Contribution {economy.VillageContributionScore} ({VillageTitleDisplay.GetName(economy.VillageTitle)})");
        if (economy.VillagePosition != VillagePosition.None)
            builder.AppendLine($"Position: {VillagePositionDisplay.GetName(economy.VillagePosition)}");

        if (_housingService.TryGetState(playerEntityId, out var house))
        {
            var homeDistance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            var atHome = PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ);
            var sleepBonus = FurnitureComfortConfig.GetCombinedSleepRecovery(house.HouseTier, house.ComfortScore);

            builder.AppendLine();
            builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Home"));
            builder.AppendLine(
                $"({house.HouseX:F0}, {house.HouseZ:F0}) — {homeDistance:F1}m away{(atHome ? " · at home" : string.Empty)}");
            builder.AppendLine(
                $"{HouseTierDisplay.GetName(house.HouseTier)} · Comfort {house.ComfortScore} ({FurnitureComfortConfig.FormatComfortTierLabel(house.ComfortScore)}) · Sleep +{sleepBonus:F0} Energy");
            builder.AppendLine(StatusDisplayConfig.FormatCompactFurniture(house.PlacedFurniture));

            if (HouseUpgradeConfig.IsMaxTier(house.HouseTier))
                builder.AppendLine("Upgrade: fully upgraded.");
            else if (HouseUpgradeConfig.TryGetUpgradeRequirements(house.HouseTier, out var nextTier, out var requirements))
            {
                builder.AppendLine(
                    $"Next → {HouseTierDisplay.GetName(nextTier)}: {HouseUpgradeConfig.FormatRequirements(requirements)}");
            }

            if (atHome)
                builder.AppendLine(HomeActivityConfig.FormatAvailableActivities(house.PlacedFurniture));
        }

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatCompactInventory(economy.Inventory.ToStacks().ToList()));

        AppendNearbyRelationships(builder, playerEntityId, playerX, playerZ);
        AppendVillageLifeStatus(builder, playerEntityId, playerX, playerZ, gameDay);
        AppendPersonalRhythmStatus(builder);
        AppendDailyRhythmStatus(builder, playerEntityId);
        AppendNearbyActivitiesStatus(builder, playerX, playerZ);
        AppendVillageSocialStandingStatus(builder, playerEntityId);
        AppendSocialStandingStatus(builder, playerEntityId);
        AppendCommunityRecognitionStatus(builder, playerEntityId);
        AppendVillageBondRecognitionStatus(builder, playerEntityId);
        AppendLongTermGoalStatus(builder, playerEntityId);
        AppendPersonalMilestoneStatus(builder, playerEntityId);

        if (_projectStateService is not null)
        {
            builder.AppendLine();
            builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Village Progress"));
            builder.AppendLine(_projectStateService.FormatStatusAtmosphere());

            var projectBenefits = _projectStateService.FormatStatusBenefits();
            if (!string.IsNullOrWhiteSpace(projectBenefits))
                builder.AppendLine(projectBenefits);
        }

        if (_areaService is not null)
        {
            builder.AppendLine();
            builder.Append(_areaService.FormatStatusAreas());
        }

        return new ClientQueryResponse(ClientQueryKind.Status, true, builder.ToString().TrimEnd());
    }

    private ClientQueryResponse BuildNearby(uint playerEntityId, float playerX, float playerZ)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Nearby within {NearbyRadiusMeters:F0}m:");

        builder.AppendLine("NPCs:");
        var npcLines = 0;
        foreach (var simulation in _npcManager.SimulationStates)
        {
            var distance = GetDistance(playerX, playerZ, simulation.Npc.PositionX, simulation.Npc.PositionZ);
            if (distance > NearbyRadiusMeters)
                continue;

            var inAoi = _aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId);
            var aoiLabel = inAoi ? "in AOI" : "out of AOI";
            builder.AppendLine(
                $"  - {simulation.Npc.Name} at ({simulation.Npc.PositionX:F1}, {simulation.Npc.PositionZ:F1}), {distance:F1}m, {aoiLabel}");
            npcLines++;
        }

        if (npcLines == 0)
            builder.AppendLine("  (none)");

        builder.AppendLine("Resource nodes:");
        var nodeLines = 0;
        foreach (var node in ResourceNodeRegistry.All.OrderBy(n => GetDistance(playerX, playerZ, n.WorldX, n.WorldZ)))
        {
            var distance = GetDistance(playerX, playerZ, node.WorldX, node.WorldZ);
            if (distance > NearbyRadiusMeters)
                continue;

            var inAoi = _aoiSystem.IsPositionVisibleToPlayer(playerEntityId, node.WorldX, node.WorldZ);
            var item = ItemDatabase.GetDisplayName(node.OutputItem);
            builder.AppendLine(
                $"  - {node.Name} ({item}) at ({node.WorldX:F1}, {node.WorldZ:F1}), {distance:F1}m, {(inAoi ? "in AOI" : "out of AOI")}");
            nodeLines++;
        }

        if (nodeLines == 0)
            builder.AppendLine("  (none)");

        return new ClientQueryResponse(ClientQueryKind.Nearby, true, builder.ToString().TrimEnd());
    }

    private ClientQueryResponse BuildNodes(uint playerEntityId, float playerX, float playerZ)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Resource nodes (nearest first):");

        foreach (var node in ResourceNodeRegistry.All
                     .OrderBy(n => GetDistance(playerX, playerZ, n.WorldX, n.WorldZ)))
        {
            var distance = GetDistance(playerX, playerZ, node.WorldX, node.WorldZ);
            var cooldown = _gatheringHandler.GetNodeCooldownGameMinutes(node.NodeId);
            var cooldownLabel = cooldown > 0
                ? $"cooldown {cooldown:F0} game min"
                : "ready";
            var inAoi = _aoiSystem.IsPositionVisibleToPlayer(playerEntityId, node.WorldX, node.WorldZ);
            var item = ItemDatabase.GetDisplayName(node.OutputItem);

            builder.AppendLine(
                $"  - {node.Name} [{item}] ({node.WorldX:F1}, {node.WorldZ:F1}) | {distance:F1}m | {cooldownLabel} | {(inAoi ? "in AOI" : "out of AOI")}");
        }

        return new ClientQueryResponse(ClientQueryKind.Nodes, true, builder.ToString().TrimEnd());
    }

    private void AppendPersonalRhythmStatus(StringBuilder builder)
    {
        if (_routineService is null)
            return;

        builder.AppendLine();
        builder.AppendLine(_routineService.FormatPersonalRhythmStatus());
    }

    private void AppendDailyRhythmStatus(StringBuilder builder, uint playerEntityId)
    {
        if (_dailyRhythmService is null)
            return;

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Daily Rhythm"));
        builder.AppendLine(_dailyRhythmService.FormatStatus(playerEntityId));
    }

    private void AppendNearbyActivitiesStatus(StringBuilder builder, float playerX, float playerZ)
    {
        string? dailyLine = _dailyVillageActivityService?.FormatNearbyStatus(playerX, playerZ);
        string? communityLine = _communityActivityService?.FormatNearbyStatus(playerX, playerZ);
        if (string.IsNullOrWhiteSpace(dailyLine) && string.IsNullOrWhiteSpace(communityLine))
            return;

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Nearby Activities"));
        if (!string.IsNullOrWhiteSpace(dailyLine))
            builder.AppendLine(dailyLine);
        if (!string.IsNullOrWhiteSpace(communityLine))
            builder.AppendLine(communityLine);
        builder.AppendLine("Lists: daily · community");
    }

    private void AppendCommunityRecognitionStatus(StringBuilder builder, uint playerEntityId)
    {
        if (_legacyService is null)
            return;

        var regardLine = _legacyService.FormatCommunityRecognitionStatus(playerEntityId);
        var participationLine = _legacyService.FormatCommunityParticipationStatus(playerEntityId);

        if (string.IsNullOrWhiteSpace(regardLine) && string.IsNullOrWhiteSpace(participationLine))
            return;

        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(regardLine))
            builder.AppendLine(regardLine);

        if (!string.IsNullOrWhiteSpace(participationLine))
            builder.AppendLine(participationLine);
    }

    private void AppendVillageBondRecognitionStatus(StringBuilder builder, uint playerEntityId)
    {
        var focusCloseFriends = VillageBondRecognitionConfig.GetFocusCloseFriendNpcIds(
            id => _relationshipService.GetTier(playerEntityId, id));
        var villageNoticed = _memoryService?.HasVillageNoticedBondsMemory(playerEntityId) ?? false;
        var passiveBenefitActive = VillageBondRecognitionConfig.IsEligibleForPassiveBenefit(
            focusCloseFriends.Count);
        var statusLine = VillageBondRecognitionConfig.FormatRecognitionStatus(
            focusCloseFriends,
            villageNoticed,
            passiveBenefitActive);

        if (string.IsNullOrWhiteSpace(statusLine))
            return;

        builder.AppendLine();
        builder.AppendLine(statusLine);
    }

    private ClientQueryResponse BuildGoal(uint playerEntityId)
    {
        if (_longTermGoalService is null)
            return new ClientQueryResponse(ClientQueryKind.Goal, false, "Long-term goal tracking is unavailable.");

        return new ClientQueryResponse(
            ClientQueryKind.Goal,
            true,
            _longTermGoalService.FormatGoalDetail(playerEntityId));
    }

    private void AppendLongTermGoalStatus(StringBuilder builder, uint playerEntityId)
    {
        if (_longTermGoalService is null)
            return;

        builder.AppendLine();
        builder.AppendLine(_longTermGoalService.FormatGoalStatusLine(playerEntityId));
    }

    private void AppendPersonalMilestoneStatus(StringBuilder builder, uint playerEntityId)
    {
        if (_milestoneService is null)
            return;

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Personal Milestones"));
        builder.AppendLine(_milestoneService.FormatStatusLine(playerEntityId));
    }

    private void AppendVillageSocialStandingStatus(StringBuilder builder, uint playerEntityId)
    {
        var focusCloseFriends = VillageBondRecognitionConfig.GetFocusCloseFriendNpcIds(
            id => _relationshipService.GetTier(playerEntityId, id));
        var villageNoticed = _memoryService?.HasVillageNoticedBondsMemory(playerEntityId) ?? false;
        var standingLine = VillageSocialStandingConfig.FormatStandingStatus(
            focusCloseFriends.Count,
            focusCloseFriends,
            villageNoticed);

        if (string.IsNullOrWhiteSpace(standingLine))
            return;

        var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriends.Count);

        builder.AppendLine();
        builder.AppendLine("── Social Standing ──");
        builder.AppendLine(
            $"{VillageSocialStandingConfig.GetStandingTierLabel(standingTier)} · "
            + $"{VillageSocialStandingConfig.FormatFocusCloseFriendsLabel(focusCloseFriends)}");
        builder.AppendLine(standingLine);

        var milestonesAndLegacy = VillageSocialStandingStatusFormatter.FormatMilestonesAndLegacySection(
            standingTier,
            focusCloseFriends.Count,
            villageNoticed);
        if (!string.IsNullOrWhiteSpace(milestonesAndLegacy))
        {
            builder.AppendLine();
            builder.AppendLine(milestonesAndLegacy);
        }

        var activeBenefits = VillageSocialStandingStatusFormatter.FormatActiveBenefitsSection(standingTier);
        if (!string.IsNullOrWhiteSpace(activeBenefits))
        {
            builder.AppendLine();
            builder.AppendLine(activeBenefits);
        }

        var whatYouCanDo = VillageSocialStandingStatusFormatter.FormatWhatYouCanDoSection(standingTier);
        if (!string.IsNullOrWhiteSpace(whatYouCanDo))
        {
            builder.AppendLine();
            builder.AppendLine(whatYouCanDo);
        }

        if (SocialStandingActionConfig.IsEligible(standingTier) && _memoryService is not null)
        {
            var favorEntries = NpcEmotionalBondConfig.FocusNpcEntityIds
                .Select(npcEntityId =>
                {
                    var remaining = _memoryService.GetSocialStandingFavorCooldownRemainingMinutes(
                        playerEntityId,
                        npcEntityId,
                        standingTier);
                    return (npcEntityId, remaining <= 0, remaining);
                })
                .ToList();
            var favorSummary = VillageSocialStandingStatusFormatter.FormatSocialFavorAvailabilitySummary(favorEntries);
            if (!string.IsNullOrWhiteSpace(favorSummary))
                builder.AppendLine(favorSummary);
        }

        if (SocialInfluenceActionConfig.IsEligible(standingTier) && _memoryService is not null)
        {
            var callOnEntries = SocialInfluenceActionConfig.SupportedNpcEntityIds
                .Select(npcEntityId =>
                {
                    var remaining = _memoryService.GetSocialInfluenceCooldownRemainingMinutes(
                        playerEntityId,
                        npcEntityId,
                        standingTier);
                    var ready = remaining <= 0;
                    var hasHaroldBacking = _memoryService.HasHaroldSocialInfluenceProjectBacking(playerEntityId);
                    var hasElsieBacking = _memoryService.HasElsieSocialInfluenceGardenBacking(playerEntityId);
                    var hasTomBacking = _memoryService.HasTomSocialInfluenceLumberBacking(playerEntityId);
                    var hasNoraBacking = _memoryService.HasNoraSocialInfluenceHerbalBacking(playerEntityId);
                    var hasEliasBacking = _memoryService.HasEliasSocialInfluenceSmithingBacking(playerEntityId);
                    var hasBenBacking = _memoryService.HasBenSocialInfluenceGuardBacking(playerEntityId);
                    var hasLilaBacking = _memoryService.HasLilaSocialInfluenceYouthBacking(playerEntityId);
                    var hasRowanBacking = _memoryService.HasRowanSocialInfluenceStoryBacking(playerEntityId);
                    var hasMarcusBacking = _memoryService.HasMarcusSocialInfluenceCraftingBacking(playerEntityId);
                    var hasEleanorBacking = _memoryService.HasEleanorSocialInfluenceLegacyBacking(playerEntityId);
                    var hasMiraPrivilege = _memoryService.HasMiraSocialInfluenceTradePrivilege(playerEntityId);
                    var miraBuyPrivilege = hasMiraPrivilege
                        && _memoryService.TryGetMiraSocialInfluenceTradePrivilegeIsBuy(playerEntityId, out var isBuy)
                        && isBuy;
                    var miraRemainingUses = hasMiraPrivilege
                        ? _memoryService.GetMiraSocialInfluenceTradePrivilegeRemainingUses(playerEntityId)
                        : 0;
                    _ = _memoryService.TryGetHaroldSocialInfluenceProjectBacking(playerEntityId, out var haroldBackingBonus);
                    _ = _memoryService.TryGetElsieSocialInfluenceGardenBacking(playerEntityId, out var elsieBackingBonus);
                    _ = _memoryService.TryGetTomSocialInfluenceLumberBacking(playerEntityId, out var tomBackingBonus);
                    _ = _memoryService.TryGetNoraSocialInfluenceHerbalBacking(playerEntityId, out var noraBackingBonus);
                    _ = _memoryService.TryGetEliasSocialInfluenceSmithingBacking(playerEntityId, out var eliasBackingBonus);
                    _ = _memoryService.TryGetBenSocialInfluenceGuardBacking(playerEntityId, out var benBackingBonus);
                    _ = _memoryService.TryGetLilaSocialInfluenceYouthBacking(playerEntityId, out var lilaBackingBonus);
                    _ = _memoryService.TryGetRowanSocialInfluenceStoryBacking(playerEntityId, out var rowanBackingBonus);
                    _ = _memoryService.TryGetMarcusSocialInfluenceCraftingBacking(playerEntityId, out var marcusBackingBonus);
                    _ = _memoryService.TryGetEleanorSocialInfluenceLegacyBacking(playerEntityId, out var eleanorBackingBonus);
                    var privilegeLabel = SocialInfluenceActionConfig.GetActivePrivilegeLabel(
                        npcEntityId,
                        hasHaroldBacking,
                        hasElsieBacking,
                        hasMiraPrivilege,
                        miraBuyPrivilege,
                        hasTomBacking,
                        hasNoraBacking,
                        hasEliasBacking,
                        hasBenBacking,
                        hasLilaBacking,
                        hasRowanBacking,
                        hasMarcusBacking,
                        hasEleanorBacking,
                        miraRemainingUses,
                        haroldBackingBonus,
                        elsieBackingBonus,
                        tomBackingBonus,
                        noraBackingBonus,
                        eliasBackingBonus,
                        benBackingBonus,
                        lilaBackingBonus,
                        rowanBackingBonus,
                        marcusBackingBonus,
                        eleanorBackingBonus);
                    return (npcEntityId, ready, remaining, privilegeLabel);
                })
                .ToList();
            var callOnSummary = VillageSocialStandingStatusFormatter.FormatCallOnAvailabilitySummary(
                standingTier,
                callOnEntries);
            if (!string.IsNullOrWhiteSpace(callOnSummary))
                builder.AppendLine(callOnSummary);
        }

        var actionHint = VillageSocialStandingConfig.FormatSocialStandingActionHint(standingTier);
        if (!string.IsNullOrWhiteSpace(actionHint))
            builder.AppendLine(actionHint);

        var villagePerception = VillageSocialStandingStatusFormatter.FormatHowTheVillageSeesYouSection(
            standingTier,
            villageNoticed);
        if (!string.IsNullOrWhiteSpace(villagePerception))
        {
            builder.AppendLine();
            builder.AppendLine(villagePerception);
        }
    }

    private void AppendSocialStandingStatus(StringBuilder builder, uint playerEntityId)
    {
        if (_communityReputationService is null)
            return;

        var relationships = _relationshipService.GetRelationships(playerEntityId);
        var acquaintanceCount = 0;
        var friendCount = 0;
        var closeFriendCount = 0;

        foreach (var relationship in relationships)
        {
            var tier = RelationshipTierCalculator.GetTier(relationship.AffinityValue);
            switch (tier)
            {
                case RelationshipTier.CloseFriend:
                    closeFriendCount++;
                    break;
                case RelationshipTier.Friend:
                    friendCount++;
                    break;
                case RelationshipTier.Acquaintance:
                    acquaintanceCount++;
                    break;
            }
        }

        var reputation = _communityReputationService.GetState(playerEntityId);
        builder.AppendLine();
        builder.AppendLine(SocialDynamicsConfig.FormatSocialStanding(
            friendCount,
            closeFriendCount,
            acquaintanceCount,
            reputation));
    }

    private void AppendVillageLifeStatus(
        StringBuilder builder,
        uint playerEntityId,
        float playerX,
        float playerZ,
        int gameDay)
    {
        if (_worldTime is null)
            return;

        var timeOfDay = VillageLifeConfig.GetTimeOfDay(_worldTime.GameHour);
        var developmentLevel = _projectStateService?.DevelopmentLevel ?? VillageDevelopmentLevel.Quiet;

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader("Village Today"));
        builder.AppendLine(VillageLifeConfig.FormatVillageRhythm(timeOfDay, developmentLevel));

        var eventStatus = VillageEventConfig.FormatActiveEventsStatus(_worldTime.GameDay);
        if (!string.IsNullOrWhiteSpace(eventStatus))
            builder.AppendLine(eventStatus);

        var unlockedAreas = _areaService?.GetUnlockedAreas().ToHashSet() ?? new HashSet<VillageArea>();
        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        VillageLifeConfig.TryResolveAmbientLocation(
            playerX,
            playerZ,
            unlockedAreas,
            completedProjects,
            out var location,
            out _);

        var nearbyFeel = VillageLifeConfig.FormatNearbyFeel(location);
        if (!string.IsNullOrWhiteSpace(nearbyFeel))
            builder.AppendLine(nearbyFeel);

        if (StatusDisplayConfig.ShouldShowSocialFabric(gameDay))
        {
            var legacyContext = _legacyService?.BuildContext(playerEntityId);
            builder.AppendLine(VillageSocialFabricConfig.FormatSocialFabricStatus(
                timeOfDay,
                developmentLevel,
                legacyContext));
        }

        if (StatusDisplayConfig.ShouldShowInterpersonalStatus(gameDay) && _interpersonalRelationshipService is not null)
            builder.AppendLine(_interpersonalRelationshipService.FormatRelationshipStatus());

        if (StatusDisplayConfig.ShouldShowSocialCircle(gameDay))
            builder.AppendLine(VillageSocialCircleConfig.FormatSocialCircleStatus());

        if (_projectStateService is not null
            && _projectStateService.TryGetNearestCompletedSite(playerX, playerZ, out var projectId, out _))
        {
            builder.AppendLine(VillageReactivityConfig.FormatNearbyProjectFeel(
                projectId,
                developmentLevel));
        }
    }

    private void AppendNearbyRelationships(StringBuilder builder, uint playerEntityId, float playerX, float playerZ)
    {
        var nearbyNpcs = _npcManager.SimulationStates
            .Select(simulation => new
            {
                simulation.Npc.EntityId,
                simulation.Npc.Name,
                Distance = GetDistance(playerX, playerZ, simulation.Npc.PositionX, simulation.Npc.PositionZ),
            })
            .Where(npc => npc.Distance <= NearbyRadiusMeters)
            .OrderBy(npc => npc.Distance)
            .ToList();

        if (nearbyNpcs.Count == 0)
            return;

        builder.AppendLine();
        builder.AppendLine(StatusDisplayConfig.FormatSectionHeader($"People Nearby ({NearbyRadiusMeters:F0}m)"));
        foreach (var npc in nearbyNpcs)
        {
            var affinity = _relationshipService.GetAffinity(playerEntityId, npc.EntityId);
            var tier = RelationshipTierCalculator.GetTier(affinity);
            var bondHint = string.Empty;
            if (_memoryService is not null)
            {
                var memories = _memoryService.GetMemoriesForNpc(playerEntityId, npc.EntityId);
                var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(npc.EntityId, memories)
                    ?? NpcMemoryConfig.FormatBondHint(memories, tier);
                if (!string.IsNullOrWhiteSpace(hint))
                    bondHint = $" — {hint}";
            }

            builder.AppendLine(
                $"  - {npc.Name}: {RelationshipTierDisplay.GetName(tier)} (affinity {affinity}) — {npc.Distance:F1}m{bondHint}");
        }
    }

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}