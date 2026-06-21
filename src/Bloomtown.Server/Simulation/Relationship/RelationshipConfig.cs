namespace Bloomtown.Server.Simulation.Relationship;

/// <summary>
/// Affinity growth from interactions, tier thresholds, and slow neglect decay.
/// </summary>
public static class RelationshipConfig
{
    public const int MaxAffinity = 100;

    /// <summary>Affinity gained from a successful Greet interaction.</summary>
    public const int GreetAffinityGain = 3;

    /// <summary>Affinity gained from a successful Talk interaction.</summary>
    public const int TalkAffinityGain = 6;

    // Tier thresholds (inclusive lower bound).
    public const int AcquaintanceThreshold = 15;
    public const int FriendThreshold = 35;
    public const int CloseFriendThreshold = 65;

    /// <summary>Game days without interaction before affinity begins to decay.</summary>
    public const int DecayGraceGameDays = 2;

    /// <summary>Affinity lost per idle game day once past the grace period.</summary>
    public const int DecayPerIdleGameDay = 1;
}