using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Benefits from close emotional bonds with Elsie, Harold, Mira, Tom, and Greta:
/// needs recovery, personal appreciation, village tips, and small favors.
/// Tuned to feel meaningfully valuable while staying light, rare, and natural.
/// </summary>
public static class NpcEmotionalBondImpactConfig
{
    public const int EmotionalBondInfoCooldownGameMinutes = 72;
    public const int EmotionalBondFavorCooldownGameMinutes = 82;
    public const int EmotionalBondAppreciationCooldownGameMinutes = 52;

    public const int EmotionalBondInfoChancePercent = 30;
    public const int EmotionalBondFavorChancePercent = 25;
    public const int EmotionalBondAppreciationChancePercent = 36;

    /// <summary>Connectors hear richer social tips slightly more often — still rare overall.</summary>
    public const int ConnectorInfoChanceBonusPercent = 10;

    /// <summary>When a favor line fires, this share also grants a small tangible item.</summary>
    public const int EmotionalBondFavorItemChancePercent = 38;

    // Friend-tier recovery — noticeably stronger than casual interaction.
    public const float GreetMoodBonus = 6f;
    public const float GreetSocialBonus = 8f;
    public const float TalkMoodBonus = 7f;
    public const float TalkSocialBonus = 9f;

    public const float CheckOnMoodBonus = 7f;
    public const float CheckOnSocialBonus = 9f;
    public const float SpendTimeMoodBonus = 8f;
    public const float SpendTimeSocialBonus = 10f;
    public const float ShareMomentMoodBonus = 9f;
    public const float ShareMomentSocialBonus = 12f;
    public const float HelpWithMoodBonus = 7f;
    public const float HelpWithSocialBonus = 9f;

    /// <summary>Extra recovery when the bond has deepened to Close Friend.</summary>
    public const float CloseFriendMoodBonusExtra = 3f;
    public const float CloseFriendSocialBonusExtra = 4f;

    /// <summary>Lila's energetic warmth lifts mood and social need slightly more than other focus NPCs.</summary>
    public const float LilaRecoveryMoodBonusExtra = 1f;
    public const float LilaRecoverySocialBonusExtra = 1f;

    /// <summary>Rowan's wise storyteller warmth lifts mood and social need slightly more than other focus NPCs.</summary>
    public const float RowanRecoveryMoodBonusExtra = 1f;
    public const float RowanRecoverySocialBonusExtra = 1f;

    /// <summary>Eleanor's warm nostalgic presence lifts mood and social need slightly more than other focus NPCs.</summary>
    public const float EleanorRecoveryMoodBonusExtra = 1f;
    public const float EleanorRecoverySocialBonusExtra = 1f;

    /// <summary>Close emotional bond: focus NPC, friend tier, and at least one warm memory.</summary>
    public static bool QualifiesForImpact(
        uint npcEntityId,
        RelationshipTier tier,
        IReadOnlyCollection<NpcMemoryType> memories) =>
        NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
        && tier >= NpcEmotionalBondConfig.MinEmotionalInteractionTier
        && NpcEmotionalBondConfig.HasEmotionalMemory(npcEntityId, memories);

    public static (float MoodBonus, float SocialBonus) GetInteractionRecoveryBonus(
        NpcInteractionKind kind,
        RelationshipTier tier = RelationshipTier.Acquaintance,
        uint npcEntityId = 0) =>
        ApplyLilaRecoveryBonus(
            ApplyCloseFriendBonus(kind switch
            {
                NpcInteractionKind.Greet => (GreetMoodBonus, GreetSocialBonus),
                NpcInteractionKind.Talk => (TalkMoodBonus, TalkSocialBonus),
                _ => (0f, 0f),
            }, tier),
            npcEntityId);

