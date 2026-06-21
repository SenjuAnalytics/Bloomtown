namespace Bloomtown.Shared.Goals;

/// <summary>Persisted progress for the player's active long-term goal.</summary>
public readonly record struct PlayerLongTermGoalProgress(
    PlayerLongTermGoalKind GoalKind,
    PlayerLongTermGoalMilestone HighestCompletedMilestone,
    DateTime? GoalCompletedAtUtc,
    LegacyArchetype LegacyArchetype = LegacyArchetype.None,
    LegacyArchetypeInfluence Influence = default,
    LegacyArchetype ActiveFocus = LegacyArchetype.None)
{
    public bool IsComplete =>
        HighestCompletedMilestone >= PlayerLongTermGoalMilestone.BloomtownLegacy
        && GoalCompletedAtUtc is not null;
}