using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Memory;

/// <summary>
/// Handles player-initiated emotional bonding actions with Elsie, Harold, Mira, Tom, and Greta:
/// proximity checks, cooldowns, memory recording, affinity gains, needs recovery, and clear feedback.
/// </summary>
public sealed class NpcEmotionalBondService
{
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;
    private readonly NpcMemoryService _memoryService;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerLongTermGoalService _longTermGoalService;
    private readonly PlayerNeedsService _needsService;
    private readonly PlayerEconomyService _economyService;
    private readonly WorldTimeSystem _worldTime;

    public NpcEmotionalBondService(
        NpcManager npcManager,
        AoiSystem aoiSystem,
        NpcMemoryService memoryService,
        PlayerNpcRelationshipService relationshipService,
        PlayerLongTermGoalService longTermGoalService,
        PlayerNeedsService needsService,
        PlayerEconomyService economyService,
        WorldTimeSystem worldTime)
    {
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
        _memoryService = memoryService;
        _relationshipService = relationshipService;
        _longTermGoalService = longTermGoalService;
        _needsService = needsService;
        _economyService = economyService;
        _worldTime = worldTime;
    }

    /// <summary>
    /// Applies rare emotional-bond benefits during greet/talk with focus NPCs:
    /// extra mood/social recovery, appreciation lines, village tips, and small favors.
    /// </summary>
    public EmotionalBondImpactResult TryApplyInteractionImpact(
        uint playerEntityId,
        uint npcEntityId,
        NpcInteractionKind kind,
        RelationshipTier tier,
        IReadOnlyCollection<NpcMemoryType> memories,
        GameTimeOfDay timeOfDay,
        uint variationSeed)
    {
        if (!NpcEmotionalBondImpactConfig.QualifiesForImpact(npcEntityId, tier, memories))
            return EmotionalBondImpactResult.None;

        // Recovery bonus scales with tier — close friends feel the bond lift mood and social need more.
        var (moodBonus, socialBonus) = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(kind, tier, npcEntityId);
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));

        var rainyDayActive = VillageEventConfig.IsRainyDay(_worldTime.GameDay);
        if (rainyDayActive)
        {
            var (rainyMoodAdjust, rainySocialAdjust) =
                VillageEventConfig.GetRainyDayNpcInteractionAdjustments(kind);
            moodBonus += rainyMoodAdjust;
            socialBonus += rainySocialAdjust;
        }

        var appliedRecovery = ApplyNeedsRecovery(playerEntityId, moodBonus, socialBonus);

        var appendixLines = CollectImpactAppendices(
            playerEntityId,
            npcEntityId,
            tier,
            memories,
            timeOfDay,
            variationSeed,
            kind,
            focusCloseFriendCount);

        if (rainyDayActive && VillageEventConfig.IsIndoorCalmNpcInteraction(kind))
            appendixLines.Add(VillageEventConfig.FormatRainyDayIndoorFeedback());

        if (appliedRecovery || appendixLines.Count > 0)
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            Log.Information(
                "Emotional bond interaction impact for player {PlayerId} with {NpcName} ({Kind}): recovery={Recovery}, mood+{Mood}, social-{Social}, appendices={AppendixCount}.",
                playerEntityId,
                npcLabel,
                kind,
                appliedRecovery,
                moodBonus,
                socialBonus,
                appendixLines.Count);
        }

        return new EmotionalBondImpactResult(appliedRecovery, moodBonus, socialBonus, appendixLines);
    }

    public async Task<EmotionalBondResponse> PerformBondingActionAsync(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EmotionalBondRequest request,
        uint variationSeed)
    {
        if (request.Kind != EmotionalBondRequestKind.Perform)
        {
            return Fail(
                request,
                EmotionalBondFailureReason.UnknownRequest,
                "Unknown emotional bond request.");
        }

        var action = request.Action;
        var npcEntityId = request.TargetNpcEntityId;

        if (!NpcEmotionalBondAgencyConfig.IsValidAction(action))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.InvalidAction,
                "Try: check on elsie | share moment with tom | help greta");
        }

        if (!NpcEmotionalBondAgencyConfig.IsValidTarget(npcEntityId))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.InvalidTarget,
                "Emotional bonding is only available with Elsie, Harold, Mira, Tom, or Greta.");
        }

        if (!TryFindTargetNpc(playerEntityId, playerX, playerZ, npcEntityId, out var distance))
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            return Fail(
                request,
                EmotionalBondFailureReason.NotInRange,
                $"{npcLabel} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m.");
        }

        var tier = _relationshipService.GetTier(playerEntityId, npcEntityId);
        var minTier = NpcEmotionalBondAgencyConfig.GetMinTier(action);
        if (tier < minTier)
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            return Fail(
                request,
                EmotionalBondFailureReason.RelationshipTooLow,
                $"You need to be at least {RelationshipTierDisplay.GetName(minTier)} with {npcLabel} for this.");
        }

        if (!_memoryService.TryConsumeGlobalBondingCooldown(playerEntityId, npcEntityId))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.OnCooldown,
                $"Take a little time before reaching out again ({NpcEmotionalBondAgencyConfig.GlobalBondingCooldownGameMinutes} game minutes).");
        }

        if (!_memoryService.TryConsumeBondingActionCooldown(playerEntityId, npcEntityId, action))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.OnCooldown,
                NpcEmotionalBondAgencyConfig.FormatBondingCooldownHint(action));
        }

        var npcLabel2 = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var archetype = _longTermGoalService.GetLegacyArchetype(playerEntityId);
        var memoriesBefore = _memoryService.GetMemoriesForNpc(playerEntityId, npcEntityId);

        // Record action-specific memory when new; active bonding also nudges companion progress.
        var actionMemory = NpcEmotionalBondAgencyConfig.GetMemoryForAction(npcEntityId, action);
        var recordedNewMemory = false;
        if (actionMemory is not null
            && await _memoryService.TryRecordMemoryAsync(playerEntityId, npcEntityId, actionMemory.Value))
        {
            recordedNewMemory = true;
            Log.Information(
                "Player {PlayerId} bonding action {Action} with {NpcName}: recorded memory {MemoryType}.",
                playerEntityId,
                action,
                npcLabel2,
                actionMemory.Value);
        }

        await _memoryService.OnFocusNpcInteractionAsync(playerEntityId, npcEntityId);

        var focusCloseFriendCountBefore = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var standingTierBefore = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCountBefore);

        var affinityGain = NpcEmotionalBondAgencyConfig.GetAffinityGain(action);
        var affinityChange = await _relationshipService.ApplyAffinityGainAsync(
            playerEntityId,
            npcEntityId,
            affinityGain);

        var tierIncreased = affinityChange.NewTier > affinityChange.PreviousTier;
        if (tierIncreased)
        {
            Log.Information(
                "Player {PlayerId} emotional bond tier with {NpcName} advanced {PreviousTier} -> {NewTier} via {Action}.",
                playerEntityId,
                npcLabel2,
                RelationshipTierDisplay.GetName(affinityChange.PreviousTier),
                RelationshipTierDisplay.GetName(affinityChange.NewTier),
                action);
        }

        await TryRecordVillageBondRecognitionMemoryAsync(playerEntityId);

        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var focusCloseFriendCountAfter = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var standingTierAfter = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCountAfter);
        var standingPromotionFeedback = VillageSocialStandingConfig.TryFormatTierPromotionFeedback(
            standingTierBefore,
            standingTierAfter,
            villageNoticed);

        var npcResponse = NpcEmotionalBondAgencyConfig.TryGetBondingActionResponse(
            npcEntityId,
            action,
            affinityChange.NewTier,
            archetype,
            variationSeed);

        var feedback = NpcEmotionalBondAgencyConfig.BuildBondDeepenedFeedback(
            npcLabel2,
            action,
            recordedNewMemory,
            tierIncreased,
            affinityChange.NewTier);

        var remembranceMemories = recordedNewMemory
            ? _memoryService.GetMemoriesForNpc(playerEntityId, npcEntityId)
            : memoriesBefore;

        var impact = TryApplyBondingActionImpact(
            playerEntityId,
            npcEntityId,
            action,
            affinityChange.NewTier,
            remembranceMemories,
            variationSeed + 31);

        var messageParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(npcResponse))
            messageParts.Add(npcResponse);

        messageParts.Add(feedback);

        if (!string.IsNullOrWhiteSpace(standingPromotionFeedback))
            messageParts.Add(standingPromotionFeedback);

        if (impact.AppliedRecovery)
        {
            var recoveryFeedback = NpcEmotionalBondImpactConfig.FormatNeedsRecoveryFeedback(
                npcLabel2,
                impact.MoodBonus,
                impact.SocialBonus);
            if (!string.IsNullOrWhiteSpace(recoveryFeedback))
                messageParts.Add(recoveryFeedback);
        }

        foreach (var appendix in impact.AppendixLines)
            messageParts.Add(appendix);

        var archetypeHint = _longTermGoalService.TryGetActiveBondingArchetypeFeedback(
            playerEntityId,
            action,
            variationSeed + 17);
        if (!string.IsNullOrWhiteSpace(archetypeHint))
            messageParts.Add(archetypeHint);

        if (NpcEmotionalBondAgencyConfig.ShouldTriggerNpcRemembrance(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes)
            && NpcEmotionalBondAgencyConfig.TryGetNpcRemembranceLine(
                npcEntityId,
                remembranceMemories,
                affinityChange.NewTier,
                archetype,
                variationSeed + 23) is { } remembrance
            && !string.IsNullOrWhiteSpace(remembrance))
        {
            messageParts.Add(remembrance);
            Log.Information(
                "NPC remembrance from {NpcName} to player {PlayerId} after {Action}: \"{Line}\"",
                npcLabel2,
                playerEntityId,
                action,
                remembrance);
        }

        var message = string.Join(" ", messageParts);

        Log.Information(
            "Player {PlayerId} performed bonding action {Action} with {NpcName}: affinity +{Gain}, newMemory={NewMemory}, message=\"{Message}\"",
            playerEntityId,
            action,
            npcLabel2,
            affinityGain,
            recordedNewMemory,
            message);

        return new EmotionalBondResponse(
            true,
            request.Kind,
            action,
            EmotionalBondFailureReason.None,
            message);
    }

    public async Task<EmotionalBondResponse> PerformStandingFavorRequestAsync(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EmotionalBondRequest request,
        uint variationSeed)
    {
        if (request.Kind != EmotionalBondRequestKind.RequestStandingFavor)
        {
            return Fail(
                request,
                EmotionalBondFailureReason.UnknownRequest,
                "Unknown social favor request.");
        }

        var npcEntityId = request.TargetNpcEntityId;

        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.InvalidTarget,
                "Social favors are only available with Elsie, Harold, Mira, Tom, or Greta.");
        }

        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);

        if (!SocialStandingActionConfig.IsEligible(standingTier))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.StandingTooLow,
                "You need Respected standing in Bloomtown (2+ focus close friends) to ask for village favors.");
        }

        if (!TryFindTargetNpc(playerEntityId, playerX, playerZ, npcEntityId, out var distance))
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            return Fail(
                request,
                EmotionalBondFailureReason.NotInRange,
                $"{npcLabel} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m.");
        }

        if (!_memoryService.TryConsumeSocialStandingFavorCooldown(playerEntityId, npcEntityId, standingTier))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.OnCooldown,
                SocialStandingActionConfig.FormatCooldownHint(standingTier));
        }

        var npcLabel2 = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var messageParts = new List<string>
        {
            SocialStandingActionConfig.FormatPlayerRequestFeedback(npcLabel2, standingTier),
        };

        if (!SocialStandingActionConfig.ShouldSucceed(
                playerEntityId,
                npcEntityId,
                standingTier,
                _worldTime.TotalGameMinutes,
                variationSeed))
        {
            var declineLine = NpcResponseGenerator.TryGetSocialStandingFavorDeclineResponse(
                npcEntityId,
                standingTier,
                variationSeed);

            if (!string.IsNullOrWhiteSpace(declineLine))
                messageParts.Add(declineLine);

            messageParts.Add(SocialStandingActionConfig.FormatDeclineFeedback(npcLabel2));

            var declineMessage = string.Join(" ", messageParts);
            Log.Information(
                "Player {PlayerId} social-standing favor declined by {NpcName} at tier {Tier}: \"{Message}\"",
                playerEntityId,
                npcLabel2,
                standingTier,
                declineMessage);

            return new EmotionalBondResponse(
                true,
                request.Kind,
                EmotionalBondActionKind.None,
                EmotionalBondFailureReason.None,
                declineMessage);
        }

        var outcome = SocialStandingActionConfig.ResolveOutcome(
            playerEntityId,
            npcEntityId,
            standingTier,
            variationSeed + 3);

        var responseLine = NpcResponseGenerator.TryGetSocialStandingFavorResponse(
            npcEntityId,
            standingTier,
            outcome,
            villageNoticed,
            variationSeed + 7);

        if (!string.IsNullOrWhiteSpace(responseLine))
            messageParts.Add(responseLine);

        messageParts.Add(SocialStandingActionConfig.FormatOutcomeFeedback(npcLabel2, outcome));

        switch (outcome)
        {
            case SocialStandingFavorOutcomeKind.Recovery:
            {
                var (moodBonus, socialBonus) = SocialStandingActionConfig.GetRecoveryBonus(standingTier);
                if (ApplyNeedsRecovery(playerEntityId, moodBonus, socialBonus))
                {
                    var recoveryFeedback = SocialStandingActionConfig.FormatRecoveryFeedback(moodBonus, socialBonus);
                    if (!string.IsNullOrWhiteSpace(recoveryFeedback))
                        messageParts.Add(recoveryFeedback);
                }

                break;
            }
            case SocialStandingFavorOutcomeKind.Item:
            {
                var grant = SocialStandingActionConfig.TryGetItemGrant(npcEntityId, standingTier, variationSeed + 11);
                if (grant is not null && _economyService.TryGetState(playerEntityId, out var economy))
                {
                    economy.Inventory.AddItem(grant.Value.ItemType, grant.Value.Quantity);
                    await _economyService.SavePlayerAsync(playerEntityId);
                    messageParts.Add(
                        SocialStandingActionConfig.FormatItemGrantFeedback(npcLabel2, grant.Value));

                    Log.Information(
                        "Social-standing favor item from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
                        npcLabel2,
                        playerEntityId,
                        grant.Value.Quantity,
                        grant.Value.ItemType);
                }

                break;
            }
        }

        await TryRecordVillageBondRecognitionMemoryAsync(playerEntityId);

        var message = string.Join(" ", messageParts);
        Log.Information(
            "Player {PlayerId} social-standing favor from {NpcName} at tier {Tier} ({Outcome}): \"{Message}\"",
            playerEntityId,
            npcLabel2,
            standingTier,
            outcome,
            message);

        return new EmotionalBondResponse(
            true,
            request.Kind,
            EmotionalBondActionKind.None,
            EmotionalBondFailureReason.None,
            message);
    }

    public async Task<EmotionalBondResponse> PerformSocialInfluenceRequestAsync(
        uint playerEntityId,
        float playerX,
        float playerZ,
        EmotionalBondRequest request,
        uint variationSeed)
    {
        if (request.Kind != EmotionalBondRequestKind.RequestSocialInfluence)
        {
            return Fail(
                request,
                EmotionalBondFailureReason.UnknownRequest,
                "Unknown social influence request.");
        }

        var npcEntityId = request.TargetNpcEntityId;

        if (!SocialInfluenceActionConfig.IsSupportedNpc(npcEntityId))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.InvalidTarget,
                "Social influence is only available with focus NPCs (Harold, Greta, Mira, Elsie, Tom, Nora, Elias, Ben, Lila, Rowan, Marcus, or Eleanor).");
        }

        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);

        if (!SocialInfluenceActionConfig.IsEligible(standingTier))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.StandingTooLow,
                "You need Respected standing in Bloomtown (2+ focus close friends) to call on focus NPCs for social influence.");
        }

        if (!TryFindTargetNpc(playerEntityId, playerX, playerZ, npcEntityId, out var distance))
        {
            var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
            return Fail(
                request,
                EmotionalBondFailureReason.NotInRange,
                $"{npcLabel} is too far away ({distance:F1}m). Move within {InteractionConfig.InteractionRadiusMeters:F0}m.");
        }

        if (!_memoryService.TryConsumeSocialInfluenceCooldown(playerEntityId, npcEntityId, standingTier))
        {
            return Fail(
                request,
                EmotionalBondFailureReason.OnCooldown,
                SocialInfluenceActionConfig.FormatCooldownHint(npcEntityId, standingTier));
        }

        var npcLabel2 = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var backingBonus = SocialInfluenceActionConfig.GetBackingProgressBonus(standingTier);
        var activityBonus = SocialInfluenceActionConfig.GetActivityBonus(standingTier);
        var messageParts = new List<string>
        {
            SocialInfluenceActionConfig.FormatPlayerRequestFeedback(npcLabel2, standingTier),
        };

        if (!SocialInfluenceActionConfig.ShouldSucceed(
                playerEntityId,
                npcEntityId,
                standingTier,
                _worldTime.TotalGameMinutes,
                variationSeed))
        {
            var declineLine = NpcResponseGenerator.TryGetSocialInfluenceDeclineResponse(
                npcEntityId,
                variationSeed,
                standingTier);

            if (!string.IsNullOrWhiteSpace(declineLine))
                messageParts.Add(declineLine);

            messageParts.Add(SocialInfluenceActionConfig.FormatDeclineFeedback(npcLabel2, standingTier));

            var declineMessage = string.Join(" ", messageParts);
            Log.Information(
                "Player {PlayerId} social-influence call declined by {NpcName}: \"{Message}\"",
                playerEntityId,
                npcLabel2,
                declineMessage);

            return new EmotionalBondResponse(
                true,
                request.Kind,
                EmotionalBondActionKind.None,
                EmotionalBondFailureReason.None,
                declineMessage);
        }

        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId,
            npcEntityId,
            variationSeed + 3,
            standingTier);

        var responseLine = NpcResponseGenerator.TryGetSocialInfluenceResponse(
            npcEntityId,
            outcome,
            villageNoticed,
            variationSeed + 7,
            standingTier);

        if (!string.IsNullOrWhiteSpace(responseLine))
            messageParts.Add(responseLine);

        messageParts.Add(SocialInfluenceActionConfig.FormatSuccessFeedback(npcLabel2, standingTier));

        switch (outcome)
        {
            case SocialInfluenceOutcomeKind.Info:
            {
                var counsel = SocialInfluenceActionConfig.TryGetActionableInfoCounsel(
                    npcEntityId,
                    variationSeed + 13,
                    standingTier);
                if (!string.IsNullOrWhiteSpace(counsel))
                    messageParts.Add(counsel);

                break;
            }

            case SocialInfluenceOutcomeKind.ProjectBacking:
                _memoryService.GrantHaroldSocialInfluenceProjectBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatProjectBackingFeedback(backingBonus, standingTier));
                break;

            case SocialInfluenceOutcomeKind.GardenBacking:
                _memoryService.GrantElsieSocialInfluenceGardenBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatGardenBackingFeedback(backingBonus, standingTier));
                break;

            case SocialInfluenceOutcomeKind.LumberBacking:
                _memoryService.GrantTomSocialInfluenceLumberBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatLumberBackingFeedback(backingBonus, standingTier));
                break;

            case SocialInfluenceOutcomeKind.HerbalBacking:
                _memoryService.GrantNoraSocialInfluenceHerbalBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatHerbalBackingFeedback(backingBonus, standingTier));
                break;

            case SocialInfluenceOutcomeKind.SmithingBacking:
                _memoryService.GrantEliasSocialInfluenceSmithingBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatSmithingBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.GuardBacking:
                _memoryService.GrantBenSocialInfluenceGuardBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatGuardBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.YouthBacking:
                _memoryService.GrantLilaSocialInfluenceYouthBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatYouthBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.StoryBacking:
                _memoryService.GrantRowanSocialInfluenceStoryBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatStoryBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.CraftingBacking:
                _memoryService.GrantMarcusSocialInfluenceCraftingBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatCraftingBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.LegacyBacking:
                _memoryService.GrantEleanorSocialInfluenceLegacyBacking(playerEntityId, backingBonus);
                messageParts.Add(SocialInfluenceActionConfig.FormatLegacyBackingFeedback(
                    backingBonus,
                    activityBonus,
                    standingTier));
                break;

            case SocialInfluenceOutcomeKind.TradePrivilege:
            {
                var buyPrivilege = SocialInfluenceActionConfig.ResolveMiraTradePrivilegeIsBuy(variationSeed + 11);
                _memoryService.GrantMiraSocialInfluenceTradePrivilege(playerEntityId, buyPrivilege);
                messageParts.Add(SocialInfluenceActionConfig.FormatTradePrivilegeFeedback(buyPrivilege));
                break;
            }

            case SocialInfluenceOutcomeKind.Recovery:
            {
                var (moodBonus, socialBonus) = SocialInfluenceActionConfig.GetRecoveryBonus(npcEntityId, standingTier);
                if (ApplyNeedsRecovery(playerEntityId, moodBonus, socialBonus))
                {
                    var recoveryFeedback = SocialInfluenceActionConfig.FormatRecoveryFeedback(
                        moodBonus,
                        socialBonus,
                        standingTier,
                        npcEntityId);
                    if (!string.IsNullOrWhiteSpace(recoveryFeedback))
                        messageParts.Add(recoveryFeedback);
                }

                break;
            }

            case SocialInfluenceOutcomeKind.Item:
            {
                var grant = SocialInfluenceActionConfig.TryGetItemGrant(
                    npcEntityId,
                    variationSeed + 11,
                    standingTier);
                if (grant is not null && _economyService.TryGetState(playerEntityId, out var economy))
                {
                    economy.Inventory.AddItem(grant.Value.ItemType, grant.Value.Quantity);
                    await _economyService.SavePlayerAsync(playerEntityId);
                    messageParts.Add(
                        SocialInfluenceActionConfig.FormatItemGrantFeedback(npcLabel2, grant.Value, standingTier));

                    Log.Information(
                        "Social-influence item from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
                        npcLabel2,
                        playerEntityId,
                        grant.Value.Quantity,
                        grant.Value.ItemType);
                }

                break;
            }
        }

        await TryRecordVillageBondRecognitionMemoryAsync(playerEntityId);

        var message = string.Join(" ", messageParts);
        Log.Information(
            "Player {PlayerId} social-influence call from {NpcName} ({Outcome}): \"{Message}\"",
            playerEntityId,
            npcLabel2,
            outcome,
            message);

        return new EmotionalBondResponse(
            true,
            request.Kind,
            EmotionalBondActionKind.None,
            EmotionalBondFailureReason.None,
            message);
    }

    private bool TryFindTargetNpc(
        uint playerEntityId,
        float playerX,
        float playerZ,
        uint targetNpcEntityId,
        out float distance)
    {
        distance = float.MaxValue;

        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (simulation.Npc.EntityId != targetNpcEntityId)
                continue;

            if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
                return false;

            var dx = playerX - simulation.Npc.PositionX;
            var dz = playerZ - simulation.Npc.PositionZ;
            distance = MathF.Sqrt(dx * dx + dz * dz);
            return distance <= InteractionConfig.InteractionRadiusMeters;
        }

        return false;
    }

    private EmotionalBondImpactResult TryApplyBondingActionImpact(
        uint playerEntityId,
        uint npcEntityId,
        EmotionalBondActionKind action,
        RelationshipTier tier,
        IReadOnlyCollection<NpcMemoryType> memories,
        uint variationSeed)
    {
        if (!NpcEmotionalBondImpactConfig.QualifiesForImpact(npcEntityId, tier, memories))
            return EmotionalBondImpactResult.None;

        var (moodBonus, socialBonus) = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(action, tier, npcEntityId);
        var rainyDayActive = VillageEventConfig.IsRainyDay(_worldTime.GameDay);
        if (rainyDayActive)
        {
            var (rainyMoodAdjust, rainySocialAdjust) =
                VillageEventConfig.GetRainyDayBondingActionAdjustments(action);
            moodBonus += rainyMoodAdjust;
            socialBonus += rainySocialAdjust;
        }

        var appliedRecovery = ApplyNeedsRecovery(playerEntityId, moodBonus, socialBonus);

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var appendixLines = new List<string>();
        if (rainyDayActive)
        {
            if (VillageEventConfig.IsIndoorCalmBondingAction(action))
                appendixLines.Add(VillageEventConfig.FormatRainyDayIndoorFeedback());
            else if (action == EmotionalBondActionKind.HelpWith)
                appendixLines.Add(VillageEventConfig.FormatRainyDayOutdoorFeedback());
        }
        if (_memoryService.TryConsumeEmotionalBondFavorCooldown(playerEntityId, npcEntityId)
            && NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondFavor(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes)
            && NpcEmotionalBondImpactConfig.TryGetBondingActionFavorLine(
                npcEntityId,
                action,
                tier,
                variationSeed + 5) is { } favorLine
            && !string.IsNullOrWhiteSpace(favorLine))
        {
            appendixLines.Add(NpcEmotionalBondImpactConfig.FormatBondFavorFeedback(npcLabel, favorLine));
            TryAppendFavorItemGrant(
                playerEntityId,
                npcEntityId,
                npcLabel,
                memories,
                tier,
                bondingAction: action,
                variationSeed + 19,
                appendixLines);
            Log.Information(
                "Emotional bond favor from {NpcName} to player {PlayerId} via {Action}: \"{Line}\"",
                NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
                playerEntityId,
                action,
                favorLine);
        }

        if (appliedRecovery || appendixLines.Count > 0)
        {
            Log.Information(
                "Emotional bond action impact for player {PlayerId} with {NpcName} ({Action}): recovery={Recovery}, mood+{Mood}, social-{Social}.",
                playerEntityId,
                NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
                action,
                appliedRecovery,
                moodBonus,
                socialBonus);
        }

        return new EmotionalBondImpactResult(appliedRecovery, moodBonus, socialBonus, appendixLines);
    }

    private List<string> CollectImpactAppendices(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        IReadOnlyCollection<NpcMemoryType> memories,
        GameTimeOfDay timeOfDay,
        uint variationSeed,
        NpcInteractionKind interactionKind,
        int focusCloseFriendCount)
    {
        var appendixLines = new List<string>();
        var archetype = _longTermGoalService.GetLegacyArchetype(playerEntityId);
        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);
        var infoChanceBonus = VillageSocialStandingImpactConfig.GetInfoChanceBonusPercent(standingTier);
        var favorChanceBonus = VillageSocialStandingImpactConfig.GetFavorChanceBonusPercent(standingTier);

        if (_memoryService.TryConsumeEmotionalBondAppreciationCooldown(playerEntityId, npcEntityId)
            && NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondAppreciation(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes)
            && NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
                npcEntityId,
                memories,
                tier,
                variationSeed + 7) is { } appreciation
            && !string.IsNullOrWhiteSpace(appreciation))
        {
            var appreciationFeedback = NpcEmotionalBondImpactConfig.FormatBondAppreciationFeedback(
                npcLabel,
                appreciation);
            appendixLines.Add(appreciationFeedback);
            Log.Information(
                "Emotional bond appreciation from {NpcName} to player {PlayerId}: \"{Line}\"",
                npcLabel,
                playerEntityId,
                appreciationFeedback);
        }

        if (_memoryService.TryConsumeEmotionalBondInfoCooldown(playerEntityId, npcEntityId)
            && NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondInfo(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes,
                archetype,
                infoChanceBonus)
            && NpcEmotionalBondImpactConfig.TryGetEmotionalBondInfoTip(
                npcEntityId,
                timeOfDay,
                archetype,
                variationSeed + 11) is { } infoTip
            && !string.IsNullOrWhiteSpace(infoTip))
        {
            var infoFeedback = NpcEmotionalBondImpactConfig.FormatBondInfoFeedback(npcLabel, infoTip);
            appendixLines.Add(infoFeedback);
            Log.Information(
                "Emotional bond info from {NpcName} to player {PlayerId}: \"{Tip}\"",
                npcLabel,
                playerEntityId,
                infoFeedback);
        }

        if (_memoryService.TryConsumeEmotionalBondFavorCooldown(playerEntityId, npcEntityId)
            && NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondFavor(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes,
                favorChanceBonus)
            && NpcEmotionalBondImpactConfig.TryGetHelpfulFavorLine(
                npcEntityId,
                interactionKind,
                memories,
                tier,
                variationSeed + 13) is { } favorLine
            && !string.IsNullOrWhiteSpace(favorLine))
        {
            var favorFeedback = NpcEmotionalBondImpactConfig.FormatBondFavorFeedback(npcLabel, favorLine);
            appendixLines.Add(favorFeedback);
            TryAppendFavorItemGrant(
                playerEntityId,
                npcEntityId,
                npcLabel,
                memories,
                tier,
                bondingAction: null,
                variationSeed + 29,
                appendixLines);
            Log.Information(
                "Emotional bond favor from {NpcName} to player {PlayerId} via {Kind}: \"{Line}\"",
                npcLabel,
                playerEntityId,
                interactionKind,
                favorFeedback);
        }

        TryAppendCrossNpcBondRecognition(
            playerEntityId,
            npcEntityId,
            tier,
            variationSeed + 37,
            appendixLines);

        TryAppendVillageBondAppreciation(
            playerEntityId,
            npcEntityId,
            tier,
            variationSeed + 43,
            appendixLines);

        TryAppendWellLikedStandingPrivilege(
            playerEntityId,
            npcEntityId,
            standingTier,
            variationSeed + 53,
            appendixLines);

        TryAppendVillageSocialStandingAwareness(
            playerEntityId,
            npcEntityId,
            standingTier,
            villageNoticed,
            variationSeed + 59,
            appendixLines);

        TryAppendSocialLegacyJourney(
            playerEntityId,
            npcEntityId,
            standingTier,
            villageNoticed,
            variationSeed + 67,
            appendixLines);

        return appendixLines;
    }

    private void TryAppendSocialLegacyJourney(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        bool villageNoticed,
        uint variationSeed,
        List<string> appendixLines)
    {
        if (!SocialLegacyConfig.IsEligibleForLegacyNpcMention(npcEntityId)
            || !SocialLegacyConfig.IsLegacyActive(standingTier))
        {
            return;
        }

        if (!_memoryService.TryConsumeSocialLegacyNpcMentionCooldown(playerEntityId, npcEntityId))
            return;

        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));
        var totalGameMinutes = _worldTime.TotalGameMinutes;
        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);

        var pillarAcknowledgment = NpcResponseGenerator.TryGetVillagePillarAcknowledgmentResponse(
            npcEntityId,
            standingTier,
            focusCloseFriendCount,
            villageNoticed,
            playerEntityId,
            totalGameMinutes,
            variationSeed);
        if (!string.IsNullOrWhiteSpace(pillarAcknowledgment))
        {
            appendixLines.Add(
                SocialLegacyConfig.FormatVillagePillarAcknowledgmentFeedback(npcLabel, pillarAcknowledgment));
            return;
        }

        var journeyLine = NpcResponseGenerator.TryGetSocialLegacyJourneyResponse(
            npcEntityId,
            standingTier,
            villageNoticed,
            playerEntityId,
            totalGameMinutes,
            variationSeed);

        if (string.IsNullOrWhiteSpace(journeyLine))
            return;

        appendixLines.Add(SocialLegacyConfig.FormatLegacyNpcMentionFeedback(npcLabel, journeyLine));
    }

    private void TryAppendVillageSocialStandingAwareness(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        bool villageNoticed,
        uint variationSeed,
        List<string> appendixLines)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
            || !VillageSocialStandingImpactConfig.IsEligibleForFocusNpcBonus(standingTier))
        {
            return;
        }

        if (!_memoryService.TryConsumeVillageSocialStandingAwarenessCooldown(playerEntityId, npcEntityId))
            return;

        if (!VillageSocialStandingImpactConfig.ShouldTriggerVillageSocialStandingAwareness(
                playerEntityId,
                npcEntityId,
                standingTier,
                _worldTime.TotalGameMinutes,
                variationSeed))
        {
            return;
        }

        var awarenessLine = VillageSocialStandingDialogue.TryGetVillageSocialStandingAwarenessLine(
            npcEntityId,
            standingTier,
            villageNoticed,
            variationSeed);

        if (string.IsNullOrWhiteSpace(awarenessLine))
            return;

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        appendixLines.Add(
            VillageSocialStandingImpactConfig.FormatVillageSocialStandingAwarenessFeedback(
                npcLabel,
                awarenessLine));
    }

    private void TryAppendWellLikedStandingPrivilege(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        uint variationSeed,
        List<string> appendixLines)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
            || !VillageSocialStandingImpactConfig.IsEligibleForWellLikedPrivilege(standingTier))
        {
            return;
        }

        if (!_memoryService.TryConsumeWellLikedStandingPrivilegeCooldown(playerEntityId, npcEntityId))
            return;

        if (!VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrivilege(
                playerEntityId,
                npcEntityId,
                standingTier,
                _worldTime.TotalGameMinutes,
                variationSeed))
        {
            return;
        }

        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var privilegeLine = VillageSocialStandingDialogue.TryGetWellLikedPrivilegeLine(
            npcEntityId,
            villageNoticed,
            variationSeed);

        if (string.IsNullOrWhiteSpace(privilegeLine))
            return;

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        appendixLines.Add(
            VillageSocialStandingImpactConfig.FormatWellLikedPrivilegeFeedback(npcLabel, privilegeLine));

        if (VillageSocialStandingImpactConfig.ShouldGrantWellLikedPrivilegeItem(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes,
                variationSeed + 7)
            && VillageSocialStandingImpactConfig.TryGetWellLikedPrivilegeItemGrant(npcEntityId, variationSeed + 7)
                is { } grant
            && _economyService.TryGetState(playerEntityId, out var economy))
        {
            economy.Inventory.AddItem(grant.ItemType, grant.Quantity);
            _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
            appendixLines.Add(
                VillageSocialStandingImpactConfig.FormatWellLikedPrivilegeItemFeedback(npcLabel, grant));

            Log.Information(
                "Well-liked standing privilege item from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
                npcLabel,
                playerEntityId,
                grant.Quantity,
                grant.ItemType);
        }

        Log.Information(
            "Well-liked standing privilege from {NpcName} to player {PlayerId}: \"{Line}\"",
            npcLabel,
            playerEntityId,
            privilegeLine);
    }

    private void TryAppendVillageBondAppreciation(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        uint variationSeed,
        List<string> appendixLines)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));

        if (!VillageBondRecognitionConfig.IsEligibleForVillageAppreciation(
                npcEntityId,
                tier,
                focusCloseFriendCount))
        {
            return;
        }

        if (!_memoryService.TryConsumeVillageBondAppreciationCooldown(playerEntityId, npcEntityId))
            return;

        if (!VillageBondRecognitionConfig.ShouldTriggerVillageAppreciation(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return;
        }

        var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
        var appreciationLine = VillageBondRecognitionConfig.TryGetVillageAppreciationLine(
            npcEntityId,
            focusCloseFriendCount,
            villageNoticed,
            variationSeed);

        if (string.IsNullOrWhiteSpace(appreciationLine))
            return;

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        appendixLines.Add(
            VillageBondRecognitionConfig.FormatVillageAppreciationFeedback(npcLabel, appreciationLine));

        var grant = VillageBondRecognitionConfig.TryGetVillageAppreciationItemGrant(npcEntityId, variationSeed + 7);
        if (grant is not null && _economyService.TryGetState(playerEntityId, out var economy))
        {
            economy.Inventory.AddItem(grant.Value.ItemType, grant.Value.Quantity);
            _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
            appendixLines.Add(
                VillageBondRecognitionConfig.FormatVillageAppreciationItemFeedback(npcLabel, grant.Value));

            Log.Information(
                "Village bond appreciation item from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
                npcLabel,
                playerEntityId,
                grant.Value.Quantity,
                grant.Value.ItemType);
        }

        Log.Information(
            "Village bond appreciation from {NpcName} to player {PlayerId}: \"{Line}\"",
            npcLabel,
            playerEntityId,
            appreciationLine);
    }

    private void TryAppendCrossNpcBondRecognition(
        uint playerEntityId,
        uint npcEntityId,
        RelationshipTier tier,
        uint variationSeed,
        List<string> appendixLines)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));

        if (!VillageBondRecognitionConfig.IsEligibleForCrossNpcRecognition(
                npcEntityId,
                tier,
                focusCloseFriendCount))
        {
            return;
        }

        if (!_memoryService.TryConsumeCrossNpcBondRecognitionCooldown(playerEntityId, npcEntityId))
            return;

        if (!VillageBondRecognitionConfig.ShouldTriggerCrossNpcRecognition(
                playerEntityId,
                npcEntityId,
                _worldTime.TotalGameMinutes))
        {
            return;
        }

        var focusCloseFriends = VillageBondRecognitionConfig.GetFocusCloseFriendNpcIds(
            id => _relationshipService.GetTier(playerEntityId, id));
        var recognitionLine = VillageBondRecognitionConfig.TryGetCrossNpcRecognitionLine(
            npcEntityId,
            focusCloseFriends,
            variationSeed);

        if (string.IsNullOrWhiteSpace(recognitionLine))
            return;

        var npcLabel = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        var feedback = VillageBondRecognitionConfig.FormatCrossNpcRecognitionFeedback(
            npcLabel,
            recognitionLine);
        appendixLines.Add(feedback);

        Log.Information(
            "Cross-NPC bond recognition from {NpcName} to player {PlayerId}: \"{Line}\"",
            npcLabel,
            playerEntityId,
            recognitionLine);
    }

    private async Task TryRecordVillageBondRecognitionMemoryAsync(uint playerEntityId)
    {
        var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
            id => _relationshipService.GetTier(playerEntityId, id));

        if (await _memoryService.TryRecordVillageNoticedBondsIfEligibleAsync(
                playerEntityId,
                focusCloseFriendCount))
        {
            Log.Information(
                "Recorded village bond recognition memory for player {PlayerId} with {CloseFriendCount} focus close friend(s).",
                playerEntityId,
                focusCloseFriendCount);
        }
    }

    /// <summary>
    /// Grants a rare small item when an emotional-bond favor fires — tangible proof the relationship matters.
    /// </summary>
    private void TryAppendFavorItemGrant(
        uint playerEntityId,
        uint npcEntityId,
        string npcLabel,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        EmotionalBondActionKind? bondingAction,
        uint variationSeed,
        List<string> appendixLines)
    {
        var memoryType = NpcEmotionalBondConfig.GetActiveEmotionalMemory(npcEntityId, memories);
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            npcEntityId,
            memoryType,
            tier,
            bondingAction,
            variationSeed);

        if (grant is null)
            return;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        economy.Inventory.AddItem(grant.Value.ItemType, grant.Value.Quantity);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var itemFeedback = NpcEmotionalBondImpactConfig.FormatBondItemGrantFeedback(npcLabel, grant.Value);
        appendixLines.Add(itemFeedback);

        Log.Information(
            "Emotional bond item grant from {NpcName} to player {PlayerId}: {Quantity}x {ItemType}.",
            npcLabel,
            playerEntityId,
            grant.Value.Quantity,
            grant.Value.ItemType);
    }

    private bool ApplyNeedsRecovery(uint playerEntityId, float moodBonus, float socialBonus)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return false;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return false;

        _needsService.ApplyEmotionalBondRecovery(economy, moodBonus, socialBonus);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        return true;
    }

    private static EmotionalBondResponse Fail(
        EmotionalBondRequest request,
        EmotionalBondFailureReason reason,
        string message) =>
        new(false, request.Kind, request.Action, reason, message);
}