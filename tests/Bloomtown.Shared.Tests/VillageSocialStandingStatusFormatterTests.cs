using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialStandingStatusFormatterTests
{
    [Fact]
    public void FormatMilestonesAndLegacySection_HighlightsVillagePillarAtWellLiked()
    {
        var status = VillageSocialStandingStatusFormatter.FormatMilestonesAndLegacySection(
            VillageSocialStandingTier.WellLiked,
            focusCloseFriendCount: 3,
            villageNoticedMemory: true);

        Assert.NotNull(status);
        Assert.Contains("Milestones & Legacy", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("★", status, StringComparison.Ordinal);
        Assert.Contains("Village Pillar", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Legacy active", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatHowTheVillageSeesYouSection_DescribesRespectedPerception()
    {
        var status = VillageSocialStandingStatusFormatter.FormatHowTheVillageSeesYouSection(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: true);

        Assert.NotNull(status);
        Assert.Contains("How the Village Sees You", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bloomtown sees you", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Ordinary villagers", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatHowTheVillageSeesYouSection_IncludesPrestigeAtWellLiked()
    {
        var status = VillageSocialStandingStatusFormatter.FormatHowTheVillageSeesYouSection(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.NotNull(status);
        Assert.Contains("prestige", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Broader village recognition", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatWhatYouCanDoSection_DescribesRespectedActions()
    {
        var status = VillageSocialStandingStatusFormatter.FormatWhatYouCanDoSection(
            VillageSocialStandingTier.Respected);

        Assert.NotNull(status);
        Assert.Contains("What You Can Do Now", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("call-on", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Locked at Well-liked", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trade favors", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatCallOnAvailabilitySummary_GroupsReadyAndCooldown()
    {
        var summary = VillageSocialStandingStatusFormatter.FormatCallOnAvailabilitySummary(
            VillageSocialStandingTier.Respected,
            [
                (NpcEntityIds.Harold, true, 0, null),
                (NpcEntityIds.Greta, false, 42, null),
            ]);

        Assert.Contains("1 ready", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Harold", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1 on cooldown", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatTierPromotionFeedback_IsConcise()
    {
        var respected = VillageSocialStandingDialogue.FormatTierPromotionFeedback(
            VillageSocialStandingTier.Respected,
            villageNoticedMemory: false);
        var wellLiked = VillageSocialStandingDialogue.FormatTierPromotionFeedback(
            VillageSocialStandingTier.WellLiked,
            villageNoticedMemory: true);

        Assert.NotNull(respected);
        Assert.NotNull(wellLiked);
        Assert.True(respected!.Length < 220);
        Assert.True(wellLiked!.Length < 220);
        Assert.Contains("Unlocked:", respected, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("legacy", wellLiked, StringComparison.OrdinalIgnoreCase);
    }
}