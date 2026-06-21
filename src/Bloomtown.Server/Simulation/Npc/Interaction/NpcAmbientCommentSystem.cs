using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Legacy;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc.Interaction;

/// <summary>
/// Occasionally sends ambient village flavor: time-of-day, location, emergent events, and NPC chatter.
/// Comments grow more frequent and contextual as village development level rises.
/// </summary>
public sealed class NpcAmbientCommentSystem : ISimulationSystem
{
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly NpcMemoryService _memoryService;
    private readonly WorldTimeSystem _worldTime;
    private readonly VillageProjectStateService? _projectStateService;
    private readonly VillageAreaService? _areaService;
    private readonly PlayerLegacyService? _legacyService;
    private readonly CommunityReputationService? _communityReputationService;
    private readonly SocialDynamicsService? _socialDynamicsService;
    private readonly PlayerLongTermGoalService? _longTermGoalService;
    private readonly NpcInterpersonalRelationshipService? _interpersonalService;
    private readonly VillageReactivityService? _reactivityService;
    private readonly Func<IReadOnlyList<(uint EntityId, float X, float Z)>> _getPlayerPositions;
    private readonly Action<uint, uint, string> _sendComment;
    private readonly Dictionary<uint, DateTime> _timeLocationCooldowns = new();
    private readonly Dictionary<uint, DateTime> _emergentEventCooldowns = new();
    private readonly Dictionary<uint, DateTime> _npcToNpcCooldowns = new();
    private readonly Dictionary<uint, DateTime> _villageGossipCooldowns = new();
    private readonly Dictionary<uint, uint> _emergentAttemptCounters = new();
    private readonly Dictionary<uint, uint> _villageGossipAttemptCounters = new();
    private readonly Dictionary<uint, DateTime> _growthReactionCooldowns = new();
    private readonly Dictionary<uint, DateTime> _projectCompletionReactionCooldowns = new();
    private readonly Dictionary<uint, DateTime> _communityMomentCooldowns = new();
    private readonly Dictionary<uint, uint> _communityMomentAttemptCounters = new();
    private readonly Dictionary<uint, DateTime> _groupSocialMomentCooldowns = new();
    private readonly Dictionary<uint, uint> _groupSocialMomentAttemptCounters = new();
    private readonly Dictionary<uint, uint> _villageBondRecognitionAttemptCounters = new();
    private readonly Dictionary<uint, uint> _villageSocialStandingAttemptCounters = new();
    private readonly Dictionary<uint, DateTime> _villageEventAmbientCooldowns = new();
    private readonly Dictionary<uint, uint> _villageEventAmbientAttemptCounters = new();
    private double _secondsSinceLastCheck;

    public NpcAmbientCommentSystem(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        PlayerNpcRelationshipService relationshipService,
        NpcMemoryService memoryService,
        WorldTimeSystem worldTime,
        Func<IReadOnlyList<(uint EntityId, float X, float Z)>> getPlayerPositions,
        Action<uint, uint, string> sendComment,
        VillageProjectStateService? projectStateService = null,
        VillageAreaService? areaService = null,
        PlayerLegacyService? legacyService = null,
        CommunityReputationService? communityReputationService = null,
        SocialDynamicsService? socialDynamicsService = null,
        PlayerLongTermGoalService? longTermGoalService = null,
        NpcInterpersonalRelationshipService? interpersonalService = null,
        VillageReactivityService? reactivityService = null)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _relationshipService = relationshipService;
        _memoryService = memoryService;
        _worldTime = worldTime;
        _projectStateService = projectStateService;
        _areaService = areaService;
        _legacyService = legacyService;
        _communityReputationService = communityReputationService;
        _socialDynamicsService = socialDynamicsService;
        _longTermGoalService = longTermGoalService;
        _interpersonalService = interpersonalService;
        _reactivityService = reactivityService;
        _getPlayerPositions = getPlayerPositions;
        _sendComment = sendComment;
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        var developmentLevel = _projectStateService?.DevelopmentLevel ?? VillageDevelopmentLevel.Quiet;
        var checkInterval = VillageAtmosphereConfig.GetAmbientCheckIntervalSeconds(developmentLevel);

        _secondsSinceLastCheck += deltaTimeSeconds;
        if (_secondsSinceLastCheck < checkInterval)
            return;

        _secondsSinceLastCheck = 0;

