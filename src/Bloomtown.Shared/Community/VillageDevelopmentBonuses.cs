namespace Bloomtown.Shared.Community;

/// <summary>
/// Passive gameplay bonuses granted by the current village development level.
/// Values are intentionally small — atmosphere first, not power creep.
/// </summary>
public readonly struct VillageDevelopmentBonuses
{
    /// <summary>Multiplier on fatigue rise per game minute (lower = tires slower).</summary>
    public float FatigueRiseMultiplier { get; init; }

    /// <summary>Passive mood recovery per game minute when the player is not stressed.</summary>
    public float PassiveMoodRecoveryPerGameMinute { get; init; }

    /// <summary>Multiplier on mood decay while stressed (lower = mood holds up better).</summary>
    public float MoodDecayUnderStressMultiplier { get; init; }

    public static VillageDevelopmentBonuses None => new()
    {
        FatigueRiseMultiplier = 1f,
        MoodDecayUnderStressMultiplier = 1f,
    };

    public bool HasPassiveBonus =>
        FatigueRiseMultiplier < 1f
        || PassiveMoodRecoveryPerGameMinute > 0f
        || MoodDecayUnderStressMultiplier < 1f;
}