using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Shared.Activities;
using Bloomtown.Shared.World;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Console;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Validates community-help activities, applies social-focused need effects, and attaches NPC acknowledgment.
/// </summary>
public sealed class CommunityActivityService
{
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly VillageAreaService _areaService;
    private readonly VillageProjectStateService _projectStateService;
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly NpcInterpersonalRelationshipService? _interpersonalService;
    private readonly CommunityReputationService? _reputationService;
    private readonly PlayerLongTermGoalService? _longTermGoalService;
    private readonly NpcMemoryService? _memoryService;
    private readonly PlayerNpcRelationshipService? _relationshipService;
    private readonly WorldTimeSystem? _worldTime;
    private readonly Action<uint, uint, string>? _sendAmbientComment;
    private readonly PlayerDailyRhythmTracker? _dailyRhythmTracker;
    private readonly PlayerMilestoneService? _milestoneService;
    private readonly Dictionary<(uint PlayerId, CommunityActivityKind Kind), DateTime> _cooldowns = new();
    private readonly Dictionary<uint, DateTime> _ambientReactionCooldowns = new();

    public CommunityActivityService(
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        VillageAreaService areaService,
        VillageProjectStateService projectStateService,
        NpcManager npcManager,
        AoiSystem aoiSystem,
        NpcInterpersonalRelationshipService? interpersonalService = null,
        CommunityReputationService? reputationService = null,
        PlayerLongTermGoalService? longTermGoalService = null,
        NpcMemoryService? memoryService = null,
        PlayerNpcRelationshipService? relationshipService = null,
        WorldTimeSystem? worldTime = null,
        Action<uint, uint, string>? sendAmbientComment = null,
        PlayerDailyRhythmTracker? dailyRhythmTracker = null,
        PlayerMilestoneService? milestoneService = null)
    {
        _economyService = economyService;
        _needsService = needsService;
        _areaService = areaService;
        _projectStateService = projectStateService;
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _interpersonalService = interpersonalService;
        _reputationService = reputationService;
        _longTermGoalService = longTermGoalService;
        _memoryService = memoryService;
        _relationshipService = relationshipService;
        _worldTime = worldTime;
        _sendAmbientComment = sendAmbientComment;
        _dailyRhythmTracker = dailyRhythmTracker;
        _milestoneService = milestoneService;
    }

