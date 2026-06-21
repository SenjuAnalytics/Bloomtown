using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Activities;

/// <summary>
/// Light daily-rhythm rules: passive bonuses from the day's pattern and optional phase agency.
/// </summary>
public static class DailyRhythmConfig
{
    public const int MorningRecoveryAfternoonMoodThreshold = 2;
    public const float MorningRecoveryAfternoonMoodBonus = 3f;
    public const float MorningRecoveryEveningMoodBonus = 2f;
    public const int SocialBurstThreshold = 3;
    public const float SocialBurstPenalty = 3f;
    public const float EveningCozyFatigueBonus = 3f;
    public const float CalmIntentCozyMoodBonus = 2f;
    public const float ActiveIntentSocialBonus = 2f;
    public const float StartCalmImmediateMood = 2f;
    public const float StartActiveImmediateMood = 2f;
    public const float StartActiveImmediateFatigueReduction = 3f;
    public const float FocusedBreakImmediateMood = 1f;
    public const float FocusedBreakImmediateFatigueReduction = 2f;
    public const float FocusedBreakNextActivityMoodBonus = 3f;
    public const float FocusedBreakNextActivityFatigueBonus = 2f;
    public const float RestEarlyImmediateFatigueReduction = 3f;
    public const float RestEarlyImmediateMoodCost = 1f;
    public const float RestEarlyEveningFatigueBonus = 3f;
    public const float PushThroughImmediateMood = 2f;
    public const float PushThroughImmediateFatigueCost = 2f;
    public const float PushThroughEveningSocialMoodBonus = 2f;
    public const float PushThroughEveningSocialBonus = 2f;

    public readonly record struct RhythmActivityBonus(
        float MoodBonus,
        float FatigueReductionBonus,
        float SocialReductionBonus,
        string? FeedbackNote);

    public readonly record struct DayRhythmSnapshot(
        int RecoveryCount,
        int LeisureCount,
        int SocialCount,
        int MorningRecoveryCount,
        int PhaseSocialCount,
        MorningRhythmIntent MorningIntent,
        bool AfternoonFocusedBreakUsed,
        bool FocusedBreakBonusPending,
        EveningPaceIntent EveningPace,
        bool EveningWindDownUsed,
        string? LastActivityLabel,
        GameTimeOfDay? LastActivityPhase,
        string? LastRhythmMoment,
        TimeSpan? ElapsedSinceLast);

    public static RhythmActivityBonus ComputeActivityBonus(
        DailyRhythmActivityCategory category,
        GameTimeOfDay currentPhase,
        in DayRhythmSnapshot snapshot)
    {
        var moodBonus = 0f;
        var fatigueBonus = 0f;
        var socialBonus = 0f;
        string? note = null;

        if (snapshot.MorningRecoveryCount >= MorningRecoveryAfternoonMoodThreshold)
        {
            if (currentPhase == GameTimeOfDay.Afternoon)
            {
                moodBonus += MorningRecoveryAfternoonMoodBonus;
                note = "Your gentle morning still warms the afternoon.";
            }
            else if (currentPhase == GameTimeOfDay.Evening)
            {
                moodBonus += MorningRecoveryEveningMoodBonus;
                note = "Your refreshing morning still lingers into the evening.";
            }
        }

        if (snapshot.PhaseSocialCount >= SocialBurstThreshold && category == DailyRhythmActivityCategory.Social)
        {
            socialBonus -= SocialBurstPenalty;
            note = CombineNote(note, "You've been quite social — conversation feels noticeably draining now.");
        }

        if (category is DailyRhythmActivityCategory.Recovery or DailyRhythmActivityCategory.Leisure
            && currentPhase is GameTimeOfDay.Evening or GameTimeOfDay.Night
            && snapshot.RecoveryCount + snapshot.LeisureCount >= 1)
        {
            fatigueBonus += EveningCozyFatigueBonus;
            note = CombineNote(note, "A cozy day eases your body toward rest.");
        }

        if (snapshot.MorningIntent == MorningRhythmIntent.Calm
            && category is DailyRhythmActivityCategory.Recovery or DailyRhythmActivityCategory.Leisure)
        {
            moodBonus += CalmIntentCozyMoodBonus;
            note = CombineNote(note, "Your calm start keeps cozy moments sweeter.");
        }

        if (snapshot.MorningIntent == MorningRhythmIntent.Active
            && category == DailyRhythmActivityCategory.Social)
        {
            socialBonus += ActiveIntentSocialBonus;
            note = CombineNote(note, "Your active start helps village chatter land more easily.");
        }

        if (snapshot.FocusedBreakBonusPending)
        {
            moodBonus += FocusedBreakNextActivityMoodBonus;
            if (category is DailyRhythmActivityCategory.Recovery or DailyRhythmActivityCategory.Leisure)
                fatigueBonus += FocusedBreakNextActivityFatigueBonus;

            note = CombineNote(note, "Your focused break sharpens what comes next.");
        }

        if (currentPhase is GameTimeOfDay.Evening or GameTimeOfDay.Night)
        {
            if (snapshot.EveningPace == EveningPaceIntent.RestEarly
                && category is DailyRhythmActivityCategory.Recovery or DailyRhythmActivityCategory.Leisure)
            {
                fatigueBonus += RestEarlyEveningFatigueBonus;
                note = CombineNote(note, "Resting early lets cozy moments restore you more deeply.");
            }

            if (snapshot.EveningPace == EveningPaceIntent.PushThrough
                && category == DailyRhythmActivityCategory.Social)
            {
                moodBonus += PushThroughEveningSocialMoodBonus;
                socialBonus += PushThroughEveningSocialBonus;
                note = CombineNote(note, "Pushing through lends extra warmth to evening connection.");
            }
        }

        if (note is null)
            return new RhythmActivityBonus(moodBonus, fatigueBonus, socialBonus, null);

        return new RhythmActivityBonus(moodBonus, fatigueBonus, socialBonus, $" [Daily rhythm — {note}]");
    }

