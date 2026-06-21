using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bonds with focus NPCs (Elsie, Harold, Mira, Tom, Greta, Nora, Elias, Ben, Lila, Rowan, Marcus, Eleanor): memory thresholds, rare personal moments,
/// and archetype-linked warmth integrated with legacy identity.
/// </summary>
public static class NpcEmotionalBondConfig
{
    public const int FrequentCompanionInteractionThreshold = 5;
    public const int FrequentAreaHelpThreshold = 3;

    public const int EmotionalMomentCooldownGameMinutes = 80;
    public const int EmotionalMomentChancePercent = 10;

    public const int EmotionalArchetypeBondCooldownGameMinutes = 45;
    public const int EmotionalArchetypeBondChancePercent = 32;

    public const int EmotionalAmbientCooldownGameMinutes = 55;
    public const int EmotionalAmbientChancePercent = 18;

    public const int CommunityHelpEmotionalAckChancePercent = 24;

    public const RelationshipTier MinEmotionalInteractionTier = RelationshipTier.Friend;

    private static readonly NpcMemoryType[] ElsieMemoryPriority =
    [
        NpcMemoryType.HelpedGardenOften,
        NpcMemoryType.FrequentElsieCompanion,
        NpcMemoryType.GaveFavoriteGiftToElsie,
        NpcMemoryType.SpentQuietTimeWithElsie,
        NpcMemoryType.SharedMomentWithElsie,
        NpcMemoryType.ConsciouslyHelpedElsie,
        NpcMemoryType.CheckedOnElsie,
    ];

    private static readonly NpcMemoryType[] HaroldMemoryPriority =
    [
        NpcMemoryType.HelpedWellOften,
        NpcMemoryType.FrequentHaroldCompanion,
        NpcMemoryType.GaveFavoriteGiftToHarold,
        NpcMemoryType.SpentQuietTimeWithHarold,
        NpcMemoryType.SharedMomentWithHarold,
        NpcMemoryType.ConsciouslyHelpedHarold,
        NpcMemoryType.CheckedOnHarold,
    ];

    private static readonly NpcMemoryType[] MiraMemoryPriority =
    [
        NpcMemoryType.HelpedMarketOften,
        NpcMemoryType.FrequentMiraCompanion,
        NpcMemoryType.GaveFavoriteGiftToMira,
        NpcMemoryType.SpentQuietTimeWithMira,
        NpcMemoryType.SharedMomentWithMira,
        NpcMemoryType.ConsciouslyHelpedMira,
        NpcMemoryType.CheckedOnMira,
    ];

    private static readonly NpcMemoryType[] TomMemoryPriority =
    [
        NpcMemoryType.HelpedLumberOften,
        NpcMemoryType.FrequentTomCompanion,
        NpcMemoryType.GaveFavoriteGiftToTom,
        NpcMemoryType.SpentQuietTimeWithTom,
        NpcMemoryType.SharedMomentWithTom,
        NpcMemoryType.ConsciouslyHelpedTom,
        NpcMemoryType.CheckedOnTom,
    ];

    private static readonly NpcMemoryType[] GretaMemoryPriority =
    [
        NpcMemoryType.HelpedInnOften,
        NpcMemoryType.FrequentGretaCompanion,
        NpcMemoryType.GaveFavoriteGiftToGreta,
        NpcMemoryType.SpentQuietTimeWithGreta,
        NpcMemoryType.SharedMomentWithGreta,
        NpcMemoryType.ConsciouslyHelpedGreta,
        NpcMemoryType.CheckedOnGreta,
    ];

    private static readonly NpcMemoryType[] NoraMemoryPriority =
    [
        NpcMemoryType.HelpedHerbGardenOften,
        NpcMemoryType.FrequentNoraCompanion,
        NpcMemoryType.GaveFavoriteGiftToNora,
        NpcMemoryType.SpentQuietTimeWithNora,
        NpcMemoryType.SharedMomentWithNora,
        NpcMemoryType.ConsciouslyHelpedNora,
        NpcMemoryType.CheckedOnNora,
    ];

