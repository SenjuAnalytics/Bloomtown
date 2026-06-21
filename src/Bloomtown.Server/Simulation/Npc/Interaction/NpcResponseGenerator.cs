using Bloomtown.Server.Simulation.Npc.Schedule;
using Bloomtown.Server.Simulation.Npc.Utility;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Server.Simulation.Npc.Interaction;

/// <summary>
/// Builds NPC dialogue from personality, current state, and player relationship tier.
/// </summary>
public static class NpcResponseGenerator
{
    /// <summary>
    /// Recurring social-role line when community-help frequency earns a village habit.
    /// Caller should gate with cooldown and chance rolls via <see cref="CommunityReputationService"/>.
    /// </summary>
    public static string? TryGetRecurringSocialRoleRecognition(
        CommunityReputationState state,
        uint npcEntityId,
        uint variationSeed)
    {
        return CommunityReputationConfig.TryGetInteractionRecognition(state, npcEntityId, variationSeed);
    }

    /// <summary>
    /// Dependence-flavored line when the player has a named social role and talks to Elsie or Tom.
    /// Selected by social role so the village feels like it leans on the player's habits.
    /// </summary>
    public static string? TryGetVillageDependenceRecognition(
        CommunityReputationState state,
        uint npcEntityId,
        uint variationSeed)
    {
        var role = CommunityReputationConfig.GetDominantSocialRole(state);
        if (role == CommunitySocialRole.None)
            return null;

        return CommunityReputationConfig.TryGetInteractionRecognition(state, npcEntityId, variationSeed);
    }

