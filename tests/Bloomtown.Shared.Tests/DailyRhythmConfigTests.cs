using Bloomtown.Shared.Activities;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class DailyRhythmConfigTests
{
    [Fact]
    public void ComputeActivityBonus_MorningRecoveryBoostsAfternoonMood()
    {
        var snapshot = Snapshot(
            recoveryCount: 2,
            morningRecoveryCount: 2,
            lastActivityLabel: "nap",
            lastActivityPhase: GameTimeOfDay.Morning,
            lastRhythmMoment: "Morning nap felt refreshing.",
            elapsedSinceLast: TimeSpan.FromMinutes(30));

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Recovery,
            GameTimeOfDay.Afternoon,
            snapshot);

        Assert.Equal(DailyRhythmConfig.MorningRecoveryAfternoonMoodBonus, bonus.MoodBonus);
        Assert.Contains("morning", bonus.FeedbackNote!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComputeActivityBonus_MorningRecoveryLingersIntoEvening()
    {
        var snapshot = Snapshot(recoveryCount: 2, morningRecoveryCount: 2);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Leisure,
            GameTimeOfDay.Evening,
            snapshot);

        Assert.Equal(DailyRhythmConfig.MorningRecoveryEveningMoodBonus, bonus.MoodBonus);
    }

    [Fact]
    public void ComputeActivityBonus_SocialBurstReducesSocialRelief()
    {
        var snapshot = Snapshot(socialCount: 3, phaseSocialCount: 3);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Social,
            GameTimeOfDay.Afternoon,
            snapshot);

        Assert.Equal(-DailyRhythmConfig.SocialBurstPenalty, bonus.SocialReductionBonus);
    }

    [Fact]
    public void ComputeActivityBonus_CalmIntentBoostsCozyActivities()
    {
        var snapshot = Snapshot(
            recoveryCount: 1,
            morningRecoveryCount: 1,
            morningIntent: MorningRhythmIntent.Calm);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Recovery,
            GameTimeOfDay.Afternoon,
            snapshot);

        Assert.Equal(DailyRhythmConfig.CalmIntentCozyMoodBonus, bonus.MoodBonus);
    }

    [Fact]
    public void GetWindDownEffects_CozyDayFavorsFatigueRecovery()
    {
        var snapshot = Snapshot(
            recoveryCount: 2,
            leisureCount: 1,
            morningRecoveryCount: 2,
            morningIntent: MorningRhythmIntent.Calm,
            eveningWindDownUsed: true);

        var (mood, fatigue, social, message) = DailyRhythmConfig.GetWindDownEffects(snapshot);

        Assert.True(fatigue >= 4f);
        Assert.True(mood >= 3f);
        Assert.Contains("cozy", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComputeActivityBonus_EveningCozyActivityBoostsFatigueRecovery()
    {
        var snapshot = Snapshot(recoveryCount: 1, morningRecoveryCount: 1);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Leisure,
            GameTimeOfDay.Evening,
            snapshot);

        Assert.Equal(DailyRhythmConfig.EveningCozyFatigueBonus, bonus.FatigueReductionBonus);
        Assert.Contains("cozy", bonus.FeedbackNote!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComputeActivityBonus_ActiveIntentBoostsSocialRelief()
    {
        var snapshot = Snapshot(morningIntent: MorningRhythmIntent.Active);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Social,
            GameTimeOfDay.Afternoon,
            snapshot);

        Assert.Equal(DailyRhythmConfig.ActiveIntentSocialBonus, bonus.SocialReductionBonus);
    }

    [Fact]
    public void ComputeActivityBonus_FocusedBreakBoostsNextActivity()
    {
        var snapshot = Snapshot(focusedBreakBonusPending: true);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Recovery,
            GameTimeOfDay.Afternoon,
            snapshot);

        Assert.Equal(DailyRhythmConfig.FocusedBreakNextActivityMoodBonus, bonus.MoodBonus);
        Assert.Equal(DailyRhythmConfig.FocusedBreakNextActivityFatigueBonus, bonus.FatigueReductionBonus);
    }

    [Fact]
    public void ComputeActivityBonus_RestEarlyBoostsEveningCozyFatigueRelief()
    {
        var snapshot = Snapshot(eveningPace: EveningPaceIntent.RestEarly);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Recovery,
            GameTimeOfDay.Evening,
            snapshot);

        Assert.Equal(DailyRhythmConfig.RestEarlyEveningFatigueBonus, bonus.FatigueReductionBonus);
    }

    [Fact]
    public void ComputeActivityBonus_PushThroughBoostsEveningSocialHelp()
    {
        var snapshot = Snapshot(eveningPace: EveningPaceIntent.PushThrough);

        var bonus = DailyRhythmConfig.ComputeActivityBonus(
            DailyRhythmActivityCategory.Social,
            GameTimeOfDay.Evening,
            snapshot);

        Assert.Equal(DailyRhythmConfig.PushThroughEveningSocialMoodBonus, bonus.MoodBonus);
        Assert.Equal(DailyRhythmConfig.PushThroughEveningSocialBonus, bonus.SocialReductionBonus);
    }

    [Fact]
    public void GetDayToneLine_SocialDayDescribesConnectedRhythm()
    {
        var snapshot = Snapshot(socialCount: 3, phaseSocialCount: 2);

        var tone = DailyRhythmConfig.GetDayToneLine(snapshot, GameTimeOfDay.Afternoon);

        Assert.Contains("connected", tone!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetDayToneLine_FocusedBreakPendingMentionsNextActivityBoost()
    {
        var snapshot = Snapshot(
            recoveryCount: 1,
            focusedBreakBonusPending: true);

        var tone = DailyRhythmConfig.GetDayToneLine(snapshot, GameTimeOfDay.Afternoon);

        Assert.Contains("next activity", tone!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetWindDownEffects_SocialDayFavorsSocialRelief()
    {
        var snapshot = Snapshot(socialCount: 2, phaseSocialCount: 2);

        var (_, _, social, message) = DailyRhythmConfig.GetWindDownEffects(snapshot);

        Assert.Equal(4f, social);
        Assert.Contains("social", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatFullStatus_IncludesDaySummaryAndAgencyHint()
    {
        var snapshot = Snapshot(
            recoveryCount: 1,
            leisureCount: 1,
            socialCount: 2,
            morningRecoveryCount: 1,
            phaseSocialCount: 1,
            morningIntent: MorningRhythmIntent.Active,
            lastActivityLabel: "nap",
            lastActivityPhase: GameTimeOfDay.Morning,
            lastRhythmMoment: "Morning nap felt refreshing.",
            elapsedSinceLast: TimeSpan.FromMinutes(20));

        var status = DailyRhythmConfig.FormatFullStatus(GameTimeOfDay.Evening, snapshot);

        Assert.Contains("Today so far", status);
        Assert.Contains("Day tone:", status);
        Assert.Contains("Active start", status);
        Assert.Contains("rest early", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Morning nap felt refreshing", status);
    }

    [Fact]
    public void FormatAgencyList_IncludesNewAgencyCommands()
    {
        var snapshot = Snapshot();

        var list = DailyRhythmConfig.FormatAgencyList(GameTimeOfDay.Afternoon, snapshot);

        Assert.Contains("focused break", list, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rest early", list, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("push through", list, StringComparison.OrdinalIgnoreCase);
    }

    private static DailyRhythmConfig.DayRhythmSnapshot Snapshot(
        int recoveryCount = 0,
        int leisureCount = 0,
        int socialCount = 0,
        int morningRecoveryCount = 0,
        int phaseSocialCount = 0,
        MorningRhythmIntent morningIntent = MorningRhythmIntent.None,
        bool afternoonFocusedBreakUsed = false,
        bool focusedBreakBonusPending = false,
        EveningPaceIntent eveningPace = EveningPaceIntent.None,
        bool eveningWindDownUsed = false,
        string? lastActivityLabel = null,
        GameTimeOfDay? lastActivityPhase = null,
        string? lastRhythmMoment = null,
        TimeSpan? elapsedSinceLast = null) =>
        new(
            recoveryCount,
            leisureCount,
            socialCount,
            morningRecoveryCount,
            phaseSocialCount,
            morningIntent,
            afternoonFocusedBreakUsed,
            focusedBreakBonusPending,
            eveningPace,
            eveningWindDownUsed,
            lastActivityLabel,
            lastActivityPhase,
            lastRhythmMoment,
            elapsedSinceLast);
}