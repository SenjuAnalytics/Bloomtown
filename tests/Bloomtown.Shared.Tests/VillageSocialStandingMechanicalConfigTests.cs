using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialStandingMechanicalConfigTests
{
    [Theory]
    [InlineData(VillageSocialStandingTier.Known, false)]
    [InlineData(VillageSocialStandingTier.Respected, true)]
    [InlineData(VillageSocialStandingTier.WellLiked, true)]
    public void IsEligibleForMiraTradeBonus_RequiresRespectedOrHigher(
        VillageSocialStandingTier tier,
        bool expected)
    {
        Assert.Equal(expected, VillageSocialStandingMechanicalConfig.IsEligibleForMiraTradeBonus(tier));
    }

    [Fact]
    public void GetMiraBuyPriceMultiplier_ReducesPriceAtHigherTiers()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetMiraBuyPriceMultiplier(
            VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingMechanicalConfig.GetMiraBuyPriceMultiplier(
            VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked < respected);
        Assert.True(respected < 1f);
    }

    [Fact]
    public void GetMiraSellPriceMultiplier_IncreasesPayoutAtHigherTiers()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetMiraSellPriceMultiplier(
            VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingMechanicalConfig.GetMiraSellPriceMultiplier(
            VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked > respected);
        Assert.True(respected > 1f);
    }

    [Theory]
    [InlineData(VillageSocialStandingTier.Respected, 0)]
    [InlineData(VillageSocialStandingTier.WellLiked, 1)]
    public void GetContributionScoreBonus_OnlyAtWellLiked(
        VillageSocialStandingTier tier,
        int expected)
    {
        Assert.Equal(expected, VillageSocialStandingMechanicalConfig.GetContributionScoreBonus(tier));
    }

    [Fact]
    public void ShouldGrantContributionProgressBonus_UsesLowThreshold()
    {
        var roll = (5u * 157 + 2u * 43 + 3u * 19) % 100;
        Assert.Equal(
            roll < VillageSocialStandingMechanicalConfig.WellLikedContributionProgressBonusChancePercent,
            VillageSocialStandingMechanicalConfig.ShouldGrantContributionProgressBonus(5, 2, 3));
    }

    [Fact]
    public void ShouldGrantRespectedContributionProgressBonus_UsesRespectedThreshold()
    {
        var roll = (5u * 159 + 2u * 41 + 3u * 17) % 100;
        Assert.Equal(
            roll < VillageSocialStandingMechanicalConfig.RespectedContributionProgressBonusChancePercent,
            VillageSocialStandingMechanicalConfig.ShouldGrantRespectedContributionProgressBonus(5, 2, 3));
    }

    [Fact]
    public void GetCommunityActivityStandingBonus_RespectedGetsExclusiveHelpBonus()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(
            CommunityActivityKind.HelpGarden,
            VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(
            CommunityActivityKind.HelpGarden,
            VillageSocialStandingTier.WellLiked);

        Assert.Equal(
            VillageSocialStandingMechanicalConfig.RespectedRoleActivityMoodBonus
                + VillageSocialStandingMechanicalConfig.RespectedExclusiveHelpMoodBonus,
            respected.MoodBonus);
        Assert.Equal(
            VillageSocialStandingMechanicalConfig.RespectedRoleActivitySocialBonus
                + VillageSocialStandingMechanicalConfig.RespectedExclusiveHelpSocialBonus,
            respected.SocialBonus);
        Assert.True(wellLiked.SocialBonus > respected.SocialBonus);
    }

    [Fact]
    public void FormatMechanicalBenefitsHint_ShowsMiraAndFavorBenefitsAtRespected()
    {
        var hint = VillageSocialStandingMechanicalConfig.FormatMechanicalBenefitsHint(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(hint);
        Assert.Contains("Mira", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("favor", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMechanicalBenefitsHint_IncludesContributionAtWellLiked()
    {
        var hint = VillageSocialStandingMechanicalConfig.FormatMechanicalBenefitsHint(
            VillageSocialStandingTier.WellLiked);

        Assert.NotNull(hint);
        Assert.Contains("contribution", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetElsieGardenHelpBonus_OnlyAtRespectedOrHigher()
    {
        var known = VillageSocialStandingMechanicalConfig.GetElsieGardenHelpBonus(VillageSocialStandingTier.Known);
        var respected = VillageSocialStandingMechanicalConfig.GetElsieGardenHelpBonus(VillageSocialStandingTier.Respected);

        Assert.Equal((0f, 0f), known);
        Assert.True(respected.MoodBonus > 0f);
    }

    [Fact]
    public void IsElsieGardenActivity_OnlyGarden()
    {
        Assert.True(VillageSocialStandingMechanicalConfig.IsElsieGardenActivity(
            CommunityActivityKind.HelpGarden));
        Assert.False(VillageSocialStandingMechanicalConfig.IsElsieGardenActivity(
            CommunityActivityKind.HelpHerbGarden));
        Assert.False(VillageSocialStandingMechanicalConfig.IsElsieGardenActivity(
            CommunityActivityKind.HelpLumber));
    }

    [Fact]
    public void GetCommunityActivityStandingBonus_CoversRoleActivities()
    {
        var respectedGarden = VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(
            CommunityActivityKind.HelpGarden,
            VillageSocialStandingTier.Respected);
        var wellLikedInn = VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(
            CommunityActivityKind.HelpInn,
            VillageSocialStandingTier.WellLiked);
        var knownSmithy = VillageSocialStandingMechanicalConfig.GetCommunityActivityStandingBonus(
            CommunityActivityKind.HelpSmithy,
            VillageSocialStandingTier.Known);

        Assert.True(respectedGarden.MoodBonus > 0f);
        Assert.True(wellLikedInn.SocialBonus > respectedGarden.SocialBonus);
        Assert.Equal((0f, 0f), knownSmithy);
    }

    [Fact]
    public void IsHaroldCommunityProjectContribution_IncludesStoneAndVillageSites()
    {
        Assert.True(VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(
            VillageSiteIds.Well,
            ItemType.Stone));
        Assert.True(VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(
            VillageSiteIds.Bridge,
            ItemType.Plank));
        Assert.False(VillageSocialStandingMechanicalConfig.IsHaroldCommunityProjectContribution(
            projectId: 99,
            ItemType.Apple));
    }

    [Fact]
    public void FormatMechanicalBenefitsHint_MentionsHaroldGretaNoraAndEliasAtRespected()
    {
        var hint = VillageSocialStandingMechanicalConfig.FormatMechanicalBenefitsHint(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(hint);
        Assert.Contains("Harold", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Greta", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nora", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Elias", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldGrantTomWoodBonusYield_RequiresRespectedTier()
    {
        Assert.False(VillageSocialStandingMechanicalConfig.ShouldGrantTomWoodBonusYield(
            5,
            nodeId: 2,
            VillageSocialStandingTier.Known));

        var roll = (5u * 179 + 2u * 61) % 100;
        Assert.Equal(
            roll < VillageSocialStandingMechanicalConfig.RespectedTomWoodBonusYieldChancePercent,
            VillageSocialStandingMechanicalConfig.ShouldGrantTomWoodBonusYield(
                5,
                nodeId: 2,
                VillageSocialStandingTier.Respected));
    }

    [Fact]
    public void IsTomWoodProjectContribution_OnlyWoodItems()
    {
        Assert.True(VillageSocialStandingMechanicalConfig.IsTomWoodProjectContribution(ItemType.Wood));
        Assert.False(VillageSocialStandingMechanicalConfig.IsTomWoodProjectContribution(ItemType.Stone));
    }

    [Fact]
    public void ShouldGrantHaroldWellLikedElderInfluenceBonus_RequiresWellLikedTier()
    {
        Assert.False(VillageSocialStandingMechanicalConfig.ShouldGrantHaroldWellLikedElderInfluenceBonus(
            5,
            projectId: 2,
            acceptedQuantity: 3,
            VillageSocialStandingTier.Respected));

        var roll = (5u * 197 + 2u * 61 + 3u * 37) % 100;
        Assert.Equal(
            roll < VillageSocialStandingMechanicalConfig.WellLikedHaroldElderInfluenceProgressBonusChancePercent,
            VillageSocialStandingMechanicalConfig.ShouldGrantHaroldWellLikedElderInfluenceBonus(
                5,
                projectId: 2,
                acceptedQuantity: 3,
                VillageSocialStandingTier.WellLiked));
    }

    [Fact]
    public void ShouldGrantGretaWellLikedInnGuestInfo_RequiresWellLikedAndInnActivity()
    {
        Assert.False(VillageSocialStandingMechanicalConfig.ShouldGrantGretaWellLikedInnGuestInfo(
            5,
            CommunityActivityKind.HelpGarden,
            VillageSocialStandingTier.WellLiked));

        var roll = (5u * 199 + (uint)CommunityActivityKind.HelpInn * 71) % 100;
        Assert.Equal(
            roll < VillageSocialStandingMechanicalConfig.GretaWellLikedInnGuestInfoChancePercent,
            VillageSocialStandingMechanicalConfig.ShouldGrantGretaWellLikedInnGuestInfo(
                5,
                CommunityActivityKind.HelpInn,
                VillageSocialStandingTier.WellLiked));
    }

    [Fact]
    public void FormatMechanicalBenefitsHint_MentionsElsieAndTomAtRespected()
    {
        var hint = VillageSocialStandingMechanicalConfig.FormatMechanicalBenefitsHint(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(hint);
        Assert.Contains("Elsie", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Tom", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetDailyVillageActivityStandingBonus_RespectedGetsLightBonus()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetDailyVillageActivityStandingBonus(
            DailyVillageActivityKind.WatchVillage,
            VillageSocialStandingTier.Respected);
        var known = VillageSocialStandingMechanicalConfig.GetDailyVillageActivityStandingBonus(
            DailyVillageActivityKind.WatchVillage,
            VillageSocialStandingTier.Known);

        Assert.Equal(
            (VillageSocialStandingMechanicalConfig.RespectedDailyLeisureMoodBonus,
                VillageSocialStandingMechanicalConfig.RespectedDailyLeisureSocialBonus,
                0f),
            respected);
        Assert.Equal((0f, 0f, 0f), known);
    }

    [Fact]
    public void GetDailyVillageActivityStandingBonus_SocialActivityFavorsSocialRelief()
    {
        var wellLiked = VillageSocialStandingMechanicalConfig.GetDailyVillageActivityStandingBonus(
            DailyVillageActivityKind.ChatWithLocals,
            VillageSocialStandingTier.WellLiked);

        Assert.Equal(VillageSocialStandingMechanicalConfig.WellLikedDailySocialMoodBonus, wellLiked.MoodBonus);
        Assert.Equal(VillageSocialStandingMechanicalConfig.WellLikedDailySocialSocialBonus, wellLiked.SocialBonus);
    }

    [Fact]
    public void GetDailyVillageActivityStandingBonus_ProductiveActivityAddsFatigueRelief()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetDailyVillageActivityStandingBonus(
            DailyVillageActivityKind.PracticeWorkshop,
            VillageSocialStandingTier.Respected);

        Assert.Equal(VillageSocialStandingMechanicalConfig.RespectedDailyProductiveMoodBonus, respected.MoodBonus);
        Assert.Equal(VillageSocialStandingMechanicalConfig.RespectedDailyProductiveFatigueBonus, respected.FatigueBonus);
    }

    [Fact]
    public void FormatDailyVillageActivityStandingFeedback_NewActivitiesMentionSocialTone()
    {
        var chatFeedback = VillageSocialStandingMechanicalConfig.FormatDailyVillageActivityStandingFeedback(
            DailyVillageActivityKind.ChatWithLocals,
            VillageSocialStandingTier.Respected);
        var gardenFeedback = VillageSocialStandingMechanicalConfig.FormatDailyVillageActivityStandingFeedback(
            DailyVillageActivityKind.TendPublicGarden,
            VillageSocialStandingTier.WellLiked);

        Assert.Contains("trust", chatFeedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("esteems", gardenFeedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetHomeNapStandingMoodBonus_WellLikedExceedsRespected()
    {
        var respected = VillageSocialStandingMechanicalConfig.GetHomeNapStandingMoodBonus(
            VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingMechanicalConfig.GetHomeNapStandingMoodBonus(
            VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked > respected);
        Assert.Equal(0f, VillageSocialStandingMechanicalConfig.GetHomeNapStandingMoodBonus(
            VillageSocialStandingTier.Known));
    }

    [Fact]
    public void FormatDailyVillageActivityStandingFeedback_MentionsSocialTone()
    {
        var feedback = VillageSocialStandingMechanicalConfig.FormatDailyVillageActivityStandingFeedback(
            DailyVillageActivityKind.SitOnBench,
            VillageSocialStandingTier.WellLiked);

        Assert.Contains("well-liked", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mood +", feedback);
    }
}