    public static (float MoodBonus, float SocialBonus) GetBondingActionRecoveryBonus(
        EmotionalBondActionKind action,
        RelationshipTier tier = RelationshipTier.Acquaintance,
        uint npcEntityId = 0) =>
        ApplyLilaRecoveryBonus(
            ApplyCloseFriendBonus(action switch
            {
                EmotionalBondActionKind.CheckOn => (CheckOnMoodBonus, CheckOnSocialBonus),
                EmotionalBondActionKind.SpendTime => (SpendTimeMoodBonus, SpendTimeSocialBonus),
                EmotionalBondActionKind.ShareMoment => (ShareMomentMoodBonus, ShareMomentSocialBonus),
                EmotionalBondActionKind.HelpWith => (HelpWithMoodBonus, HelpWithSocialBonus),
                _ => (0f, 0f),
            }, tier),
            npcEntityId);

    /// <summary>
    /// Player-facing feedback — makes clear the recovery comes from emotional closeness, not a generic interaction.
    /// </summary>
    public static string FormatNeedsRecoveryFeedback(
        string npcDisplayName,
        float moodBonus,
        float socialBonus)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return string.Empty;

        var parts = new List<string>();
        if (moodBonus > 0f)
            parts.Add($"mood +{moodBonus:F0}");
        if (socialBonus > 0f)
            parts.Add($"social need -{socialBonus:F0}");