    public CommunityActivityResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        CommunityActivityRequest request)
    {
        return request.Kind switch
        {
            CommunityActivityRequestKind.List => HandleList(playerEntityId),
            CommunityActivityRequestKind.Perform => Perform(playerEntityId, playerX, playerZ, request.Activity),
            _ => Fail(
                CommunityActivityRequestKind.List,
                CommunityActivityFailureReason.UnknownRequest,
                "Unknown community activity request."),
        };
    }

    public string FormatNearbyStatus(float playerX, float playerZ)
    {
        var unlockedAreas = _areaService.GetUnlockedAreas().ToHashSet();
        var completedProjects = _projectStateService.GetCompletedProjectIds().ToHashSet();
        return CommunityActivityConfig.FormatNearbyStatus(
            playerX,
            playerZ,
            unlockedAreas,
            completedProjects);
    }

    private CommunityActivityResponse HandleList(uint playerEntityId)
    {
        var unlockedAreas = _areaService.GetUnlockedAreas().ToHashSet();
        var completedProjects = _projectStateService.GetCompletedProjectIds().ToHashSet();
        var message = CommunityActivityConfig.FormatActivityList(unlockedAreas, completedProjects);
        Log.Information("Player {PlayerId} viewed community help activities.", playerEntityId);

        return new CommunityActivityResponse(
            true,
            CommunityActivityRequestKind.List,
            CommunityActivityFailureReason.None,
            message);
    }

    private CommunityActivityResponse Perform(
        uint playerEntityId,
        float playerX,
        float playerZ,
        CommunityActivityKind activity)
    {
        if (!CommunityActivityConfig.TryGet(activity, out var definition))
        {
            return Fail(
                CommunityActivityRequestKind.Perform,
                CommunityActivityFailureReason.UnknownActivity,
                "Unknown community activity.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                CommunityActivityRequestKind.Perform,
                CommunityActivityFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        var unlockedAreas = _areaService.GetUnlockedAreas().ToHashSet();
        var completedProjects = _projectStateService.GetCompletedProjectIds().ToHashSet();

        if (!CommunityActivityConfig.MeetsPrerequisites(definition, unlockedAreas, completedProjects))
        {
            return Fail(
                CommunityActivityRequestKind.Perform,
                CommunityActivityFailureReason.NotUnlocked,
                FormatPrerequisiteFailure(definition));
        }

        var distance = CommunityActivityConfig.GetDistance(
            playerX,
            playerZ,
            definition.WorldX,
            definition.WorldZ);

        if (distance > CommunityActivityConfig.InteractionRadiusMeters)
        {
            return Fail(
                CommunityActivityRequestKind.Perform,
                CommunityActivityFailureReason.NotInRange,
                $"{definition.LocationName} is too far away ({distance:F1}m). Move within {CommunityActivityConfig.InteractionRadiusMeters:F0}m of ({definition.WorldX:F0}, {definition.WorldZ:F0}).");
        }

        if (TryGetCooldownFailure(playerEntityId, activity, definition, out var cooldownFailure))
            return cooldownFailure;

        var moodBefore = economy.Mood;
        var socialBefore = economy.SocialNeed;

        // Consistent helpers earn a subtle bonus in their usual area — applied before counters increment.
        var reputationBefore = _reputationService?.GetState(playerEntityId)
            ?? CommunityReputationConfig.CreateEmpty();
        var (moodBonus, socialBonus) = CommunityReputationConfig.GetConsistentHelperEffectBonus(
            reputationBefore,
            activity);
        var caretakerAgencyBonus = _longTermGoalService?.GetCaretakerCommunityHelpMoodBonus(playerEntityId, activity) ?? 0f;
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var (standingMoodBonus, standingSocialBonus) =
            VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(activity, standingTier);
        var communityWorkDayActive = _worldTime is not null
            && VillageEventConfig.IsCommunityWorkDay(_worldTime.GameDay)
            && VillageEventConfig.IsHelpActivity(activity);
        var eventMoodBonus = communityWorkDayActive ? VillageEventConfig.CommunityWorkDayMoodBonus : 0f;
        var eventSocialBonus = communityWorkDayActive ? VillageEventConfig.CommunityWorkDaySocialBonus : 0f;

        var rainyDayActive = _worldTime is not null && VillageEventConfig.IsRainyDay(_worldTime.GameDay);
        var (rainyMoodAdjust, rainySocialAdjust) = rainyDayActive
            ? VillageEventConfig.GetRainyDayCommunityActivityAdjustments(activity)
            : (0f, 0f);

        var currentPhase = _worldTime is not null
            ? GameTimeHelper.GetTimeOfDay(_worldTime.GameHour)
            : GameTimeOfDay.Afternoon;
        var rhythmBonus = _dailyRhythmTracker?.GetActivityBonus(
            playerEntityId,
            DailyRhythmActivityCategory.Social,
            currentPhase,
            _worldTime?.GameDay ?? 0)
            ?? default;

        var effectiveMoodGain = definition.MoodGain + moodBonus + caretakerAgencyBonus + standingMoodBonus
            + eventMoodBonus + rainyMoodAdjust + rhythmBonus.MoodBonus;
        var effectiveSocialReduction = MathF.Max(
            0f,
            definition.SocialReduction + socialBonus + standingSocialBonus
            + eventSocialBonus + rainySocialAdjust + rhythmBonus.SocialReductionBonus);

        // Community help emphasizes social connection — stronger Social relief than area leisure.
        _needsService.ApplyCommunityActivity(economy, effectiveMoodGain, effectiveSocialReduction);
        SetCooldown(playerEntityId, activity);

        _dailyRhythmTracker?.Record(
            playerEntityId,
            DailyRhythmActivityCategory.Social,
            definition.CommandHint,
            currentPhase,
            _worldTime?.GameDay ?? 0);

        var grantedStandingBonusItem = false;
        var eliasSmithingBackingApplied = false;
        var marcusCraftingBackingApplied = false;
        var benGuardBackingApplied = false;
        var lilaYouthBackingApplied = false;
        var rowanStoryBackingApplied = false;
        var eleanorLegacyBackingApplied = false;
        if (standingMoodBonus > 0f || standingSocialBonus > 0f)
        {
            if (VillageSocialStandingMechanicalConfig.ShouldGrantElsieGardenBonusHarvest(
                    playerEntityId,
                    activity,
                    standingTier))
            {
                economy.Inventory.AddItem(ItemType.Apple, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantNoraHerbBonusHarvest(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Apple, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantEliasSmithyBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Wood, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantMarcusWorkshopBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Plank, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantBenPatrolBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Wood, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantLilaVillageBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Apple, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantRowanStoryBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Wood, 1);
                grantedStandingBonusItem = true;
            }
            else if (VillageSocialStandingMechanicalConfig.ShouldGrantEleanorLegacyBonusYield(
                         playerEntityId,
                         activity,
                         standingTier))
            {
                economy.Inventory.AddItem(ItemType.Apple, 1);
                grantedStandingBonusItem = true;
            }
        }

        var eliasActivityBackingBonus = 0;
        var marcusActivityBackingBonus = 0;
        var benActivityBackingBonus = 0;
        var lilaActivityBackingBonus = 0;
        var rowanActivityBackingBonus = 0;
        var eleanorActivityBackingBonus = 0;
        if (_memoryService?.HasEliasSocialInfluenceSmithingBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsEliasSmithyActivity(activity)
            && _memoryService.TryConsumeEliasSocialInfluenceSmithingBacking(playerEntityId, out eliasActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Wood, eliasActivityBackingBonus);
            eliasSmithingBackingApplied = true;
        }

        if (_memoryService?.HasMarcusSocialInfluenceCraftingBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsMarcusWorkshopActivity(activity)
            && _memoryService.TryConsumeMarcusSocialInfluenceCraftingBacking(playerEntityId, out marcusActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Plank, marcusActivityBackingBonus);
            marcusCraftingBackingApplied = true;
        }

        if (_memoryService?.HasBenSocialInfluenceGuardBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsBenPatrolActivity(activity)
            && _memoryService.TryConsumeBenSocialInfluenceGuardBacking(playerEntityId, out benActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Wood, benActivityBackingBonus);
            benGuardBackingApplied = true;
        }

        if (_memoryService?.HasLilaSocialInfluenceYouthBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsLilaVillageActivity(activity)
            && _memoryService.TryConsumeLilaSocialInfluenceYouthBacking(playerEntityId, out lilaActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Apple, lilaActivityBackingBonus);
            lilaYouthBackingApplied = true;
        }

        if (_memoryService?.HasRowanSocialInfluenceStoryBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsRowanStoryActivity(activity)
            && _memoryService.TryConsumeRowanSocialInfluenceStoryBacking(playerEntityId, out rowanActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Wood, rowanActivityBackingBonus);
            rowanStoryBackingApplied = true;
        }

        if (_memoryService?.HasEleanorSocialInfluenceLegacyBacking(playerEntityId) == true
            && VillageSocialStandingMechanicalConfig.IsEleanorLegacyActivity(activity)
            && _memoryService.TryConsumeEleanorSocialInfluenceLegacyBacking(playerEntityId, out eleanorActivityBackingBonus))
        {
            economy.Inventory.AddItem(ItemType.Apple, eleanorActivityBackingBonus);
            eleanorLegacyBackingApplied = true;
        }

        if (moodBonus > 0f || socialBonus > 0f)
        {
            Log.Information(
                "Consistent helper bonus for player {PlayerId} ({Role}) during {Activity}: mood +{MoodBonus:F0}, social +{SocialBonus:F0}.",
                playerEntityId,
                CommunityReputationConfig.GetDominantSocialRole(reputationBefore),
                activity,
                moodBonus,
                socialBonus);
        }

        // Track help frequency — recurring social-role acknowledgment keys off these counters.
        var reputationState = _reputationService is not null
            ? _reputationService.RecordHelpAsync(playerEntityId, activity).GetAwaiter().GetResult()
            : CommunityReputationConfig.CreateEmpty();

        var flavorSeed = playerEntityId + (uint)activity + (uint)_cooldowns.Count;
        if (_longTermGoalService is not null)
            _longTermGoalService.RecordCommunityHelpAndReconcileAsync(playerEntityId, activity, flavorSeed).GetAwaiter().GetResult();

        _milestoneService?.ReconcileAsync(playerEntityId).GetAwaiter().GetResult();

        if (_memoryService is not null
            && activity is CommunityActivityKind.HelpGarden
                or CommunityActivityKind.HelpWell
                or CommunityActivityKind.HelpMarket
                or CommunityActivityKind.HelpLumber
                or CommunityActivityKind.HelpInn
                or CommunityActivityKind.HelpHerbGarden
                or CommunityActivityKind.HelpSmithy
                or CommunityActivityKind.HelpWorkshop
                or CommunityActivityKind.HelpPatrol
                or CommunityActivityKind.HelpVillage
                or CommunityActivityKind.ListenToStories
                or CommunityActivityKind.ChatWithEleanor)
        {
            var areaMemory = _memoryService
                .OnFocusAreaCommunityHelpAsync(playerEntityId, activity)
                .GetAwaiter()
                .GetResult();
            if (areaMemory is not null)
            {
                Log.Information(
                    "Player {PlayerId} earned emotional area-help memory {Memory} via {Activity}.",
                    playerEntityId,
                    areaMemory,
                    activity);
            }
        }

        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Player {PlayerId} community activity {Activity} at {Location} — mood {MoodBefore:F0}->{MoodAfter:F0}, social {SocialBefore:F0}->{SocialAfter:F0}.",
            playerEntityId,
            definition.CommandHint,
            definition.LocationName,
            moodBefore,
            economy.Mood,
            socialBefore,
            economy.SocialNeed);

        var flavor = CommunityActivityConfig.PickFlavorText(definition, flavorSeed);
        var contribution = CommunityActivityConfig.PickContributionText(definition, flavorSeed + 3);

        var needLine = ActivityFeedbackFormat.FormatNeedChanges(
            moodBefore,
            economy.Mood,
            fatigueBefore: 0f,
            fatigueAfter: 0f,
            socialBefore,
            economy.SocialNeed,
            includeSocial: true);
        var message = new System.Text.StringBuilder();
        message.AppendLine(flavor);
        message.AppendLine(contribution);
        if (!string.IsNullOrWhiteSpace(needLine))
            message.AppendLine(needLine);

        if (communityWorkDayActive)
            message.Append($"{Environment.NewLine}{VillageEventConfig.FormatCommunityWorkDayHelpFeedback()}");

        if (rainyDayActive)
        {
            if (VillageEventConfig.IsOutdoorCommunityActivity(activity))
                message.Append($"{Environment.NewLine}{VillageEventConfig.FormatRainyDayOutdoorFeedback()}");
            else if (VillageEventConfig.IsIndoorCalmCommunityActivity(activity))
                message.Append($"{Environment.NewLine}{VillageEventConfig.FormatRainyDayIndoorFeedback()}");
        }

        if (moodBonus > 0f || socialBonus > 0f)
        {
            message.Append(
                $"{Environment.NewLine}The village welcomes a familiar helper — a little extra warmth for your usual role.");
        }

        if (caretakerAgencyBonus > 0f)
        {
            message.Append(
                $"{Environment.NewLine}Caretaker legacy — the village feels your steady care a little more today.");
        }

        if (standingMoodBonus > 0f || standingSocialBonus > 0f)
        {
            var standingFeedback = VillageSocialStandingMechanicalConfig.FormatCommunityActivityStandingFeedback(
                activity,
                standingTier,
                grantedStandingBonusItem);
            if (!string.IsNullOrWhiteSpace(standingFeedback))
                message.Append($"{Environment.NewLine}{standingFeedback}");
        }

        if (!string.IsNullOrWhiteSpace(rhythmBonus.FeedbackNote))
            message.Append(rhythmBonus.FeedbackNote);

        if (eliasSmithingBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatSmithingActivityBackingFeedback(
                eliasActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(eliasActivityBackingBonus))}");
        }

        if (marcusCraftingBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatCraftingActivityBackingFeedback(
                marcusActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(marcusActivityBackingBonus))}");
        }

        if (benGuardBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatGuardActivityBackingFeedback(
                benActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(benActivityBackingBonus))}");
        }

        if (lilaYouthBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatYouthActivityBackingFeedback(
                lilaActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(lilaActivityBackingBonus))}");
        }

        if (rowanStoryBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatStoryActivityBackingFeedback(
                rowanActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(rowanActivityBackingBonus))}");
        }

        if (eleanorLegacyBackingApplied)
        {
            message.Append($"{Environment.NewLine}{SocialInfluenceActionConfig.FormatLegacyActivityBackingFeedback(
                eleanorActivityBackingBonus,
                ResolveSocialInfluenceBackingTier(eleanorActivityBackingBonus))}");
        }

        if (VillageSocialStandingMechanicalConfig.ShouldGrantGretaWellLikedInnGuestInfo(
                playerEntityId,
                activity,
                standingTier))
        {
            var guestInfoFeedback = VillageSocialStandingMechanicalConfig.FormatGretaWellLikedInnGuestInfoFeedback(
                flavorSeed + 13);
            if (!string.IsNullOrWhiteSpace(guestInfoFeedback))
                message.Append($"{Environment.NewLine}{guestInfoFeedback}");
        }

        var agencyFeedback = _longTermGoalService?.TryGetCommunityHelpAgencyFeedback(
            playerEntityId,
            activity,
            flavorSeed + 5);
        if (!string.IsNullOrWhiteSpace(agencyFeedback))
            message.Append($"{Environment.NewLine}{agencyFeedback}");

        var influencePath = LegacyArchetypeAgencyConfig.GetInfluenceForCommunityActivity(activity);
        if (influencePath is not null
            && _longTermGoalService?.TryGetInfluenceGainFeedback(
                playerEntityId,
                influencePath.Value,
                flavorSeed + 7) is { } influenceFeedback
            && !string.IsNullOrWhiteSpace(influenceFeedback))
        {
            message.Append($"{Environment.NewLine}{influenceFeedback}");
        }

        var personalFeedback = _longTermGoalService?.TryGetPersonalAlignedActionFeedback(
            playerEntityId,
            LegacyAlignedActionKind.CommunityHelp,
            flavorSeed + 9);
        if (!string.IsNullOrWhiteSpace(personalFeedback))
            message.Append($"{Environment.NewLine}{personalFeedback}");

        if (_memoryService is not null
            && _longTermGoalService is not null
            && activity is CommunityActivityKind.HelpGarden
                or CommunityActivityKind.HelpWell
                or CommunityActivityKind.HelpMarket
                or CommunityActivityKind.HelpLumber
                or CommunityActivityKind.HelpInn
                or CommunityActivityKind.HelpHerbGarden
                or CommunityActivityKind.HelpSmithy
                or CommunityActivityKind.HelpWorkshop
                or CommunityActivityKind.HelpPatrol
                or CommunityActivityKind.HelpVillage
                or CommunityActivityKind.ListenToStories
                or CommunityActivityKind.ChatWithEleanor)
        {
            var focusNpc = NpcEmotionalBondConfig.GetFocusNpcForActivity(activity);
            var archetype = _longTermGoalService.GetLegacyArchetype(playerEntityId);
            var memories = _memoryService.GetMemoriesForNpc(playerEntityId, focusNpc);
            var gameMinutes = _worldTime?.TotalGameMinutes ?? 0;

            if (NpcEmotionalBondConfig.ShouldTriggerCommunityHelpEmotionalAck(
                    playerEntityId,
                    focusNpc,
                    gameMinutes)
                && NpcEmotionalBondConfig.TryGetCommunityHelpEmotionalAcknowledgment(
                    focusNpc,
                    memories,
                    archetype,
                    flavorSeed + 13) is { } emotionalAck
                && !string.IsNullOrWhiteSpace(emotionalAck))
            {
                message.Append($"{Environment.NewLine}{emotionalAck}");
                Log.Information(
                    "Emotional community-help acknowledgment from focus NPC {NpcId} to player {PlayerId} ({Activity}): \"{Ack}\"",
                    focusNpc,
                    playerEntityId,
                    activity,
                    emotionalAck);
            }
        }

        if (_longTermGoalService?.TryConsumePendingMilestoneFeedback(
                playerEntityId,
                flavorSeed + 11,
                out var goalFeedback) == true
            && !string.IsNullOrWhiteSpace(goalFeedback))
        {
            message.Append($"{Environment.NewLine}{goalFeedback}");
        }

        if (_milestoneService?.TryConsumePendingFeedback(playerEntityId, out var milestoneFeedback) == true
            && !string.IsNullOrWhiteSpace(milestoneFeedback))
        {
            message.Append($"{Environment.NewLine}{milestoneFeedback}");
        }

        // NPC acknowledgment when a visible neighbor is close enough to notice the help.
        if (TryFindNearestVisibleNpc(playerEntityId, playerX, playerZ, out var npc, out _))
        {
            var ackSeed = flavorSeed + npc.Npc.EntityId;
            var interpersonalRelationship = _interpersonalService?.ElsieTomRelationship
                ?? NpcInterpersonalRelationshipConfig.DefaultRelationship;

            // Recurring acknowledgment when the player has helped often enough in this area.
            string? ack = null;
            if (_reputationService?.TryGetRecurringHelpAcknowledgment(
                    playerEntityId,
                    activity,
                    npc.Npc.EntityId,
                    ackSeed,
                    out var recurringAck) == true)
            {
                ack = recurringAck;
            }
            else
            {
                if (_worldTime is not null
                    && VillageEventDialogue.TryGetCommunityWorkHelpAcknowledgment(
                        playerEntityId,
                        npc.Npc.EntityId,
                        activity,
                        _worldTime.GameDay,
                        ackSeed) is { } workDayAck
                    && !string.IsNullOrWhiteSpace(workDayAck))
                {
                    ack = workDayAck;
                }
                else if (_worldTime is not null
                    && VillageEventDialogue.TryGetRainyDayActivityAcknowledgment(
                        playerEntityId,
                        npc.Npc.EntityId,
                        activity,
                        _worldTime.GameDay,
                        ackSeed) is { } rainyAck
                    && !string.IsNullOrWhiteSpace(rainyAck))
                {
                    ack = rainyAck;
                }
                else
                {
                    // Prefer personalized social-fabric reaction; fall back to generic acknowledgment.
                    ack = VillageSocialFabricConfig.TryGetCommunityHelpAmbientReaction(
                        activity,
                        interpersonalRelationship,
                        npc.Npc.EntityId,
                        ackSeed)
                        ?? CommunityActivityDialogue.PickNpcAcknowledgment(activity, ackSeed);
                }
            }

            if (!string.IsNullOrWhiteSpace(ack))
                message.Append($"{Environment.NewLine}{ack}");

            _ = reputationState;

            // Secondary ambient packet so the village feels like it noticed the contribution.
            TrySendCommunityHelpAmbientReaction(
                playerEntityId,
                npc,
                activity,
                interpersonalRelationship,
                ackSeed);
        }

        return new CommunityActivityResponse(
            true,
            CommunityActivityRequestKind.Perform,
            CommunityActivityFailureReason.None,
            message.ToString().TrimEnd());
    }

    /// <summary>
    /// Sends a follow-up ambient line when cooldown allows — reinforces that community help is socially noticed.
    /// </summary>
    private void TrySendCommunityHelpAmbientReaction(
        uint playerEntityId,
        NpcSimulationState npc,
        CommunityActivityKind activity,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed)
    {
        if (_sendAmbientComment is null)
            return;

        if (_ambientReactionCooldowns.TryGetValue(playerEntityId, out var lastSent)
            && DateTime.UtcNow - lastSent < VillageSocialFabricConfig.CommunityHelpAmbientReactionCooldown)
        {
            return;
        }

        var followUpSeed = variationSeed + (uint)activity + 17;
        var followUp = VillageSocialFabricConfig.TryGetCommunityHelpFollowUp(activity, interpersonalRelationship, followUpSeed);
        if (string.IsNullOrWhiteSpace(followUp))
            return;

        _ambientReactionCooldowns[playerEntityId] = DateTime.UtcNow;
        _sendAmbientComment(playerEntityId, npc.Npc.EntityId, $"{npc.Npc.Name}: \"{followUp}\"");
        Log.Information(
            "Community help ambient reaction ({Activity}) from {NpcName} to player {PlayerId}: \"{Comment}\"",
            activity,
            npc.Npc.Name,
            playerEntityId,
            followUp);
    }

    private bool TryFindNearestVisibleNpc(
        uint playerEntityId,
        float playerX,
        float playerZ,
        out NpcSimulationState bestNpc,
        out float bestDistance)
    {
        bestNpc = null!;
        bestDistance = float.MaxValue;
        NpcSimulationState? candidate = null;

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

    private bool TryGetCooldownFailure(
        uint playerEntityId,
        CommunityActivityKind activity,
        CommunityActivityDefinition definition,
        out CommunityActivityResponse failure)
    {
        var key = (playerEntityId, activity);
        if (_cooldowns.TryGetValue(key, out var lastUsed) && DateTime.UtcNow - lastUsed < definition.Cooldown)
        {
            var remaining = definition.Cooldown - (DateTime.UtcNow - lastUsed);
            failure = Fail(
                CommunityActivityRequestKind.Perform,
                CommunityActivityFailureReason.OnCooldown,
                $"Please wait {remaining.TotalSeconds:F0}s before '{definition.CommandHint}' again.");
            return true;
        }

        failure = default;
        return false;
    }

    private void SetCooldown(uint playerEntityId, CommunityActivityKind activity)
    {
        _cooldowns[(playerEntityId, activity)] = DateTime.UtcNow;
    }

    private static string FormatPrerequisiteFailure(CommunityActivityDefinition definition)
    {
        if (definition.RequiredArea is VillageArea area
            && VillageAreaConfig.TryGet(area, out var areaDefinition))
        {
            return
                $"{areaDefinition.Name} is not open yet. Grow the village to {VillageAreaConfig.FormatUnlockRequirement(areaDefinition.RequiredLevel)} first.";
        }

        if (definition.RequiresWellCompleted)
            return "The Village Well must be completed before you can help maintain it.";

        return "This community activity is not available yet.";
    }

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId)
    {
        if (_relationshipService is null)
            return VillageSocialStandingTier.Stranger;

        return VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
    }

    private static VillageSocialStandingTier ResolveSocialInfluenceBackingTier(int backingBonus) =>
        backingBonus >= SocialInfluenceActionConfig.WellLikedBackingProgressBonus
            ? VillageSocialStandingTier.WellLiked
            : VillageSocialStandingTier.Respected;

    private static CommunityActivityResponse Fail(
        CommunityActivityRequestKind kind,
        CommunityActivityFailureReason reason,
        string message)
    {
        Log.Information("Community activity request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new CommunityActivityResponse(false, kind, reason, message);
    }
}