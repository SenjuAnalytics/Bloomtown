using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Light social standing from focus-NPC close bonds: tier thresholds, ambient villager reactions,
/// status summaries, and warmer focus-NPC treatment at Respected and Well-liked tiers.
/// </summary>
public static class VillageSocialStandingConfig
{
    public const int MinCloseFriendsForAmbientComment = 1;
    public const int KnownAmbientChancePercent = 6;
    public const int RespectedAmbientChancePercent = 22;
    public const int WellLikedAmbientChancePercent = 48;
    public const int VillageNoticedAmbientChanceBonusPercent = 7;
    public const int WellLikedPrestigeAmbientBonusPercent = 10;
    public const int WellLikedOrdinaryVillagerAmbientBonusPercent = 10;

    public const int KnownAmbientCooldownGameMinutes = 75;
    public const int RespectedAmbientCooldownGameMinutes = 34;
    public const int WellLikedAmbientCooldownGameMinutes = 16;

    public static VillageSocialStandingTier ResolveTier(int focusCloseFriendCount) =>
        focusCloseFriendCount switch
        {
            <= 0 => VillageSocialStandingTier.Stranger,
            1 => VillageSocialStandingTier.Known,
            2 => VillageSocialStandingTier.Respected,
            _ => VillageSocialStandingTier.WellLiked,
        };

    public static VillageSocialStandingTier ResolveTier(Func<uint, RelationshipTier> getTier) =>
        ResolveTier(VillageBondRecognitionConfig.CountFocusCloseFriends(getTier));

    public static bool IsEligibleForVillagerAmbientComment(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Known;

    public static int GetAmbientChancePercent(VillageSocialStandingTier tier, bool villageNoticedMemory)
    {
        var chance = tier switch
        {
            VillageSocialStandingTier.Known => KnownAmbientChancePercent,
            VillageSocialStandingTier.Respected => RespectedAmbientChancePercent,
            VillageSocialStandingTier.WellLiked => WellLikedAmbientChancePercent,
            _ => 0,
        };

        if (villageNoticedMemory)
            chance += VillageNoticedAmbientChanceBonusPercent;

        if (tier >= VillageSocialStandingTier.WellLiked)
        {
            chance += WellLikedPrestigeAmbientBonusPercent;
            if (villageNoticedMemory)
                chance += WellLikedOrdinaryVillagerAmbientBonusPercent;
        }

        return SocialLegacyConfig.ApplyAmbientChanceBonus(chance, tier);
    }

    public static int GetAmbientCooldownGameMinutes(VillageSocialStandingTier tier)
    {
        var cooldown = tier switch
        {
            VillageSocialStandingTier.Known => KnownAmbientCooldownGameMinutes,
            VillageSocialStandingTier.Respected => RespectedAmbientCooldownGameMinutes,
            VillageSocialStandingTier.WellLiked => WellLikedAmbientCooldownGameMinutes,
            _ => 0,
        };

        return SocialLegacyConfig.ApplyAmbientCooldownReduction(cooldown, tier);
    }

    public static bool ShouldTriggerVillagerAmbientComment(
        uint playerEntityId,
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        long totalGameMinutes,
        uint attemptCounter)
    {
        if (!IsEligibleForVillagerAmbientComment(tier))
            return false;

        var chance = GetAmbientChancePercent(tier, villageNoticedMemory);
        var roll = (playerEntityId * 113 + (uint)(totalGameMinutes % 929) + attemptCounter * 13) % 100;
        return roll < chance;
    }

    public static string? TryGetVillagerAmbientComment(
        VillageSocialStandingTier tier,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (!IsEligibleForVillagerAmbientComment(tier))
            return null;

        return VillageSocialStandingDialogue.TryGetVillagerAmbientComment(
            tier,
            focusCloseFriendNpcIds,
            villageNoticedMemory,
            variationSeed);
    }

    public static string? FormatStandingStatus(
        int focusCloseFriendCount,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory)
    {
        var tier = ResolveTier(focusCloseFriendCount);
        if (tier == VillageSocialStandingTier.Stranger)
            return null;

        return VillageSocialStandingDialogue.FormatStandingStatus(
            tier,
            focusCloseFriendNpcIds,
            villageNoticedMemory);
    }

    public static string? FormatStandingImpactHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        if (tier < VillageSocialStandingTier.Respected)
            return null;

        return VillageSocialStandingDialogue.FormatStandingImpactHint(tier, villageNoticedMemory);
    }

