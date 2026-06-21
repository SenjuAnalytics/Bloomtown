using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Legacy;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc.Interaction;

/// <summary>
/// Validates proximity and AOI, resolves interactions, and applies social/affinity effects.
/// </summary>
public sealed class NpcInteractionHandler
{
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly NpcMemoryService _memoryService;
    private readonly VillageProjectStateService? _projectStateService;
    private readonly PlayerLegacyService? _legacyService;
    private readonly CommunityReputationService? _communityReputationService;
    private readonly SocialDynamicsService? _socialDynamicsService;
    private readonly PlayerLongTermGoalService? _longTermGoalService;
    private readonly PlayerMilestoneService? _milestoneService;
    private readonly NpcEmotionalBondService? _emotionalBondService;
    private readonly WorldTimeSystem? _worldTime;

    public NpcInteractionHandler(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        PlayerNpcRelationshipService relationshipService,
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        NpcMemoryService memoryService,
        VillageProjectStateService? projectStateService = null,
        PlayerLegacyService? legacyService = null,
        CommunityReputationService? communityReputationService = null,
        SocialDynamicsService? socialDynamicsService = null,
        PlayerLongTermGoalService? longTermGoalService = null,
        NpcEmotionalBondService? emotionalBondService = null,
        WorldTimeSystem? worldTime = null,
        PlayerMilestoneService? milestoneService = null)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _relationshipService = relationshipService;
        _economyService = economyService;
        _needsService = needsService;
        _memoryService = memoryService;
        _projectStateService = projectStateService;
        _legacyService = legacyService;
        _communityReputationService = communityReputationService;
        _socialDynamicsService = socialDynamicsService;
        _longTermGoalService = longTermGoalService;
        _emotionalBondService = emotionalBondService;
        _worldTime = worldTime;
        _milestoneService = milestoneService;
    }

    public NpcInteractionResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        NpcInteractionRequest request)
    {
        if (request.Kind is not (NpcInteractionKind.Greet or NpcInteractionKind.Talk))
        {
            return LogAndFail(
                playerEntityId,
                request.Kind,
                0,
                null,
                NpcInteractionFailureReason.UnknownInteraction,
                "Unknown interaction type.");
        }

        var hasTarget = request.TargetNpcEntityId != 0;
        var targetLabel = hasTarget
            ? NpcNameLookup.GetDisplayNameOrDefault(request.TargetNpcEntityId)
            : null;

        NpcSimulationState? simulation = hasTarget
            ? _npcManager.GetState(request.TargetNpcEntityId)
            : NpcProximityDetector.FindNearestInRange(
                playerX,
                playerZ,
                _npcManager.SimulationStates,
                InteractionConfig.InteractionRadiusMeters);

        if (simulation is null)
        {
            if (hasTarget)
            {
                return LogAndFail(
                    playerEntityId,
                    request.Kind,
                    request.TargetNpcEntityId,
                    targetLabel,
                    NpcInteractionFailureReason.InvalidTarget,
                    $"NPC '{targetLabel}' was not found. Known NPCs: {NpcNameLookup.KnownNamesList}.");
            }

            return LogAndFail(
                playerEntityId,
                request.Kind,
                0,
                null,
                NpcInteractionFailureReason.NoNpcNearby,
                $"No NPC is within {InteractionConfig.InteractionRadiusMeters:F0}m. Move closer to an NPC.");
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
            return LogAndFail(
                playerEntityId,
                request.Kind,
                simulation.Npc.EntityId,
                npcName,
                NpcInteractionFailureReason.TooFar,
                $"{npcName} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m to interact.");
        }

        if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
        {
            return LogAndFail(
                playerEntityId,
                request.Kind,
                simulation.Npc.EntityId,
                npcName,
                NpcInteractionFailureReason.NotInAoi,
                $"{npcName} is not in your area. Walk closer until they appear in your AOI.");
        }

        var socialBefore = simulation.Needs.Social;
        ApplyInteractionEffects(simulation, request.Kind);
        var socialAfter = simulation.Needs.Social;

        var standingTierBefore = VillageSocialStandingConfig.ResolveTier(
            VillageBondRecognitionConfig.CountFocusCloseFriends(
                id => _relationshipService.GetTier(playerEntityId, id)));

        var affinityChange = _relationshipService
            .ApplyInteractionGainAsync(playerEntityId, simulation.Npc.EntityId, request.Kind)
            .GetAwaiter()
            .GetResult();

        var flavorSeed = playerEntityId + simulation.Npc.EntityId;
        _longTermGoalService?.RecordNpcInteractionAndReconcileAsync(playerEntityId, flavorSeed).GetAwaiter().GetResult();
        _milestoneService?.ReconcileAsync(playerEntityId).GetAwaiter().GetResult();

        var newCompanionMemory = _memoryService
            .OnFocusNpcInteractionAsync(playerEntityId, simulation.Npc.EntityId)
            .GetAwaiter()
            .GetResult();
        if (newCompanionMemory is not null)
        {
            Log.Information(
                "Player {PlayerId} earned emotional companion memory {Memory} with {NpcName}.",
                playerEntityId,
                newCompanionMemory,
                npcName);
        }

        TryRecordVillageBondRecognitionMemory(playerEntityId);

        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);
        var villageNoticedStanding = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);

        var reputationState = _communityReputationService?.GetState(playerEntityId)
            ?? CommunityReputationConfig.CreateEmpty();
        var socialRole = CommunityReputationConfig.GetDominantSocialRole(reputationState);
        var (moodBonus, socialBonus) = SocialDynamicsConfig.GetInteractionBonus(
            affinityChange.NewTier,
            socialRole);

        string? standingRecoveryFeedback = null;
        if (_economyService.TryGetState(playerEntityId, out var economy))
        {
            if (request.Kind == NpcInteractionKind.Talk)
                _needsService.ApplyTalk(economy);
            else if (request.Kind == NpcInteractionKind.Greet)
                _needsService.ApplyGreet(economy);

            if (SocialDynamicsConfig.QualifiesForBetterTreatment(affinityChange.NewTier, socialRole))
                _needsService.ApplySocialInteractionBonus(economy, moodBonus, socialBonus);

            if (NpcEmotionalBondConfig.IsFocusNpc(simulation.Npc.EntityId)
                && VillageSocialStandingImpactConfig.IsEligibleForFocusNpcBonus(standingTier))
            {
                var (standingMood, standingSocial) = VillageSocialStandingImpactConfig.GetInteractionBonus(
                    standingTier,
                    simulation.Npc.EntityId);
                if (standingMood > 0f || standingSocial > 0f)
                {
                    _needsService.ApplySocialInteractionBonus(economy, standingMood, standingSocial);
                    standingRecoveryFeedback = VillageSocialStandingImpactConfig.FormatStandingRecoveryFeedback(
                        npcName,
                        standingMood,
                        standingSocial);
                }
            }

            _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        }

        var memories = _memoryService.GetMemoriesForNpc(playerEntityId, simulation.Npc.EntityId);
        var legacyArchetype = _longTermGoalService?.GetLegacyArchetype(playerEntityId) ?? LegacyArchetype.None;
        var developmentLevel = _projectStateService?.DevelopmentLevel ?? VillageDevelopmentLevel.Quiet;
        var completedProjects = _projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        var timeOfDay = _worldTime is not null
            ? VillageLifeConfig.GetTimeOfDay(_worldTime.GameHour)
            : GameTimeOfDay.Afternoon;
        var legacyContext = _legacyService?.BuildContext(playerEntityId);
        string? legacyLine = null;
        var usedLegacyRecognition = legacyContext is not null
            && _legacyService!.TryGetElderRecognitionResponse(
                playerEntityId,
                simulation.Npc.EntityId,
                request.Kind,
                legacyContext,
                flavorSeed,
                out legacyLine)
            && legacyLine is not null;

        string? socialRoleLine = null;
        var usedSocialRoleRecognition = !usedLegacyRecognition
            && _communityReputationService?.TryGetInteractionRecognition(
                playerEntityId,
                simulation.Npc.EntityId,
                flavorSeed,
                out socialRoleLine) == true
            && socialRoleLine is not null;

        string? personalHabitLine = null;
        var usedPersonalHabit = !usedLegacyRecognition
            && !usedSocialRoleRecognition
            && _socialDynamicsService?.TryGetPersonalHabitResponse(
                playerEntityId,
                simulation.Npc.EntityId,
                affinityChange.NewTier,
                reputationState,
                flavorSeed,
                out personalHabitLine) == true
            && personalHabitLine is not null;

        string? emotionalBondLine = null;
        var usedEmotionalBond = !usedLegacyRecognition
            && !usedSocialRoleRecognition
            && !usedPersonalHabit
            && NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
                simulation.Npc.EntityId,
                request.Kind,
                memories,
                affinityChange.NewTier,
                legacyArchetype,
                flavorSeed) is { } bondLine
            && !string.IsNullOrWhiteSpace(bondLine)
            && (emotionalBondLine = bondLine) is not null;

        var usedPersonalized = !usedLegacyRecognition
            && !usedSocialRoleRecognition
            && !usedPersonalHabit
            && !usedEmotionalBond
            && affinityChange.NewTier >= RelationshipTier.Friend
            && memories.Count > 0
            && NpcMemoryConfig.TryGetPersonalizedResponse(
                request.Kind,
                memories,
                affinityChange.NewTier,
                flavorSeed) is not null;

        var responseText = usedLegacyRecognition
            ? legacyLine!
            : usedSocialRoleRecognition
                ? socialRoleLine!
                : usedPersonalHabit
                    ? personalHabitLine!
                    : usedEmotionalBond
                        ? emotionalBondLine!
                        : NpcResponseGenerator.Generate(
                            simulation,
                            request.Kind,
                            affinityChange.NewTier,
                            memories,
                            developmentLevel,
                            flavorSeed);

        // Rare light info tip — only from Elsie/Mira/Harold when acquaintance+ and cooldown allows.
        if (affinityChange.NewTier >= RelationshipTier.Acquaintance
            && SocialDynamicsConfig.IsInfoSharingNpc(simulation.Npc.EntityId)
            && _socialDynamicsService?.TryGetLightInfoAppendix(
                playerEntityId,
                simulation.Npc.EntityId,
                timeOfDay,
                developmentLevel,
                completedProjects,
                flavorSeed + 3,
                out var infoAppendix) == true)
        {
            responseText = $"{responseText} {infoAppendix}";
        }

        if (_longTermGoalService?.TryGetMilestoneInteractionFeedback(
                playerEntityId,
                simulation.Npc.EntityId,
                flavorSeed + 17,
                out var milestoneFeedback) == true
            && !string.IsNullOrWhiteSpace(milestoneFeedback))
        {
            responseText = $"{responseText} {milestoneFeedback}";
            if (_longTermGoalService.TryGetEmotionalMilestoneBond(
                    playerEntityId,
                    simulation.Npc.EntityId,
                    flavorSeed + 18,
                    out var emotionalMilestone)
                && !string.IsNullOrWhiteSpace(emotionalMilestone))
            {
                responseText = $"{responseText} {emotionalMilestone}";
            }
        }
        else if (_longTermGoalService?.TryConsumePendingMilestoneFeedback(
                     playerEntityId,
                     flavorSeed + 19,
                     out var pendingMilestoneFeedback) == true
                 && !string.IsNullOrWhiteSpace(pendingMilestoneFeedback))
        {
            responseText = $"{responseText} {pendingMilestoneFeedback}";
        }

        if (_milestoneService?.TryConsumePendingFeedback(playerEntityId, out var personalMilestoneFeedback) == true
            && !string.IsNullOrWhiteSpace(personalMilestoneFeedback))
        {
            responseText = $"{responseText} {personalMilestoneFeedback}";
        }

        var standingPromotionFeedback = VillageSocialStandingConfig.TryFormatTierPromotionFeedback(
            standingTierBefore,
            standingTier,
            villageNoticedStanding);
        if (!string.IsNullOrWhiteSpace(standingPromotionFeedback))
            responseText = $"{responseText} {standingPromotionFeedback}";

        if (_worldTime is not null)
        {
            var eventLine = NpcResponseGenerator.TryGetVillageEventInteractionLine(
                playerEntityId,
                simulation.Npc.EntityId,
                _worldTime.GameDay,
                flavorSeed + 47);
            if (!string.IsNullOrWhiteSpace(eventLine))
                responseText = $"{responseText} {eventLine}";
        }

        var connectorInsight = _longTermGoalService?.TryGetConnectorSocialInsight(playerEntityId, flavorSeed + 23);
        if (!string.IsNullOrWhiteSpace(connectorInsight))
            responseText = $"{responseText} {connectorInsight}";

        var connectorInfluenceFeedback = _longTermGoalService?.TryGetInfluenceGainFeedback(
            playerEntityId,
            LegacyArchetype.Connector,
            flavorSeed + 29);
        if (!string.IsNullOrWhiteSpace(connectorInfluenceFeedback))
            responseText = $"{responseText} {connectorInfluenceFeedback}";

        if (_longTermGoalService?.TryGetNpcArchetypeRecognition(
                playerEntityId,
                simulation.Npc.EntityId,
                flavorSeed + 31,
                out var archetypeRecognition) == true
            && !string.IsNullOrWhiteSpace(archetypeRecognition))
        {
            responseText = $"{responseText} {archetypeRecognition}";
        }

        var personalNpcFeedback = _longTermGoalService?.TryGetPersonalAlignedActionFeedback(
            playerEntityId,
            LegacyAlignedActionKind.NpcInteraction,
            flavorSeed + 37);
        if (!string.IsNullOrWhiteSpace(personalNpcFeedback))
            responseText = $"{responseText} {personalNpcFeedback}";

        if (_longTermGoalService?.TryGetEmotionalArchetypeBond(
                playerEntityId,
                simulation.Npc.EntityId,
                affinityChange.NewTier,
                _memoryService,
                flavorSeed + 41,
                out var emotionalArchetypeBond) == true
            && !string.IsNullOrWhiteSpace(emotionalArchetypeBond))
        {
            responseText = $"{responseText} {emotionalArchetypeBond}";
        }

        var standingWarmth = NpcResponseGenerator.TryGetSocialStandingWarmthResponse(
            simulation.Npc.EntityId,
            standingTier,
            villageNoticedStanding,
            playerEntityId,
            _worldTime?.TotalGameMinutes ?? 0,
            flavorSeed + 2);
        if (!string.IsNullOrWhiteSpace(standingWarmth))
            responseText = $"{responseText} {standingWarmth}";

        if (!string.IsNullOrWhiteSpace(standingRecoveryFeedback))
            responseText = $"{responseText} {standingRecoveryFeedback}";

        var prestigeRecognition = NpcResponseGenerator.TryGetWellLikedPrestigeRecognitionResponse(
            simulation.Npc.EntityId,
            standingTier,
            villageNoticedStanding,
            playerEntityId,
            _worldTime?.TotalGameMinutes ?? 0,
            flavorSeed + 17);
        if (!string.IsNullOrWhiteSpace(prestigeRecognition))
        {
            responseText =
                $"{responseText} {VillageSocialStandingImpactConfig.FormatWellLikedPrestigeRecognitionFeedback(npcName, prestigeRecognition)}";
        }

        if (_memoryService.TryConsumeSocialLegacyNpcMentionCooldown(playerEntityId, simulation.Npc.EntityId))
        {
            var totalGameMinutes = _worldTime?.TotalGameMinutes ?? 0;
            var legacySeed = flavorSeed + 23;

            var pillarAcknowledgment = NpcResponseGenerator.TryGetVillagePillarAcknowledgmentResponse(
                simulation.Npc.EntityId,
                standingTier,
                focusCloseFriendCount,
                villageNoticedStanding,
                playerEntityId,
                totalGameMinutes,
                legacySeed);
            if (!string.IsNullOrWhiteSpace(pillarAcknowledgment))
            {
                responseText =
                    $"{responseText} {SocialLegacyConfig.FormatVillagePillarAcknowledgmentFeedback(npcName, pillarAcknowledgment)}";
            }
            else
            {
                var legacyJourney = NpcResponseGenerator.TryGetSocialLegacyJourneyResponse(
                    simulation.Npc.EntityId,
                    standingTier,
                    villageNoticedStanding,
                    playerEntityId,
                    totalGameMinutes,
                    legacySeed);
                if (!string.IsNullOrWhiteSpace(legacyJourney))
                {
                    responseText =
                        $"{responseText} {SocialLegacyConfig.FormatLegacyNpcMentionFeedback(npcName, legacyJourney)}";
                }
            }
        }

        if (NpcEmotionalBondConfig.IsFocusNpc(simulation.Npc.EntityId)
            && VillageSocialStandingImpactConfig.IsEligibleForWellLikedPrivilege(standingTier)
            && _memoryService.TryConsumeWellLikedStandingPrivilegeCooldown(playerEntityId, simulation.Npc.EntityId)
            && VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrivilege(
                playerEntityId,
                simulation.Npc.EntityId,
                standingTier,
                _worldTime?.TotalGameMinutes ?? 0,
                flavorSeed + 5)
            && VillageSocialStandingDialogue.TryGetWellLikedPrivilegeLine(
                simulation.Npc.EntityId,
                villageNoticedStanding,
                flavorSeed + 5) is { } privilegeLine
            && !string.IsNullOrWhiteSpace(privilegeLine))
        {
            responseText =
                $"{responseText} {VillageSocialStandingImpactConfig.FormatWellLikedPrivilegeFeedback(npcName, privilegeLine)}";

            if (VillageSocialStandingImpactConfig.ShouldGrantWellLikedPrivilegeItem(
                    playerEntityId,
                    simulation.Npc.EntityId,
                    _worldTime?.TotalGameMinutes ?? 0,
                    flavorSeed + 11)
                && VillageSocialStandingImpactConfig.TryGetWellLikedPrivilegeItemGrant(
                    simulation.Npc.EntityId,
                    flavorSeed + 11) is { } privilegeGrant
                && _economyService.TryGetState(playerEntityId, out var privilegeEconomy))
            {
                privilegeEconomy.Inventory.AddItem(privilegeGrant.ItemType, privilegeGrant.Quantity);
                _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
                responseText =
                    $"{responseText} {VillageSocialStandingImpactConfig.FormatWellLikedPrivilegeItemFeedback(npcName, privilegeGrant)}";

                Log.Information(
                    "Well-liked standing privilege item from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
                    npcName,
                    playerEntityId,
                    privilegeGrant.Quantity,
                    privilegeGrant.ItemType);
            }

            Log.Information(
                "Well-liked standing privilege from {NpcName} to player {PlayerId}: \"{Line}\"",
                npcName,
                playerEntityId,
                privilegeLine);
        }

        if (NpcEmotionalBondConfig.IsFocusNpc(simulation.Npc.EntityId)
            && affinityChange.NewTier >= NpcEmotionalBondConfig.MinEmotionalInteractionTier
            && NpcEmotionalBondConfig.HasEmotionalMemory(simulation.Npc.EntityId, memories)
            && _memoryService.TryConsumeEmotionalMomentCooldown(playerEntityId, simulation.Npc.EntityId)
            && NpcEmotionalBondConfig.ShouldTriggerEmotionalPersonalMoment(
                playerEntityId,
                simulation.Npc.EntityId,
                _worldTime?.TotalGameMinutes ?? 0)
            && NpcEmotionalBondConfig.TryGetEmotionalPersonalMoment(
                simulation.Npc.EntityId,
                memories,
                affinityChange.NewTier,
                flavorSeed + 43) is { } personalMoment
            && !string.IsNullOrWhiteSpace(personalMoment))
        {
            responseText = $"{responseText} {personalMoment}";
            Log.Information(
                "Emotional personal moment from {NpcName} to player {PlayerId}: \"{Moment}\"",
                npcName,
                playerEntityId,
                personalMoment);
        }

        // Emotional bond impact — extra needs recovery, appreciation, tips, and favors when close to a focus NPC.
        var emotionalBondImpact = _emotionalBondService?.TryApplyInteractionImpact(
            playerEntityId,
            simulation.Npc.EntityId,
            request.Kind,
            affinityChange.NewTier,
            memories,
            timeOfDay,
            flavorSeed + 47) ?? EmotionalBondImpactResult.None;

        if (emotionalBondImpact.AppliedRecovery)
        {
            var recoveryFeedback = NpcEmotionalBondImpactConfig.FormatNeedsRecoveryFeedback(
                npcName,
                emotionalBondImpact.MoodBonus,
                emotionalBondImpact.SocialBonus);
            if (!string.IsNullOrWhiteSpace(recoveryFeedback))
                responseText = $"{responseText} {recoveryFeedback}";
        }

        foreach (var appendix in emotionalBondImpact.AppendixLines)
            responseText = $"{responseText} {appendix}";

        var interactionVerb = request.Kind == NpcInteractionKind.Greet ? "greeted" : "talked with";
        var targetNote = hasTarget ? $" (targeted {npcName})" : $" (nearest NPC: {npcName})";

        Log.Information(
            "Player {PlayerId} affinity with {NpcName} increased from {OldAffinity} to {NewAffinity} ({Interaction}). Current tier: {Tier}.",
            playerEntityId,
            npcName,
            affinityChange.PreviousAffinity,
            affinityChange.NewAffinity,
            request.Kind,
            RelationshipTierDisplay.GetName(affinityChange.NewTier));

        Log.Information(
            "Player {PlayerId} {InteractionVerb} {NpcName}{TargetNote}. Social {SocialBefore:F0}->{SocialAfter:F0}. Personalized={Personalized}, EmotionalBond={EmotionalBond}, LegacyRecognition={LegacyRecognition}, PersonalHabit={PersonalHabit}. Response: \"{Response}\"",
            playerEntityId,
            interactionVerb,
            npcName,
            targetNote,
            socialBefore,
            socialAfter,
            usedPersonalized,
            usedEmotionalBond,
            usedLegacyRecognition,
            usedPersonalHabit,
            responseText);

        return new NpcInteractionResponse(
            Success: true,
            request.Kind,
            simulation.Npc.EntityId,
            NpcInteractionFailureReason.None,
            responseText);
    }

    private static void ApplyInteractionEffects(NpcSimulationState simulation, NpcInteractionKind kind)
    {
        var socialReduction = kind switch
        {
            NpcInteractionKind.Greet => Needs.NpcNeedsConfig.GreetSocialReduction,
            NpcInteractionKind.Talk => Needs.NpcNeedsConfig.TalkSocialReduction,
            _ => 0f,
        };

        if (socialReduction > 0f)
            simulation.Needs.SatisfySocial(socialReduction);
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

    private static NpcInteractionResponse LogAndFail(
        uint playerEntityId,
        NpcInteractionKind kind,
        uint npcEntityId,
        string? npcName,
        NpcInteractionFailureReason reason,
        string message)
    {
        Log.Information(
            "Player {PlayerId} {Interaction} with {NpcName} failed ({Reason}): {Message}",
            playerEntityId,
            kind,
            npcName ?? "none",
            reason,
            message);

        return new NpcInteractionResponse(false, kind, npcEntityId, reason, message);
    }
}