using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Active social-standing favors: tier gates, success chance, cooldowns, and light outcomes per focus NPC.
/// </summary>
public static class SocialStandingActionConfig
{
    public const int RespectedSuccessChancePercent = 72;
    public const int WellLikedSuccessChancePercent = 92;

    public const int RespectedCooldownGameMinutes = 80;
    public const int WellLikedCooldownGameMinutes = 60;

    public const float RespectedMoodRecovery = 5f;
    public const float RespectedSocialRecovery = 6f;
    public const float WellLikedMoodRecovery = 9f;
    public const float WellLikedSocialRecovery = 12f;

    public const int RespectedInfoOutcomeWeight = 34;
    public const int RespectedItemOutcomeWeight = 36;
    public const int RespectedRecoveryOutcomeWeight = 30;

    public const int WellLikedInfoOutcomeWeight = 24;
    public const int WellLikedItemOutcomeWeight = 48;
    public const int WellLikedRecoveryOutcomeWeight = 28;

    public static bool IsEligible(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Respected;

    public static int GetSuccessChancePercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedSuccessChancePercent,
            VillageSocialStandingTier.Respected => RespectedSuccessChancePercent,
            _ => 0,
        };

    public static int GetCooldownGameMinutes(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedCooldownGameMinutes,
            VillageSocialStandingTier.Respected => RespectedCooldownGameMinutes,
            _ => 0,
        };

    public static (float MoodBonus, float SocialBonus) GetRecoveryBonus(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => (WellLikedMoodRecovery, WellLikedSocialRecovery),
            VillageSocialStandingTier.Respected => (RespectedMoodRecovery, RespectedSocialRecovery),
            _ => (0f, 0f),
        };

    public static bool ShouldSucceed(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligible(tier))
            return false;

        var chance = GetSuccessChancePercent(tier);
        var roll = (playerEntityId * 107 + npcEntityId * 79 + variationSeed * 53 + (uint)(totalGameMinutes % 911)) % 100;
        return roll < chance;
    }

    public static SocialStandingFavorOutcomeKind ResolveOutcome(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        uint variationSeed)
    {
        var (infoWeight, itemWeight, recoveryWeight) = tier switch
        {
            VillageSocialStandingTier.WellLiked => (
                WellLikedInfoOutcomeWeight,
                WellLikedItemOutcomeWeight,
                WellLikedRecoveryOutcomeWeight),
            VillageSocialStandingTier.Respected => (
                RespectedInfoOutcomeWeight,
                RespectedItemOutcomeWeight,
                RespectedRecoveryOutcomeWeight),
            _ => (0, 0, 0),
        };

        var total = infoWeight + itemWeight + recoveryWeight;
        if (total <= 0)
            return SocialStandingFavorOutcomeKind.Info;

        var roll = (playerEntityId * 109 + npcEntityId * 83 + variationSeed * 59) % (uint)total;
        if (roll < infoWeight)
            return SocialStandingFavorOutcomeKind.Info;

        roll -= (uint)infoWeight;
        if (roll < itemWeight)
            return SocialStandingFavorOutcomeKind.Item;

        return SocialStandingFavorOutcomeKind.Recovery;
    }

    public static EmotionalBondFavorGrant? TryGetItemGrant(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        var generous = tier >= VillageSocialStandingTier.WellLiked && variationSeed % 3 != 2;

        return npcEntityId switch
        {
            NpcEntityIds.Elsie => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Harold => new EmotionalBondFavorGrant(ItemType.Wood, generous ? 2 : 1),
            NpcEntityIds.Mira => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Tom => new EmotionalBondFavorGrant(ItemType.Plank, 1),
            NpcEntityIds.Greta => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Nora => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Elias => new EmotionalBondFavorGrant(ItemType.Wood, generous ? 2 : 1),
            NpcEntityIds.Marcus => new EmotionalBondFavorGrant(ItemType.Plank, generous ? 2 : 1),
            NpcEntityIds.Ben => new EmotionalBondFavorGrant(ItemType.Tool, 1),
            NpcEntityIds.Lila => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Rowan => new EmotionalBondFavorGrant(ItemType.Wood, generous ? 2 : 1),
            _ => null,
        };
    }

    public static string FormatPlayerRequestFeedback(
        string npcDisplayName,
        VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"[Because Bloomtown regards you as well-liked, you ask {npcDisplayName} for a favor — they listen closely.]",
            VillageSocialStandingTier.Respected =>
                $"[Because neighbors speak well of you, you ask {npcDisplayName} for help — your standing earns a hearing.]",
            _ => string.Empty,
        };

    public static string FormatOutcomeFeedback(
        string npcDisplayName,
        SocialStandingFavorOutcomeKind outcome) =>
        outcome switch
        {
            SocialStandingFavorOutcomeKind.Info =>
                $"[{npcDisplayName} shares something useful — a favor earned through your standing in Bloomtown.]",
            SocialStandingFavorOutcomeKind.Item =>
                $"[{npcDisplayName} offers a gift — well-liked regulars often receive a little extra.]",
            SocialStandingFavorOutcomeKind.Recovery =>
                $"[{npcDisplayName} helps you feel steadier — your standing means the village looks after you.]",
            _ => string.Empty,
        };

    public static string FormatRecoveryFeedback(float moodBonus, float socialBonus)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return string.Empty;

        var parts = new List<string>();
        if (moodBonus > 0f)
            parts.Add($"mood +{moodBonus:F0}");
        if (socialBonus > 0f)
            parts.Add($"social need -{socialBonus:F0}");

        return $"[Because your standing earned this favor, their kindness steadies you: {string.Join(", ", parts)}.]";
    }

    public static string FormatItemGrantFeedback(
        string npcDisplayName,
        EmotionalBondFavorGrant grant)
    {
        var itemLabel = grant.Quantity == 1
            ? ItemDatabase.GetDisplayName(grant.ItemType)
            : $"{grant.Quantity} {ItemDatabase.GetDisplayName(grant.ItemType)}";

        return $"[{npcDisplayName} hands you {itemLabel} — a standing favor from someone who trusts you in Bloomtown.]";
    }

    public static string FormatDeclineFeedback(string npcDisplayName) =>
        $"[{npcDisplayName} can't spare much right now — but they still treat you warmly. Try again later.]";

    public static string FormatCooldownHint(VillageSocialStandingTier tier) =>
        $"Take a little time before asking again ({GetCooldownGameMinutes(tier)} game minutes per NPC).";

    public static string FormatNpcAvailability(
        uint npcEntityId,
        bool ready,
        int remainingMinutes)
    {
        var name = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
        return ready
            ? $"  {name}: ready — ask {name.ToLowerInvariant()} for help (nearby)"
            : $"  {name}: resting (~{remainingMinutes}m)";
    }

    public static string BuildUsageHint() =>
        "Social favors (Respected+, near NPC): ask elsie for help | request favor from greta | ask tom for advice";
}