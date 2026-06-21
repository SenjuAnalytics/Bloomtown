using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Light mechanical benefits from social standing: Mira trade, Elsie garden help, Tom wood work,
/// Harold community projects, Greta inn help, Nora herb tending, Elias smithy work, Marcus workshop work,
/// communal contributions, and favor reliability.
/// </summary>
public static class VillageSocialStandingMechanicalConfig
{
    public const float RespectedMiraBuyDiscountPercent = 9f;
    public const float WellLikedMiraBuyDiscountPercent = 12f;
    public const float RespectedMiraSellBonusPercent = 9f;
    public const float WellLikedMiraSellBonusPercent = 12f;

    public const int WellLikedContributionScoreBonus = 1;
    public const int WellLikedContributionProgressBonusChancePercent = 32;
    public const int RespectedContributionProgressBonusChancePercent = 14;

    public const float RespectedRoleActivityMoodBonus = 2f;
    public const float WellLikedRoleActivityMoodBonus = 3f;
    public const float RespectedRoleActivitySocialBonus = 2f;
    public const float WellLikedRoleActivitySocialBonus = 4f;
    public const float RespectedDailyLeisureMoodBonus = 2f;
    public const float WellLikedDailyLeisureMoodBonus = 3f;
    public const float RespectedDailyLeisureSocialBonus = 2f;
    public const float WellLikedDailyLeisureSocialBonus = 3f;
    public const float RespectedDailySocialMoodBonus = 2f;
    public const float WellLikedDailySocialMoodBonus = 3f;
    public const float RespectedDailySocialSocialBonus = 3f;
    public const float WellLikedDailySocialSocialBonus = 4f;
    public const float RespectedDailyCommunityMoodBonus = 2f;
    public const float WellLikedDailyCommunityMoodBonus = 3f;
    public const float RespectedDailyCommunitySocialBonus = 2f;
    public const float WellLikedDailyCommunitySocialBonus = 3f;
    public const float RespectedDailyProductiveMoodBonus = 2f;
    public const float WellLikedDailyProductiveMoodBonus = 3f;
    public const float RespectedDailyProductiveFatigueBonus = 1f;
    public const float WellLikedDailyProductiveFatigueBonus = 2f;
    public const float RespectedHomeNapMoodBonus = 2f;
    public const float WellLikedHomeNapMoodBonus = 3f;
    public const float RespectedExclusiveHelpMoodBonus = 1f;
    public const float RespectedExclusiveHelpSocialBonus = 1f;
    public const float WellLikedGretaInnMoodBonusExtra = 1f;
    public const float WellLikedGretaInnSocialBonusExtra = 2f;

    public const int WellLikedElsieGardenBonusAppleChancePercent = 30;
    public const int WellLikedNoraHerbBonusAppleChancePercent = 28;
    public const int WellLikedEliasSmithyBonusWoodChancePercent = 32;
    public const int WellLikedMarcusWorkshopBonusPlankChancePercent = 32;
    public const int WellLikedBenPatrolBonusWoodChancePercent = 32;
    public const int WellLikedLilaVillageBonusAppleChancePercent = 32;
    public const int WellLikedRowanStoryBonusWoodChancePercent = 32;
    public const int WellLikedEleanorLegacyBonusAppleChancePercent = 32;

    public const int RespectedTomWoodBonusYieldChancePercent = 30;
    public const int WellLikedTomWoodBonusYieldChancePercent = 45;

    public const int RespectedElsieProjectProgressBonusChancePercent = 22;
    public const int WellLikedElsieProjectProgressBonusChancePercent = 35;
    public const int RespectedTomWoodProjectProgressBonusChancePercent = 22;
    public const int WellLikedTomWoodProjectProgressBonusChancePercent = 35;
    public const int RespectedHaroldProjectProgressBonusChancePercent = 22;
    public const int WellLikedHaroldProjectProgressBonusChancePercent = 35;

    /// <summary>Well-liked exclusive: Harold's elder influence on communal project contributions.</summary>
    public const int WellLikedHaroldElderInfluenceProgressBonusChancePercent = 18;

    /// <summary>Well-liked exclusive: Greta shares useful guest gossip during inn help.</summary>
    public const int GretaWellLikedInnGuestInfoChancePercent = 24;

    /// <summary>Warehouse project — communal storage for harvested goods.</summary>
    public const byte ElsieThemedWarehouseProjectId = VillageSiteIds.Warehouse;

    public static bool IsEligibleForMiraTradeBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Respected;

    public static bool IsEligibleForRoleMechanicalBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.Respected;

    public static bool IsEligibleForContributionBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked;

