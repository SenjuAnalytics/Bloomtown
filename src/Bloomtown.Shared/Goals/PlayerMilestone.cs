namespace Bloomtown.Shared.Goals;

/// <summary>
/// Per-player tracking for optional personal milestones and light rhythm counters.
/// </summary>
public sealed class PlayerMilestoneProgress
{
    public HashSet<PlayerMilestoneKind> Completed { get; } = new();
    public HashSet<int> RhythmAgencyDays { get; } = new();
    public int DailyActivityCount { get; set; }
    public HashSet<int> DailyActivityDays { get; } = new();

    public bool IsCompleted(PlayerMilestoneKind kind) => Completed.Contains(kind);

    public int CompletedCount => Completed.Count;
}

/// <summary>
/// Live game-state inputs used to evaluate personal milestone criteria.
/// </summary>
public sealed record PlayerMilestoneSnapshot(
    int PlacedFurnitureCount,
    int ComfortScore,
    int TotalHelpCount,
    int FocusCloseFriendCount,
    int RhythmAgencyDayCount,
    int DailyActivityCount,
    int DailyActivityDayCount);