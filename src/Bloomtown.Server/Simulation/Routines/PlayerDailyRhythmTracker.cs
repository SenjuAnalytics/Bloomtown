using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Server.Simulation.Routines;

/// <summary>
/// Tracks daily activity patterns, passive rhythm bonuses, and phase agency choices.
/// </summary>
public sealed class PlayerDailyRhythmTracker
{
    private readonly Dictionary<uint, PlayerRhythmState> _states = new();

    public void ClearPlayer(uint playerEntityId) => _states.Remove(playerEntityId);

    public DailyRhythmConfig.DayRhythmSnapshot GetSnapshot(uint playerEntityId, GameTimeOfDay currentPhase, int gameDay)
    {
        var state = GetOrCreate(playerEntityId, gameDay, currentPhase);
        SyncPhase(state, currentPhase);

        return new DailyRhythmConfig.DayRhythmSnapshot(
            state.RecoveryCount,
            state.LeisureCount,
            state.SocialCount,
            state.MorningRecoveryCount,
            state.PhaseSocialCount,
            state.MorningIntent,
            state.AfternoonFocusedBreakUsed,
            state.FocusedBreakBonusPending,
            state.EveningPace,
            state.EveningWindDownUsed,
            state.LastActivity?.ActivityLabel,
            state.LastActivity?.PhaseAtActivity,
            state.LastActivity?.RhythmMoment,
            state.LastActivity is { } lastActivity ? DateTime.UtcNow - lastActivity.RecordedAtUtc : null);
    }

    public DailyRhythmConfig.RhythmActivityBonus GetActivityBonus(
        uint playerEntityId,
        DailyRhythmActivityCategory category,
        GameTimeOfDay currentPhase,
        int gameDay)
    {
        var snapshot = GetSnapshot(playerEntityId, currentPhase, gameDay);
        return DailyRhythmConfig.ComputeActivityBonus(category, currentPhase, snapshot);
    }

    public void Record(
        uint playerEntityId,
        DailyRhythmActivityCategory category,
        string activityLabel,
        GameTimeOfDay phaseAtActivity,
        int gameDay)
    {
        if (string.IsNullOrWhiteSpace(activityLabel))
            return;

        var state = GetOrCreate(playerEntityId, gameDay, phaseAtActivity);
        SyncPhase(state, phaseAtActivity);
        state.FocusedBreakBonusPending = false;

        switch (category)
        {
            case DailyRhythmActivityCategory.Recovery:
                state.RecoveryCount++;
                if (phaseAtActivity == GameTimeOfDay.Morning)
                    state.MorningRecoveryCount++;
                break;
            case DailyRhythmActivityCategory.Leisure:
                state.LeisureCount++;
                break;
            case DailyRhythmActivityCategory.Social:
                state.SocialCount++;
                state.PhaseSocialCount++;
                break;
        }

        var rhythmMoment = ActivityTimeOfDayConfig.GetRhythmMomentLabel(activityLabel, phaseAtActivity);
        state.LastActivity = new DailyRhythmEntry(
            activityLabel,
            phaseAtActivity,
            rhythmMoment,
            DateTime.UtcNow);
    }

    public bool TrySetMorningIntent(
        uint playerEntityId,
        MorningRhythmIntent intent,
        GameTimeOfDay currentPhase,
        int gameDay,
        out DailyRhythmFailureReason failureReason,
        out string errorMessage)
    {
        failureReason = DailyRhythmFailureReason.None;
        errorMessage = string.Empty;
        var state = GetOrCreate(playerEntityId, gameDay, currentPhase);
        SyncPhase(state, currentPhase);

        if (currentPhase != GameTimeOfDay.Morning)
        {
            failureReason = DailyRhythmFailureReason.WrongPhase;
            errorMessage = "Morning intent can only be set during the Morning phase.";
            return false;
        }

        if (state.MorningIntent != MorningRhythmIntent.None)
        {
            failureReason = DailyRhythmFailureReason.AlreadyUsed;
            errorMessage = $"You already chose a morning rhythm ({FormatMorningIntent(state.MorningIntent)}).";
            return false;
        }

        state.MorningIntent = intent;
        return true;
    }

    public bool TryUseFocusedBreak(
        uint playerEntityId,
        GameTimeOfDay currentPhase,
        int gameDay,
        out DailyRhythmFailureReason failureReason,
        out string errorMessage)
    {
        failureReason = DailyRhythmFailureReason.None;
        errorMessage = string.Empty;
        var state = GetOrCreate(playerEntityId, gameDay, currentPhase);
        SyncPhase(state, currentPhase);

        if (currentPhase != GameTimeOfDay.Afternoon)
        {
            failureReason = DailyRhythmFailureReason.WrongPhase;
            errorMessage = "Focused break is available during the Afternoon phase.";
            return false;
        }

        if (state.AfternoonFocusedBreakUsed)
        {
            failureReason = DailyRhythmFailureReason.AlreadyUsed;
            errorMessage = "You already took a focused break this afternoon.";
            return false;
        }

        state.AfternoonFocusedBreakUsed = true;
        state.FocusedBreakBonusPending = true;
        return true;
    }

