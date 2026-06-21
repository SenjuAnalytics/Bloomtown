using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialStandingConfigTests
{
    [Theory]
    [InlineData(0, VillageSocialStandingTier.Stranger)]
    [InlineData(1, VillageSocialStandingTier.Known)]
    [InlineData(2, VillageSocialStandingTier.Respected)]
    [InlineData(3, VillageSocialStandingTier.WellLiked)]
    [InlineData(5, VillageSocialStandingTier.WellLiked)]
    public void ResolveTier_MapsCloseFriendCount(int closeFriends, VillageSocialStandingTier expected)
    {
        Assert.Equal(expected, VillageSocialStandingConfig.ResolveTier(closeFriends));
    }

    [Fact]
    public void GetAmbientChancePercent_RisesWithStandingAndVillageMemory()
    {
        var known = VillageSocialStandingConfig.GetAmbientChancePercent(
            VillageSocialStandingTier.Known,
            villageNoticedMemory: false);
        var respected = VillageSocialStandingConfig.GetAmbientChancePercent(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false);
        var wellLikedNoticed = VillageSocialStandingConfig.GetAmbientChancePercent(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.True(respected > known);
        Assert.True(wellLikedNoticed > respected);
        Assert.Equal(
            VillageSocialStandingConfig.WellLikedAmbientChancePercent
                + VillageSocialStandingConfig.VillageNoticedAmbientChanceBonusPercent
                + VillageSocialStandingConfig.WellLikedPrestigeAmbientBonusPercent
                + VillageSocialStandingConfig.WellLikedOrdinaryVillagerAmbientBonusPercent
                + SocialLegacyConfig.AmbientChanceBonusPercent,
            wellLikedNoticed);
    }

    [Fact]
    public void TryGetVillagerAmbientComment_ReturnsWarmerLineForMultipleCloseFriends()
    {
        var ids = new[] { NpcEntityIds.Elsie, NpcEntityIds.Greta, NpcEntityIds.Mira };
        var comment = VillageSocialStandingConfig.TryGetVillagerAmbientComment(
            VillageSocialStandingTier.WellLiked,
            ids,
            villageNoticedMemory: true,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.Contains("Bloomtown", comment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatStandingStatus_ShowsRespectedTone()
    {
        var line = VillageSocialStandingConfig.FormatStandingStatus(
            focusCloseFriendCount: 2,
            [NpcEntityIds.Elsie, NpcEntityIds.Harold],
            villageNoticedMemory: false);

        Assert.NotNull(line);
        Assert.Contains("Respected", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("one of their own", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAmbientCooldownGameMinutes_DropsAtHigherTiers()
    {
        var known = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(VillageSocialStandingTier.Known);
        var respected = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(VillageSocialStandingTier.WellLiked);

        Assert.True(respected < known);
        Assert.True(wellLiked < respected);
    }

    [Fact]
    public void FormatStandingImpactHint_RequiresRespectedTier()
    {
        Assert.Null(VillageSocialStandingConfig.FormatStandingImpactHint(
            VillageSocialStandingTier.Known,
            villageNoticedMemory: false));
        Assert.False(string.IsNullOrWhiteSpace(VillageSocialStandingConfig.FormatStandingImpactHint(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: true)));
    }

    [Fact]
    public void ShouldTriggerVillagerAmbientComment_UsesLowThreshold()
    {
        var roll = (5u * 113 + (uint)(100 % 929) + 3u * 13) % 100;
        Assert.Equal(
            roll < VillageSocialStandingConfig.GetAmbientChancePercent(
                VillageSocialStandingTier.Known,
                villageNoticedMemory: false),
            VillageSocialStandingConfig.ShouldTriggerVillagerAmbientComment(
                5,
                VillageSocialStandingTier.Known,
                villageNoticedMemory: false,
                totalGameMinutes: 100,
                attemptCounter: 3));
    }

    [Fact]
    public void IsEligibleForVillagerAmbientComment_RequiresKnownTierOrHigher()
    {
        Assert.False(VillageSocialStandingConfig.IsEligibleForVillagerAmbientComment(
            VillageSocialStandingTier.Stranger));
        Assert.True(VillageSocialStandingConfig.IsEligibleForVillagerAmbientComment(
            VillageSocialStandingTier.Known));
    }

    [Fact]
    public void FormatVillageAtmosphereHint_ShowsWarmerToneAtRespected()
    {
        Assert.Null(VillageSocialStandingConfig.FormatVillageAtmosphereHint(
            VillageSocialStandingTier.Known,
            villageNoticedMemory: false));
        Assert.False(string.IsNullOrWhiteSpace(VillageSocialStandingConfig.FormatVillageAtmosphereHint(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: true)));
    }

    [Fact]
    public void FormatNpcMechanicalBenefitsSummary_MentionsRoleNpcsAtRespected()
    {
        var summary = VillageSocialStandingConfig.FormatNpcMechanicalBenefitsSummary(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(summary);
        Assert.Contains("Harold", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Greta", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nora", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Elias", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatStandingImpactHint_MentionsRoleNpcsAtWellLiked()
    {
        var hint = VillageSocialStandingDialogue.FormatStandingImpactHint(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.NotNull(hint);
        Assert.Contains("Harold", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Greta", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatVillagerAmbientCommentForSpeaker_ExtractsQuotedSpeech()
    {
        var direct = VillageSocialStandingDialogue.FormatVillagerAmbientCommentForSpeaker(
            "A neighbor murmurs, \"Bloomtown trusts you here.\"");

        Assert.Equal("Bloomtown trusts you here.", direct);
    }

    [Fact]
    public void FormatPrestigeStatusHint_OnlyAtWellLiked()
    {
        Assert.Null(VillageSocialStandingConfig.FormatPrestigeStatusHint(
            VillageSocialStandingTier.Respected));
        Assert.False(string.IsNullOrWhiteSpace(VillageSocialStandingConfig.FormatPrestigeStatusHint(
            VillageSocialStandingTier.WellLiked)));
    }

    [Fact]
    public void FormatPrestigeStatusHint_MentionsHaroldAndGreta()
    {
        var hint = VillageSocialStandingConfig.FormatPrestigeStatusHint(
            VillageSocialStandingTier.WellLiked);

        Assert.NotNull(hint);
        Assert.Contains("Harold", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Greta", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("prestige", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatVillagerAmbientFeedback_UsesRespectfulToneAtWellLiked()
    {
        var feedback = VillageSocialStandingConfig.FormatVillagerAmbientFeedback(
            VillageSocialStandingTier.WellLiked);

        Assert.Contains("respect", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ordinary villager", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatVillagerAmbientFeedback_UsesRecognitionToneAtRespected()
    {
        var feedback = VillageSocialStandingConfig.FormatVillagerAmbientFeedback(
            VillageSocialStandingTier.Respected);

        Assert.Contains("recognize", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("standing", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatVillageAtmosphereHint_DescribesHowBloomtownSeesPlayer()
    {
        var respected = VillageSocialStandingConfig.FormatVillageAtmosphereHint(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false);
        var wellLiked = VillageSocialStandingConfig.FormatVillageAtmosphereHint(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.NotNull(respected);
        Assert.NotNull(wellLiked);
        Assert.Contains("How Bloomtown sees you", respected, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("honored", wellLiked, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatBroaderVillageRecognitionHint_OnlyAtWellLiked()
    {
        Assert.Null(VillageSocialStandingConfig.FormatBroaderVillageRecognitionHint(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: true));
        Assert.False(string.IsNullOrWhiteSpace(VillageSocialStandingConfig.FormatBroaderVillageRecognitionHint(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true)));
    }

    [Fact]
    public void FormatOrdinaryVillagerAmbientStatusHint_DescribesAmbientFrequencyAtRespectedAndWellLiked()
    {
        var respected = VillageSocialStandingConfig.FormatOrdinaryVillagerAmbientStatusHint(
            VillageSocialStandingTier.Respected);
        var wellLiked = VillageSocialStandingConfig.FormatOrdinaryVillagerAmbientStatusHint(
            VillageSocialStandingTier.WellLiked);

        Assert.NotNull(respected);
        Assert.NotNull(wellLiked);
        Assert.Contains("Ordinary villagers", respected, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("starting to recognize", respected, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ambient chance", wellLiked, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillagerAmbientComment_WellLikedMentionsBroaderRecognition()
    {
        var comment = VillageSocialStandingConfig.TryGetVillagerAmbientComment(
            VillageSocialStandingTier.WellLiked,
            [NpcEntityIds.Elsie, NpcEntityIds.Harold, NpcEntityIds.Greta],
            villageNoticedMemory: true,
            variationSeed: 12);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(
            comment.Contains("honored", StringComparison.OrdinalIgnoreCase)
                || comment.Contains("respected", StringComparison.OrdinalIgnoreCase)
                || comment.Contains("esteem", StringComparison.OrdinalIgnoreCase),
            comment);
    }

    [Fact]
    public void TryGetWellLikedPrestigeRecognitionLine_ReturnsForFocusNpcs()
    {
        var line = VillageSocialStandingDialogue.TryGetWellLikedPrestigeRecognitionLine(
            NpcEntityIds.Harold,
            villageNoticedMemory: true,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Harold", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetGretaWellLikedInnGuestInfoLine_ReturnsGuestGossip()
    {
        var line = VillageSocialStandingDialogue.TryGetGretaWellLikedInnGuestInfoLine(variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Greta", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetHaroldWellLikedProjectAcknowledgmentLine_ReturnsElderAcknowledgment()
    {
        var line = VillageSocialStandingDialogue.TryGetHaroldWellLikedProjectAcknowledgmentLine(variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Harold", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatTierPromotionFeedback_ReturnsRespectedMessage()
    {
        var feedback = VillageSocialStandingDialogue.FormatTierPromotionFeedback(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false);

        Assert.NotNull(feedback);
        Assert.Contains("Respected", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("call-on", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatTierPromotionFeedback_ReturnsWellLikedMessage()
    {
        var feedback = VillageSocialStandingDialogue.FormatTierPromotionFeedback(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.NotNull(feedback);
        Assert.Contains("Well-liked", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("legacy", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryFormatTierPromotionFeedback_OnlyOnTierIncrease()
    {
        Assert.Null(VillageSocialStandingConfig.TryFormatTierPromotionFeedback(
            VillageSocialStandingTier.Respected,
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false));
        Assert.Null(VillageSocialStandingConfig.TryFormatTierPromotionFeedback(
            VillageSocialStandingTier.WellLiked,
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false));
        Assert.NotNull(VillageSocialStandingConfig.TryFormatTierPromotionFeedback(
            VillageSocialStandingTier.Known,
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false));
    }

    [Fact]
    public void FormatCurrentTierActionsHint_DescribesRespectedCapabilities()
    {
        var hint = VillageSocialStandingConfig.FormatCurrentTierActionsHint(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(hint);
        Assert.Contains("call-on", hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Well-liked", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillageSocialStandingAwarenessLine_ReturnsVillageWideRecognition()
    {
        var line = VillageSocialStandingDialogue.TryGetVillageSocialStandingAwarenessLine(
            NpcEntityIds.Elsie,
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("village", line, StringComparison.OrdinalIgnoreCase);
    }
}