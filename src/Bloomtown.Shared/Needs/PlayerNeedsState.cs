namespace Bloomtown.Shared.Needs;

/// <summary>
/// Snapshot of a player's daily-life needs (all values 0–100).
/// </summary>
public readonly struct PlayerNeedsState
{
    public float Mood { get; init; }
    public float Fatigue { get; init; }
    public float SocialNeed { get; init; }

    public PlayerNeedsState(float mood, float fatigue, float socialNeed)
    {
        Mood = mood;
        Fatigue = fatigue;
        SocialNeed = socialNeed;
    }
}