namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerLongTermGoalRecord
{
    public uint PlayerEntityId { get; init; }
    public int GoalKind { get; init; } = 1;
    public int HighestCompletedMilestone { get; init; }
    public DateTime? GoalCompletedAtUtc { get; init; }
    public int LegacyArchetype { get; init; }
    public int BuilderInfluence { get; init; }
    public int CaretakerInfluence { get; init; }
    public int ConnectorInfluence { get; init; }
    public int LegacyFocus { get; init; }
}