    private static readonly NpcMemoryType[] EliasMemoryPriority =
    [
        NpcMemoryType.HelpedSmithyOften,
        NpcMemoryType.FrequentEliasCompanion,
        NpcMemoryType.GaveFavoriteGiftToElias,
        NpcMemoryType.SpentQuietTimeWithElias,
        NpcMemoryType.SharedMomentWithElias,
        NpcMemoryType.ConsciouslyHelpedElias,
        NpcMemoryType.CheckedOnElias,
    ];

    private static readonly NpcMemoryType[] BenMemoryPriority =
    [
        NpcMemoryType.HelpedPatrolOften,
        NpcMemoryType.FrequentBenCompanion,
        NpcMemoryType.GaveFavoriteGiftToBen,
        NpcMemoryType.SpentQuietTimeWithBen,
        NpcMemoryType.SharedMomentWithBen,
        NpcMemoryType.ConsciouslyHelpedBen,
        NpcMemoryType.CheckedOnBen,
    ];

    private static readonly NpcMemoryType[] LilaMemoryPriority =
    [
        NpcMemoryType.HelpedVillageOften,
        NpcMemoryType.FrequentLilaCompanion,
        NpcMemoryType.GaveFavoriteGiftToLila,
        NpcMemoryType.SpentQuietTimeWithLila,
        NpcMemoryType.SharedMomentWithLila,
        NpcMemoryType.ConsciouslyHelpedLila,
        NpcMemoryType.CheckedOnLila,
    ];

    private static readonly NpcMemoryType[] RowanMemoryPriority =
    [
        NpcMemoryType.ListenedToStoriesOften,
        NpcMemoryType.FrequentRowanCompanion,
        NpcMemoryType.GaveFavoriteGiftToRowan,
        NpcMemoryType.SpentQuietTimeWithRowan,
        NpcMemoryType.SharedMomentWithRowan,
        NpcMemoryType.ConsciouslyHelpedRowan,
        NpcMemoryType.CheckedOnRowan,
    ];

    private static readonly NpcMemoryType[] MarcusMemoryPriority =
    [
        NpcMemoryType.HelpedWorkshopOften,
        NpcMemoryType.FrequentMarcusCompanion,
        NpcMemoryType.GaveFavoriteGiftToMarcus,
        NpcMemoryType.SpentQuietTimeWithMarcus,
        NpcMemoryType.SharedMomentWithMarcus,
        NpcMemoryType.ConsciouslyHelpedMarcus,
        NpcMemoryType.CheckedOnMarcus,
    ];

    private static readonly NpcMemoryType[] EleanorMemoryPriority =
    [
        NpcMemoryType.ListenedToEleanorStories,
        NpcMemoryType.FrequentEleanorCompanion,
        NpcMemoryType.GaveFavoriteGiftToEleanor,
        NpcMemoryType.SpentQuietTimeWithEleanor,
        NpcMemoryType.SharedMomentWithEleanor,
        NpcMemoryType.ConsciouslyHelpedEleanor,
        NpcMemoryType.CheckedOnEleanor,
    ];

    public static readonly uint[] FocusNpcEntityIds =
    [
        NpcEntityIds.Elsie,
        NpcEntityIds.Harold,
        NpcEntityIds.Mira,
        NpcEntityIds.Tom,
        NpcEntityIds.Greta,
        NpcEntityIds.Nora,
        NpcEntityIds.Elias,
        NpcEntityIds.Ben,
        NpcEntityIds.Lila,
        NpcEntityIds.Rowan,
        NpcEntityIds.Marcus,
        NpcEntityIds.Eleanor,
    ];

