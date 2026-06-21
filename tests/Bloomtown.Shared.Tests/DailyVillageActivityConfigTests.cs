using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class DailyVillageActivityConfigTests
{
    [Fact]
    public void All_ContainsFiveDailyActivities()
    {
        Assert.Equal(5, DailyVillageActivityConfig.All.Count);
        Assert.Contains(DailyVillageActivityConfig.All, a => a.Kind == DailyVillageActivityKind.SitOnBench);
        Assert.Contains(DailyVillageActivityConfig.All, a => a.Kind == DailyVillageActivityKind.WatchVillage);
        Assert.Contains(DailyVillageActivityConfig.All, a => a.Kind == DailyVillageActivityKind.ChatWithLocals);
        Assert.Contains(DailyVillageActivityConfig.All, a => a.Kind == DailyVillageActivityKind.TendPublicGarden);
        Assert.Contains(DailyVillageActivityConfig.All, a => a.Kind == DailyVillageActivityKind.PracticeWorkshop);
    }

    [Fact]
    public void SitOnBench_HasBalancedLightEffects()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.SitOnBench, out var bench));
        Assert.Equal("sit bench", bench.CommandHint);
        Assert.Equal(5f, bench.MoodGain);
        Assert.Equal(5f, bench.FatigueReduction);
        Assert.Equal(3f, bench.SocialReduction);
    }

    [Fact]
    public void WatchVillage_FavorsMoodAndSocialOverFatigue()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.WatchVillage, out var watch));
        Assert.Equal("watch village", watch.CommandHint);
        Assert.True(watch.MoodGain > watch.FatigueReduction);
        Assert.True(watch.SocialReduction > watch.FatigueReduction);
    }

    [Fact]
    public void ChatWithLocals_FavorsSocialRelief()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.ChatWithLocals, out var chat));
        Assert.Equal("chat locals", chat.CommandHint);
        Assert.Equal(DailyRhythmActivityCategory.Social, chat.RhythmCategory);
        Assert.True(chat.SocialReduction > chat.MoodGain);
    }

    [Fact]
    public void TendPublicGarden_IsCommunitySocialRitual()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.TendPublicGarden, out var garden));
        Assert.Equal("tend public garden", garden.CommandHint);
        Assert.Equal(DailyRhythmActivityCategory.Social, garden.RhythmCategory);
        Assert.True(garden.SocialReduction >= 4f);
    }

    [Fact]
    public void PracticeWorkshop_IsProductiveLeisure()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.PracticeWorkshop, out var workshop));
        Assert.Equal("practice workshop", workshop.CommandHint);
        Assert.Equal(DailyRhythmActivityCategory.Leisure, workshop.RhythmCategory);
        Assert.True(workshop.MoodGain >= 4f);
    }

    [Fact]
    public void ChatWithLocals_IsAvailableAtPrimaryOrSecondaryLocation()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.ChatWithLocals, out var chat));
        Assert.True(DailyVillageActivityConfig.IsAvailableAt(chat, chat.WorldX, chat.WorldZ));
        Assert.True(DailyVillageActivityConfig.IsAvailableAt(chat, chat.SecondaryWorldX!.Value, chat.SecondaryWorldZ!.Value));
        Assert.False(DailyVillageActivityConfig.IsAvailableAt(chat, 0f, 0f));
    }

    [Fact]
    public void IsAvailableAt_RequiresProximity()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.SitOnBench, out var bench));
        Assert.True(DailyVillageActivityConfig.IsAvailableAt(bench, bench.WorldX, bench.WorldZ));
        Assert.False(DailyVillageActivityConfig.IsAvailableAt(bench, 0f, 0f));
    }

    [Fact]
    public void PickFlavorText_UsesStandingLinesWhenRequested()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.WatchVillage, out var watch));
        var baseText = DailyVillageActivityConfig.PickFlavorText(watch, useStandingFlavor: false, variationSeed: 0);
        var standingText = DailyVillageActivityConfig.PickFlavorText(watch, useStandingFlavor: true, variationSeed: 0);

        Assert.Contains("outlook", baseText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Familiar", standingText, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(baseText, standingText);
    }

    [Fact]
    public void FormatActivityList_IncludesNewCommands()
    {
        var text = DailyVillageActivityConfig.FormatActivityList();
        Assert.Contains("sit bench", text);
        Assert.Contains("watch village", text);
        Assert.Contains("chat locals", text);
        Assert.Contains("tend public garden", text);
        Assert.Contains("practice workshop", text);
        Assert.Contains("social", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("productive", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatNearbyStatus_ListsAvailableCommands()
    {
        Assert.True(DailyVillageActivityConfig.TryGet(DailyVillageActivityKind.SitOnBench, out var bench));
        var nearby = DailyVillageActivityConfig.FormatNearbyStatus(bench.WorldX, bench.WorldZ);
        Assert.Contains("sit bench", nearby);
    }
}