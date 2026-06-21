using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class SocialLegacyConfigTests
{
    [Theory]
    [InlineData(VillageSocialStandingTier.Respected, 2, SocialMilestoneKind.KnownInBloomtown, true)]
    [InlineData(VillageSocialStandingTier.Respected, 2, SocialMilestoneKind.TrustedNeighbor, false)]
    [InlineData(VillageSocialStandingTier.WellLiked, 3, SocialMilestoneKind.TrustedNeighbor, true)]
    [InlineData(VillageSocialStandingTier.WellLiked, 3, SocialMilestoneKind.VillagePillar, true)]
    [InlineData(VillageSocialStandingTier.Known, 1, SocialMilestoneKind.KnownInBloomtown, false)]
    public void HasMilestone_UnlocksAtExpectedThresholds(
        VillageSocialStandingTier tier,
        int focusCloseFriends,
        SocialMilestoneKind milestone,
        bool expected)
    {
        Assert.Equal(
            expected,
            SocialLegacyConfig.HasMilestone(milestone, tier, focusCloseFriends));
    }

    [Fact]
    public void GetEarnedMilestones_RespectedShowsKnownOnly()
    {
        var milestones = SocialLegacyConfig.GetEarnedMilestones(
            VillageSocialStandingTier.Respected,
            focusCloseFriendCount: 2);

        Assert.Single(milestones);
        Assert.Equal(SocialMilestoneKind.KnownInBloomtown, milestones[0]);
    }

    [Fact]
    public void GetEarnedMilestones_WellLikedShowsAllThree()
    {
        var milestones = SocialLegacyConfig.GetEarnedMilestones(
            VillageSocialStandingTier.WellLiked,
            focusCloseFriendCount: 3);

        Assert.Equal(3, milestones.Count);
        Assert.Contains(SocialMilestoneKind.KnownInBloomtown, milestones);
        Assert.Contains(SocialMilestoneKind.TrustedNeighbor, milestones);
        Assert.Contains(SocialMilestoneKind.VillagePillar, milestones);
    }

    [Fact]
    public void IsLegacyActive_OnlyAtWellLiked()
    {
        Assert.False(SocialLegacyConfig.IsLegacyActive(VillageSocialStandingTier.Respected));
        Assert.True(SocialLegacyConfig.IsLegacyActive(VillageSocialStandingTier.WellLiked));
    }

    [Fact]
    public void ApplyAmbientChanceBonus_OnlyBoostsWellLiked()
    {
        var respected = SocialLegacyConfig.ApplyAmbientChanceBonus(18, VillageSocialStandingTier.Respected);
        var wellLiked = SocialLegacyConfig.ApplyAmbientChanceBonus(42, VillageSocialStandingTier.WellLiked);

        Assert.Equal(18, respected);
        Assert.Equal(42 + SocialLegacyConfig.AmbientChanceBonusPercent, wellLiked);
    }

    [Fact]
    public void ApplyAmbientCooldownReduction_ShortensWellLikedCooldown()
    {
        var respected = SocialLegacyConfig.ApplyAmbientCooldownReduction(
            VillageSocialStandingConfig.WellLikedAmbientCooldownGameMinutes,
            VillageSocialStandingTier.Respected);
        var wellLiked = SocialLegacyConfig.ApplyAmbientCooldownReduction(
            VillageSocialStandingConfig.WellLikedAmbientCooldownGameMinutes,
            VillageSocialStandingTier.WellLiked);

        Assert.Equal(VillageSocialStandingConfig.WellLikedAmbientCooldownGameMinutes, respected);
        Assert.Equal(
            VillageSocialStandingConfig.WellLikedAmbientCooldownGameMinutes
                - SocialLegacyConfig.AmbientCooldownReductionGameMinutes,
            wellLiked);
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Rowan)]
    [InlineData(NpcEntityIds.Marcus)]
    [InlineData(NpcEntityIds.Eleanor)]
    public void TryGetLegacyJourneyLine_ReturnsPersonalLine(uint npcEntityId)
    {
        var line = SocialLegacyDialogue.TryGetLegacyJourneyLine(
            npcEntityId,
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("story", line, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Rowan)]
    [InlineData(NpcEntityIds.Marcus)]
    [InlineData(NpcEntityIds.Eleanor)]
    public void TryGetVillagePillarAcknowledgmentLine_ReturnsPillarLine(uint npcEntityId)
    {
        var line = SocialLegacyDialogue.TryGetVillagePillarAcknowledgmentLine(
            npcEntityId,
            villageNoticedMemory: true,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Pillar", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatMilestonesStatus_IncludesEarnedMilestones()
    {
        var status = SocialLegacyConfig.FormatMilestonesStatus(
            VillageSocialStandingTier.WellLiked,
            focusCloseFriendCount: 3);

        Assert.NotNull(status);
        Assert.Contains("Milestones earned:", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Known in Bloomtown", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Trusted Neighbor", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Village Pillar", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatLegacyEffectsStatus_OnlyAtWellLiked()
    {
        Assert.Null(SocialLegacyConfig.FormatLegacyEffectsStatus(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false));

        var status = SocialLegacyConfig.FormatLegacyEffectsStatus(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true,
            focusCloseFriendCount: 3);

        Assert.NotNull(status);
        Assert.Contains("Social Legacy", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Passive mood", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Village Pillar", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatContributionLegacyFeedback_ReturnsVariedLines()
    {
        var lineA = SocialLegacyConfig.FormatContributionLegacyFeedback(variationSeed: 0);
        var lineB = SocialLegacyConfig.FormatContributionLegacyFeedback(variationSeed: 1);

        Assert.False(string.IsNullOrWhiteSpace(lineA));
        Assert.False(string.IsNullOrWhiteSpace(lineB));
        Assert.NotEqual(lineA, lineB);
    }

    [Fact]
    public void GetPassiveMoodRecoveryPerGameMinute_OnlyAtWellLiked()
    {
        Assert.Equal(0f, SocialLegacyConfig.GetPassiveMoodRecoveryPerGameMinute(
            VillageSocialStandingTier.Respected));
        Assert.Equal(
            SocialLegacyConfig.PassiveMoodRecoveryPerGameMinute,
            SocialLegacyConfig.GetPassiveMoodRecoveryPerGameMinute(
                VillageSocialStandingTier.WellLiked));
    }
}