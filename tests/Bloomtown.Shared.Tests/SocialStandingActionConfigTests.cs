using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class SocialStandingActionConfigTests
{
    [Theory]
    [InlineData(VillageSocialStandingTier.Known, false)]
    [InlineData(VillageSocialStandingTier.Respected, true)]
    [InlineData(VillageSocialStandingTier.WellLiked, true)]
    public void IsEligible_RequiresRespectedOrHigher(
        VillageSocialStandingTier tier,
        bool expected)
    {
        Assert.Equal(expected, SocialStandingActionConfig.IsEligible(tier));
    }

    [Fact]
    public void GetSuccessChancePercent_WellLikedHigherThanRespected()
    {
        var respected = SocialStandingActionConfig.GetSuccessChancePercent(VillageSocialStandingTier.Respected);
        var wellLiked = SocialStandingActionConfig.GetSuccessChancePercent(VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked > respected);
    }

    [Fact]
    public void GetCooldownGameMinutes_WellLikedShorterThanRespected()
    {
        var respected = SocialStandingActionConfig.GetCooldownGameMinutes(VillageSocialStandingTier.Respected);
        var wellLiked = SocialStandingActionConfig.GetCooldownGameMinutes(VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked < respected);
        Assert.InRange(respected, 75, 85);
        Assert.InRange(wellLiked, 55, 65);
    }

    [Theory]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Mira)]
    [InlineData(NpcEntityIds.Tom)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Nora)]
    public void TryGetSuccessResponseLine_ReturnsPersonalityLine(uint npcEntityId)
    {
        var line = SocialStandingActionDialogue.TryGetSuccessResponseLine(
            npcEntityId,
            VillageSocialStandingTier.Respected,
            SocialStandingFavorOutcomeKind.Info,
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains(
            NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
            line,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldSucceed_UsesTierThreshold()
    {
        var roll = (5u * 107 + NpcEntityIds.Mira * 79 + 7u * 53 + (uint)(300 % 911)) % 100;
        Assert.Equal(
            roll < SocialStandingActionConfig.RespectedSuccessChancePercent,
            SocialStandingActionConfig.ShouldSucceed(
                5,
                NpcEntityIds.Mira,
                VillageSocialStandingTier.Respected,
                totalGameMinutes: 300,
                variationSeed: 7));
    }

    [Fact]
    public void FormatSocialStandingActionHint_RequiresRespectedTier()
    {
        Assert.Null(VillageSocialStandingConfig.FormatSocialStandingActionHint(VillageSocialStandingTier.Known));
        Assert.False(string.IsNullOrWhiteSpace(
            VillageSocialStandingConfig.FormatSocialStandingActionHint(VillageSocialStandingTier.Respected)));
    }

    [Fact]
    public void GetRecoveryBonus_WellLikedHigherThanRespected()
    {
        var respected = SocialStandingActionConfig.GetRecoveryBonus(VillageSocialStandingTier.Respected);
        var wellLiked = SocialStandingActionConfig.GetRecoveryBonus(VillageSocialStandingTier.WellLiked);

        Assert.True(wellLiked.MoodBonus > respected.MoodBonus);
        Assert.True(wellLiked.SocialBonus > respected.SocialBonus);
    }

    [Fact]
    public void FormatMechanicalBenefitsHint_RequiresRespectedTier()
    {
        Assert.Null(VillageSocialStandingConfig.FormatMechanicalBenefitsHint(VillageSocialStandingTier.Known));
        Assert.False(string.IsNullOrWhiteSpace(
            VillageSocialStandingConfig.FormatMechanicalBenefitsHint(VillageSocialStandingTier.Respected)));
    }
}