    public static (float Mood, float FatigueReduction, float SocialReduction, string Message) GetWindDownEffects(
        in DayRhythmSnapshot snapshot)
    {
        var mood = 2f;
        var fatigue = 2f;
        var social = 0f;
        string message;

        if (snapshot.RecoveryCount + snapshot.LeisureCount > snapshot.SocialCount)
        {
            fatigue += 3f;
            mood += 2f;
            message = "You settle into the evening — today's cozy pauses leave you noticeably lighter.";
        }
        else if (snapshot.SocialCount >= 2)
        {
            social = 4f;
            message = "You wind down after a social day — quiet feels welcome, and connection still warms you.";
        }
        else
        {
            message = "You wind down gently — the day closes with a small, steadying breath.";
        }

        if (snapshot.MorningIntent == MorningRhythmIntent.Calm)
        {
            mood += 1f;
            message += " Your calm start made this moment especially peaceful.";
        }
        else if (snapshot.MorningIntent == MorningRhythmIntent.Active)
        {
            fatigue += 1f;
            message += " Your active start finally loosens its grip.";
        }

        if (snapshot.EveningPace == EveningPaceIntent.RestEarly)
        {
            fatigue += 1f;
            message += " Resting early deepens the relief.";
        }
        else if (snapshot.EveningPace == EveningPaceIntent.PushThrough)
        {
            mood += 1f;
            message += " The day's momentum eases into quiet satisfaction.";
        }

        return (mood, fatigue, social, message);
    }

    public static string FormatDaySummary(in DayRhythmSnapshot snapshot, GameTimeOfDay currentPhase)
    {
        var parts = new List<string>();
        if (snapshot.RecoveryCount > 0)
            parts.Add($"{snapshot.RecoveryCount} cozy moment{(snapshot.RecoveryCount == 1 ? string.Empty : "s")}");
        if (snapshot.LeisureCount > 0)
            parts.Add($"{snapshot.LeisureCount} leisure pause{(snapshot.LeisureCount == 1 ? string.Empty : "s")}");
        if (snapshot.SocialCount > 0)
            parts.Add($"{snapshot.SocialCount} social help{(snapshot.SocialCount == 1 ? string.Empty : "s")}");

        var dayLine = parts.Count == 0
            ? "Your day is still open — small rituals will shape it."
            : $"Today so far — {string.Join(", ", parts)}.";

        var toneLine = GetDayToneLine(snapshot, currentPhase);
        var intentLine = GetMorningIntentLine(snapshot, currentPhase);
        var agencyChoiceLine = GetAgencyChoiceLine(snapshot);
        var passiveLine = GetPassiveLiftLine(snapshot, currentPhase);
        var agencyLine = GetAgencyHint(snapshot, currentPhase);

        var lines = new List<string> { dayLine };
        if (!string.IsNullOrWhiteSpace(toneLine))
            lines.Add(toneLine);
        if (!string.IsNullOrWhiteSpace(intentLine))
            lines.Add(intentLine);
        if (!string.IsNullOrWhiteSpace(agencyChoiceLine))
            lines.Add(agencyChoiceLine);
        if (!string.IsNullOrWhiteSpace(passiveLine))
            lines.Add(passiveLine);
        if (!string.IsNullOrWhiteSpace(agencyLine))
            lines.Add(agencyLine);

        return string.Join(Environment.NewLine + "  ", lines);
    }

