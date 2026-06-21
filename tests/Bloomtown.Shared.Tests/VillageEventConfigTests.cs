using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageEventConfigTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(6, true)]
    [InlineData(3, false)]
    [InlineData(10, true)]
    public void IsMarketDay_FollowsFourDayInterval(int gameDay, bool expected)
    {
        Assert.Equal(expected, VillageEventConfig.IsMarketDay(gameDay));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, true)]
    [InlineData(9, true)]
    [InlineData(5, false)]
    [InlineData(14, true)]
    public void IsCommunityWorkDay_FollowsFiveDayInterval(int gameDay, bool expected)
    {
        Assert.Equal(expected, VillageEventConfig.IsCommunityWorkDay(gameDay));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(7, true)]
    [InlineData(14, true)]
    [InlineData(21, true)]
    [InlineData(8, false)]
    public void IsRainyDay_FollowsSevenDayInterval(int gameDay, bool expected)
    {
        Assert.Equal(expected, VillageEventConfig.IsRainyDay(gameDay));
    }

    [Fact]
    public void GetActiveEvents_ReturnsAllThreeOnOverlapDay()
    {
        var events = VillageEventConfig.GetActiveEvents(gameDay: 14);

        Assert.Equal(3, events.Count);
        Assert.Contains(VillageEventKind.MarketDay, events);
        Assert.Contains(VillageEventKind.CommunityWorkDay, events);
        Assert.Contains(VillageEventKind.RainyDay, events);
    }

    [Fact]
    public void FormatActiveEventsStatus_ShowsMarketDayDetails()
    {
        var status = VillageEventConfig.FormatActiveEventsStatus(gameDay: 2);

        Assert.NotNull(status);
        Assert.Contains("Market Day", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Village Event", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Mira", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatActiveEventsStatus_ShowsCommunityWorkDayDetails()
    {
        var status = VillageEventConfig.FormatActiveEventsStatus(gameDay: 4);

        Assert.NotNull(status);
        Assert.Contains("Community Work Day", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("help", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatActiveEventsStatus_ShowsRainyDayDetails()
    {
        var status = VillageEventConfig.FormatActiveEventsStatus(gameDay: 7);

        Assert.NotNull(status);
        Assert.Contains("Rainy Day", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Village Event", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rain", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyMarketDayBuyPrice_ReducesPriceSlightly()
    {
        var adjusted = VillageEventConfig.ApplyMarketDayBuyPrice(unitPrice: 100);

        Assert.Equal(98, adjusted);
    }

    [Fact]
    public void ApplyMarketDaySellPrice_IncreasesPriceSlightly()
    {
        var adjusted = VillageEventConfig.ApplyMarketDaySellPrice(unitPrice: 100);

        Assert.Equal(102, adjusted);
    }

    [Fact]
    public void IsHelpActivity_IncludesCommunityHelpKinds()
    {
        Assert.True(VillageEventConfig.IsHelpActivity(CommunityActivityKind.HelpGarden));
        Assert.True(VillageEventConfig.IsHelpActivity(CommunityActivityKind.HelpWorkshop));
        Assert.False(VillageEventConfig.IsHelpActivity(CommunityActivityKind.None));
    }

    [Fact]
    public void IsOutdoorCommunityActivity_ClassifiesOutdoorHelp()
    {
        Assert.True(VillageEventConfig.IsOutdoorCommunityActivity(CommunityActivityKind.HelpGarden));
        Assert.True(VillageEventConfig.IsOutdoorCommunityActivity(CommunityActivityKind.HelpPatrol));
        Assert.False(VillageEventConfig.IsOutdoorCommunityActivity(CommunityActivityKind.ChatWithEleanor));
    }

    [Fact]
    public void IsIndoorCalmCommunityActivity_ClassifiesIndoorHelp()
    {
        Assert.True(VillageEventConfig.IsIndoorCalmCommunityActivity(CommunityActivityKind.ChatWithEleanor));
        Assert.True(VillageEventConfig.IsIndoorCalmCommunityActivity(CommunityActivityKind.ListenToStories));
        Assert.True(VillageEventConfig.IsIndoorCalmCommunityActivity(CommunityActivityKind.HelpInn));
        Assert.False(VillageEventConfig.IsIndoorCalmCommunityActivity(CommunityActivityKind.HelpGarden));
    }

    [Fact]
    public void GetRainyDayCommunityActivityAdjustments_PenalizesOutdoorAndBonusesIndoor()
    {
        var outdoor = VillageEventConfig.GetRainyDayCommunityActivityAdjustments(
            CommunityActivityKind.HelpGarden);
        var indoor = VillageEventConfig.GetRainyDayCommunityActivityAdjustments(
            CommunityActivityKind.ChatWithEleanor);

        Assert.Equal(-VillageEventConfig.RainyDayOutdoorMoodPenalty, outdoor.MoodAdjust);
        Assert.Equal(-VillageEventConfig.RainyDayOutdoorSocialPenalty, outdoor.SocialAdjust);
        Assert.Equal(VillageEventConfig.RainyDayIndoorMoodBonus, indoor.MoodAdjust);
        Assert.Equal(VillageEventConfig.RainyDayIndoorSocialBonus, indoor.SocialAdjust);
    }

    [Fact]
    public void TryGetNpcEventLine_ReturnsMiraMarketDayDialogue()
    {
        var line = VillageEventDialogue.TryGetNpcEventLine(
            NpcEntityIds.Mira,
            VillageEventKind.MarketDay,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Mira", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetNpcEventLine_ReturnsGretaRainyDayDialogue()
    {
        var line = VillageEventDialogue.TryGetNpcEventLine(
            NpcEntityIds.Greta,
            VillageEventKind.RainyDay,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Rain", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillagerAmbientComment_ReturnsCommunityWorkDayLine()
    {
        var line = VillageEventDialogue.TryGetVillagerAmbientComment(
            VillageEventKind.CommunityWorkDay,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Work Day", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillagerAmbientComment_ReturnsRainyDayLine()
    {
        var line = VillageEventDialogue.TryGetVillagerAmbientComment(
            VillageEventKind.RainyDay,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("Rain", line, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(6, true)]
    [InlineData(7, true)]
    public void HasActiveEvent_OnScheduledDays(int gameDay, bool expected)
    {
        Assert.Equal(expected, VillageEventConfig.HasActiveEvent(gameDay));
    }
}