    public static bool IsFocusNpc(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Elsie or NpcEntityIds.Harold or NpcEntityIds.Mira or NpcEntityIds.Tom
            or NpcEntityIds.Greta or NpcEntityIds.Nora or NpcEntityIds.Elias or NpcEntityIds.Ben
            or NpcEntityIds.Lila or NpcEntityIds.Rowan or NpcEntityIds.Marcus or NpcEntityIds.Eleanor;

    public static NpcMemoryType? GetCompanionMemoryForNpc(uint npcEntityId) =>
        npcEntityId switch
        {
            NpcEntityIds.Elsie => NpcMemoryType.FrequentElsieCompanion,
            NpcEntityIds.Harold => NpcMemoryType.FrequentHaroldCompanion,
            NpcEntityIds.Mira => NpcMemoryType.FrequentMiraCompanion,
            NpcEntityIds.Tom => NpcMemoryType.FrequentTomCompanion,
            NpcEntityIds.Greta => NpcMemoryType.FrequentGretaCompanion,
            NpcEntityIds.Nora => NpcMemoryType.FrequentNoraCompanion,
            NpcEntityIds.Elias => NpcMemoryType.FrequentEliasCompanion,
            NpcEntityIds.Ben => NpcMemoryType.FrequentBenCompanion,
            NpcEntityIds.Lila => NpcMemoryType.FrequentLilaCompanion,
            NpcEntityIds.Rowan => NpcMemoryType.FrequentRowanCompanion,
            NpcEntityIds.Marcus => NpcMemoryType.FrequentMarcusCompanion,
            NpcEntityIds.Eleanor => NpcMemoryType.FrequentEleanorCompanion,
            _ => null,
        };

    public static NpcMemoryType? GetAreaHelpMemoryForActivity(CommunityActivityKind activity) =>
        activity switch
        {
            CommunityActivityKind.HelpGarden => NpcMemoryType.HelpedGardenOften,
            CommunityActivityKind.HelpWell => NpcMemoryType.HelpedWellOften,
            CommunityActivityKind.HelpMarket => NpcMemoryType.HelpedMarketOften,
            CommunityActivityKind.HelpLumber => NpcMemoryType.HelpedLumberOften,
            CommunityActivityKind.HelpInn => NpcMemoryType.HelpedInnOften,
            CommunityActivityKind.HelpHerbGarden => NpcMemoryType.HelpedHerbGardenOften,
            CommunityActivityKind.HelpSmithy => NpcMemoryType.HelpedSmithyOften,
            CommunityActivityKind.HelpPatrol => NpcMemoryType.HelpedPatrolOften,
            CommunityActivityKind.HelpVillage => NpcMemoryType.HelpedVillageOften,
            CommunityActivityKind.ListenToStories => NpcMemoryType.ListenedToStoriesOften,
            CommunityActivityKind.HelpWorkshop => NpcMemoryType.HelpedWorkshopOften,
            CommunityActivityKind.ChatWithEleanor => NpcMemoryType.ListenedToEleanorStories,
            _ => null,
        };

    public static uint GetFocusNpcForActivity(CommunityActivityKind activity) =>
        activity switch
        {
            CommunityActivityKind.HelpGarden => NpcEntityIds.Elsie,
            CommunityActivityKind.HelpWell => NpcEntityIds.Harold,
            CommunityActivityKind.HelpMarket => NpcEntityIds.Mira,
            CommunityActivityKind.HelpLumber => NpcEntityIds.Tom,
            CommunityActivityKind.HelpInn => NpcEntityIds.Greta,
            CommunityActivityKind.HelpHerbGarden => NpcEntityIds.Nora,
            CommunityActivityKind.HelpSmithy => NpcEntityIds.Elias,
            CommunityActivityKind.HelpPatrol => NpcEntityIds.Ben,
            CommunityActivityKind.HelpVillage => NpcEntityIds.Lila,
            CommunityActivityKind.ListenToStories => NpcEntityIds.Rowan,
            CommunityActivityKind.HelpWorkshop => NpcEntityIds.Marcus,
            CommunityActivityKind.ChatWithEleanor => NpcEntityIds.Eleanor,
            _ => 0,
        };

    public static bool HasEmotionalMemory(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories) =>
        GetActiveEmotionalMemory(npcEntityId, memories) is not null;

    /// <summary>
    /// Full emotional greet/talk line when focus NPC remembers something specific about the player.
    /// </summary>
    public static string? TryGetEmotionalInteractionResponse(
        uint npcEntityId,
        NpcInteractionKind kind,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId) || tier < MinEmotionalInteractionTier)
            return null;

        if (kind is not (NpcInteractionKind.Talk or NpcInteractionKind.Greet))
            return null;