    /// <summary>
    /// Post-help acknowledgment when the village already depends on or recognizes the player in this area.
    /// Caller should gate with cooldown and chance rolls via <see cref="CommunityReputationService"/>.
    /// </summary>
    public static string? TryGetCommunityHelpDependenceAcknowledgment(
        CommunityReputationState state,
        CommunityActivityKind activity,
        uint npcEntityId,
        uint variationSeed)
    {
        return CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            activity,
            state,
            npcEntityId,
            variationSeed);
    }

    /// <summary>
    /// Personal habit line — NPC shows they recognize the player's routines or community role.
    /// Caller should gate with cooldown and chance via <see cref="SocialDynamicsService"/>.
    /// </summary>
    public static string? TryGetPersonalHabitRecognition(
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        uint variationSeed) =>
        SocialDynamicsConfig.TryGetPersonalHabitLine(npcEntityId, tier, reputation, variationSeed);

    /// <summary>
    /// Light village tip appended after greet/talk — location hints, not quest-critical info.
    /// Caller should gate with cooldown and chance via <see cref="SocialDynamicsService"/>.
    /// </summary>
    public static string? TryGetLightSocialInfoTip(
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed) =>
        SocialDynamicsConfig.TryGetLightInfoTip(
            npcEntityId,
            timeOfDay,
            developmentLevel,
            completedProjectIds,
            variationSeed);

    /// <summary>
    /// Rare elder-voice recognition when legacy context warrants a special line.
    /// Caller should gate with cooldown and chance rolls via <see cref="PlayerLegacyService"/>.
    /// </summary>
    public static string? TryGetElderLegacyRecognition(
        uint npcEntityId,
        NpcInteractionKind kind,
        PlayerLegacyContext legacyContext,
        uint variationSeed)
    {
        if (!PlayerLegacyConfig.IsElderVoiceNpc(npcEntityId))
            return null;

        return PlayerLegacyConfig.TryGetElderRecognitionLine(kind, legacyContext, variationSeed);
    }

    /// <summary>
    /// Rare warmth line when a focus NPC recognizes the player's high social standing in Bloomtown.
    /// Caller should gate with <see cref="VillageSocialStandingImpactConfig.ShouldTriggerStandingWarmth"/>.
    /// </summary>
    public static string? TryGetSocialStandingWarmthResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        bool villageNoticedMemory,
        uint playerEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        if (!VillageSocialStandingImpactConfig.ShouldTriggerStandingWarmth(
                playerEntityId,
                standingTier,
                totalGameMinutes,
                variationSeed))
        {
            return null;
        }

        return VillageSocialStandingDialogue.TryGetFocusNpcStandingWarmthLine(
            npcEntityId,
            standingTier,
            villageNoticedMemory,
            variationSeed);
    }

    /// <summary>
    /// Well-liked exclusive: Harold, Greta, Mira, or Elsie greets the player with extra prestige recognition.
    /// </summary>
    public static string? TryGetWellLikedPrestigeRecognitionResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        bool villageNoticedMemory,
        uint playerEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
            || !VillageSocialStandingImpactConfig.IsEligibleForWellLikedPrivilege(standingTier))
        {
            return null;
        }

        if (!VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrestigeRecognition(
                playerEntityId,
                npcEntityId,
                standingTier,
                totalGameMinutes,
                variationSeed))
        {
            return null;
        }

        return VillageSocialStandingDialogue.TryGetWellLikedPrestigeRecognitionLine(
            npcEntityId,
            villageNoticedMemory,
            variationSeed);
    }

    /// <summary>
    /// Well-liked exclusive: Harold, Greta, or Elsie acknowledges the player's long social journey.
    /// </summary>
    public static string? TryGetSocialLegacyJourneyResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        bool villageNoticedMemory,
        uint playerEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!SocialLegacyConfig.IsEligibleForLegacyNpcMention(npcEntityId)
            || !SocialLegacyConfig.IsLegacyActive(standingTier))
        {
            return null;
        }

        if (!SocialLegacyConfig.ShouldTriggerLegacyNpcMention(
                playerEntityId,
                npcEntityId,
                standingTier,
                totalGameMinutes,
                variationSeed))
        {
            return null;
        }

        return VillageSocialStandingConfig.TryGetLegacyJourneyLine(
            npcEntityId,
            standingTier,
            villageNoticedMemory,
            variationSeed);
    }

    /// <summary>
    /// Well-liked Village Pillar milestone: Harold, Greta, Rowan, Marcus, or Eleanor speaks of the player as a village pillar.
    /// </summary>
    public static string? TryGetVillagePillarAcknowledgmentResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        int focusCloseFriendCount,
        bool villageNoticedMemory,
        uint playerEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!SocialLegacyConfig.IsLegacyActive(standingTier)
            || !SocialLegacyConfig.HasMilestone(
                SocialMilestoneKind.VillagePillar,
                standingTier,
                focusCloseFriendCount))
        {
            return null;
        }

        if (!SocialLegacyConfig.ShouldUseVillagePillarAcknowledgment(
                playerEntityId,
                npcEntityId,
                totalGameMinutes,
                variationSeed))
        {
            return null;
        }

        return SocialLegacyDialogue.TryGetVillagePillarAcknowledgmentLine(
            npcEntityId,
            villageNoticedMemory,
            variationSeed);
    }

    /// <summary>
    /// NPC response when a Respected or Well-liked player actively calls on focus NPCs for social influence.
    /// </summary>
    public static string? TryGetSocialInfluenceResponse(
        uint npcEntityId,
        SocialInfluenceOutcomeKind outcome,
        bool villageNoticedMemory,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (!SocialInfluenceActionConfig.IsSupportedNpc(npcEntityId))
            return null;

        return SocialInfluenceActionDialogue.TryGetSuccessResponseLine(
            npcEntityId,
            outcome,
            villageNoticedMemory,
            variationSeed,
            tier);
    }

    /// <summary>
    /// NPC response when a social-influence call fails the success roll.
    /// </summary>
    public static string? TryGetSocialInfluenceDeclineResponse(
        uint npcEntityId,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (!SocialInfluenceActionConfig.IsSupportedNpc(npcEntityId))
            return null;

        return SocialInfluenceActionDialogue.TryGetDeclineLine(npcEntityId, variationSeed, tier);
    }

    /// <summary>
    /// NPC response when a player actively requests a social-standing favor and succeeds.
    /// </summary>
    public static string? TryGetSocialStandingFavorResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        SocialStandingFavorOutcomeKind outcome,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        return SocialStandingActionDialogue.TryGetSuccessResponseLine(
            npcEntityId,
            standingTier,
            outcome,
            villageNoticedMemory,
            variationSeed);
    }

    /// <summary>
    /// NPC response when a standing favor request fails the success roll.
    /// </summary>
    public static string? TryGetSocialStandingFavorDeclineResponse(
        uint npcEntityId,
        VillageSocialStandingTier standingTier,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        return SocialStandingActionDialogue.TryGetDeclineLine(npcEntityId, standingTier, variationSeed);
    }

    /// <summary>
    /// Emotional bond line for Elsie or Harold when shared memories warrant a warmer response.
    /// Caller should prefer this over generic personalized lines for focus NPCs.
    /// </summary>
    public static string? TryGetEmotionalBondResponse(
        uint npcEntityId,
        NpcInteractionKind kind,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        LegacyArchetype archetype,
        uint variationSeed) =>
        NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            npcEntityId,
            kind,
            memories,
            tier,
            archetype,
            variationSeed);

    /// <summary>
    /// Archetype-linked emotional warmth from Elsie or Harold — legacy identity meets personal trust.
    /// </summary>
    public static string? TryGetArchetypeEmotionalBondLine(
        uint npcEntityId,
        LegacyArchetype archetype,
        RelationshipTier tier,
        uint variationSeed) =>
        NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(npcEntityId, archetype, tier, variationSeed);

    /// <summary>
    /// Scheduled village event line during greet/talk — Market Day, Community Work Day, or Rainy Day.
    /// </summary>
    public static string? TryGetVillageEventInteractionLine(
        uint playerEntityId,
        uint npcEntityId,
        int gameDay,
        uint variationSeed) =>
        VillageEventDialogue.TryGetNpcInteractionLine(playerEntityId, npcEntityId, gameDay, variationSeed);

    /// <summary>
    /// Ambient villager comment when a scheduled village event is active today.
    /// </summary>
    public static string? TryGetVillageEventAmbientComment(
        int gameDay,
        uint variationSeed)
    {
        var events = VillageEventConfig.GetActiveEvents(gameDay);
        if (events.Count == 0)
            return null;

        var eventKind = events[(int)(variationSeed % (uint)events.Count)];
        return VillageEventDialogue.TryGetVillagerAmbientComment(eventKind, variationSeed);
    }

    public static string Generate(
        NpcSimulationState simulation,
        NpcInteractionKind kind,
        RelationshipTier relationshipTier,
        IReadOnlyList<NpcMemoryType> memories,
        VillageDevelopmentLevel developmentLevel = VillageDevelopmentLevel.Quiet,
        uint flavorSeed = 0)
    {
        if (simulation.Needs.IsHungerCritical || simulation.Needs.IsEnergyCritical)
            return GenerateNeedConstrainedResponse(simulation, kind);

        // Friend+ with shared memories: use a fuller personalized line instead of generic small talk.
        if (relationshipTier >= RelationshipTier.Friend && memories.Count > 0)
        {
            var personalized = NpcMemoryConfig.TryGetPersonalizedResponse(
                kind,
                memories,
                relationshipTier,
                flavorSeed);
            if (personalized is not null)
                return ApplyVillageAtmosphereTone(personalized, kind, developmentLevel, flavorSeed);
        }

        var baseResponse = relationshipTier switch
        {
            RelationshipTier.Stranger => GenerateStrangerResponse(simulation, kind),
            RelationshipTier.CloseFriend => ApplyCloseFriendTone(GenerateAcquaintanceResponse(simulation, kind), kind),
            RelationshipTier.Friend => ApplyFriendTone(GenerateAcquaintanceResponse(simulation, kind), kind),
            _ => GenerateAcquaintanceResponse(simulation, kind),
        };

        var withMemory = ApplyMemoryTone(baseResponse, kind, memories, flavorSeed);
        return ApplyVillageAtmosphereTone(withMemory, kind, developmentLevel, flavorSeed);
    }

    private static string GenerateNeedConstrainedResponse(NpcSimulationState simulation, NpcInteractionKind kind)
    {
        if (simulation.Needs.IsHungerCritical)
        {
            return kind == NpcInteractionKind.Greet
                ? "Hello! Sorry, I'm quite hungry right now — I was heading to eat."
                : "Honestly, I can barely focus — I need to find something to eat soon.";
        }

        return kind == NpcInteractionKind.Greet
            ? "Oh, hello... I'm exhausted. I really need some rest."
            : "I'm too tired to chat properly. I should rest first.";
    }

    private static string GenerateStrangerResponse(NpcSimulationState simulation, NpcInteractionKind kind)
    {
        if (kind == NpcInteractionKind.Talk)
        {
            return simulation.Personality switch
            {
                NpcPersonalityTrait.Diligent => "I'm not sure I know you well enough to chat yet.",
                NpcPersonalityTrait.Social => "Hi... we haven't really talked before, have we?",
                _ => "Sorry, I don't think we've met properly yet.",
            };
        }

        return simulation.Personality switch
        {
            NpcPersonalityTrait.Diligent => "Hello.",
            NpcPersonalityTrait.Social => "Oh, hi there.",
            NpcPersonalityTrait.Lazy => "Hey...",
            _ => "Hello.",
        };
    }

    private static string GenerateAcquaintanceResponse(NpcSimulationState simulation, NpcInteractionKind kind)
    {
        return kind switch
        {
            NpcInteractionKind.Greet => GenerateGreet(simulation),
            NpcInteractionKind.Talk => GenerateTalk(simulation),
            _ => "I'm not sure how to respond to that.",
        };
    }

    private static string ApplyFriendTone(string baseResponse, NpcInteractionKind kind)
    {
        return kind == NpcInteractionKind.Greet
            ? $"{baseResponse} Good to see a familiar face!"
            : $"{baseResponse} I always enjoy our conversations.";
    }

    private static string ApplyCloseFriendTone(string baseResponse, NpcInteractionKind kind)
    {
        return kind == NpcInteractionKind.Greet
            ? $"Ah, it's you! {baseResponse} You know you're always welcome here."
            : $"{baseResponse} Talking with you really brightens my day in Bloomtown — I'm glad we're close.";
    }

    private static string GenerateGreet(NpcSimulationState simulation)
    {
        return simulation.Personality switch
        {
            NpcPersonalityTrait.Diligent => simulation.CurrentActivity switch
            {
                NpcActivityType.Work => "Hello! I'm busy with work, but it's good to see you.",
                NpcActivityType.Patrol => "Good day! I'm on patrol — stay safe out there.",
                NpcActivityType.Eat => "Hi there! I was just about to eat. Care to join?",
                NpcActivityType.Rest => "Hello. I'm taking a short break.",
                NpcActivityType.Social => "Hello! Nice to meet someone new.",
                _ => "Hello! Good to see you.",
            },
            NpcPersonalityTrait.Social => simulation.CurrentActivity switch
            {
                NpcActivityType.Social => "Hey! I was hoping someone would come by to chat!",
                NpcActivityType.Patrol => "Hey there! Great to see a friendly face on my patrol.",
                NpcActivityType.Work => "Hi! Work can wait — it's always nice to say hello.",
                NpcActivityType.Eat => "Hello! Food tastes better with company.",
                NpcActivityType.Rest => "Oh, hi! Even resting is better with a visitor.",
                _ => "Hey! So glad you stopped by!",
            },
            NpcPersonalityTrait.Lazy => "Hey... hello. Hope you're having a relaxing day too.",
            _ => "Hello!",
        };
    }

    private static string GenerateTalk(NpcSimulationState simulation)
    {
        if (simulation.Needs.IsSocialNeedElevated)
        {
            return simulation.Personality switch
            {
                NpcPersonalityTrait.Social =>
                    "It's been quiet around here. I really appreciate you stopping to talk!",
                NpcPersonalityTrait.Diligent =>
                    "A quick chat is fine — I've been keeping busy, but company is welcome.",
                _ => "Thanks for chatting. It's been a bit lonely.",
            };
        }

        return simulation.Personality switch
        {
            NpcPersonalityTrait.Diligent => simulation.CurrentActivity switch
            {
                NpcActivityType.Work => "I'm focused on my tasks today. Bloomtown keeps us all busy!",
                NpcActivityType.Patrol => "I patrol this area regularly. Everything looks peaceful so far.",
                _ => "It's a productive day in Bloomtown. How are things on your end?",
            },
            NpcPersonalityTrait.Social => simulation.CurrentActivity switch
            {
                NpcActivityType.Social => "I love meeting people! Tell me what you've been up to.",
                NpcActivityType.Patrol => "Patrolling is more fun when I can wave at folks like you.",
                _ => "Always happy to chat! The town feels more alive with visitors around.",
            },
            NpcPersonalityTrait.Lazy => "Mmm, not much to say... just taking things slow today.",
            _ => "Nice weather for a walk around town, isn't it?",
        };
    }

    /// <summary>
    /// Builds NPC dialogue when receiving a gift, based on relationship tier and item preference.
    /// </summary>
    public static string GenerateGiftResponse(
        NpcSimulationState simulation,
        ItemType itemType,
        bool isPreferred,
        RelationshipTier relationshipTier,
        IReadOnlyList<NpcMemoryType> memories,
        bool justRecordedFirstPreferredGift = false)
    {
        if (simulation.Needs.IsHungerCritical || simulation.Needs.IsEnergyCritical)
            return GenerateGiftNeedConstrainedResponse(itemType);

        if (justRecordedFirstPreferredGift)
            return NpcMemoryConfig.GetFirstPreferredGiftRecordedLine();

        var baseResponse = relationshipTier switch
        {
            RelationshipTier.Stranger => GenerateStrangerGiftResponse(simulation, itemType, isPreferred),
            RelationshipTier.CloseFriend => ApplyCloseFriendGiftTone(
                GenerateWarmGiftResponse(simulation, itemType, isPreferred),
                itemType,
                isPreferred),
            RelationshipTier.Friend => ApplyFriendGiftTone(
                GenerateWarmGiftResponse(simulation, itemType, isPreferred),
                itemType,
                isPreferred),
            _ => GenerateNeutralGiftResponse(simulation, itemType, isPreferred),
        };

        var memoryLine = NpcMemoryConfig.TryGetGiftMemoryLine(
            memories,
            isPreferred,
            relationshipTier,
            variationSeed: (uint)(itemType + (byte)relationshipTier));
        return memoryLine is null ? baseResponse : $"{baseResponse} {memoryLine}";
    }

    /// <summary>
    /// Appends a short memory-aware line so repeat interactions feel personal.
    /// </summary>
    /// <summary>Appends a lighter memory-aware line for acquaintances who share some history.</summary>
    private static string ApplyMemoryTone(
        string baseResponse,
        NpcInteractionKind kind,
        IReadOnlyList<NpcMemoryType> memories,
        uint variationSeed)
    {
        if (memories.Count == 0)
            return baseResponse;

        var memoryLine = kind switch
        {
            NpcInteractionKind.Talk => NpcMemoryConfig.TryGetTalkMemoryLine(memories, variationSeed),
            NpcInteractionKind.Greet => NpcMemoryConfig.TryGetGreetMemoryLine(memories, variationSeed),
            _ => null,
        };

        return memoryLine is null ? baseResponse : $"{baseResponse} {memoryLine}";
    }

    /// <summary>Adds a short village-growth flavor line when the town feels Lively or Bustling.</summary>
    private static string ApplyVillageAtmosphereTone(
        string baseResponse,
        NpcInteractionKind kind,
        VillageDevelopmentLevel developmentLevel,
        uint flavorSeed)
    {
        if (kind is not (NpcInteractionKind.Greet or NpcInteractionKind.Talk))
            return baseResponse;

        var flavorKind = kind == NpcInteractionKind.Greet
            ? NpcInteractionFlavorKind.Greet
            : NpcInteractionFlavorKind.Talk;

        var flavorLine = VillageAtmosphereConfig.TryGetInteractionFlavor(developmentLevel, flavorKind, flavorSeed);
        return flavorLine is null ? baseResponse : $"{baseResponse} {flavorLine}";
    }

    private static string GenerateGiftNeedConstrainedResponse(ItemType itemType)
    {
        var itemName = ItemDatabase.GetDisplayName(itemType);
        return $"Oh... a {itemName}? That's kind, but I'm not feeling well enough to appreciate it properly right now.";
    }

    private static string GenerateStrangerGiftResponse(
        NpcSimulationState simulation,
        ItemType itemType,
        bool isPreferred)
    {
        var itemName = ItemDatabase.GetDisplayName(itemType);
        if (isPreferred)
        {
            return simulation.Personality switch
            {
                NpcPersonalityTrait.Social => $"Oh! {itemName}? That's... actually quite thoughtful. Thank you.",
                _ => $"A {itemName}? That is unexpectedly nice. Thank you.",
            };
        }

        return simulation.Personality switch
        {
            NpcPersonalityTrait.Social => $"For me? A {itemName}? Well... thank you, I suppose.",
            NpcPersonalityTrait.Diligent => $"A {itemName}? I wasn't expecting a gift. Thank you.",
            _ => $"Oh... a {itemName}. Thanks.",
        };
    }

    private static string GenerateNeutralGiftResponse(
        NpcSimulationState simulation,
        ItemType itemType,
        bool isPreferred)
    {
        var itemName = ItemDatabase.GetDisplayName(itemType);
        if (isPreferred)
            return $"A {itemName}! You remembered what I like. Thank you!";

        return simulation.Personality switch
        {
            NpcPersonalityTrait.Social => $"A {itemName} for me? How sweet — thank you!",
            _ => $"Thank you for the {itemName}. I appreciate it.",
        };
    }

    private static string GenerateWarmGiftResponse(
        NpcSimulationState simulation,
        ItemType itemType,
        bool isPreferred)
    {
        var itemName = ItemDatabase.GetDisplayName(itemType);
        if (isPreferred)
        {
            return simulation.Personality switch
            {
                NpcPersonalityTrait.Social => $"You know I adore {itemName}! You're the best — thank you!",
                _ => $"Perfect — a {itemName} is exactly what I needed. Thank you so much!",
            };
        }

        return $"A {itemName}? That is really thoughtful of you. Thank you!";
    }

    private static string ApplyFriendGiftTone(string baseResponse, ItemType itemType, bool isPreferred)
    {
        return isPreferred
            ? $"{baseResponse} Gifts like this always make me smile."
            : $"{baseResponse} I'm glad we're getting to know each other.";
    }

    private static string ApplyCloseFriendGiftTone(string baseResponse, ItemType itemType, bool isPreferred)
    {
        return isPreferred
            ? $"{baseResponse} You always know how to brighten my day in Bloomtown."
            : $"{baseResponse} It means a lot coming from you.";
    }
}