        return $"[Your bond with {npcDisplayName} comforts you — this moment feels warmer: {string.Join(", ", parts)}.]";
    }

    /// <summary>Wraps a village tip so the player knows it was shared because of trust, not idle chatter.</summary>
    public static string FormatBondInfoFeedback(string npcDisplayName, string infoTip) =>
        $"[Because you're close to {npcDisplayName}, they share something personal:] {infoTip}";

    /// <summary>Wraps a favor line so the helpful gesture reads as earned through the relationship.</summary>
    public static string FormatBondFavorFeedback(string npcDisplayName, string favorLine) =>
        $"{favorLine} [{npcDisplayName} does this because your bond matters to them.]";

    /// <summary>Wraps appreciation so the personal recognition is explicitly tied to the relationship.</summary>
    public static string FormatBondAppreciationFeedback(string npcDisplayName, string appreciationLine) =>
        $"[{npcDisplayName} sees you clearly — not as a stranger, but as someone who matters:] {appreciationLine}";

    /// <summary>Feedback when a favor also grants a small item.</summary>
    public static string FormatBondItemGrantFeedback(
        string npcDisplayName,
        EmotionalBondFavorGrant grant)
    {
        var itemLabel = grant.Quantity == 1
            ? ItemDatabase.GetDisplayName(grant.ItemType)
            : $"{grant.Quantity} {ItemDatabase.GetDisplayName(grant.ItemType)}";

        return $"[{npcDisplayName} slips you {itemLabel} — a small gift earned through your bond.]";
    }

    public static bool ShouldTriggerEmotionalBondInfo(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes,
        LegacyArchetype archetype = LegacyArchetype.None,
        int chanceBonusPercent = 0)
    {
        var roll = (playerEntityId * 71 + npcEntityId * 53 + (uint)(totalGameMinutes % 881)) % 100;
        var threshold = EmotionalBondInfoChancePercent + chanceBonusPercent;
        if (archetype == LegacyArchetype.Connector)
            threshold += ConnectorInfoChanceBonusPercent;

        return roll < Math.Min(threshold, 99);
    }

    public static bool ShouldTriggerEmotionalBondFavor(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes,
        int chanceBonusPercent = 0)
    {
        var roll = (playerEntityId * 79 + npcEntityId * 61 + (uint)(totalGameMinutes % 863)) % 100;
        var threshold = EmotionalBondFavorChancePercent + chanceBonusPercent;
        return roll < Math.Min(threshold, 99);
    }

    public static bool ShouldTriggerEmotionalBondAppreciation(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 83 + npcEntityId * 67 + (uint)(totalGameMinutes % 857)) % 100;
        return roll < EmotionalBondAppreciationChancePercent;
    }

    public static string? TryGetPersonalAppreciationLine(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (!QualifiesForImpact(npcEntityId, tier, memories))
            return null;

        var memoryType = NpcEmotionalBondConfig.GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null)
            return null;

        var lines = NpcEmotionalBondImpactDialogue.GetPersonalAppreciationLines(
            npcEntityId,
            memoryType.Value,
            tier);

        return PickLine(lines, variationSeed);
    }

    public static string? TryGetEmotionalBondInfoTip(
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        var lines = NpcEmotionalBondImpactDialogue.GetEmotionalInfoTips(npcEntityId, timeOfDay, archetype);
        return PickLine(lines, variationSeed);
    }

    public static string? TryGetHelpfulFavorLine(
        uint npcEntityId,
        NpcInteractionKind kind,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (!QualifiesForImpact(npcEntityId, tier, memories))
            return null;

        var memoryType = NpcEmotionalBondConfig.GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null)
            return null;

        var lines = NpcEmotionalBondImpactDialogue.GetHelpfulFavorLines(
            npcEntityId,
            kind,
            memoryType.Value,
            tier);

        return PickLine(lines, variationSeed);
    }

    public static string? TryGetBondingActionFavorLine(
        uint npcEntityId,
        EmotionalBondActionKind action,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId) || tier < NpcEmotionalBondConfig.MinEmotionalInteractionTier)
            return null;

        var lines = NpcEmotionalBondImpactDialogue.GetBondingActionFavorLines(npcEntityId, action, tier);
        return PickLine(lines, variationSeed);
    }

    /// <summary>
    /// Rare tangible favor — 1-2 small items when an emotional-bond favor fires.
    /// Items are chosen by NPC context and active memory so the gift feels remembered, not random.
    /// </summary>
    public static EmotionalBondFavorGrant? TryGetFavorItemGrant(
        uint npcEntityId,
        NpcMemoryType? memoryType,
        RelationshipTier tier,
        EmotionalBondActionKind? bondingAction,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId) || tier < NpcEmotionalBondConfig.MinEmotionalInteractionTier)
            return null;

        var itemRoll = (variationSeed * 97 + npcEntityId * 41) % 100;
        if (itemRoll >= EmotionalBondFavorItemChancePercent)
            return null;

        if (bondingAction == EmotionalBondActionKind.ShareMoment && tier >= RelationshipTier.CloseFriend)
        {
            return npcEntityId switch
            {
                NpcEntityIds.Elsie => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                NpcEntityIds.Harold => new EmotionalBondFavorGrant(ItemType.Wood, 2),
                NpcEntityIds.Mira => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                NpcEntityIds.Tom => new EmotionalBondFavorGrant(ItemType.Wood, 2),
                NpcEntityIds.Greta => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                NpcEntityIds.Nora => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                NpcEntityIds.Elias => new EmotionalBondFavorGrant(ItemType.Plank, 2),
                NpcEntityIds.Ben => new EmotionalBondFavorGrant(ItemType.Wood, 2),
                NpcEntityIds.Lila => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                NpcEntityIds.Rowan => new EmotionalBondFavorGrant(ItemType.Wood, 2),
                NpcEntityIds.Marcus => new EmotionalBondFavorGrant(ItemType.Plank, 2),
                NpcEntityIds.Eleanor => new EmotionalBondFavorGrant(ItemType.Apple, 2),
                _ => null,
            };
        }

        return (npcEntityId, memoryType, tier) switch
        {
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Elsie, NpcMemoryType.CheckedOnElsie, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Elsie, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),

            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 2),
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Harold, NpcMemoryType.SharedMomentWithHarold, _) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),
            (NpcEntityIds.Harold, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),

            (NpcEntityIds.Mira, NpcMemoryType.HelpedMarketOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Mira, NpcMemoryType.HelpedMarketOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Mira, NpcMemoryType.CheckedOnMira, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Mira, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),

            (NpcEntityIds.Tom, NpcMemoryType.HelpedLumberOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 2),
            (NpcEntityIds.Tom, NpcMemoryType.HelpedLumberOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Tom, NpcMemoryType.CheckedOnTom, _) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),
            (NpcEntityIds.Tom, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),

            (NpcEntityIds.Greta, NpcMemoryType.HelpedInnOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Greta, NpcMemoryType.HelpedInnOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Greta, NpcMemoryType.CheckedOnGreta, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Greta, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),

            (NpcEntityIds.Nora, NpcMemoryType.HelpedHerbGardenOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Nora, NpcMemoryType.HelpedHerbGardenOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Nora, NpcMemoryType.CheckedOnNora, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Nora, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),

            (NpcEntityIds.Elias, NpcMemoryType.HelpedSmithyOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Tool, 1),
            (NpcEntityIds.Elias, NpcMemoryType.HelpedSmithyOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Elias, NpcMemoryType.CheckedOnElias, _) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),
            (NpcEntityIds.Elias, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),

            (NpcEntityIds.Ben, NpcMemoryType.HelpedPatrolOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Tool, 1),
            (NpcEntityIds.Ben, NpcMemoryType.HelpedPatrolOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Ben, NpcMemoryType.CheckedOnBen, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Ben, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Tool, 1),

            (NpcEntityIds.Lila, NpcMemoryType.HelpedVillageOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Lila, NpcMemoryType.HelpedVillageOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Lila, NpcMemoryType.CheckedOnLila, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Lila, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),

            (NpcEntityIds.Rowan, NpcMemoryType.ListenedToStoriesOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 2),
            (NpcEntityIds.Rowan, NpcMemoryType.ListenedToStoriesOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Rowan, NpcMemoryType.CheckedOnRowan, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Rowan, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),

            (NpcEntityIds.Marcus, NpcMemoryType.HelpedWorkshopOften, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Tool, 1),
            (NpcEntityIds.Marcus, NpcMemoryType.HelpedWorkshopOften, _) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),
            (NpcEntityIds.Marcus, NpcMemoryType.CheckedOnMarcus, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Marcus, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Plank, 1),

            (NpcEntityIds.Eleanor, NpcMemoryType.ListenedToEleanorStories, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 2),
            (NpcEntityIds.Eleanor, NpcMemoryType.ListenedToEleanorStories, _) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),
            (NpcEntityIds.Eleanor, NpcMemoryType.CheckedOnEleanor, _) =>
                new EmotionalBondFavorGrant(ItemType.Wood, 1),
            (NpcEntityIds.Eleanor, _, RelationshipTier.CloseFriend) =>
                new EmotionalBondFavorGrant(ItemType.Apple, 1),

            _ => null,
        };
    }

    private static (float MoodBonus, float SocialBonus) ApplyLilaRecoveryBonus(
        (float Mood, float Social) baseBonus,
        uint npcEntityId)
    {
        if (npcEntityId == NpcEntityIds.Lila)
            return (baseBonus.Mood + LilaRecoveryMoodBonusExtra, baseBonus.Social + LilaRecoverySocialBonusExtra);

        if (npcEntityId == NpcEntityIds.Rowan)
            return (baseBonus.Mood + RowanRecoveryMoodBonusExtra, baseBonus.Social + RowanRecoverySocialBonusExtra);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return (baseBonus.Mood + EleanorRecoveryMoodBonusExtra, baseBonus.Social + EleanorRecoverySocialBonusExtra);

        return baseBonus;
    }

    private static (float MoodBonus, float SocialBonus) ApplyCloseFriendBonus(
        (float Mood, float Social) baseBonus,
        RelationshipTier tier)
    {
        if (tier < RelationshipTier.CloseFriend)
            return baseBonus;

        return (baseBonus.Mood + CloseFriendMoodBonusExtra, baseBonus.Social + CloseFriendSocialBonusExtra);
    }

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}