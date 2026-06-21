namespace Bloomtown.Shared.Needs;

/// <summary>
/// Tunable decay, recovery, thresholds, and penalties for player Mood, Fatigue, and Social Need.
/// </summary>
public static class PlayerNeedsConfig
{
    public const float MinValue = 0f;
    public const float MaxValue = 100f;

    public const float DefaultMood = 70f;
    public const float DefaultFatigue = 20f;
    public const float DefaultSocialNeed = 25f;

    // --- Passive decay per game minute (applied by PlayerNeedsService each tick) ---

    /// <summary>Fatigue rises while awake (higher = more tired).</summary>
    public const float FatigueRisePerGameMinute = 0.25f;

    /// <summary>Social need rises over time (higher = lonelier).</summary>
    public const float SocialNeedRisePerGameMinute = 0.35f;

    /// <summary>Mood drops when the player is stressed (high fatigue, loneliness, or hunger).</summary>
    public const float MoodDecayUnderStressPerGameMinute = 0.2f;

    // --- Thresholds for status labels and penalties ---

    public const float GoodMoodThreshold = 60f;
    public const float LowMoodThreshold = 30f;

    public const float RestedFatigueThreshold = 30f;
    public const float HighFatigueThreshold = 70f;

    public const float ConnectedSocialThreshold = 30f;
    public const float LonelySocialThreshold = 60f;

    /// <summary>Hunger at or above this value contributes to mood stress decay.</summary>
    public const float StressHungerThreshold = 70f;

    // --- Gathering penalties (multipliers stack multiplicatively) ---

    /// <summary>Exhausted players gather 20% slower.</summary>
    public const double HighFatigueGatherDurationMultiplier = 1.2;

    /// <summary>Very low mood adds a small gathering slowdown.</summary>
    public const double LowMoodGatherDurationMultiplier = 1.1;

    // --- Activity recovery / event deltas ---

    public const float GiftMoodGain = 5f;
    public const float GiftSocialReduction = 10f;

    public const float TalkSocialReduction = 15f;
    public const float GreetSocialReduction = 8f;

    public const float RestMoodGain = 5f;
    public const float RestFatigueReduction = 25f;
    public const float SleepFatigueReduction = 50f;

    public const float GatherStartFatigueGain = 2f;
    public const float GatherCompleteFatigueGain = 6f;

    public const float HouseUpgradeMoodGain = 8f;
    public const float PlaceFurnitureMoodGain = 4f;

    public const float LowComfortSleepMoodGain = 3f;
    public const float MediumComfortSleepMoodGain = 5f;
    public const float HighComfortSleepMoodGain = 8f;

    public const int MediumComfortThreshold = 21;
    public const int HighComfortThreshold = 41;

    public static bool IsGoodMood(float mood) => mood >= GoodMoodThreshold;
    public static bool IsLowMood(float mood) => mood < LowMoodThreshold;

    public static bool IsRested(float fatigue) => fatigue < RestedFatigueThreshold;
    public static bool IsExhausted(float fatigue) => fatigue >= HighFatigueThreshold;

    public static bool IsConnected(float socialNeed) => socialNeed < ConnectedSocialThreshold;
    public static bool IsLonely(float socialNeed) => socialNeed >= LonelySocialThreshold;

    public static bool IsUnderStress(float fatigue, float socialNeed, float hunger)
    {
        return IsExhausted(fatigue)
               || IsLonely(socialNeed)
               || hunger >= StressHungerThreshold;
    }

    /// <summary>
    /// Combines fatigue and mood penalties for gathering duration.
    /// </summary>
    public static double GetGatherDurationMultiplier(float mood, float fatigue)
    {
        var multiplier = 1.0;
        if (IsExhausted(fatigue))
            multiplier *= HighFatigueGatherDurationMultiplier;
        if (IsLowMood(mood))
            multiplier *= LowMoodGatherDurationMultiplier;
        return multiplier;
    }

    public static float GetSleepMoodGain(int comfortScore)
    {
        if (comfortScore >= HighComfortThreshold)
            return HighComfortSleepMoodGain;
        if (comfortScore >= MediumComfortThreshold)
            return MediumComfortSleepMoodGain;
        return LowComfortSleepMoodGain;
    }

    public static float Clamp(float value) => Math.Clamp(value, MinValue, MaxValue);

    public static string FormatMoodLabel(float mood)
    {
        if (IsGoodMood(mood))
            return "Good";
        if (IsLowMood(mood))
            return "Low";
        return "Okay";
    }

    public static string FormatFatigueLabel(float fatigue)
    {
        if (IsExhausted(fatigue))
            return "Exhausted";
        if (!IsRested(fatigue))
            return "Tired";
        return "Rested";
    }

    public static string FormatSocialLabel(float socialNeed)
    {
        if (IsLonely(socialNeed))
            return "Lonely";
        if (!IsConnected(socialNeed))
            return "Okay";
        return "Connected";
    }
}