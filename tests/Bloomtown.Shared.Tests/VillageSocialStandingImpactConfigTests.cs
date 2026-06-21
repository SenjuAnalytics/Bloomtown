using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialStandingImpactConfigTests
{
    [Theory]
    [InlineData(VillageSocialStandingTier.Known, false)]
    [InlineData(VillageSocialStandingTier.Stranger, false)]
    [InlineData(VillageSocialStandingTier.Respected, true)]
    [InlineData(VillageSocialStandingTier.WellLiked, true)]
    public void IsEligibleForFocusNpcBonus_RequiresRespectedOrHigher(
        VillageSocialStandingTier tier,
        bool expected)
    {
        Assert.Equal(expected, VillageSocialStandingImpactConfig.IsEligibleForFocusNpcBonus(tier));
    }

    [Fact]
    public void GetInteractionBonus_GretaReceivesExtraComfortAtRespected()
    {
        var greta = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.Respected,
            NpcEntityIds.Greta);
        var elsie = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.Respected,
            NpcEntityIds.Elsie);

        Assert.True(greta.MoodBonus > elsie.MoodBonus);
        Assert.True(greta.SocialBonus > elsie.SocialBonus);
        Assert.Equal(VillageSocialStandingImpactConfig.RespectedMoodBonus, elsie.MoodBonus);
    }

    [Theory]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Nora)]
    [InlineData(NpcEntityIds.Elias)]
    [InlineData(NpcEntityIds.Ben)]
    [InlineData(NpcEntityIds.Lila)]
    [InlineData(NpcEntityIds.Rowan)]
    public void GetInteractionBonus_RoleNpcsReceiveExtraComfortAtRespected(uint npcEntityId)
    {
        var roleBonus = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.Respected,
            npcEntityId);
        var baseBonus = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.Respected,
            NpcEntityIds.Mira);

        Assert.True(roleBonus.MoodBonus > baseBonus.MoodBonus);
        Assert.True(roleBonus.SocialBonus > baseBonus.SocialBonus);
    }

    [Fact]
    public void GetInfoChanceBonusPercent_RisesWithStanding()
    {
        Assert.Equal(
            VillageSocialStandingImpactConfig.RespectedInfoChanceBonusPercent,
            VillageSocialStandingImpactConfig.GetInfoChanceBonusPercent(VillageSocialStandingTier.Respected));
        Assert.True(
            VillageSocialStandingImpactConfig.GetInfoChanceBonusPercent(VillageSocialStandingTier.WellLiked)
            > VillageSocialStandingImpactConfig.GetInfoChanceBonusPercent(VillageSocialStandingTier.Respected));
    }

    [Theory]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Mira)]
    [InlineData(NpcEntityIds.Tom)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Nora)]
    public void TryGetFocusNpcStandingWarmthLine_ReturnsPersonalityLine(uint npcEntityId)
    {
        var line = VillageSocialStandingDialogue.TryGetFocusNpcStandingWarmthLine(
            npcEntityId,
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains(
            NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
            line,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatStandingImpactHint_ShowsTreatmentAtRespected()
    {
        var hint = VillageSocialStandingDialogue.FormatStandingImpactHint(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false);

        Assert.NotNull(hint);
        Assert.Contains("Village treatment:", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerStandingWarmth_UsesLowThreshold()
    {
        var roll = (5u * 131 + 9u * 67 + (uint)(100 % 917)) % 100;
        Assert.Equal(
            roll < VillageSocialStandingImpactConfig.RespectedStandingWarmthChancePercent,
            VillageSocialStandingImpactConfig.ShouldTriggerStandingWarmth(
                5,
                VillageSocialStandingTier.Respected,
                totalGameMinutes: 100,
                variationSeed: 9));
    }

    [Theory]
    [InlineData(VillageSocialStandingTier.Respected, false)]
    [InlineData(VillageSocialStandingTier.WellLiked, true)]
    public void IsEligibleForWellLikedPrivilege_RequiresWellLikedTier(
        VillageSocialStandingTier tier,
        bool expected)
    {
        Assert.Equal(expected, VillageSocialStandingImpactConfig.IsEligibleForWellLikedPrivilege(tier));
    }

    [Fact]
    public void GetInteractionBonus_WellLikedStrongerThanRespected()
    {
        var respected = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.Respected,
            NpcEntityIds.Elsie);
        var wellLiked = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.WellLiked,
            NpcEntityIds.Elsie);

        Assert.True(wellLiked.MoodBonus > respected.MoodBonus);
        Assert.True(wellLiked.SocialBonus > respected.SocialBonus);
    }

    [Theory]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Harold)]
    [InlineData(NpcEntityIds.Mira)]
    [InlineData(NpcEntityIds.Tom)]
    [InlineData(NpcEntityIds.Greta)]
    [InlineData(NpcEntityIds.Nora)]
    [InlineData(NpcEntityIds.Elias)]
    public void TryGetWellLikedPrivilegeLine_ReturnsPersonalityLine(uint npcEntityId)
    {
        var line = VillageSocialStandingDialogue.TryGetWellLikedPrivilegeLine(
            npcEntityId,
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains(
            NpcNameLookup.GetDisplayNameOrDefault(npcEntityId),
            line,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerVillageSocialStandingAwareness_RequiresRespectedTier()
    {
        Assert.False(VillageSocialStandingImpactConfig.ShouldTriggerVillageSocialStandingAwareness(
            5,
            NpcEntityIds.Elsie,
            VillageSocialStandingTier.Known,
            totalGameMinutes: 100,
            variationSeed: 3));

        var roll = (5u * 149 + NpcEntityIds.Elsie * 61 + 7u * 53 + (uint)(100 % 911)) % 100;
        Assert.Equal(
            roll < VillageSocialStandingImpactConfig.RespectedVillageAwarenessChancePercent,
            VillageSocialStandingImpactConfig.ShouldTriggerVillageSocialStandingAwareness(
                5,
                NpcEntityIds.Elsie,
                VillageSocialStandingTier.Respected,
                totalGameMinutes: 100,
                variationSeed: 7));
    }

    [Fact]
    public void ShouldTriggerWellLikedPrestigeRecognition_RequiresWellLikedTier()
    {
        Assert.False(VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrestigeRecognition(
            5,
            NpcEntityIds.Greta,
            VillageSocialStandingTier.Respected,
            totalGameMinutes: 200,
            variationSeed: 3));

        var roll = (5u * 101 + NpcEntityIds.Greta * 71 + 3u * 43 + (uint)(200 % 919)) % 100;
        Assert.Equal(
            roll < VillageSocialStandingImpactConfig.WellLikedPrestigeRecognitionChancePercent,
            VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrestigeRecognition(
                5,
                NpcEntityIds.Greta,
                VillageSocialStandingTier.WellLiked,
                totalGameMinutes: 200,
                variationSeed: 3));
    }

    [Fact]
    public void ShouldTriggerWellLikedPrivilege_UsesLowThreshold()
    {
        var roll = (5u * 97 + NpcEntityIds.Greta * 73 + 3u * 41 + (uint)(200 % 907)) % 100;
        Assert.Equal(
            roll < VillageSocialStandingImpactConfig.WellLikedPrivilegeChancePercent,
            VillageSocialStandingImpactConfig.ShouldTriggerWellLikedPrivilege(
                5,
                NpcEntityIds.Greta,
                VillageSocialStandingTier.WellLiked,
                totalGameMinutes: 200,
                variationSeed: 3));
    }
}