    public static string FormatFullStatus(GameTimeOfDay currentPhase, in DayRhythmSnapshot snapshot)
    {
        var phaseName = GameTimeHelper.GetDisplayName(currentPhase);
        var builder = new System.Text.StringBuilder();
        builder.Append($"Phase: {phaseName} — ");
        builder.Append(FormatDaySummary(snapshot, currentPhase));

        if (!string.IsNullOrWhiteSpace(snapshot.LastActivityLabel) && snapshot.LastActivityPhase is not null)
        {
            var agoLabel = snapshot.ElapsedSinceLast is null
                ? "recently"
                : snapshot.ElapsedSinceLast.Value.TotalMinutes < 1
                    ? "just now"
                    : snapshot.ElapsedSinceLast.Value.TotalMinutes < 60
                        ? $"{snapshot.ElapsedSinceLast.Value.TotalMinutes:F0}m ago"
                        : $"{snapshot.ElapsedSinceLast.Value.TotalHours:F0}h ago";

            var moment = string.IsNullOrWhiteSpace(snapshot.LastRhythmMoment)
                ? $"{GameTimeHelper.GetDisplayName(snapshot.LastActivityPhase.Value)} {snapshot.LastActivityLabel}"
                : snapshot.LastRhythmMoment;

            builder.AppendLine();
            builder.Append($"  Last — {moment} ({agoLabel}).");
        }

        builder.AppendLine();
        builder.Append($"  {ActivityTimeOfDayConfig.GetLightSuggestion(currentPhase)}");

        return builder.ToString().TrimEnd();
    }

