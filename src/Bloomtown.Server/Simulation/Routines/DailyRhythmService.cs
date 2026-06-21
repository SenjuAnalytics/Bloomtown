using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Routines;

/// <summary>
/// Handles optional daily-rhythm agency choices and applies their light need effects.
/// </summary>
public sealed class DailyRhythmService
{
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly PlayerDailyRhythmTracker _rhythmTracker;
    private readonly WorldTimeSystem _worldTime;
    private readonly PlayerMilestoneService? _milestoneService;

    public DailyRhythmService(
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        PlayerDailyRhythmTracker rhythmTracker,
        WorldTimeSystem worldTime,
        PlayerMilestoneService? milestoneService = null)
    {
        _economyService = economyService;
        _needsService = needsService;
        _rhythmTracker = rhythmTracker;
        _worldTime = worldTime;
        _milestoneService = milestoneService;
    }

    public DailyRhythmResponse Handle(uint playerEntityId, DailyRhythmRequest request)
    {
        return request.Kind switch
        {
            DailyRhythmRequestKind.List => HandleList(playerEntityId),
            DailyRhythmRequestKind.StartCalm => HandleStartCalm(playerEntityId),
            DailyRhythmRequestKind.StartActive => HandleStartActive(playerEntityId),
            DailyRhythmRequestKind.WindDown => HandleWindDown(playerEntityId),
            DailyRhythmRequestKind.FocusedBreak => HandleFocusedBreak(playerEntityId),
            DailyRhythmRequestKind.RestEarly => HandleRestEarly(playerEntityId),
            DailyRhythmRequestKind.PushThrough => HandlePushThrough(playerEntityId),
            _ => Fail(
                DailyRhythmRequestKind.List,
                DailyRhythmFailureReason.UnknownRequest,
                "Unknown daily rhythm request."),
        };
    }

    public string FormatStatus(uint playerEntityId) =>
        _rhythmTracker.FormatStatus(playerEntityId, CurrentPhase, _worldTime.GameDay);

    private DailyRhythmResponse HandleList(uint playerEntityId)
    {
        Log.Information("Player {PlayerId} viewed daily rhythm agency.", playerEntityId);
        return new DailyRhythmResponse(
            true,
            DailyRhythmRequestKind.List,
            DailyRhythmFailureReason.None,
            _rhythmTracker.FormatAgencyList(playerEntityId, CurrentPhase, _worldTime.GameDay));
    }

    private DailyRhythmResponse HandleStartCalm(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.StartCalm,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TrySetMorningIntent(
                playerEntityId,
                MorningRhythmIntent.Calm,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.StartCalm, failureReason, error);
        }

        var moodBefore = economy.Mood;
        _needsService.ApplyDailyRhythmBonus(
            economy,
            DailyRhythmConfig.StartCalmImmediateMood,
            fatigueReduction: 0f,
            socialReduction: 0f);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var message =
            "You choose to start the day calmly." +
            $"{Environment.NewLine}Now: Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0}/{PlayerNeedsConfig.MaxValue:F0})." +
            $"{Environment.NewLine}Through the day: cozy and leisure activities gain +{DailyRhythmConfig.CalmIntentCozyMoodBonus:F0} mood.";