    public static string? FormatSocialStandingActionHint(VillageSocialStandingTier tier)
    {
        if (!SocialStandingActionConfig.IsEligible(tier))
            return null;

        return tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"Social favors: {SocialStandingActionConfig.BuildUsageHint()} — ~{SocialStandingActionConfig.WellLikedSuccessChancePercent}% success, stronger gifts/recovery, ~{SocialStandingActionConfig.WellLikedCooldownGameMinutes}m cooldown per NPC.",
            VillageSocialStandingTier.Respected =>
                $"Social favors: {SocialStandingActionConfig.BuildUsageHint()} — ~{SocialStandingActionConfig.RespectedSuccessChancePercent}% success, ~{SocialStandingActionConfig.RespectedCooldownGameMinutes}m cooldown per NPC.",
            _ => null,
        };
    }

    /// <summary>Clear feedback when a villager ambient comment acknowledges social standing.</summary>
    public static string FormatVillagerAmbientFeedback(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                "[An ordinary villager nearby speaks with personal respect — Bloomtown holds you among its most honored neighbors:]",
            VillageSocialStandingTier.Respected =>
                "[A villager nearby notices your standing — neighbors are starting to recognize you:]",
            _ => $"[A villager nearby — you're {VillageSocialStandingDialogue.GetTierDisplayName(tier)}:]",
        };

    public static string? FormatBroaderVillageRecognitionHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory) =>
        VillageSocialStandingDialogue.FormatBroaderVillageRecognitionHint(tier, villageNoticedMemory);

    public static string? FormatOrdinaryVillagerAmbientStatusHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"Ordinary villagers: neighbors beyond the focus regulars speak your name with respect "
                + $"(~{GetAmbientChancePercent(tier, villageNoticedMemory: true)}% ambient chance, ~{WellLikedAmbientCooldownGameMinutes}m cooldown).",
            VillageSocialStandingTier.Respected =>
                $"Ordinary villagers: some neighbors beyond your close friends are starting to recognize you "
                + $"(~{GetAmbientChancePercent(tier, villageNoticedMemory: true)}% ambient chance, ~{RespectedAmbientCooldownGameMinutes}m cooldown).",
            _ => null,
        };

    public static string? FormatPrestigeStatusHint(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked
            ? VillageSocialStandingDialogue.FormatPrestigeStatusHint(tier)
            : null;

    public static string? FormatVillageAtmosphereHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        if (tier < VillageSocialStandingTier.Respected)
            return null;

        return VillageSocialStandingDialogue.FormatVillageAtmosphereHint(tier, villageNoticedMemory);
    }

    public static string? FormatMechanicalBenefitsHint(VillageSocialStandingTier tier) =>
        VillageSocialStandingMechanicalConfig.FormatMechanicalBenefitsHint(tier);

    public static string? FormatNpcMechanicalBenefitsSummary(VillageSocialStandingTier tier) =>
        VillageSocialStandingMechanicalConfig.FormatNpcMechanicalBenefitsSummary(tier);

    public static string? FormatSocialInfluenceActionHint(VillageSocialStandingTier tier) =>
        SocialInfluenceActionConfig.FormatSocialInfluenceActionHint(tier);

    public static string? FormatSocialMilestonesStatus(
        VillageSocialStandingTier tier,
        int focusCloseFriendCount) =>
        SocialLegacyConfig.FormatMilestonesStatus(tier, focusCloseFriendCount);

    public static string? FormatSocialLegacyEffectsStatus(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        int focusCloseFriendCount = 0) =>
        SocialLegacyConfig.FormatLegacyEffectsStatus(tier, villageNoticedMemory, focusCloseFriendCount);

    public static string? TryGetLegacyAmbientComment(
        VillageSocialStandingTier tier,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed) =>
        SocialLegacyConfig.IsLegacyActive(tier)
            ? SocialLegacyDialogue.TryGetLegacyAmbientComment(
                focusCloseFriendNpcIds,
                villageNoticedMemory,
                variationSeed)
            : null;

    public static string? TryGetLegacyJourneyLine(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        uint variationSeed) =>
        SocialLegacyConfig.IsLegacyActive(tier)
            && SocialLegacyConfig.IsEligibleForLegacyNpcMention(npcEntityId)
            ? SocialLegacyDialogue.TryGetLegacyJourneyLine(npcEntityId, villageNoticedMemory, variationSeed)
            : null;

    public static string GetStandingTierLabel(VillageSocialStandingTier tier) =>
        VillageSocialStandingDialogue.GetStandingTierLabel(tier);

    public static string FormatFocusCloseFriendsLabel(IReadOnlyList<uint> focusCloseFriendNpcIds) =>
        VillageSocialStandingDialogue.FormatFocusCloseFriendsLabel(focusCloseFriendNpcIds);

    public static string? FormatCurrentTierActionsHint(VillageSocialStandingTier tier) =>
        VillageSocialStandingDialogue.FormatCurrentTierActionsHint(tier);

    public static string? TryFormatTierPromotionFeedback(
        VillageSocialStandingTier previousTier,
        VillageSocialStandingTier newTier,
        bool villageNoticedMemory)
    {
        if (newTier <= previousTier || newTier < VillageSocialStandingTier.Respected)
            return null;

        return VillageSocialStandingDialogue.FormatTierPromotionFeedback(newTier, villageNoticedMemory);
    }
}