    public static bool IsElsieGardenActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpGarden;

    public static bool IsNoraHerbActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpHerbGarden;

    public static bool IsGretaInnActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpInn;

    public static bool IsEliasSmithyActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpSmithy;

    public static bool IsMarcusWorkshopActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpWorkshop;

    public static bool IsBenPatrolActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpPatrol;

    public static bool IsLilaVillageActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpVillage;

    public static bool IsRowanStoryActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.ListenToStories;

    public static bool IsEleanorLegacyActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.ChatWithEleanor;

    public static bool IsHaroldWellActivity(CommunityActivityKind activity) =>
        activity == CommunityActivityKind.HelpWell;

    public static bool IsTomWoodGathering(ItemType itemType) =>
        itemType == ItemType.Wood;

    public static bool IsElsieThemedProjectContribution(byte projectId, ItemType itemType) =>
        itemType == ItemType.Apple || projectId == ElsieThemedWarehouseProjectId;

    public static bool IsTomWoodProjectContribution(ItemType itemType) =>
        itemType == ItemType.Wood;

    public static bool IsNoraHerbProjectContribution(ItemType itemType) =>
        itemType == ItemType.Apple;

    public static bool IsEliasSmithingProjectContribution(ItemType itemType) =>
        itemType is ItemType.Plank or ItemType.Tool;

    public static bool IsMarcusCraftingProjectContribution(ItemType itemType) =>
        itemType is ItemType.Plank or ItemType.Tool;

    public static bool IsHaroldCommunityProjectContribution(byte projectId, ItemType itemType) =>
        itemType is ItemType.Stone or ItemType.Plank
        || projectId is VillageSiteIds.Well or VillageSiteIds.Bridge or VillageSiteIds.Warehouse;

    public static bool IsBenSecurityProjectContribution(byte projectId, ItemType itemType) =>
        projectId is VillageSiteIds.Well or VillageSiteIds.Bridge
        && itemType is ItemType.Stone or ItemType.Plank or ItemType.Wood;

    public static bool IsLilaVillageProjectContribution(byte projectId, ItemType itemType) =>
        projectId == ElsieThemedWarehouseProjectId && itemType == ItemType.Apple;

    public static bool IsRowanStoryProjectContribution(byte projectId, ItemType itemType) =>
        projectId == ElsieThemedWarehouseProjectId && itemType == ItemType.Apple;

    public static bool IsEleanorLegacyProjectContribution(byte projectId, ItemType itemType) =>
        projectId == ElsieThemedWarehouseProjectId && itemType == ItemType.Apple;

    public static float GetMiraBuyDiscountPercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedMiraBuyDiscountPercent,
            VillageSocialStandingTier.Respected => RespectedMiraBuyDiscountPercent,
            _ => 0f,
        };

    public static float GetMiraSellBonusPercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedMiraSellBonusPercent,
            VillageSocialStandingTier.Respected => RespectedMiraSellBonusPercent,
            _ => 0f,
        };

    public static float GetMiraBuyPriceMultiplier(VillageSocialStandingTier tier) =>
        1f - GetMiraBuyDiscountPercent(tier) / 100f;

    public static float GetMiraSellPriceMultiplier(VillageSocialStandingTier tier) =>
        1f + GetMiraSellBonusPercent(tier) / 100f;

    public static bool IsDailyVillageActivity(DailyVillageActivityKind activity) =>
        activity is not DailyVillageActivityKind.None;

    public static bool IsDailyVillageLeisureActivity(DailyVillageActivityKind activity) =>
        activity is DailyVillageActivityKind.SitOnBench or DailyVillageActivityKind.WatchVillage;

    public static bool IsDailyVillageSocialActivity(DailyVillageActivityKind activity) =>
        activity == DailyVillageActivityKind.ChatWithLocals;

    public static bool IsDailyVillageCommunityActivity(DailyVillageActivityKind activity) =>
        activity == DailyVillageActivityKind.TendPublicGarden;

    public static bool IsDailyVillageProductiveActivity(DailyVillageActivityKind activity) =>
        activity == DailyVillageActivityKind.PracticeWorkshop;

    public static (float MoodBonus, float SocialBonus, float FatigueBonus) GetDailyVillageActivityStandingBonus(
        DailyVillageActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || !IsDailyVillageActivity(activity))
            return (0f, 0f, 0f);

        if (IsDailyVillageSocialActivity(activity))
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked => (WellLikedDailySocialMoodBonus, WellLikedDailySocialSocialBonus, 0f),
                VillageSocialStandingTier.Respected => (RespectedDailySocialMoodBonus, RespectedDailySocialSocialBonus, 0f),
                _ => (0f, 0f, 0f),
            };
        }

        if (IsDailyVillageCommunityActivity(activity))
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked => (WellLikedDailyCommunityMoodBonus, WellLikedDailyCommunitySocialBonus, 0f),
                VillageSocialStandingTier.Respected => (RespectedDailyCommunityMoodBonus, RespectedDailyCommunitySocialBonus, 0f),
                _ => (0f, 0f, 0f),
            };
        }

        if (IsDailyVillageProductiveActivity(activity))
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked => (WellLikedDailyProductiveMoodBonus, 0f, WellLikedDailyProductiveFatigueBonus),
                VillageSocialStandingTier.Respected => (RespectedDailyProductiveMoodBonus, 0f, RespectedDailyProductiveFatigueBonus),
                _ => (0f, 0f, 0f),
            };
        }

        if (!IsDailyVillageLeisureActivity(activity))
            return (0f, 0f, 0f);

        return tier switch
        {
            VillageSocialStandingTier.WellLiked => (WellLikedDailyLeisureMoodBonus, WellLikedDailyLeisureSocialBonus, 0f),
            VillageSocialStandingTier.Respected => (RespectedDailyLeisureMoodBonus, RespectedDailyLeisureSocialBonus, 0f),
            _ => (0f, 0f, 0f),
        };
    }

    public static float GetHomeNapStandingMoodBonus(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedHomeNapMoodBonus,
            VillageSocialStandingTier.Respected => RespectedHomeNapMoodBonus,
            _ => 0f,
        };

    public static (float MoodBonus, float SocialBonus) GetCommunityActivityStandingBonus(
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier))
            return (0f, 0f);

        if (IsElsieGardenActivity(activity))
            return GetRoleActivityBonus(tier);

        if (IsNoraHerbActivity(activity))
            return GetRoleActivityBonus(tier);

        if (IsGretaInnActivity(activity))
        {
            var (mood, social) = GetRoleActivityBonus(tier);
            if (tier >= VillageSocialStandingTier.WellLiked)
            {
                mood += WellLikedGretaInnMoodBonusExtra;
                social += WellLikedGretaInnSocialBonusExtra;
            }

            return (mood, social);
        }

        if (IsEliasSmithyActivity(activity) || IsMarcusWorkshopActivity(activity) || IsHaroldWellActivity(activity)
            || IsBenPatrolActivity(activity) || IsLilaVillageActivity(activity) || IsRowanStoryActivity(activity)
            || IsEleanorLegacyActivity(activity))
            return GetRoleActivityBonus(tier);

        return (0f, 0f);
    }

    public static (float MoodBonus, float SocialBonus) GetElsieGardenHelpBonus(VillageSocialStandingTier tier) =>
        IsEligibleForRoleMechanicalBonus(tier) && tier >= VillageSocialStandingTier.Respected
            ? GetRoleActivityBonus(tier)
            : (0f, 0f);

    private static (float MoodBonus, float SocialBonus) GetRoleActivityBonus(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => (WellLikedRoleActivityMoodBonus, WellLikedRoleActivitySocialBonus),
            VillageSocialStandingTier.Respected => (
                RespectedRoleActivityMoodBonus + RespectedExclusiveHelpMoodBonus,
                RespectedRoleActivitySocialBonus + RespectedExclusiveHelpSocialBonus),
            _ => (0f, 0f),
        };

    public static int GetTomWoodBonusYieldChancePercent(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedTomWoodBonusYieldChancePercent,
            VillageSocialStandingTier.Respected => RespectedTomWoodBonusYieldChancePercent,
            _ => 0,
        };

    public static int GetContributionScoreBonus(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked ? WellLikedContributionScoreBonus : 0;

    public static bool ShouldGrantContributionProgressBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity)
    {
        if (acceptedQuantity <= 0)
            return false;

        var roll = (playerEntityId * 157 + projectId * 43 + (uint)acceptedQuantity * 19) % 100;
        return roll < WellLikedContributionProgressBonusChancePercent;
    }

    public static bool ShouldGrantRespectedContributionProgressBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity)
    {
        if (acceptedQuantity <= 0)
            return false;

        var roll = (playerEntityId * 159 + projectId * 41 + (uint)acceptedQuantity * 17) % 100;
        return roll < RespectedContributionProgressBonusChancePercent;
    }

    public static bool ShouldGrantElsieProjectProgressBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || acceptedQuantity <= 0)
            return false;

        var chance = tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedElsieProjectProgressBonusChancePercent,
            VillageSocialStandingTier.Respected => RespectedElsieProjectProgressBonusChancePercent,
            _ => 0,
        };

        var roll = (playerEntityId * 163 + projectId * 47 + (uint)acceptedQuantity * 23) % 100;
        return roll < chance;
    }

    public static bool ShouldGrantTomWoodProjectProgressBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || acceptedQuantity <= 0)
            return false;

        var chance = tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedTomWoodProjectProgressBonusChancePercent,
            VillageSocialStandingTier.Respected => RespectedTomWoodProjectProgressBonusChancePercent,
            _ => 0,
        };

        var roll = (playerEntityId * 167 + projectId * 53 + (uint)acceptedQuantity * 29) % 100;
        return roll < chance;
    }

    public static bool ShouldGrantElsieGardenBonusHarvest(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsElsieGardenActivity(activity))
            return false;

        var roll = (playerEntityId * 173 + (uint)activity * 59) % 100;
        return roll < WellLikedElsieGardenBonusAppleChancePercent;
    }

    public static bool ShouldGrantNoraHerbBonusHarvest(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsNoraHerbActivity(activity))
            return false;

        var roll = (playerEntityId * 181 + (uint)activity * 63) % 100;
        return roll < WellLikedNoraHerbBonusAppleChancePercent;
    }

    public static bool ShouldGrantEliasSmithyBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsEliasSmithyActivity(activity))
            return false;

        var roll = (playerEntityId * 191 + (uint)activity * 67) % 100;
        return roll < WellLikedEliasSmithyBonusWoodChancePercent;
    }

    public static bool ShouldGrantMarcusWorkshopBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsMarcusWorkshopActivity(activity))
            return false;

        var roll = (playerEntityId * 201 + (uint)activity * 83) % 100;
        return roll < WellLikedMarcusWorkshopBonusPlankChancePercent;
    }

    public static bool ShouldGrantBenPatrolBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsBenPatrolActivity(activity))
            return false;

        var roll = (playerEntityId * 193 + (uint)activity * 71) % 100;
        return roll < WellLikedBenPatrolBonusWoodChancePercent;
    }

    public static bool ShouldGrantLilaVillageBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsLilaVillageActivity(activity))
            return false;

        var roll = (playerEntityId * 197 + (uint)activity * 73) % 100;
        return roll < WellLikedLilaVillageBonusAppleChancePercent;
    }

    public static bool ShouldGrantRowanStoryBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsRowanStoryActivity(activity))
            return false;

        var roll = (playerEntityId * 199 + (uint)activity * 79) % 100;
        return roll < WellLikedRowanStoryBonusWoodChancePercent;
    }

    public static bool ShouldGrantEleanorLegacyBonusYield(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (tier < VillageSocialStandingTier.WellLiked || !IsEleanorLegacyActivity(activity))
            return false;

        var roll = (playerEntityId * 203 + (uint)activity * 81) % 100;
        return roll < WellLikedEleanorLegacyBonusAppleChancePercent;
    }

    public static bool ShouldGrantHaroldProjectProgressBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || acceptedQuantity <= 0)
            return false;

        var chance = tier switch
        {
            VillageSocialStandingTier.WellLiked => WellLikedHaroldProjectProgressBonusChancePercent,
            VillageSocialStandingTier.Respected => RespectedHaroldProjectProgressBonusChancePercent,
            _ => 0,
        };

        var roll = (playerEntityId * 193 + projectId * 59 + (uint)acceptedQuantity * 31) % 100;
        return roll < chance;
    }

    public static bool ShouldGrantTomWoodBonusYield(
        uint playerEntityId,
        int nodeId,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier))
            return false;

        var chance = GetTomWoodBonusYieldChancePercent(tier);
        var roll = (playerEntityId * 179 + (uint)nodeId * 61) % 100;
        return roll < chance;
    }

    public static bool IsEligibleForWellLikedPrestigePrivilege(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked;

    public static bool ShouldGrantHaroldWellLikedElderInfluenceBonus(
        uint playerEntityId,
        byte projectId,
        int acceptedQuantity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForWellLikedPrestigePrivilege(tier) || acceptedQuantity <= 0)
            return false;

        var roll = (playerEntityId * 197 + projectId * 61 + (uint)acceptedQuantity * 37) % 100;
        return roll < WellLikedHaroldElderInfluenceProgressBonusChancePercent;
    }

    public static bool ShouldGrantGretaWellLikedInnGuestInfo(
        uint playerEntityId,
        CommunityActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForWellLikedPrestigePrivilege(tier) || !IsGretaInnActivity(activity))
            return false;

        var roll = (playerEntityId * 199 + (uint)activity * 71) % 100;
        return roll < GretaWellLikedInnGuestInfoChancePercent;
    }

    public static string FormatHaroldWellLikedElderInfluenceFeedback(
        VillageSocialStandingTier tier,
        int progressBonus)
    {
        if (!IsEligibleForWellLikedPrestigePrivilege(tier) || progressBonus <= 0)
            return string.Empty;

        var line = VillageSocialStandingDialogue.TryGetHaroldWellLikedProjectAcknowledgmentLine(
            (uint)(progressBonus * 17 + 3));
        return string.IsNullOrWhiteSpace(line)
            ? $" [Harold's elder influence — because Bloomtown esteems you, your contribution counts as +{progressBonus} extra progress.]"
            : $" [{line} (+{progressBonus} extra progress from your standing.)]";
    }

    public static string FormatGretaWellLikedInnGuestInfoFeedback(uint variationSeed)
    {
        var line = VillageSocialStandingDialogue.TryGetGretaWellLikedInnGuestInfoLine(variationSeed);
        return string.IsNullOrWhiteSpace(line)
            ? string.Empty
            : $" [Well-liked privilege — {line}]";
    }

    public static string FormatMiraBuyTradeFeedback(VillageSocialStandingTier tier)
    {
        var discount = GetMiraBuyDiscountPercent(tier);
        return tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"[Because Bloomtown regards you as well-liked, Mira trims ~{discount:F0}% off — you pay noticeably less.]",
            VillageSocialStandingTier.Respected =>
                $"[Because neighbors trust you here, Mira trims ~{discount:F0}% off — your standing earns a fairer price.]",
            _ => string.Empty,
        };
    }

    public static string FormatMiraSellTradeFeedback(VillageSocialStandingTier tier)
    {
        var bonus = GetMiraSellBonusPercent(tier);
        return tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"[Because you're well-liked in Bloomtown, Mira pays ~{bonus:F0}% more — the market favors trusted regulars.]",
            VillageSocialStandingTier.Respected =>
                $"[Because neighbors speak well of you, Mira pays ~{bonus:F0}% more — respected folk earn a better rate.]",
            _ => string.Empty,
        };
    }

    public static string FormatDailyVillageActivityStandingFeedback(
        DailyVillageActivityKind activity,
        VillageSocialStandingTier tier)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || !IsDailyVillageActivity(activity))
            return string.Empty;

        var (moodBonus, socialBonus, fatigueBonus) = GetDailyVillageActivityStandingBonus(activity, tier);
        var recoveryNote = moodBonus > 0f || socialBonus > 0f || fatigueBonus > 0f
            ? fatigueBonus > 0f
                ? $" (mood +{moodBonus:F0}, fatigue -{fatigueBonus:F0} from your standing)"
                : $" (mood +{moodBonus:F0}, social need -{socialBonus:F0} from your standing)"
            : string.Empty;

        return (activity, tier) switch
        {
            (DailyVillageActivityKind.SitOnBench, VillageSocialStandingTier.WellLiked) =>
                $" [Because you're well-liked here, the village green feels personally welcoming — neighbors make room beside you.{recoveryNote}]",
            (DailyVillageActivityKind.SitOnBench, VillageSocialStandingTier.Respected) =>
                $" [Because neighbors trust you, the bench feels like a familiar pause in the village day.{recoveryNote}]",
            (DailyVillageActivityKind.WatchVillage, VillageSocialStandingTier.WellLiked) =>
                $" [Because Bloomtown esteems you, watching the lanes feels like belonging to every small story below.{recoveryNote}]",
            (DailyVillageActivityKind.WatchVillage, VillageSocialStandingTier.Respected) =>
                $" [Because neighbors know your face, the outlook feels quietly meaningful today.{recoveryNote}]",
            (DailyVillageActivityKind.ChatWithLocals, VillageSocialStandingTier.WellLiked) =>
                $" [Because you're well-liked here, village chatter opens easily — locals seek you out for a friendly word.{recoveryNote}]",
            (DailyVillageActivityKind.ChatWithLocals, VillageSocialStandingTier.Respected) =>
                $" [Because neighbors trust you, small talk feels warmer and less like passing time.{recoveryNote}]",
            (DailyVillageActivityKind.TendPublicGarden, VillageSocialStandingTier.WellLiked) =>
                $" [Because Bloomtown esteems you, tending shared beds feels like caring for everyone's front yard.{recoveryNote}]",
            (DailyVillageActivityKind.TendPublicGarden, VillageSocialStandingTier.Respected) =>
                $" [Because neighbors know your hands, the public garden welcomes your steady care.{recoveryNote}]",
            (DailyVillageActivityKind.PracticeWorkshop, VillageSocialStandingTier.WellLiked) =>
                $" [Because you're well-liked here, Marcus's bench feels like a place that expects your skill to grow.{recoveryNote}]",
            (DailyVillageActivityKind.PracticeWorkshop, VillageSocialStandingTier.Respected) =>
                $" [Because neighbors trust your workmanship, practicing here feels quietly encouraged.{recoveryNote}]",
            _ => string.Empty,
        };
    }

    public static string FormatHomeNapStandingFeedback(VillageSocialStandingTier tier)
    {
        var moodBonus = GetHomeNapStandingMoodBonus(tier);
        if (moodBonus <= 0f)
            return string.Empty;

        return tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $" [Because Bloomtown regards you as well-liked, your nap feels especially peaceful — mood +{moodBonus:F0} from your standing.]",
            VillageSocialStandingTier.Respected =>
                $" [Because neighbors trust you here, resting at home feels a little sweeter — mood +{moodBonus:F0} from your standing.]",
            _ => string.Empty,
        };
    }

    public static string FormatCommunityActivityStandingFeedback(
        CommunityActivityKind activity,
        VillageSocialStandingTier tier,
        bool grantedBonusItem)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier))
            return string.Empty;

        var (moodBonus, socialBonus) = GetCommunityActivityStandingBonus(activity, tier);
        var recoveryNote = moodBonus > 0f || socialBonus > 0f
            ? $" (mood +{moodBonus:F0}, social need -{socialBonus:F0} from your standing)"
            : string.Empty;

        var baseLine = (activity, tier) switch
        {
            (CommunityActivityKind.HelpGarden, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Elsie's garden rewards trusted hands — your help goes further today.{recoveryNote}]",
            (CommunityActivityKind.HelpGarden, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the garden beds respond to steady hands.{recoveryNote}]",
            (CommunityActivityKind.HelpHerbGarden, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Nora's herb rows welcome a trusted helper — your tending goes further today.{recoveryNote}]",
            (CommunityActivityKind.HelpHerbGarden, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the herb beds respond to patient hands.{recoveryNote}]",
            (CommunityActivityKind.HelpInn, VillageSocialStandingTier.WellLiked) =>
                $"[Because Bloomtown esteems you, Greta's parlor welcomes you as an honored regular — the hearth restores you more today.{recoveryNote}]",
            (CommunityActivityKind.HelpInn, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the inn responds warmly to steady help.{recoveryNote}]",
            (CommunityActivityKind.HelpSmithy, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Elias's forge welcomes a trusted helper — your work goes further today.{recoveryNote}]",
            (CommunityActivityKind.HelpSmithy, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the smithy responds to steady hands at the anvil.{recoveryNote}]",
            (CommunityActivityKind.HelpWorkshop, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Marcus's workshop welcomes a trusted helper — your work goes further today.{recoveryNote}]",
            (CommunityActivityKind.HelpWorkshop, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the workshop responds to steady hands at the bench.{recoveryNote}]",
            (CommunityActivityKind.HelpPatrol, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Ben's patrol welcomes a trusted helper — your steps go further today.{recoveryNote}]",
            (CommunityActivityKind.HelpPatrol, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the guard post responds to steady hands on the route.{recoveryNote}]",
            (CommunityActivityKind.HelpVillage, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Lila's lanes welcome a trusted helper — your work goes further today.{recoveryNote}]",
            (CommunityActivityKind.HelpVillage, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the village lanes respond to steady hands around the square.{recoveryNote}]",
            (CommunityActivityKind.ListenToStories, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Rowan's bench welcomes a trusted listener — your patience goes further today.{recoveryNote}]",
            (CommunityActivityKind.ListenToStories, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the story bench responds to steady ears near the inn.{recoveryNote}]",
            (CommunityActivityKind.ChatWithEleanor, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Eleanor's porch welcomes a trusted listener — your patience goes further today.{recoveryNote}]",
            (CommunityActivityKind.ChatWithEleanor, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the cottage porch responds to steady ears near the well.{recoveryNote}]",
            (CommunityActivityKind.HelpWell, VillageSocialStandingTier.WellLiked) =>
                $"[Because you're well-liked here, Harold's well-side counts your communal work more today.{recoveryNote}]",
            (CommunityActivityKind.HelpWell, VillageSocialStandingTier.Respected) =>
                $"[Because neighbors trust you, the well responds to steady help at the village heart.{recoveryNote}]",
            _ => string.Empty,
        };

        if (string.IsNullOrWhiteSpace(baseLine))
            return string.Empty;

        if (!grantedBonusItem)
            return baseLine;

        var itemNote = activity switch
        {
            CommunityActivityKind.HelpGarden =>
                "A ripe apple finds its way into your basket — Elsie's quiet thank-you.",
            CommunityActivityKind.HelpHerbGarden =>
                "A small bundle of leaves and an apple find their way to you — Nora's quiet thank-you.",
            CommunityActivityKind.HelpSmithy =>
                "A piece of wood lands by your feet — Elias's quiet thank-you.",
            CommunityActivityKind.HelpWorkshop =>
                "A plank lands by your feet — Marcus's quiet thank-you.",
            CommunityActivityKind.HelpPatrol =>
                "A piece of wood lands by your feet — Ben's quiet thank-you.",
            CommunityActivityKind.HelpVillage =>
                "A ripe apple finds its way into your basket — Lila's quiet thank-you.",
            CommunityActivityKind.ListenToStories =>
                "A piece of wood lands by your feet — Rowan's quiet thank-you.",
            CommunityActivityKind.ChatWithEleanor =>
                "A ripe apple finds its way into your basket — Eleanor's quiet thank-you.",
            _ => string.Empty,
        };

        return string.IsNullOrWhiteSpace(itemNote)
            ? baseLine
            : $"{baseLine} [{itemNote}]";
    }

    public static string FormatElsieGardenHelpFeedback(
        VillageSocialStandingTier tier,
        bool grantedBonusApple) =>
        FormatCommunityActivityStandingFeedback(
            CommunityActivityKind.HelpGarden,
            tier,
            grantedBonusApple);

    public static string FormatTomWoodGatherFeedback(VillageSocialStandingTier tier, int bonusYield)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || bonusYield <= 0)
            return string.Empty;

        return tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $" [Because Bloomtown regards you as well-liked, Tom's woodpile yields {bonusYield} extra wood for you.]",
            VillageSocialStandingTier.Respected =>
                $" [Because neighbors trust your hands, you gather {bonusYield} extra wood from the pile.]",
            _ => string.Empty,
        };
    }

    public static string FormatRoleProjectContributionFeedback(
        VillageSocialStandingTier tier,
        int progressBonus,
        bool elsieThemed,
        bool tomWood,
        bool haroldCommunity = false)
    {
        if (!IsEligibleForRoleMechanicalBonus(tier) || progressBonus <= 0)
            return string.Empty;

        if (elsieThemed)
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked =>
                    $" [Because you're well-liked here, Elsie would notice — your food-and-garden effort counts as +{progressBonus} extra progress.]",
                VillageSocialStandingTier.Respected =>
                    $" [Because neighbors trust you, your village food work counts as +{progressBonus} extra progress.]",
                _ => string.Empty,
            };
        }

        if (tomWood)
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked =>
                    $" [Because you're well-liked here, Tom's yard would nod — your wood counts as +{progressBonus} extra progress.]",
                VillageSocialStandingTier.Respected =>
                    $" [Because neighbors trust your hands, your wood contribution counts as +{progressBonus} extra progress.]",
                _ => string.Empty,
            };
        }

        if (haroldCommunity)
        {
            return tier switch
            {
                VillageSocialStandingTier.WellLiked =>
                    $" [Because you're well-liked here, Harold would notice — your communal building effort counts as +{progressBonus} extra progress.]",
                VillageSocialStandingTier.Respected =>
                    $" [Because neighbors trust you at shared work, your contribution counts as +{progressBonus} extra progress.]",
                _ => string.Empty,
            };
        }

        return string.Empty;
    }

    public static string FormatContributionBonusFeedback(
        VillageSocialStandingTier tier,
        int progressBonus,
        int scoreBonus)
    {
        if (tier < VillageSocialStandingTier.WellLiked || (progressBonus <= 0 && scoreBonus <= 0))
            return string.Empty;

        var parts = new List<string>();
        if (progressBonus > 0)
            parts.Add("the village counts your effort as a little extra progress");
        if (scoreBonus > 0)
            parts.Add($"+{scoreBonus} village contribution score for your standing");

        return $" [Because Bloomtown regards you as well-liked — {string.Join("; ", parts)}.]";
    }

    public static string FormatRespectedContributionBonusFeedback(int progressBonus)
    {
        if (progressBonus <= 0)
            return string.Empty;

        return " [Because neighbors trust you at Respected standing, the village counts your effort as a little extra progress.]";
    }

    public static string? FormatCompactMechanicalBenefitsHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                "Perks: Mira trade discount · role help mood/social bonus · bonus harvests/yields · "
                + $"project +{WellLikedHaroldProjectProgressBonusChancePercent}% extra progress · "
                + $"favors ~{SocialStandingActionConfig.WellLikedSuccessChancePercent}% / {SocialStandingActionConfig.WellLikedCooldownGameMinutes}m.",
            VillageSocialStandingTier.Respected =>
                "Perks: Mira trade discount · role help mood/social bonus (+1 Respected-only) · "
                + $"~{RespectedContributionProgressBonusChancePercent}% extra project progress · occasional bonus yields · "
                + $"favors ~{SocialStandingActionConfig.RespectedSuccessChancePercent}% / {SocialStandingActionConfig.RespectedCooldownGameMinutes}m.",
            _ => null,
        };

    public static string? FormatCompactNpcPerksHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                "NPC perks: Harold project sway · Greta inn gossip · Elsie/Nora/Elias/Marcus/Tom bonus yields.",
            VillageSocialStandingTier.Respected =>
                "NPC perks: warmer help from Greta, Nora, Elias, Marcus, Elsie · Tom may yield +1 wood.",
            _ => null,
        };

    public static string? FormatMechanicalBenefitsHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                $"Mechanical benefits (Well-liked): Mira ~{WellLikedMiraBuyDiscountPercent:F0}% cheaper buys / "
                + $"~{WellLikedMiraSellBonusPercent:F0}% better sells; "
                + $"role help gives mood +{WellLikedRoleActivityMoodBonus:F0}/social -{WellLikedRoleActivitySocialBonus:F0} "
                + $"(Greta inn +{WellLikedGretaInnMoodBonusExtra + WellLikedRoleActivityMoodBonus:F0}/-{WellLikedGretaInnSocialBonusExtra + WellLikedRoleActivitySocialBonus:F0}); "
                + $"Elsie garden ~{WellLikedElsieGardenBonusAppleChancePercent}% bonus apple; "
                + $"Nora herbs ~{WellLikedNoraHerbBonusAppleChancePercent}% bonus harvest; "
                + $"Elias smithy ~{WellLikedEliasSmithyBonusWoodChancePercent}% bonus wood; "
                + $"Marcus workshop ~{WellLikedMarcusWorkshopBonusPlankChancePercent}% bonus plank; "
                + $"Tom wood ~{WellLikedTomWoodBonusYieldChancePercent}% +1 wood; "
                + $"Harold/Elsie/Tom projects ~{WellLikedHaroldProjectProgressBonusChancePercent}% extra progress; "
                + $"communal contributions ~{WellLikedContributionProgressBonusChancePercent}% extra progress "
                + $"+{WellLikedContributionScoreBonus} score; "
                + $"favors ~{SocialStandingActionConfig.WellLikedSuccessChancePercent}% success, "
                + $"~{SocialStandingActionConfig.WellLikedCooldownGameMinutes}m cooldown.",
            VillageSocialStandingTier.Respected =>
                $"Mechanical benefits (Respected): Mira ~{RespectedMiraBuyDiscountPercent:F0}% cheaper buys / "
                + $"~{RespectedMiraSellBonusPercent:F0}% better sells; "
                + $"Greta inn, Nora herbs, Elias smithy, Marcus workshop, Elsie garden, and Harold well help give mood +{RespectedRoleActivityMoodBonus + RespectedExclusiveHelpMoodBonus:F0}/social -{RespectedRoleActivitySocialBonus + RespectedExclusiveHelpSocialBonus:F0}; "
                + $"communal contributions ~{RespectedContributionProgressBonusChancePercent}% extra progress; "
                + $"Tom wood ~{RespectedTomWoodBonusYieldChancePercent}% +1 wood; "
                + $"Harold/Elsie/Tom projects ~{RespectedHaroldProjectProgressBonusChancePercent}% extra progress; "
                + $"favors ~{SocialStandingActionConfig.RespectedSuccessChancePercent}% success, "
                + $"~{SocialStandingActionConfig.RespectedCooldownGameMinutes}m cooldown.",
            _ => null,
        };

    public static string? FormatNpcMechanicalBenefitsSummary(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                "NPC standing perks (Well-liked): Mira trade discount; Greta inn guest gossip + honored treatment; "
                + "Harold elder influence on projects + exclusive privileges; Nora herbs bonus harvest; "
                + "Elias smithy bonus wood; Marcus workshop bonus plank; Elsie garden bonus apple; Tom wood +1 yield.",
            VillageSocialStandingTier.Respected =>
                "NPC standing perks (Respected): Mira trade discount; Greta, Nora, Elias role help mood/social +1 exclusive bonus; "
                + "~14% communal contribution extra progress; Tom wood may yield +1; Harold/Elsie/Tom project contributions may count extra progress.",
            _ => null,
        };
}