        Log.Information("Player {PlayerId} chose calm morning rhythm.", playerEntityId);
        return Success(DailyRhythmRequestKind.StartCalm, message, playerEntityId);
    }

    private DailyRhythmResponse HandleStartActive(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.StartActive,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TrySetMorningIntent(
                playerEntityId,
                MorningRhythmIntent.Active,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.StartActive, failureReason, error);
        }

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyDailyRhythmBonus(
            economy,
            DailyRhythmConfig.StartActiveImmediateMood,
            DailyRhythmConfig.StartActiveImmediateFatigueReduction,
            socialReduction: 0f);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var message =
            "You choose to start the day actively." +
            $"{Environment.NewLine}Now: Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0}/{PlayerNeedsConfig.MaxValue:F0}), " +
            $"Fatigue -{fatigueBefore - economy.Fatigue:F0} (now {economy.Fatigue:F0}/{PlayerNeedsConfig.MaxValue:F0})." +
            $"{Environment.NewLine}Through the day: social help gains +{DailyRhythmConfig.ActiveIntentSocialBonus:F0} relief.";

        Log.Information("Player {PlayerId} chose active morning rhythm.", playerEntityId);
        return Success(DailyRhythmRequestKind.StartActive, message, playerEntityId);
    }

    private DailyRhythmResponse HandleFocusedBreak(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.FocusedBreak,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TryUseFocusedBreak(
                playerEntityId,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.FocusedBreak, failureReason, error);
        }

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyDailyRhythmBonus(
            economy,
            DailyRhythmConfig.FocusedBreakImmediateMood,
            DailyRhythmConfig.FocusedBreakImmediateFatigueReduction,
            socialReduction: 0f);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var message =
            "You take a focused break — stepping back to reset the afternoon." +
            $"{Environment.NewLine}Now: Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0}), " +
            $"Fatigue -{fatigueBefore - economy.Fatigue:F0} (now {economy.Fatigue:F0})." +
            $"{Environment.NewLine}Next activity: +{DailyRhythmConfig.FocusedBreakNextActivityMoodBonus:F0} mood" +
            $" (+{DailyRhythmConfig.FocusedBreakNextActivityFatigueBonus:F0} fatigue relief if cozy/leisure).";

        Log.Information("Player {PlayerId} chose afternoon focused break.", playerEntityId);
        return Success(DailyRhythmRequestKind.FocusedBreak, message, playerEntityId);
    }

    private DailyRhythmResponse HandleRestEarly(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.RestEarly,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TrySetEveningPace(
                playerEntityId,
                EveningPaceIntent.RestEarly,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.RestEarly, failureReason, error);
        }

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyDailyRhythmBonus(
            economy,
            moodGain: 0f,
            DailyRhythmConfig.RestEarlyImmediateFatigueReduction,
            socialReduction: 0f);
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood - DailyRhythmConfig.RestEarlyImmediateMoodCost);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var message =
            "You choose to rest early — trading a little brightness for deeper recovery tonight." +
            $"{Environment.NewLine}Now: Fatigue -{fatigueBefore - economy.Fatigue:F0} (now {economy.Fatigue:F0}), " +
            $"Mood -{moodBefore - economy.Mood:F0} (now {economy.Mood:F0})." +
            $"{Environment.NewLine}Tonight: cozy and leisure activities gain +{DailyRhythmConfig.RestEarlyEveningFatigueBonus:F0} fatigue relief.";

        Log.Information("Player {PlayerId} chose rest early evening pace.", playerEntityId);
        return Success(DailyRhythmRequestKind.RestEarly, message, playerEntityId);
    }

    private DailyRhythmResponse HandlePushThrough(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.PushThrough,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TrySetEveningPace(
                playerEntityId,
                EveningPaceIntent.PushThrough,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.PushThrough, failureReason, error);
        }

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyDailyRhythmBonus(
            economy,
            DailyRhythmConfig.PushThroughImmediateMood,
            fatigueReduction: 0f,
            socialReduction: 0f);
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue + DailyRhythmConfig.PushThroughImmediateFatigueCost);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var message =
            "You choose to push through — riding the day's momentum a little longer." +
            $"{Environment.NewLine}Now: Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0}), " +
            $"Fatigue +{economy.Fatigue - fatigueBefore:F0} (now {economy.Fatigue:F0})." +
            $"{Environment.NewLine}Tonight: social help gains +{DailyRhythmConfig.PushThroughEveningSocialMoodBonus:F0} mood" +
            $" and +{DailyRhythmConfig.PushThroughEveningSocialBonus:F0} relief.";

        Log.Information("Player {PlayerId} chose push through evening pace.", playerEntityId);
        return Success(DailyRhythmRequestKind.PushThrough, message, playerEntityId);
    }

    private DailyRhythmResponse HandleWindDown(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyRhythmRequestKind.WindDown,
                DailyRhythmFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!_rhythmTracker.TryUseEveningWindDown(
                playerEntityId,
                CurrentPhase,
                _worldTime.GameDay,
                out var failureReason,
                out var error))
        {
            return Fail(DailyRhythmRequestKind.WindDown, failureReason, error);
        }

        var snapshot = _rhythmTracker.GetSnapshot(playerEntityId, CurrentPhase, _worldTime.GameDay);
        var (moodGain, fatigueReduction, socialReduction, flavor) =
            DailyRhythmConfig.GetWindDownEffects(snapshot);

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        var socialBefore = economy.SocialNeed;
        _needsService.ApplyDailyRhythmBonus(economy, moodGain, fatigueReduction, socialReduction);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var parts = new List<string> { flavor };
        if (economy.Mood > moodBefore)
            parts.Add($"Mood +{economy.Mood - moodBefore:F0} (now {economy.Mood:F0})");
        if (economy.Fatigue < fatigueBefore)
            parts.Add($"Fatigue -{fatigueBefore - economy.Fatigue:F0} (now {economy.Fatigue:F0})");
        if (economy.SocialNeed < socialBefore)
            parts.Add($"Social -{socialBefore - economy.SocialNeed:F0} (now {economy.SocialNeed:F0})");

        Log.Information("Player {PlayerId} used evening rhythm wind down.", playerEntityId);
        return Success(
            DailyRhythmRequestKind.WindDown,
            string.Join(Environment.NewLine, parts),
            playerEntityId);
    }

    private DailyRhythmResponse Success(
        DailyRhythmRequestKind kind,
        string message,
        uint playerEntityId)
    {
        _milestoneService?.RecordRhythmAgencyDay(playerEntityId, _worldTime.GameDay);
        _milestoneService?.ReconcileAsync(playerEntityId).GetAwaiter().GetResult();

        if (_milestoneService?.TryConsumePendingFeedback(playerEntityId, out var feedback) == true
            && !string.IsNullOrWhiteSpace(feedback))
        {
            message = $"{message}{Environment.NewLine}{feedback}";
        }

        return new DailyRhythmResponse(true, kind, DailyRhythmFailureReason.None, message);
    }

    private GameTimeOfDay CurrentPhase => GameTimeHelper.GetTimeOfDay(_worldTime.GameHour);

    private static DailyRhythmResponse Fail(
        DailyRhythmRequestKind kind,
        DailyRhythmFailureReason reason,
        string message)
    {
        Log.Information("Daily rhythm request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new DailyRhythmResponse(false, kind, reason, message);
    }
}