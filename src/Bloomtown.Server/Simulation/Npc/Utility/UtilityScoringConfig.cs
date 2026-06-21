namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Tunable weights and hysteresis thresholds for utility-based activity selection.
/// </summary>
public static class UtilityScoringConfig
{
    public const float NeedUrgencyWeight = 0.40f;
    public const float ScheduleFitWeight = 0.30f;
    public const float DistanceWeight = 0.15f;
    public const float PersonalityWeight = 0.15f;

    /// <summary>Re-score activities every N in-game minutes.</summary>
    // Diturunkan dari 3 → 2 agar NPC mengevaluasi ulang lebih sering (~2 detik real).
    public const double EvaluationIntervalGameMinutes = 2.0;

    /// <summary>
    /// New activity must beat the current activity by at least this many points
    /// before switching (unless minimum dwell time has elapsed).
    /// </summary>
    // Diturunkan dari 18 → 12 agar NPC lebih responsif terhadap kebutuhan mendesak.
    public const float ScoreMarginToSwitch = 12f;

    /// <summary>Minimum time to stay in an activity before a marginal switch is allowed.</summary>
    public const double MinActivityDurationGameMinutes = 2.0;

    /// <summary>Distance at or beyond this world-unit range yields zero distance score.</summary>
    public const float MaxDistanceForScoring = 80f;

    public static readonly Schedule.NpcActivityType[] AvailableActivities =
    [
        Schedule.NpcActivityType.Work,
        Schedule.NpcActivityType.Eat,
        Schedule.NpcActivityType.Rest,
        Schedule.NpcActivityType.Patrol,
        Schedule.NpcActivityType.Social,
    ];
}