        var memoryType = GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null)
            return null;

        var lines = NpcEmotionalBondDialogue.GetEmotionalInteractionLines(
            npcEntityId,
            kind,
            memoryType.Value,
            tier,
            archetype);

        return PickLine(lines, variationSeed);
    }

    /// <summary>
    /// Rare appendix linking the player's legacy archetype to a personal bond with Elsie or Harold.
    /// </summary>
    public static string? TryGetArchetypeEmotionalBondLine(
        uint npcEntityId,
        LegacyArchetype archetype,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId) || archetype == LegacyArchetype.None || tier < MinEmotionalInteractionTier)
            return null;

        var lines = NpcEmotionalBondDialogue.GetArchetypeEmotionalBondLines(npcEntityId, archetype, tier);
        return PickLine(lines, variationSeed);
    }

    /// <summary>Rare personal moment — NPC recalls something specific with emotional warmth.</summary>
    public static string? TryGetEmotionalPersonalMoment(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId) || tier < MinEmotionalInteractionTier)
            return null;

        var memoryType = GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null)
            return null;

        var lines = NpcEmotionalBondDialogue.GetEmotionalPersonalMomentLines(
            npcEntityId,
            memoryType.Value,
            tier);

        return PickLine(lines, variationSeed);
    }

    public static string? TryGetCommunityHelpEmotionalAcknowledgment(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId))
            return null;

        var memoryType = GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null && archetype == LegacyArchetype.None)
            return null;

        var fallbackMemory = GetCompanionMemoryForNpc(npcEntityId) ?? NpcMemoryType.FrequentElsieCompanion;
        var lines = NpcEmotionalBondDialogue.GetCommunityHelpEmotionalLines(
            npcEntityId,
            memoryType ?? fallbackMemory,
            archetype);

        return PickLine(lines, variationSeed);
    }

    public static string? TryGetEmotionalMilestoneBondLine(
        uint npcEntityId,
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId))
            return null;

        var lines = NpcEmotionalBondDialogue.GetEmotionalMilestoneLines(npcEntityId, milestone, archetype);
        return PickLine(lines, variationSeed);
    }

    public static string? TryGetEmotionalAmbientLine(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsFocusNpc(npcEntityId))
            return null;

        var memoryType = GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null && archetype == LegacyArchetype.None)
            return null;

        var lines = NpcEmotionalBondDialogue.GetEmotionalAmbientLines(
            npcEntityId,
            memoryType ?? NpcMemoryType.FrequentElsieCompanion,
            archetype);

        return PickLine(lines, variationSeed);
    }

    public static bool ShouldTriggerEmotionalPersonalMoment(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 43 + npcEntityId * 29 + (uint)(totalGameMinutes % 983)) % 100;
        return roll < EmotionalMomentChancePercent;
    }

    public static bool ShouldTriggerArchetypeEmotionalBond(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 59 + npcEntityId * 23 + (uint)(totalGameMinutes % 947)) % 100;
        return roll < EmotionalArchetypeBondChancePercent;
    }

    public static bool ShouldTriggerEmotionalAmbient(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 61 + npcEntityId * 31 + (uint)(totalGameMinutes % 929)) % 100;
        return roll < EmotionalAmbientChancePercent;
    }

    public static bool ShouldTriggerCommunityHelpEmotionalAck(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes)
    {
        var roll = (playerEntityId * 73 + npcEntityId * 41 + (uint)(totalGameMinutes % 911)) % 100;
        return roll < CommunityHelpEmotionalAckChancePercent;
    }

    public static string? FormatEmotionalBondHint(uint npcEntityId, IReadOnlyCollection<NpcMemoryType> memories)
    {
        if (!IsFocusNpc(npcEntityId))
            return null;

        var memoryType = GetActiveEmotionalMemory(npcEntityId, memories);
        return memoryType switch
        {
            NpcMemoryType.HelpedGardenOften => "feels close — remembers your garden help",
            NpcMemoryType.FrequentElsieCompanion => "feels genuinely fond of you",
            NpcMemoryType.HelpedWellOften => "feels close — remembers your well-side help",
            NpcMemoryType.FrequentHaroldCompanion => "trusts you like an old friend",
            NpcMemoryType.CheckedOnElsie => "appreciates that you check on her",
            NpcMemoryType.CheckedOnHarold => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithElsie => "treasures a quiet moment you shared",
            NpcMemoryType.SharedMomentWithHarold => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedElsie => "remembers when you offered hands-on help",
            NpcMemoryType.ConsciouslyHelpedHarold => "remembers when you lent a hand personally",
            NpcMemoryType.HelpedMarketOften => "feels close — remembers your market help",
            NpcMemoryType.FrequentMiraCompanion => "considers you a regular at the square",
            NpcMemoryType.CheckedOnMira => "appreciates that you check on her",
            NpcMemoryType.SharedMomentWithMira => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedMira => "remembers when you lent a hand at the market",
            NpcMemoryType.HelpedLumberOften => "feels close — remembers your lumber-yard help",
            NpcMemoryType.FrequentTomCompanion => "considers you a familiar face at the woodpile",
            NpcMemoryType.CheckedOnTom => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithTom => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedTom => "remembers when you lent a hand personally",
            NpcMemoryType.GaveFavoriteGiftToElsie => "treasures a favorite gift you gave her",
            NpcMemoryType.GaveFavoriteGiftToHarold => "treasures a favorite gift you gave him",
            NpcMemoryType.GaveFavoriteGiftToMira => "treasures a favorite gift you gave her",
            NpcMemoryType.GaveFavoriteGiftToTom => "treasures a favorite gift you gave him",
            NpcMemoryType.SpentQuietTimeWithElsie => "values the quiet time you've shared",
            NpcMemoryType.SpentQuietTimeWithHarold => "values the quiet time you've shared",
            NpcMemoryType.SpentQuietTimeWithMira => "values the quiet time you've shared",
            NpcMemoryType.SpentQuietTimeWithTom => "values the quiet time you've shared",
            NpcMemoryType.HelpedInnOften => "feels close — remembers your help at the inn",
            NpcMemoryType.FrequentGretaCompanion => "considers you a familiar guest at the hearth",
            NpcMemoryType.CheckedOnGreta => "appreciates that you check on her",
            NpcMemoryType.SharedMomentWithGreta => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedGreta => "remembers when you lent a hand at the inn",
            NpcMemoryType.GaveFavoriteGiftToGreta => "treasures a favorite gift you gave her",
            NpcMemoryType.SpentQuietTimeWithGreta => "values the quiet time you've shared",
            NpcMemoryType.HelpedHerbGardenOften => "feels close — remembers your help among the herbs",
            NpcMemoryType.FrequentNoraCompanion => "considers you a familiar presence at the herb garden",
            NpcMemoryType.CheckedOnNora => "appreciates that you check on her",
            NpcMemoryType.SharedMomentWithNora => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedNora => "remembers when you lent a hand among the herbs",
            NpcMemoryType.GaveFavoriteGiftToNora => "treasures a favorite gift you gave her",
            NpcMemoryType.SpentQuietTimeWithNora => "values the quiet time you've shared",
            NpcMemoryType.HelpedSmithyOften => "feels close — remembers your help at the smithy",
            NpcMemoryType.FrequentEliasCompanion => "considers you a familiar face at the forge",
            NpcMemoryType.CheckedOnElias => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithElias => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedElias => "remembers when you lent a hand at the forge",
            NpcMemoryType.GaveFavoriteGiftToElias => "treasures a favorite gift you gave him",
            NpcMemoryType.SpentQuietTimeWithElias => "values the quiet time you've shared",
            NpcMemoryType.HelpedPatrolOften => "feels close — remembers your patrol help",
            NpcMemoryType.FrequentBenCompanion => "considers you a familiar face on patrol",
            NpcMemoryType.CheckedOnBen => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithBen => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedBen => "remembers when you lent a hand at the guard post",
            NpcMemoryType.GaveFavoriteGiftToBen => "treasures a favorite gift you gave him",
            NpcMemoryType.SpentQuietTimeWithBen => "values the quiet time you've shared",
            NpcMemoryType.HelpedVillageOften => "feels close — remembers your help around the village",
            NpcMemoryType.FrequentLilaCompanion => "considers you a familiar young friend in Bloomtown",
            NpcMemoryType.CheckedOnLila => "appreciates that you check on her",
            NpcMemoryType.SharedMomentWithLila => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedLila => "remembers when you lent a hand personally",
            NpcMemoryType.GaveFavoriteGiftToLila => "treasures a favorite gift you gave her",
            NpcMemoryType.SpentQuietTimeWithLila => "values the quiet time you've shared",
            NpcMemoryType.ListenedToStoriesOften => "feels close — remembers your listening at the story bench",
            NpcMemoryType.FrequentRowanCompanion => "considers you a familiar listener in Bloomtown",
            NpcMemoryType.CheckedOnRowan => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithRowan => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedRowan => "remembers when you lent a hand personally",
            NpcMemoryType.GaveFavoriteGiftToRowan => "treasures a favorite gift you gave him",
            NpcMemoryType.SpentQuietTimeWithRowan => "values the quiet time you've shared",
            NpcMemoryType.HelpedWorkshopOften => "feels close — remembers your help at the workshop",
            NpcMemoryType.FrequentMarcusCompanion => "considers you a familiar face at the workshop",
            NpcMemoryType.CheckedOnMarcus => "appreciates that you check on him",
            NpcMemoryType.SharedMomentWithMarcus => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedMarcus => "remembers when you lent a hand at the bench",
            NpcMemoryType.GaveFavoriteGiftToMarcus => "treasures a favorite gift you gave him",
            NpcMemoryType.SpentQuietTimeWithMarcus => "values the quiet time you've shared",
            NpcMemoryType.ListenedToEleanorStories => "feels close — remembers your listening on the porch",
            NpcMemoryType.FrequentEleanorCompanion => "considers you a familiar friend on her porch",
            NpcMemoryType.CheckedOnEleanor => "appreciates that you check on her",
            NpcMemoryType.SharedMomentWithEleanor => "treasures a quiet moment you shared",
            NpcMemoryType.ConsciouslyHelpedEleanor => "remembers when you lent a hand personally",
            NpcMemoryType.GaveFavoriteGiftToEleanor => "treasures a favorite gift you gave her",
            NpcMemoryType.SpentQuietTimeWithEleanor => "values the quiet time you've shared",
            _ => null,
        };
    }

    public static NpcMemoryType? GetSpendTimeMemoryForNpc(uint npcEntityId) =>
        npcEntityId switch
        {
            NpcEntityIds.Elsie => NpcMemoryType.SpentQuietTimeWithElsie,
            NpcEntityIds.Harold => NpcMemoryType.SpentQuietTimeWithHarold,
            NpcEntityIds.Mira => NpcMemoryType.SpentQuietTimeWithMira,
            NpcEntityIds.Tom => NpcMemoryType.SpentQuietTimeWithTom,
            NpcEntityIds.Greta => NpcMemoryType.SpentQuietTimeWithGreta,
            NpcEntityIds.Nora => NpcMemoryType.SpentQuietTimeWithNora,
            NpcEntityIds.Elias => NpcMemoryType.SpentQuietTimeWithElias,
            NpcEntityIds.Ben => NpcMemoryType.SpentQuietTimeWithBen,
            NpcEntityIds.Lila => NpcMemoryType.SpentQuietTimeWithLila,
            NpcEntityIds.Rowan => NpcMemoryType.SpentQuietTimeWithRowan,
            NpcEntityIds.Marcus => NpcMemoryType.SpentQuietTimeWithMarcus,
            NpcEntityIds.Eleanor => NpcMemoryType.SpentQuietTimeWithEleanor,
            _ => null,
        };

    /// <summary>Highest-priority emotional memory the focus NPC holds about this player.</summary>
    public static NpcMemoryType? GetActiveEmotionalMemory(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories)
    {
        var priority = npcEntityId switch
        {
            NpcEntityIds.Elsie => ElsieMemoryPriority,
            NpcEntityIds.Harold => HaroldMemoryPriority,
            NpcEntityIds.Mira => MiraMemoryPriority,
            NpcEntityIds.Tom => TomMemoryPriority,
            NpcEntityIds.Greta => GretaMemoryPriority,
            NpcEntityIds.Nora => NoraMemoryPriority,
            NpcEntityIds.Elias => EliasMemoryPriority,
            NpcEntityIds.Ben => BenMemoryPriority,
            NpcEntityIds.Lila => LilaMemoryPriority,
            NpcEntityIds.Rowan => RowanMemoryPriority,
            NpcEntityIds.Marcus => MarcusMemoryPriority,
            NpcEntityIds.Eleanor => EleanorMemoryPriority,
            _ => Array.Empty<NpcMemoryType>(),
        };

        foreach (var memoryType in priority)
        {
            if (memories.Contains(memoryType))
                return memoryType;
        }

        return null;
    }

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}