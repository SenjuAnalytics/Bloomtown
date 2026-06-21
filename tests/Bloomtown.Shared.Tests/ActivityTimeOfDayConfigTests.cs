using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class ActivityTimeOfDayConfigTests
{
    [Fact]
    public void Nap_MorningGivesMoodBonus_AfternoonIsNeutral()
    {
        var morning = ActivityTimeOfDayConfig.GetHomeAdjustment(HomeActivityType.Nap, GameTimeOfDay.Morning);
        var afternoon = ActivityTimeOfDayConfig.GetHomeAdjustment(HomeActivityType.Nap, GameTimeOfDay.Afternoon);

        Assert.Equal(2f, morning.MoodBonus);
        Assert.Equal(0f, afternoon.MoodBonus);
    }

    [Fact]
    public void Relax_AfternoonSlightlyWeaker_EveningEasesFatigue()
    {
        var afternoon = ActivityTimeOfDayConfig.GetHomeAdjustment(HomeActivityType.Relax, GameTimeOfDay.Afternoon);
        var evening = ActivityTimeOfDayConfig.GetHomeAdjustment(HomeActivityType.Relax, GameTimeOfDay.Evening);

        Assert.Equal(-1f, afternoon.MoodBonus);
        Assert.Equal(1f, evening.FatigueReductionBonus);
    }

    [Fact]
    public void SitBench_NightAppliesSocialPenalty()
    {
        var night = ActivityTimeOfDayConfig.GetVillageAdjustment(
            DailyVillageActivityKind.SitOnBench,
            GameTimeOfDay.Night);

        Assert.Equal(1f, night.SocialReductionPenalty);
    }

    [Fact]
    public void WatchVillage_MorningGivesMoodBonus()
    {
        var morning = ActivityTimeOfDayConfig.GetVillageAdjustment(
            DailyVillageActivityKind.WatchVillage,
            GameTimeOfDay.Morning);

        Assert.Equal(2f, morning.MoodBonus);
    }

    [Fact]
    public void PickHomeTimedFlavor_MentionsTimeOfDay()
    {
        var flavor = ActivityTimeOfDayConfig.PickHomeTimedFlavor(
            HomeActivityType.Nap,
            GameTimeOfDay.Morning,
            variationSeed: 0);

        Assert.NotNull(flavor);
        Assert.Contains("Morning", flavor, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PickVillageTimedFlavor_MentionsAfternoonSun()
    {
        var flavor = ActivityTimeOfDayConfig.PickVillageTimedFlavor(
            DailyVillageActivityKind.SitOnBench,
            GameTimeOfDay.Afternoon,
            variationSeed: 0);

        Assert.NotNull(flavor);
        Assert.Contains("afternoon sun", flavor, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetRhythmMomentLabel_NapMorning_IsContextual()
    {
        var label = ActivityTimeOfDayConfig.GetRhythmMomentLabel("nap", GameTimeOfDay.Morning);
        Assert.Contains("refreshing", label, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatDailyRhythmStatus_IncludesSuggestionAndLastMoment()
    {
        var withHistory = ActivityTimeOfDayConfig.FormatDailyRhythmStatus(
            GameTimeOfDay.Evening,
            "nap",
            GameTimeOfDay.Morning,
            "Morning nap felt refreshing.",
            TimeSpan.FromMinutes(12));

        Assert.Contains("Daily rhythm (Evening)", withHistory);
        Assert.Contains("Morning nap felt refreshing", withHistory);
        Assert.Contains("12m ago", withHistory);
    }

    [Fact]
    public void FormatDailyRhythmStatus_WithoutHistory_StillSuggestsActivity()
    {
        var status = ActivityTimeOfDayConfig.FormatDailyRhythmStatus(
            GameTimeOfDay.Morning,
            lastActivityLabel: null,
            lastActivityPhase: null,
            lastRhythmMoment: null,
            elapsedSinceLast: null);

        Assert.Contains("Gentle start", status);
        Assert.Contains("Daily rhythm (Morning)", status);
    }

    [Fact]
    public void ChatWithLocals_AfternoonGivesMoodBonus()
    {
        var afternoon = ActivityTimeOfDayConfig.GetVillageAdjustment(
            DailyVillageActivityKind.ChatWithLocals,
            GameTimeOfDay.Afternoon);

        Assert.Equal(2f, afternoon.MoodBonus);
    }

    [Fact]
    public void PracticeWorkshop_MorningGivesMoodBonus()
    {
        var morning = ActivityTimeOfDayConfig.GetVillageAdjustment(
            DailyVillageActivityKind.PracticeWorkshop,
            GameTimeOfDay.Morning);

        Assert.Equal(2f, morning.MoodBonus);
    }

    [Theory]
    [InlineData(HomeActivityType.Nap)]
    [InlineData(HomeActivityType.Relax)]
    [InlineData(HomeActivityType.ReadBook)]
    [InlineData(HomeActivityType.EnjoyTea)]
    public void HomeAdjustments_StayWithinLightBonusRange(HomeActivityType type)
    {
        foreach (GameTimeOfDay phase in Enum.GetValues<GameTimeOfDay>())
        {
            var adjustment = ActivityTimeOfDayConfig.GetHomeAdjustment(type, phase);
            Assert.InRange(adjustment.MoodBonus, -1f, 3f);
            Assert.InRange(adjustment.FatigueReductionBonus, 0f, 3f);
        }
    }
}