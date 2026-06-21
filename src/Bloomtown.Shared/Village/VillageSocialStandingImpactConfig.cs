using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Light interaction bonuses and warmth rolls for focus NPCs when the player has Respected or Well-liked standing.
/// </summary>
public static class VillageSocialStandingImpactConfig
{
    public const float RespectedMoodBonus = 3f;
    public const float RespectedSocialBonus = 3f;
    public const float WellLikedMoodBonus = 5f;
    public const float WellLikedSocialBonus = 6f;

    public const float GretaRespectedMoodBonusExtra = 1f;
    public const float GretaRespectedSocialBonusExtra = 1f;
    public const float GretaWellLikedMoodBonusExtra = 2f;
    public const float GretaWellLikedSocialBonusExtra = 3f;

    public const float HaroldRespectedMoodBonusExtra = 1f;
    public const float HaroldRespectedSocialBonusExtra = 1f;
    public const float HaroldWellLikedMoodBonusExtra = 3f;
    public const float HaroldWellLikedSocialBonusExtra = 3f;

    public const float NoraRespectedMoodBonusExtra = 1f;
    public const float NoraRespectedSocialBonusExtra = 1f;
    public const float NoraWellLikedMoodBonusExtra = 1f;
    public const float NoraWellLikedSocialBonusExtra = 3f;

    public const float EliasRespectedMoodBonusExtra = 1f;
    public const float EliasRespectedSocialBonusExtra = 1f;
    public const float EliasWellLikedMoodBonusExtra = 2f;
    public const float EliasWellLikedSocialBonusExtra = 2f;

    public const float MarcusRespectedMoodBonusExtra = 1f;
    public const float MarcusRespectedSocialBonusExtra = 1f;
    public const float MarcusWellLikedMoodBonusExtra = 2f;
    public const float MarcusWellLikedSocialBonusExtra = 2f;

    public const float BenRespectedMoodBonusExtra = 1f;
    public const float BenRespectedSocialBonusExtra = 1f;
    public const float BenWellLikedMoodBonusExtra = 2f;
    public const float BenWellLikedSocialBonusExtra = 2f;

    public const float LilaRespectedMoodBonusExtra = 1f;
    public const float LilaRespectedSocialBonusExtra = 1f;
    public const float LilaWellLikedMoodBonusExtra = 2f;
    public const float LilaWellLikedSocialBonusExtra = 2f;
    public const float RowanRespectedMoodBonusExtra = 1f;
    public const float RowanRespectedSocialBonusExtra = 1f;
    public const float RowanWellLikedMoodBonusExtra = 2f;
    public const float RowanWellLikedSocialBonusExtra = 2f;

    public const float EleanorRespectedMoodBonusExtra = 1f;
    public const float EleanorRespectedSocialBonusExtra = 1f;
    public const float EleanorWellLikedMoodBonusExtra = 2f;
    public const float EleanorWellLikedSocialBonusExtra = 2f;

    public const int RespectedInfoChanceBonusPercent = 10;
    public const int WellLikedInfoChanceBonusPercent = 18;
    public const int RespectedFavorChanceBonusPercent = 8;
    public const int WellLikedFavorChanceBonusPercent = 14;

    public const int RespectedStandingWarmthChancePercent = 36;
    public const int WellLikedStandingWarmthChancePercent = 46;

    public const int WellLikedPrivilegeChancePercent = 18;
    public const int WellLikedPrivilegeCooldownGameMinutes = 72;
    public const int WellLikedPrivilegeItemChancePercent = 55;

    public const int WellLikedPrestigeRecognitionChancePercent = 26;

    public const int RespectedVillageAwarenessChancePercent = 14;
    public const int WellLikedVillageAwarenessChancePercent = 18;
    public const int VillageAwarenessCooldownGameMinutes = 95;

    public static bool IsEligibleForFocusNpcBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Respected;