    public static string FormatAgencyList(GameTimeOfDay currentPhase, in DayRhythmSnapshot snapshot)
    {
        var lines = new List<string>
        {
            "Daily rhythm agency (optional, once per day each):",
            "  start calm — morning: +2 mood now; cozy activities +2 mood all day.",
            "  start active — morning: +2 mood, -3 fatigue now; social help +2 relief all day.",
            "  focused break — afternoon: +1 mood, -2 fatigue now; next activity gets a strong boost.",
            "  rest early — evening/night: -3 fatigue now, -1 mood; cozy/leisure +3 fatigue relief tonight.",
            "  push through — evening/night: +2 mood, +2 fatigue now; social help +2 mood/+2 relief tonight.",
            "  rhythm wind down — evening/night: settle the day based on what you did.",
        };

        lines.Add(string.Empty);
        lines.Add(FormatFullStatus(currentPhase, snapshot));
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>Short contextual read on how the day has felt based on activity mix.</summary>
    public static string? GetDayToneLine(in DayRhythmSnapshot snapshot, GameTimeOfDay currentPhase)
    {
        var total = snapshot.RecoveryCount + snapshot.LeisureCount + snapshot.SocialCount;
        if (total == 0)
            return null;

        var cozyTotal = snapshot.RecoveryCount + snapshot.LeisureCount;

        if (snapshot.FocusedBreakBonusPending)
            return "Day tone: you paused to reset — your next activity should land with extra lift.";

        if (snapshot.MorningRecoveryCount >= MorningRecoveryAfternoonMoodThreshold
            && currentPhase is GameTimeOfDay.Afternoon or GameTimeOfDay.Evening or GameTimeOfDay.Night)
        {
            return "Day tone: your refreshing morning is still shaping the hours ahead (+mood on activities).";
        }

        if (snapshot.SocialCount >= SocialBurstThreshold && snapshot.PhaseSocialCount >= SocialBurstThreshold)
            return "Day tone: conversation has been lively — you may welcome a quieter beat soon.";

        if (snapshot.SocialCount >= 2 && snapshot.SocialCount > cozyTotal)
            return "Day tone: a warmly connected day — village life has kept you company.";

        if (cozyTotal >= 2 && cozyTotal > snapshot.SocialCount)
            return "Day tone: a gently paced day — cozy pauses have shaped your rhythm.";

        if (snapshot.RecoveryCount > 0 && snapshot.LeisureCount == 0 && snapshot.SocialCount == 0)
            return "Day tone: a restorative day — you've tended to yourself at home.";

        if (snapshot.LeisureCount > 0 && snapshot.RecoveryCount == 0 && snapshot.SocialCount == 0)
            return "Day tone: an unhurried day — you've lingered in the village's easy corners.";

        if (snapshot.SocialCount > 0 && cozyTotal == 0)
            return "Day tone: a helping day — you've been out among neighbors.";

        if (cozyTotal > 0 && snapshot.SocialCount > 0)
            return "Day tone: a balanced day — quiet moments and village connection have woven together.";

        return null;
    }

    private static string? GetMorningIntentLine(in DayRhythmSnapshot snapshot, GameTimeOfDay currentPhase)
    {
        return snapshot.MorningIntent switch
        {
            MorningRhythmIntent.Calm =>
                "Morning intent: Calm start — cozy/leisure activities gain +2 mood through the day.",
            MorningRhythmIntent.Active =>
                "Morning intent: Active start — social help gains +2 relief through the day.",
            _ => currentPhase == GameTimeOfDay.Morning
                ? "Morning intent: unset — try 'start calm' or 'start active' once today."
                : null,
        };
    }

    private static string? GetAgencyChoiceLine(in DayRhythmSnapshot snapshot)
    {
        if (snapshot.FocusedBreakBonusPending)
        {
            return "Active boost: next activity +3 mood"
                + $" (+{FocusedBreakNextActivityFatigueBonus:F0} fatigue relief if cozy/leisure).";
        }

        return snapshot.EveningPace switch
        {
            EveningPaceIntent.RestEarly =>
                $"Evening pace: Rest early — cozy/leisure gains +{RestEarlyEveningFatigueBonus:F0} fatigue relief tonight.",
            EveningPaceIntent.PushThrough =>
                $"Evening pace: Push through — social help gains +{PushThroughEveningSocialMoodBonus:F0} mood and +{PushThroughEveningSocialBonus:F0} relief tonight.",
            _ => null,
        };
    }

    private static string? GetPassiveLiftLine(in DayRhythmSnapshot snapshot, GameTimeOfDay currentPhase)
    {
        if (snapshot.MorningRecoveryCount >= MorningRecoveryAfternoonMoodThreshold)
        {
            if (currentPhase == GameTimeOfDay.Afternoon)
            {
                return $"Passive lift: +{MorningRecoveryAfternoonMoodBonus:F0} Mood on activities from your refreshing morning.";
            }

            if (currentPhase == GameTimeOfDay.Evening)
            {
                return $"Passive lift: +{MorningRecoveryEveningMoodBonus:F0} Mood on activities — morning calm still lingers.";
            }
        }

        if (snapshot.PhaseSocialCount >= SocialBurstThreshold)
        {
            return $"Social pace: many conversations this phase — next social help may cost ~{SocialBurstPenalty:F0} relief.";
        }

        if (currentPhase is GameTimeOfDay.Evening or GameTimeOfDay.Night
            && snapshot.RecoveryCount + snapshot.LeisureCount >= 1)
        {
            return $"Passive lift: +{EveningCozyFatigueBonus:F0} Fatigue relief on cozy/leisure this hour.";
        }

        return null;
    }

    private static string? GetAgencyHint(in DayRhythmSnapshot snapshot, GameTimeOfDay currentPhase)
    {
        if (currentPhase == GameTimeOfDay.Morning && snapshot.MorningIntent == MorningRhythmIntent.None)
            return "Agency available: start calm | start active (morning, once today).";

        if (currentPhase == GameTimeOfDay.Afternoon && !snapshot.AfternoonFocusedBreakUsed)
            return "Agency available: focused break (afternoon, once today).";

        if (currentPhase is GameTimeOfDay.Evening or GameTimeOfDay.Night
            && snapshot.EveningPace == EveningPaceIntent.None
            && !snapshot.EveningWindDownUsed)
        {
            return "Agency available: rest early | push through | rhythm wind down (evening, once each).";
        }

        if (currentPhase is GameTimeOfDay.Evening or GameTimeOfDay.Night
            && snapshot.EveningPace == EveningPaceIntent.None
            && snapshot.EveningWindDownUsed)
        {
            return "Agency available: rest early | push through (evening, once today).";
        }

        if (snapshot.EveningWindDownUsed)
            return "Evening wind-down: already settled tonight.";

        return null;
    }

    private static string CombineNote(string? existing, string addition) =>
        string.IsNullOrWhiteSpace(existing) ? addition : $"{existing} {addition}";
}

public enum DailyRhythmActivityCategory : byte
{
    Recovery = 0,
    Leisure = 1,
    Social = 2,
}

public enum MorningRhythmIntent : byte
{
    None = 0,
    Calm = 1,
    Active = 2,
}

public enum EveningPaceIntent : byte
{
    None = 0,
    RestEarly = 1,
    PushThrough = 2,
}