        foreach (var (playerEntityId, playerX, playerZ) in _getPlayerPositions())
            TryAmbientCommentForPlayer(playerEntityId, playerX, playerZ, developmentLevel);
    }

    private void TryAmbientCommentForPlayer(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel)
    {
        var timeOfDay = VillageLifeConfig.GetTimeOfDay(_worldTime.GameHour);
        var standingTier = VillageSocialStandingConfig.ResolveTier(
            VillageBondRecognitionConfig.CountFocusCloseFriends(
                npcId => _relationshipService.GetTier(playerEntityId, npcId)));

        if (standingTier >= VillageSocialStandingTier.Respected
            && TryVillageSocialStandingComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
        {
            return;
        }

        if (TryProjectSiteComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryProjectCompletionReactionComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryVillageGrowthReactionComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryAreaAmbientComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryTimeLocationAmbientComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryVillageEventAmbientComment(playerEntityId, developmentLevel, timeOfDay))
            return;

        if (TryEmergentFlavorEvent(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryCommunityMoment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryGroupSocialMoment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryNpcToNpcAmbientComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryVillageGossipComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryAtmosphereComment(playerEntityId, playerX, playerZ, developmentLevel))
            return;

        if (TryLegacyRecognitionComment(playerEntityId, playerX, playerZ, developmentLevel))
            return;

        if (TryLegacyArchetypeIdentityComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryLongTermGoalMilestoneComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryRecurringSocialRoleComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryContextualSocialAmbientComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryVillageSocialStandingComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryVillageBondRecognitionComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryEmotionalBondAmbientComment(playerEntityId, playerX, playerZ, developmentLevel, timeOfDay))
            return;

        if (TryPersonalMomentComment(playerEntityId, playerX, playerZ, developmentLevel))
            return;

        TryMemoryAmbientComment(playerEntityId, playerX, playerZ, developmentLevel);
    }

    /// <summary>
    /// Overheard comment when the village has named the player's legacy archetype identity.
    /// </summary>
    private bool TryLegacyArchetypeIdentityComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_longTermGoalService is null)
            return false;

        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay + 41;
        if (!_longTermGoalService.TryGetLegacyIdentityAmbientFeedback(playerEntityId, seed, out var comment, out var speakerId))
            return false;

        if (speakerId != 0
            && TryFindVisibleNpcById(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                speakerId,
                out var speakerNpc,
                out var distance)
            && speakerNpc is not null)
        {
            var message = $"{speakerNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, speakerNpc.Npc.EntityId, message);
            Log.Information(
                "Legacy archetype identity ambient from {NpcName} to player {PlayerId} ({TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
                speakerNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                distance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Legacy archetype identity ambient (overheard) to player {PlayerId} ({TimeOfDay}): \"{Comment}\"",
            playerEntityId,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            comment);
        return true;
    }

    /// <summary>
    /// Village reaction when the player recently earned a long-term goal milestone.
    /// Runs after legacy recognition — meaningful but not as frequent as everyday chatter.
    /// </summary>
    private bool TryLongTermGoalMilestoneComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_longTermGoalService is null)
            return false;

        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay;
        if (!_longTermGoalService.TryGetMilestoneAmbientFeedback(playerEntityId, seed, out var comment, out var speakerId))
            return false;

        if (speakerId != 0
            && TryFindVisibleNpcById(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                speakerId,
                out var speakerNpc,
                out var distance)
            && speakerNpc is not null)
        {
            var message = $"{speakerNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, speakerNpc.Npc.EntityId, message);
            Log.Information(
                "Long-term goal milestone ambient from {NpcName} to player {PlayerId} ({TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
                speakerNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                distance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Long-term goal milestone ambient (overheard) to player {PlayerId} ({TimeOfDay}): \"{Comment}\"",
            playerEntityId,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            comment);
        return true;
    }

    /// <summary>
    /// Overheard comment about the player's dominant social role — warmer when GardenHelper,
    /// MarketHelper, WellKeeper, or AllRoundHelper is active. Runs after legacy recognition so both stay rare.
    /// </summary>
    private bool TryRecurringSocialRoleComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_communityReputationService is null)
            return false;

        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay;
        if (!_communityReputationService.TryGetAmbientRoleComment(playerEntityId, seed, out var comment))
            return false;

        if (TryFindVisibleNpcById(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                NpcEntityIds.Elsie,
                out var elsieNpc,
                out var elsieDistance)
            && elsieNpc is not null)
        {
            var message = $"{elsieNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, elsieNpc.Npc.EntityId, message);
            Log.Information(
                "Recurring social-role ambient from {NpcName} to player {PlayerId} ({TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
                elsieNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                elsieDistance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Recurring social-role ambient overheard by player {PlayerId} ({TimeOfDay}): \"{Comment}\"",
            playerEntityId,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            comment);
        return true;
    }

    /// <summary>
    /// Contextual ambient tied to player–NPC bond, social role, and recent village work.
    /// Warmer when Friend+ with Elsie, Mira, or Harold; references current village rhythm.
    /// </summary>
    private bool TryContextualSocialAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_socialDynamicsService is null || _communityReputationService is null)
            return false;

        if (!TryFindContextualSocialSpeaker(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                out var speakerNpc,
                out var speakerDistance,
                out var speakerTier))
        {
            return false;
        }

        var reputation = _communityReputationService.GetState(playerEntityId);
        var elsieTomRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;
        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        var seed = playerEntityId + (uint)timeOfDay + speakerNpc.Npc.EntityId + (uint)_worldTime.GameDay;

        if (!_socialDynamicsService.TryGetContextualAmbientComment(
                playerEntityId,
                speakerNpc.Npc.EntityId,
                speakerTier,
                reputation,
                elsieTomRelationship,
                timeOfDay,
                developmentLevel,
                completedProjects,
                seed,
                out var comment))
        {
            return false;
        }

        var message = $"{speakerNpc.Npc.Name} says: \"{comment}\"";
        _sendComment(playerEntityId, speakerNpc.Npc.EntityId, message);
        Log.Information(
            "Contextual social ambient from {NpcName} to player {PlayerId} (tier {Tier}, {TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
            speakerNpc.Npc.Name,
            playerEntityId,
            RelationshipTierDisplay.GetName(speakerTier),
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            speakerDistance,
            comment);
        return true;
    }

    private bool TryFindContextualSocialSpeaker(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        out NpcSimulationState speakerNpc,
        out float speakerDistance,
        out RelationshipTier speakerTier)
    {
        speakerNpc = null!;
        speakerDistance = float.MaxValue;
        speakerTier = RelationshipTier.Stranger;

        NpcSimulationState? best = null;
        var bestDistance = float.MaxValue;

        foreach (var npcId in new[] { NpcEntityIds.Elsie, NpcEntityIds.Mira, NpcEntityIds.Harold })
        {
            if (!TryFindVisibleNpcById(
                    playerEntityId,
                    playerX,
                    playerZ,
                    developmentLevel,
                    npcId,
                    out var candidate,
                    out var distance)
                || candidate is null)
            {
                continue;
            }

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = candidate;
        }

        if (best is null
            && !TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out best, out bestDistance))
        {
            return false;
        }

        if (best is null)
            return false;

        speakerNpc = best;
        speakerDistance = bestDistance;
        speakerTier = RelationshipTierCalculator.GetTier(
            _relationshipService.GetAffinity(playerEntityId, best.Npc.EntityId));
        return true;
    }

    /// <summary>
    /// Rare village-wide or elder-voice recognition of the player's legacy.
    /// Runs after atmosphere comments so it stays infrequent and feels special.
    /// </summary>
    private bool TryLegacyRecognitionComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel)
    {
        if (_legacyService is null)
            return false;

        var context = _legacyService.BuildContext(playerEntityId);
        if (!context.HasRecognition)
            return false;

        var elderNearby = TryFindVisibleNpcById(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            PlayerLegacyConfig.ElderVoiceNpcEntityId,
            out var elderNpc,
            out var elderDistance);

        var seed = playerEntityId + (uint)_worldTime.GameDay + (uint)developmentLevel;
        if (!_legacyService.TryGetAmbientRecognitionComment(
                playerEntityId,
                elderNearby,
                context,
                seed,
                out var comment,
                out var speakerEntityId))
        {
            return false;
        }

        if (speakerEntityId != 0 && elderNpc is not null)
        {
            SendComment(
                playerEntityId,
                elderNpc,
                comment,
                "Legacy recognition from {NpcName} to player {PlayerId} ({Distance:F1}m): \"{Comment}\"",
                elderDistance);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Legacy village-wide recognition to player {PlayerId}: \"{Comment}\"",
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>
    /// Rare overheard comment from ordinary villagers acknowledging the player's social standing
    /// from close focus bonds — warmer and more personal as standing rises.
    /// </summary>
    private bool TryVillageSocialStandingComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
        var tier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);

        if (!VillageSocialStandingConfig.IsEligibleForVillagerAmbientComment(tier))
            return false;

        if (!_memoryService.TryConsumeVillageSocialStandingAmbientCooldown(playerEntityId, tier))
            return false;

        var attempt = _villageSocialStandingAttemptCounters.TryGetValue(playerEntityId, out var current)
            ? current + 1
            : 1u;
        _villageSocialStandingAttemptCounters[playerEntityId] = attempt;

        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        if (!VillageSocialStandingConfig.ShouldTriggerVillagerAmbientComment(
                playerEntityId,
                tier,
                villageNoticed,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        var focusCloseFriends = VillageBondRecognitionConfig.GetFocusCloseFriendNpcIds(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay + 61;
        string? comment;
        if (SocialLegacyConfig.ShouldUseLegacyAmbientComment(
                playerEntityId,
                tier,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            comment = VillageSocialStandingConfig.TryGetLegacyAmbientComment(
                tier,
                focusCloseFriends,
                villageNoticed,
                seed);
        }
        else
        {
            comment = VillageSocialStandingConfig.TryGetVillagerAmbientComment(
                tier,
                focusCloseFriends,
                villageNoticed,
                seed);
        }

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var feedback = VillageSocialStandingConfig.FormatVillagerAmbientFeedback(tier);

        if (TryFindNearestVisibleOrdinaryVillager(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                tier,
                out var speakerNpc,
                out var speakerDistance)
            && speakerNpc is not null)
        {
            var directSpeech = VillageSocialStandingDialogue.FormatVillagerAmbientCommentForSpeaker(comment);
            var message = $"{feedback} {speakerNpc.Npc.Name} says: \"{directSpeech}\"";
            _sendComment(playerEntityId, speakerNpc.Npc.EntityId, message);
            Log.Information(
                "Village social standing ambient from {NpcName} to player {PlayerId} ({Tier}, {CloseFriends}, noticed={Noticed}, {TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
                speakerNpc.Npc.Name,
                playerEntityId,
                tier,
                focusCloseFriendCount,
                villageNoticed,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                speakerDistance,
                comment);
            return true;
        }

        var overheardMessage = $"{feedback} {comment}";
        _sendComment(playerEntityId, 0, overheardMessage);
        Log.Information(
            "Village social standing ambient ({Tier}, {CloseFriends}, noticed={Noticed}) to player {PlayerId} ({TimeOfDay}): \"{Comment}\"",
            tier,
            focusCloseFriendCount,
            villageNoticed,
            playerEntityId,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            comment);
        return true;
    }

    /// <summary>Nearest visible villager who is not a focus NPC — ordinary folk acknowledging social standing.</summary>
    private bool TryFindNearestVisibleOrdinaryVillager(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        VillageSocialStandingTier standingTier,
        out NpcSimulationState bestNpc,
        out float bestDistance)
    {
        bestNpc = null!;
        bestDistance = float.MaxValue;
        NpcSimulationState? candidate = null;

        var minTier = VillageAtmosphereConfig.GetMinAmbientCommentTier(developmentLevel);
        if (standingTier >= VillageSocialStandingTier.WellLiked && minTier > RelationshipTier.Acquaintance)
            minTier = RelationshipTier.Acquaintance;

        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (NpcEmotionalBondConfig.IsFocusNpc(simulation.Npc.EntityId))
                continue;

            var distance = NpcProximityDetector.GetDistance(
                playerX,
                playerZ,
                simulation.Npc.PositionX,
                simulation.Npc.PositionZ);

            if (distance > InteractionConfig.InteractionRadiusMeters)
                continue;

            if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
                continue;

            var relationshipTier = _relationshipService.GetTier(playerEntityId, simulation.Npc.EntityId);
            if (relationshipTier < minTier)
                continue;

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            candidate = simulation;
        }

        if (candidate is null)
            return false;

        bestNpc = candidate;
        return true;
    }

    /// <summary>
    /// Rare village-wide ambient recognition when the player has close bonds with focus NPCs.
    /// </summary>
    private bool TryVillageBondRecognitionComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));

        if (!VillageBondRecognitionConfig.IsEligibleForAmbientRecognition(focusCloseFriendCount))
            return false;

        if (!_memoryService.TryConsumeVillageBondRecognitionAmbientCooldown(
                playerEntityId,
                focusCloseFriendCount))
            return false;

        var attempt = _villageBondRecognitionAttemptCounters.TryGetValue(playerEntityId, out var current)
            ? current + 1
            : 1u;
        _villageBondRecognitionAttemptCounters[playerEntityId] = attempt;

        if (!VillageBondRecognitionConfig.ShouldTriggerAmbientRecognition(
                playerEntityId,
                focusCloseFriendCount,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        var focusCloseFriends = VillageBondRecognitionConfig.GetFocusCloseFriendNpcIds(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay + 57;
        var comment = VillageBondRecognitionConfig.TryGetVillageAmbientComment(
            focusCloseFriends,
            villageNoticed,
            seed);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var feedback = VillageBondRecognitionConfig.FormatAmbientRecognitionFeedback(focusCloseFriends);

        if (TryFindNearestVisibleNpc(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                out var speakerNpc,
                out var speakerDistance)
            && speakerNpc is not null)
        {
            var message = $"{feedback} {speakerNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, speakerNpc.Npc.EntityId, message);
            Log.Information(
                "Village bond recognition ambient from {NpcName} to player {PlayerId} ({TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
                speakerNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                speakerDistance,
                comment);
            return true;
        }

        var overheardMessage = $"{feedback} (Overheard nearby:) \"{comment}\"";
        _sendComment(playerEntityId, 0, overheardMessage);
        Log.Information(
            "Village bond recognition ambient overheard by player {PlayerId} ({TimeOfDay}): \"{Comment}\"",
            playerEntityId,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            comment);
        return true;
    }

    /// <summary>
    /// Rare emotional bond ambient from Elsie or Harold — personal warmth tied to memory and legacy archetype.
    /// </summary>
    private bool TryEmotionalBondAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_longTermGoalService is null)
            return false;

        if (!TryFindNearestFocusNpcWithEmotionalBond(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                out var focusNpc,
                out var distance))
        {
            return false;
        }

        if (focusNpc is null)
            return false;

        var npcEntityId = focusNpc.Npc.EntityId;
        var tier = _relationshipService.GetTier(playerEntityId, npcEntityId);
        if (tier < NpcEmotionalBondConfig.MinEmotionalInteractionTier)
            return false;

        if (!_memoryService.TryConsumeEmotionalAmbientCooldown(playerEntityId, npcEntityId))
            return false;

        if (!NpcEmotionalBondConfig.ShouldTriggerEmotionalAmbient(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var memories = _memoryService.GetMemoriesForNpc(playerEntityId, npcEntityId);
        var archetype = _longTermGoalService.GetLegacyArchetype(playerEntityId);
        var seed = playerEntityId + npcEntityId + (uint)timeOfDay + (uint)_worldTime.GameDay + 53;
        var comment = NpcEmotionalBondConfig.TryGetEmotionalAmbientLine(
            npcEntityId,
            memories,
            archetype,
            seed);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        SendComment(
            playerEntityId,
            focusNpc,
            comment,
            "Emotional bond ambient from {NpcName} to player {PlayerId} ({TimeOfDay}, {Distance:F1}m): \"{Comment}\"",
            distance,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay));

        return true;
    }

    private bool TryFindNearestFocusNpcWithEmotionalBond(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        out NpcSimulationState? focusNpc,
        out float distance)
    {
        focusNpc = null;
        distance = float.MaxValue;

        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (!NpcEmotionalBondConfig.IsFocusNpc(simulation.Npc.EntityId))
                continue;

            if (!TryFindVisibleNpcById(
                    playerEntityId,
                    playerX,
                    playerZ,
                    developmentLevel,
                    simulation.Npc.EntityId,
                    out var visibleNpc,
                    out var npcDistance)
                || visibleNpc is null)
            {
                continue;
            }

            var memories = _memoryService.GetMemoriesForNpc(playerEntityId, simulation.Npc.EntityId);
            if (!NpcEmotionalBondConfig.HasEmotionalMemory(simulation.Npc.EntityId, memories))
                continue;

            if (npcDistance >= distance)
                continue;

            focusNpc = visibleNpc;
            distance = npcDistance;
        }

        return focusNpc is not null;
    }

    /// <summary>Rare personal moment when a nearby NPC shares warm memories with the player.</summary>
    private bool TryPersonalMomentComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel)
    {
        if (!TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var bestNpc, out var bestNpcDistance))
            return false;

        var npcEntityId = bestNpc.Npc.EntityId;
        var tier = _relationshipService.GetTier(playerEntityId, npcEntityId);
        if (tier < RelationshipTier.Friend)
            return false;

        var memories = _memoryService.GetMemoriesForNpc(playerEntityId, npcEntityId);
        if (memories.Count == 0)
            return false;

        if (!_memoryService.TryConsumePersonalMomentCooldown(playerEntityId, npcEntityId))
            return false;

        if (!NpcMemoryConfig.ShouldTriggerPersonalMoment(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var seed = playerEntityId + npcEntityId + (uint)_worldTime.GameDay;
        var comment = NpcMemoryConfig.TryGetPersonalMoment(memories, tier, bestNpc.Npc.Name, seed);
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        SendComment(
            playerEntityId,
            bestNpc,
            comment,
            "Personal moment from {NpcName} to player {PlayerId} (tier {Tier}, {Distance:F1}m): \"{Comment}\"",
            bestNpcDistance,
            RelationshipTierDisplay.GetName(tier));

        return true;
    }

    /// <summary>Area-specific flavor with time-of-day variation while in an unlocked village area.</summary>
    private bool TryAreaAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_areaService is null)
            return false;

        if (!_areaService.TryGetUnlockedAreaAtPosition(playerX, playerZ, out var area))
            return false;

        if (!_areaService.TryConsumeAreaAmbientCommentCooldown(playerEntityId, area))
            return false;

        var location = MapAreaToAmbientLocation(area);
        var seed = playerEntityId + (uint)area + (uint)timeOfDay + (uint)_worldTime.GameDay;
        var comment = VillageLifeDialogue.TryGetLocationTimeComment(location, timeOfDay, developmentLevel, seed);
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        return SendLocationFlavor(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            comment,
            "Area ambient",
            VillageAreaConfig.TryGet(area, out var definition) ? definition.Name : area.ToString(),
            timeOfDay);
    }

    /// <summary>Time- and location-aware flavor for project sites and general village wandering.</summary>
    private bool TryTimeLocationAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_timeLocationCooldowns, playerEntityId, VillageLifeConfig.TimeLocationCommentCooldown))
            return false;

        var unlockedAreas = GetUnlockedAreaSet();
        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        VillageLifeConfig.TryResolveAmbientLocation(
            playerX,
            playerZ,
            unlockedAreas,
            completedProjects,
            out var location,
            out _);

        var seed = playerEntityId + (uint)location + (uint)timeOfDay + (uint)_worldTime.GameMinute;
        var comment = VillageLifeDialogue.TryGetLocationTimeComment(location, timeOfDay, developmentLevel, seed);
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        return SendLocationFlavor(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            comment,
            "Time-location ambient",
            VillageLifeConfig.GetLocationDisplayName(location),
            timeOfDay);
    }

    /// <summary>Scheduled village event ambient — Market Day or Community Work Day.</summary>
    private bool TryVillageEventAmbientComment(
        uint playerEntityId,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!VillageEventConfig.HasActiveEvent(_worldTime.GameDay))
            return false;

        if (!TryConsumeCooldown(_villageEventAmbientCooldowns, playerEntityId, VillageLifeConfig.EmergentEventCooldown))
            return false;

        var attempt = _villageEventAmbientAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _villageEventAmbientAttemptCounters[playerEntityId] = attempt;

        if (!VillageEventConfig.ShouldTriggerEventAmbientComment(
                playerEntityId,
                _worldTime.GameDay,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        var seed = playerEntityId + (uint)timeOfDay + attempt + (uint)_worldTime.GameDay + 61;
        var comment = NpcResponseGenerator.TryGetVillageEventAmbientComment(_worldTime.GameDay, seed);
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Village event ambient ({Events}, {TimeOfDay}, {Development}) to player {PlayerId}: \"{Comment}\"",
            string.Join(", ", VillageEventConfig.GetActiveEvents(_worldTime.GameDay)),
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            developmentLevel,
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>Rare emergent flavor events — resting villagers, distant chatter, village mood shifts.</summary>
    private bool TryEmergentFlavorEvent(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_emergentEventCooldowns, playerEntityId, VillageLifeConfig.EmergentEventCooldown))
            return false;

        var attempt = _emergentAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _emergentAttemptCounters[playerEntityId] = attempt;

        if (!VillageLifeConfig.ShouldTriggerEmergentEvent(playerEntityId, _worldTime.TotalGameMinutes, attempt))
            return false;

        _interpersonalService?.TryEvolveFromTime(_projectStateService);
        var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;

        var seed = playerEntityId + (uint)timeOfDay + attempt + (uint)_worldTime.GameDay;
        var comment = VillageLifeDialogue.TryGetEmergentEvent(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            seed,
            out var eventKind);
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Emergent village event ({EventKind}, {TimeOfDay}, {Relationship}) to player {PlayerId}: \"{Comment}\"",
            eventKind,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship),
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>
    /// Rare community moments — warm gatherings, small preparations, distant neighbor clusters.
    /// Shaped by Elsie–Tom relationship tone and village development level.
    /// </summary>
    private bool TryCommunityMoment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_communityMomentCooldowns, playerEntityId, VillageSocialFabricConfig.CommunityMomentCooldown))
            return false;

        var attempt = _communityMomentAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _communityMomentAttemptCounters[playerEntityId] = attempt;

        if (!VillageSocialFabricConfig.ShouldTriggerCommunityMoment(
                playerEntityId,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        _interpersonalService?.TryEvolveFromTime(_projectStateService);
        var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;

        var seed = playerEntityId + (uint)timeOfDay + attempt + (uint)developmentLevel;

        // Alternate between fabric community moments and Elsie–Tom emergent social glimpses.
        string? comment;
        VillageCommunityMomentKind momentKind;
        if (seed % 3 == 0)
        {
            comment = VillageCommunityLifeConfig.TryGetEmergentSocialMoment(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship,
                seed,
                out momentKind);
        }
        else
        {
            comment = VillageSocialFabricConfig.TryGetCommunityMoment(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship,
                seed,
                out momentKind);
        }

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Community moment ({MomentKind}, {TimeOfDay}, {Relationship}, {DevelopmentLevel}) to player {PlayerId}: \"{Comment}\"",
            VillageSocialFabricConfig.GetCommunityMomentDisplayName(momentKind),
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship),
            VillageAtmosphereConfig.GetDisplayName(developmentLevel),
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>
    /// Rare group social moment — three or more villagers (Elsie, Tom, Mira, Harold) in one overheard scene.
    /// Kept separate from pair chatter so multi-NPC flavor stays distinct and infrequent.
    /// </summary>
    private bool TryGroupSocialMoment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_groupSocialMomentCooldowns, playerEntityId, VillageSocialCircleConfig.GroupSocialMomentCooldown))
            return false;

        var attempt = _groupSocialMomentAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _groupSocialMomentAttemptCounters[playerEntityId] = attempt;

        if (!VillageSocialCircleConfig.ShouldTriggerGroupSocialMoment(
                playerEntityId,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        _interpersonalService?.TryEvolveFromTime(_projectStateService);
        var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;

        var seed = playerEntityId + (uint)timeOfDay + attempt + (uint)developmentLevel + (uint)_worldTime.GameDay;
        var comment = VillageSocialCircleConfig.TryGetGroupSocialMoment(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            seed,
            out var momentKind);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Group social moment ({MomentKind}, {TimeOfDay}, {Relationship}, {DevelopmentLevel}) to player {PlayerId}: \"{Comment}\"",
            VillageSocialFabricConfig.GetCommunityMomentDisplayName(momentKind),
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship),
            VillageAtmosphereConfig.GetDisplayName(developmentLevel),
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>
    /// NPC-to-NPC social flavor — villagers who know each other, shaped by completed projects
    /// and development level. Delivered by the preferred speaker when nearby.
    /// </summary>
    private bool TryNpcToNpcAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_npcToNpcCooldowns, playerEntityId, VillageCommunityLifeConfig.NpcToNpcCommentCooldown))
            return false;

        if (!VillageCommunityLifeConfig.ShouldTriggerNpcToNpcComment(playerEntityId, _worldTime.TotalGameMinutes))
            return false;

        _interpersonalService?.TryEvolveFromTime(_projectStateService);
        var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;

        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        var seed = playerEntityId + (uint)timeOfDay + (uint)developmentLevel + (uint)_worldTime.GameDay;
        var comment = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            timeOfDay,
            developmentLevel,
            completedProjects,
            interpersonalRelationship,
            seed,
            out var preferredSpeakerNpcEntityId,
            out var socialPair);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var projectLabel = VillageCommunityLifeConfig.TryGetPrimaryCompletedProject(completedProjects);
        var relationshipLabel = NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship);
        var pairLabel = socialPair is null
            ? "Elsie–Tom"
            : VillageSocialCircleConfig.GetSocialPairDisplayName(socialPair.Value);
        return DeliverSocialAmbientComment(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            timeOfDay,
            comment,
            preferredSpeakerNpcEntityId,
            logKind: "NPC-to-NPC social",
            contextLabel: projectLabel is null
                ? $"{pairLabel}, {relationshipLabel}, {VillageAtmosphereConfig.GetDisplayName(developmentLevel)}"
                : $"{pairLabel}, {relationshipLabel}, {VillageAtmosphereConfig.GetDisplayName(developmentLevel)}, project {VillageProjectBenefitConfig.FormatProjectDisplayName(projectLabel.Value)}");
    }

    /// <summary>Rare overheard village gossip — project-aware and development-sensitive flavor only.</summary>
    private bool TryVillageGossipComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (!TryConsumeCooldown(_villageGossipCooldowns, playerEntityId, VillageCommunityLifeConfig.VillageGossipCooldown))
            return false;

        var attempt = _villageGossipAttemptCounters.TryGetValue(playerEntityId, out var count) ? count + 1 : 1u;
        _villageGossipAttemptCounters[playerEntityId] = attempt;

        if (!VillageCommunityLifeConfig.ShouldTriggerVillageGossip(
                playerEntityId,
                _worldTime.TotalGameMinutes,
                attempt))
        {
            return false;
        }

        _interpersonalService?.TryEvolveFromTime(_projectStateService);
        var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
            ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;
        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        var seed = playerEntityId + (uint)timeOfDay + attempt + (uint)developmentLevel;
        var comment = VillageCommunityLifeConfig.TryGetVillageGossip(
            timeOfDay,
            developmentLevel,
            completedProjects,
            interpersonalRelationship,
            seed,
            out var gossipKind,
            out var socialPair);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var pairLabel = socialPair is null
            ? string.Empty
            : $", {VillageSocialCircleConfig.GetSocialPairDisplayName(socialPair.Value)}";

        if (TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var relayNpc, out var relayDistance))
        {
            var message = $"{relayNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, relayNpc.Npc.EntityId, message);
            Log.Information(
                "Village gossip ({GossipKind}{PairLabel}, {Relationship}) relayed by {NpcName} to player {PlayerId} ({TimeOfDay}, {DevelopmentLevel}, {Distance:F1}m): \"{Comment}\"",
                VillageCommunityLifeConfig.GetGossipKindDisplayName(gossipKind),
                pairLabel,
                NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship),
                relayNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                VillageAtmosphereConfig.GetDisplayName(developmentLevel),
                relayDistance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "Village gossip ({GossipKind}{PairLabel}, {Relationship}, {TimeOfDay}, {DevelopmentLevel}) overheard by player {PlayerId}: \"{Comment}\"",
            VillageCommunityLifeConfig.GetGossipKindDisplayName(gossipKind),
            pairLabel,
            NpcInterpersonalRelationshipConfig.GetDisplayName(interpersonalRelationship),
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            VillageAtmosphereConfig.GetDisplayName(developmentLevel),
            playerEntityId,
            comment);
        return true;
    }

    private bool DeliverSocialAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay,
        string comment,
        uint preferredSpeakerNpcEntityId,
        string logKind,
        string contextLabel)
    {
        if (VillageCommunityLifeConfig.IsKnownVillager(preferredSpeakerNpcEntityId)
            && TryFindVisibleNpcById(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                preferredSpeakerNpcEntityId,
                out var preferredNpc,
                out var preferredDistance)
            && preferredNpc is not null)
        {
            var message = $"{preferredNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, preferredNpc.Npc.EntityId, message);
            Log.Information(
                "{LogKind} from {NpcName} to player {PlayerId} ({TimeOfDay}, {ContextLabel}, {Distance:F1}m): \"{Comment}\"",
                logKind,
                preferredNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                contextLabel,
                preferredDistance,
                comment);
            return true;
        }

        if (TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var nearestNpc, out var nearestDistance))
        {
            var message = $"{nearestNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, nearestNpc.Npc.EntityId, message);
            Log.Information(
                "{LogKind} from {NpcName} to player {PlayerId} ({TimeOfDay}, {ContextLabel}, {Distance:F1}m): \"{Comment}\"",
                logKind,
                nearestNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                contextLabel,
                nearestDistance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "{LogKind} ({TimeOfDay}, {ContextLabel}) to player {PlayerId}: \"{Comment}\"",
            logKind,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            contextLabel,
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>
    /// NPC remarks near a completed project site — location feedback shifts with development level.
    /// </summary>
    private bool TryProjectSiteComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_projectStateService is null)
            return false;

        if (!_projectStateService.TryGetNearestCompletedSite(playerX, playerZ, out var projectId, out var distance))
            return false;

        if (!_projectStateService.TryConsumeProjectSiteCommentCooldown(playerEntityId, projectId))
            return false;

        var seed = playerEntityId + projectId + (uint)timeOfDay + (uint)developmentLevel;
        var siteLocation = MapProjectToAmbientLocation(projectId);
        var comment = VillageReactivityConfig.TryGetProjectSiteGrowthComment(
                          projectId,
                          developmentLevel,
                          timeOfDay,
                          seed)
                      ?? VillageLifeDialogue.TryGetLocationTimeComment(siteLocation, timeOfDay, developmentLevel, seed)
                      ?? VillageProjectNpcDialogue.TryGetSiteAmbientComment(
                          projectId,
                          developmentLevel,
                          seed);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var preferredSpeaker = VillageReactivityConfig.SelectProjectReactionSpeaker(projectId, seed);
        return DeliverGrowthReactionComment(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            timeOfDay,
            comment,
            preferredSpeaker,
            logKind: "Project-site growth",
            contextLabel: $"{VillageAtmosphereConfig.GetDisplayName(developmentLevel)}, {VillageProjectBenefitConfig.FormatProjectDisplayName(projectId)} ({distance:F1}m)");
    }

    /// <summary>
    /// Boosted reactions shortly after a major project completes — Elsie or Tom may comment anywhere.
    /// </summary>
    private bool TryProjectCompletionReactionComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_projectStateService is null || _reactivityService is null)
            return false;

        if (!TryConsumeCooldown(
                _projectCompletionReactionCooldowns,
                playerEntityId,
                VillageReactivityConfig.ProjectCompletionReactionCooldown))
        {
            return false;
        }

        if (!_reactivityService.TryGetMostRecentReactionProject(out var projectId))
            return false;

        if (!VillageReactivityConfig.ShouldTriggerProjectCompletionReaction(
                playerEntityId,
                projectId,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var seed = playerEntityId + projectId + (uint)_worldTime.GameDay;
        var comment = VillageReactivityConfig.TryGetProjectCompletionReaction(
            projectId,
            developmentLevel,
            seed,
            out var preferredSpeaker);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        return DeliverGrowthReactionComment(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            timeOfDay,
            comment,
            preferredSpeaker,
            logKind: "Project completion reaction",
            contextLabel: $"{VillageProjectBenefitConfig.FormatProjectDisplayName(projectId)}, {VillageAtmosphereConfig.GetDisplayName(developmentLevel)}");
    }

    /// <summary>
    /// Village-wide growth reactions when atmosphere is Lively or Bustling.
    /// Chance rises with development level so growth feels more visible, not more noisy.
    /// </summary>
    private bool TryVillageGrowthReactionComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        if (_projectStateService is null || developmentLevel == VillageDevelopmentLevel.Quiet)
            return false;

        if (!TryConsumeCooldown(
                _growthReactionCooldowns,
                playerEntityId,
                VillageReactivityConfig.GrowthReactionCooldown))
        {
            return false;
        }

        if (!VillageReactivityConfig.ShouldTriggerGrowthReaction(
                playerEntityId,
                developmentLevel,
                _worldTime.TotalGameMinutes))
        {
            return false;
        }

        var completedProjects = _projectStateService.GetCompletedProjectIds();
        var seed = playerEntityId + (uint)developmentLevel + (uint)timeOfDay;
        var comment = VillageReactivityConfig.TryGetGrowthReactionComment(
            developmentLevel,
            completedProjects,
            seed);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        var preferredSpeaker = VillageReactivityConfig.SelectProjectReactionSpeaker(
            completedProjects.FirstOrDefault(),
            seed);

        return DeliverGrowthReactionComment(
            playerEntityId,
            playerX,
            playerZ,
            developmentLevel,
            timeOfDay,
            comment,
            preferredSpeaker,
            logKind: "Village growth reaction",
            contextLabel: VillageAtmosphereConfig.GetDisplayName(developmentLevel));
    }

    private bool DeliverGrowthReactionComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay,
        string comment,
        uint preferredSpeakerNpcEntityId,
        string logKind,
        string contextLabel)
    {
        if (TryFindVisibleNpcById(
                playerEntityId,
                playerX,
                playerZ,
                developmentLevel,
                preferredSpeakerNpcEntityId,
                out var preferredNpc,
                out var preferredDistance)
            && preferredNpc is not null)
        {
            var message = $"{preferredNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, preferredNpc.Npc.EntityId, message);
            Log.Information(
                "{LogKind} from {NpcName} to player {PlayerId} ({TimeOfDay}, {ContextLabel}, {Distance:F1}m): \"{Comment}\"",
                logKind,
                preferredNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                contextLabel,
                preferredDistance,
                comment);
            return true;
        }

        if (TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var nearestNpc, out var nearestDistance))
        {
            var message = $"{nearestNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, nearestNpc.Npc.EntityId, message);
            Log.Information(
                "{LogKind} from {NpcName} to player {PlayerId} ({TimeOfDay}, {ContextLabel}, {Distance:F1}m): \"{Comment}\"",
                logKind,
                nearestNpc.Npc.Name,
                playerEntityId,
                VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
                contextLabel,
                nearestDistance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "{LogKind} ({TimeOfDay}, {ContextLabel}) to player {PlayerId}: \"{Comment}\"",
            logKind,
            VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay),
            contextLabel,
            playerEntityId,
            comment);
        return true;
    }

    /// <summary>General village-growth flavor when the atmosphere is Lively or Bustling.</summary>
    private bool TryAtmosphereComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel)
    {
        if (_projectStateService is null || developmentLevel == VillageDevelopmentLevel.Quiet)
            return false;

        if (!_projectStateService.TryConsumeAtmosphereCommentCooldown(playerEntityId))
            return false;

        if (!TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var bestNpc, out var bestNpcDistance))
            return false;

        var seed = playerEntityId + (uint)developmentLevel;

        var comment = seed % 2 == 0
            ? VillageAtmosphereConfig.TryGetLevelExclusiveAmbientComment(developmentLevel, seed)
            : null;

        comment ??= VillageAtmosphereConfig.TryGetGeneralAmbientComment(
            developmentLevel,
            _projectStateService.GetCompletedProjectIds(),
            seed);

        if (string.IsNullOrWhiteSpace(comment))
            return false;

        SendComment(
            playerEntityId,
            bestNpc,
            comment,
            "Atmosphere NPC comment from {NpcName} to player {PlayerId} ({DevelopmentLevel}, {Distance:F1}m): \"{Comment}\"",
            bestNpcDistance,
            VillageAtmosphereConfig.GetDisplayName(developmentLevel));

        return true;
    }

    private void TryMemoryAmbientComment(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel)
    {
        if (!TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var bestNpc, out var bestNpcDistance))
            return;

        var npcEntityId = bestNpc.Npc.EntityId;
        if (!_memoryService.TryConsumeAmbientCommentCooldown(playerEntityId, npcEntityId))
            return;

        var memories = _memoryService.GetMemoriesForNpc(playerEntityId, npcEntityId);
        var tier = _relationshipService.GetTier(playerEntityId, npcEntityId);
        var comment = NpcMemoryConfig.TryGetAmbientComment(memories, tier, playerEntityId + npcEntityId);
        if (string.IsNullOrWhiteSpace(comment))
            return;

        SendComment(
            playerEntityId,
            bestNpc,
            comment,
            "Ambient NPC comment from {NpcName} to player {PlayerId} (tier {Tier}, {Distance:F1}m): \"{Comment}\"",
            bestNpcDistance,
            RelationshipTierDisplay.GetName(tier));
    }

    private bool SendLocationFlavor(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        string comment,
        string logKind,
        string locationLabel,
        GameTimeOfDay timeOfDay)
    {
        var timeLabel = VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay);

        if (TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, developmentLevel, out var bestNpc, out var bestNpcDistance))
        {
            var message = $"{bestNpc.Npc.Name} says: \"{comment}\"";
            _sendComment(playerEntityId, bestNpc.Npc.EntityId, message);

            Log.Information(
                "{LogKind} from {NpcName} to player {PlayerId} ({TimeLabel} at {LocationLabel}, {Distance:F1}m): \"{Comment}\"",
                logKind,
                bestNpc.Npc.Name,
                playerEntityId,
                timeLabel,
                locationLabel,
                bestNpcDistance,
                comment);
            return true;
        }

        _sendComment(playerEntityId, 0, comment);
        Log.Information(
            "{LogKind} to player {PlayerId} ({TimeLabel} at {LocationLabel}): \"{Comment}\"",
            logKind,
            playerEntityId,
            timeLabel,
            locationLabel,
            comment);
        return true;
    }

    private HashSet<VillageArea> GetUnlockedAreaSet()
    {
        if (_areaService is null)
            return new HashSet<VillageArea>();

        return _areaService.GetUnlockedAreas().ToHashSet();
    }

    private static bool TryConsumeCooldown(
        Dictionary<uint, DateTime> cooldowns,
        uint playerEntityId,
        TimeSpan cooldown)
    {
        if (cooldowns.TryGetValue(playerEntityId, out var lastUsed) && DateTime.UtcNow - lastUsed < cooldown)
            return false;

        cooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    private static VillageAmbientLocation MapAreaToAmbientLocation(VillageArea area) =>
        area switch
        {
            VillageArea.MarketSquare => VillageAmbientLocation.MarketSquare,
            VillageArea.CommunityGarden => VillageAmbientLocation.CommunityGarden,
            VillageArea.RiversideWalk => VillageAmbientLocation.RiversideWalk,
            _ => VillageAmbientLocation.General,
        };

    private static VillageAmbientLocation MapProjectToAmbientLocation(byte projectId) =>
        projectId switch
        {
            VillageSiteIds.Well => VillageAmbientLocation.VillageWell,
            VillageSiteIds.Bridge => VillageAmbientLocation.RepairedBridge,
            VillageSiteIds.Warehouse => VillageAmbientLocation.VillageWarehouse,
            _ => VillageAmbientLocation.General,
        };

    private bool TryFindVisibleNpcById(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        uint npcEntityId,
        out NpcSimulationState? npc,
        out float distance)
    {
        npc = null;
        distance = float.MaxValue;

        var simulation = _npcManager.GetState(npcEntityId);
        if (simulation is null)
            return false;

        distance = NpcProximityDetector.GetDistance(
            playerX,
            playerZ,
            simulation.Npc.PositionX,
            simulation.Npc.PositionZ);

        if (distance > InteractionConfig.InteractionRadiusMeters)
            return false;

        if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
            return false;

        var minTier = VillageAtmosphereConfig.GetMinAmbientCommentTier(developmentLevel);
        var relationshipTier = _relationshipService.GetTier(playerEntityId, simulation.Npc.EntityId);
        if (relationshipTier < minTier)
            return false;

        npc = simulation;
        return true;
    }

    private bool TryFindNearestVisibleNpc(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageDevelopmentLevel developmentLevel,
        out NpcSimulationState bestNpc,
        out float bestDistance)
    {
        bestNpc = null!;
        bestDistance = float.MaxValue;
        NpcSimulationState? candidate = null;

        var minTier = VillageAtmosphereConfig.GetMinAmbientCommentTier(developmentLevel);

        foreach (var simulation in _npcManager.SimulationStates)
        {
            var distance = NpcProximityDetector.GetDistance(
                playerX,
                playerZ,
                simulation.Npc.PositionX,
                simulation.Npc.PositionZ);

            if (distance > InteractionConfig.InteractionRadiusMeters)
                continue;

            if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
                continue;

            var relationshipTier = _relationshipService.GetTier(playerEntityId, simulation.Npc.EntityId);
            if (relationshipTier < minTier)
                continue;

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            candidate = simulation;
        }

        if (candidate is null)
            return false;

        bestNpc = candidate;
        return true;
    }

    private void SendComment(
        uint playerEntityId,
        NpcSimulationState npc,
        string comment,
        string logTemplate,
        float distance,
        string? extraLabel = null)
    {
        var message = $"{npc.Npc.Name} says: \"{comment}\"";
        _sendComment(playerEntityId, npc.Npc.EntityId, message);

        if (extraLabel is null)
        {
            Log.Information(
                logTemplate,
                npc.Npc.Name,
                playerEntityId,
                distance,
                comment);
            return;
        }

        Log.Information(
            logTemplate,
            npc.Npc.Name,
            playerEntityId,
            extraLabel,
            distance,
            comment);
    }
}