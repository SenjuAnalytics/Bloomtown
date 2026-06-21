namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerMilestoneRecord
{
    public uint PlayerEntityId { get; init; }
    public int CompletedBitmask { get; init; }
    public string RhythmAgencyDays { get; init; } = string.Empty;
    public int DailyActivityCount { get; init; }
    public string DailyActivityDays { get; init; } = string.Empty;
}