    public bool TrySetEveningPace(
        uint playerEntityId,
        EveningPaceIntent pace,
        GameTimeOfDay currentPhase,
        int gameDay,
        out DailyRhythmFailureReason failureReason,
        out string errorMessage)
    {
        failureReason = DailyRhythmFailureReason.None;
        errorMessage = string.Empty;
        var state = GetOrCreate(playerEntityId, gameDay, currentPhase);
        SyncPhase(state, currentPhase);

        if (currentPhase is not GameTimeOfDay.Evening and not GameTimeOfDay.Night)
        {
            failureReason = DailyRhythmFailureReason.WrongPhase;
            errorMessage = "Evening pace can only be set during Evening or Night.";
            return false;
        }

        if (state.EveningPace != EveningPaceIntent.None)
        {
            failureReason = DailyRhythmFailureReason.AlreadyUsed;
            errorMessage = $"You already chose an evening pace ({FormatEveningPace(state.EveningPace)}).";
            return false;
        }

        state.EveningPace = pace;
        return true;
    }

    public bool TryUseEveningWindDown(
        uint playerEntityId,
        GameTimeOfDay currentPhase,
        int gameDay,
        out DailyRhythmFailureReason failureReason,
        out string errorMessage)
    {
        failureReason = DailyRhythmFailureReason.None;
        errorMessage = string.Empty;
        var state = GetOrCreate(playerEntityId, gameDay, currentPhase);
        SyncPhase(state, currentPhase);

        if (currentPhase is not GameTimeOfDay.Evening and not GameTimeOfDay.Night)
        {
            failureReason = DailyRhythmFailureReason.WrongPhase;
            errorMessage = "Rhythm wind down is available during Evening or Night.";
            return false;
        }

        if (state.EveningWindDownUsed)
        {
            failureReason = DailyRhythmFailureReason.AlreadyUsed;
            errorMessage = "You already wound down tonight.";
            return false;
        }

        state.EveningWindDownUsed = true;
        return true;
    }

    public string FormatStatus(uint playerEntityId, GameTimeOfDay currentPhase, int gameDay)
    {
        var snapshot = GetSnapshot(playerEntityId, currentPhase, gameDay);
        return DailyRhythmConfig.FormatFullStatus(currentPhase, snapshot);
    }

    public string FormatAgencyList(uint playerEntityId, GameTimeOfDay currentPhase, int gameDay)
    {
        var snapshot = GetSnapshot(playerEntityId, currentPhase, gameDay);
        return DailyRhythmConfig.FormatAgencyList(currentPhase, snapshot);
    }

    private PlayerRhythmState GetOrCreate(uint playerEntityId, int gameDay, GameTimeOfDay currentPhase)
    {
        if (!_states.TryGetValue(playerEntityId, out var state) || state.GameDay != gameDay)
        {
            state = new PlayerRhythmState
            {
                GameDay = gameDay,
                TrackedPhase = currentPhase,
            };
            _states[playerEntityId] = state;
        }

        return state;
    }

    private static void SyncPhase(PlayerRhythmState state, GameTimeOfDay currentPhase)
    {
        if (state.TrackedPhase == currentPhase)
            return;

        state.TrackedPhase = currentPhase;
        state.PhaseSocialCount = 0;
    }

    private static string FormatMorningIntent(MorningRhythmIntent intent) =>
        intent switch
        {
            MorningRhythmIntent.Calm => "calm start",
            MorningRhythmIntent.Active => "active start",
            _ => "none",
        };

    private static string FormatEveningPace(EveningPaceIntent pace) =>
        pace switch
        {
            EveningPaceIntent.RestEarly => "rest early",
            EveningPaceIntent.PushThrough => "push through",
            _ => "none",
        };

    private sealed class PlayerRhythmState
    {
        public int GameDay;
        public MorningRhythmIntent MorningIntent;
        public bool AfternoonFocusedBreakUsed;
        public bool FocusedBreakBonusPending;
        public EveningPaceIntent EveningPace;
        public bool EveningWindDownUsed;
        public int RecoveryCount;
        public int LeisureCount;
        public int SocialCount;
        public int MorningRecoveryCount;
        public int PhaseSocialCount;
        public GameTimeOfDay TrackedPhase;
        public DailyRhythmEntry? LastActivity;
    }

    private readonly record struct DailyRhythmEntry(
        string ActivityLabel,
        GameTimeOfDay PhaseAtActivity,
        string RhythmMoment,
        DateTime RecordedAtUtc);
}