    public static bool IsEligibleForWellLikedPrivilege(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked;

    public static (float MoodBonus, float SocialBonus) GetInteractionBonus(
        VillageSocialStandingTier tier,
        uint npcEntityId)
    {
        if (!IsEligibleForFocusNpcBonus(tier))
            return (0f, 0f);

        var (mood, social) = tier switch
        {
            VillageSocialStandingTier.WellLiked => (WellLikedMoodBonus, WellLikedSocialBonus),
            VillageSocialStandingTier.Respected => (RespectedMoodBonus, RespectedSocialBonus),
            _ => (0f, 0f),
        };

        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Greta,
            GretaRespectedMoodBonusExtra,
            GretaRespectedSocialBonusExtra,
            GretaWellLikedMoodBonusExtra,
            GretaWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Harold,
            HaroldRespectedMoodBonusExtra,
            HaroldRespectedSocialBonusExtra,
            HaroldWellLikedMoodBonusExtra,
            HaroldWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Nora,
            NoraRespectedMoodBonusExtra,
            NoraRespectedSocialBonusExtra,
            NoraWellLikedMoodBonusExtra,
            NoraWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Elias,
            EliasRespectedMoodBonusExtra,
            EliasRespectedSocialBonusExtra,
            EliasWellLikedMoodBonusExtra,
            EliasWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Marcus,
            MarcusRespectedMoodBonusExtra,
            MarcusRespectedSocialBonusExtra,
            MarcusWellLikedMoodBonusExtra,
            MarcusWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Ben,
            BenRespectedMoodBonusExtra,
            BenRespectedSocialBonusExtra,
            BenWellLikedMoodBonusExtra,
            BenWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Lila,
            LilaRespectedMoodBonusExtra,
            LilaRespectedSocialBonusExtra,
            LilaWellLikedMoodBonusExtra,
            LilaWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Rowan,
            RowanRespectedMoodBonusExtra,
            RowanRespectedSocialBonusExtra,
            RowanWellLikedMoodBonusExtra,
            RowanWellLikedSocialBonusExtra);
        ApplyRoleInteractionBonus(
            npcEntityId,
            tier,
            ref mood,
            ref social,
            NpcEntityIds.Eleanor,
            EleanorRespectedMoodBonusExtra,
            EleanorRespectedSocialBonusExtra,
            EleanorWellLikedMoodBonusExtra,
            EleanorWellLikedSocialBonusExtra);

        return (mood, social);
    }

    private static void ApplyRoleInteractionBonus(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        ref float mood,
        ref float social,
        uint roleNpcId,
        float respectedMoodExtra,
        float respectedSocialExtra,
        float wellLikedMoodExtra,
        float wellLikedSocialExtra)
    {
        if (npcEntityId != roleNpcId)
            return;

        if (tier >= VillageSocialStandingTier.WellLiked)
        {
            mood += wellLikedMoodExtra;
            social += wellLikedSocialExtra;
        }
        else
        {
            mood += respectedMoodExtra;
            social += respectedSocialExtra;
        }
    }

    public static int GetInfoChanceBonusPercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedInfoChanceBonusPercent,
            VillageSocialStandingTier.Respected => RespectedInfoChanceBonusPercent,
            _ => 0,
        };

    public static int GetFavorChanceBonusPercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedFavorChanceBonusPercent,
            VillageSocialStandingTier.Respected => RespectedFavorChanceBonusPercent,
            _ => 0,
        };

    public static int GetStandingWarmthChancePercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedStandingWarmthChancePercent,
            VillageSocialStandingTier.Respected => RespectedStandingWarmthChancePercent,
            _ => 0,
        };

    public static bool ShouldTriggerStandingWarmth(
        uint playerEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligibleForFocusNpcBonus(tier))
            return false;

        var chance = GetStandingWarmthChancePercent(tier);
        var roll = (playerEntityId * 131 + variationSeed * 67 + (uint)(totalGameMinutes % 917)) % 100;
        return roll < chance;
    }

    public static bool ShouldTriggerWellLikedPrestigeRecognition(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligibleForWellLikedPrivilege(tier) || !NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return false;

        var roll = (playerEntityId * 101 + npcEntityId * 71 + variationSeed * 43 + (uint)(totalGameMinutes % 919)) % 100;
        return roll < WellLikedPrestigeRecognitionChancePercent;
    }

    public static string FormatWellLikedPrestigeRecognitionFeedback(string npcDisplayName, string recognitionLine) =>
        $"[Because Bloomtown esteems you, {npcDisplayName} greets you with special regard:] {recognitionLine}";

    public static bool ShouldTriggerWellLikedPrivilege(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligibleForWellLikedPrivilege(tier) || !NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return false;

        var roll = (playerEntityId * 97 + npcEntityId * 73 + variationSeed * 41 + (uint)(totalGameMinutes % 907)) % 100;
        return roll < WellLikedPrivilegeChancePercent;
    }

    public static bool ShouldGrantWellLikedPrivilegeItem(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        var roll = (playerEntityId * 103 + npcEntityId * 89 + variationSeed * 47 + (uint)(totalGameMinutes % 893)) % 100;
        return roll < WellLikedPrivilegeItemChancePercent;
    }

    public static EmotionalBondFavorGrant? TryGetWellLikedPrivilegeItemGrant(
        uint npcEntityId,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        var generous = variationSeed % 3 != 2;

        return npcEntityId switch
        {
            NpcEntityIds.Elsie => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Harold => new EmotionalBondFavorGrant(ItemType.Wood, 2),
            NpcEntityIds.Mira => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Tom => new EmotionalBondFavorGrant(ItemType.Plank, generous ? 2 : 1),
            NpcEntityIds.Greta => new EmotionalBondFavorGrant(ItemType.Apple, 2),
            NpcEntityIds.Nora => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Elias => new EmotionalBondFavorGrant(ItemType.Plank, generous ? 2 : 1),
            NpcEntityIds.Marcus => new EmotionalBondFavorGrant(ItemType.Plank, generous ? 2 : 1),
            NpcEntityIds.Ben => new EmotionalBondFavorGrant(ItemType.Wood, generous ? 2 : 1),
            NpcEntityIds.Lila => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            NpcEntityIds.Rowan => new EmotionalBondFavorGrant(ItemType.Wood, generous ? 2 : 1),
            NpcEntityIds.Eleanor => new EmotionalBondFavorGrant(ItemType.Apple, generous ? 2 : 1),
            _ => null,
        };
    }

    public static string FormatStandingRecoveryFeedback(
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

        return $"[Because your Social Standing is high, Bloomtown treats you warmly — {npcDisplayName} welcomes you like a trusted regular: {string.Join(", ", parts)}.]";
    }

    public static string FormatWellLikedPrivilegeFeedback(string npcDisplayName, string privilegeLine) =>
        $"[Because you're well-liked in Bloomtown, {npcDisplayName} offers a noticeable extra kindness:] {privilegeLine}";

    public static int GetVillageAwarenessChancePercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedVillageAwarenessChancePercent,
            VillageSocialStandingTier.Respected => RespectedVillageAwarenessChancePercent,
            _ => 0,
        };

    public static bool ShouldTriggerVillageSocialStandingAwareness(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligibleForFocusNpcBonus(tier) || !NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return false;

        var chance = GetVillageAwarenessChancePercent(tier);
        var roll = (playerEntityId * 149 + npcEntityId * 61 + variationSeed * 53 + (uint)(totalGameMinutes % 911)) % 100;
        return roll < chance;
    }

    public static string FormatVillageSocialStandingAwarenessFeedback(string npcDisplayName, string awarenessLine) =>
        $"[{npcDisplayName} notices how the village regards you:] {awarenessLine}";

    public static string FormatWellLikedPrivilegeItemFeedback(
        string npcDisplayName,
        EmotionalBondFavorGrant grant)
    {
        var itemLabel = grant.Quantity == 1
            ? ItemDatabase.GetDisplayName(grant.ItemType)
            : $"{grant.Quantity} {ItemDatabase.GetDisplayName(grant.ItemType)}";

        return $"[{npcDisplayName} slips you {itemLabel} — a Well-liked privilege for someone the village trusts.]";
    }
}