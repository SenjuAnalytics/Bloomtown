namespace Bloomtown.Shared.Goals;

/// <summary>Tiered milestones toward completing the village legacy goal.</summary>
public enum PlayerLongTermGoalMilestone : byte
{
    None = 0,

    /// <summary>Help the village enough times to feel like a regular face.</summary>
    PuttingDownRoots = 1,

    /// <summary>Earn a named social role through consistent community help.</summary>
    TrustedNeighbor = 2,

    /// <summary>Become a recognized helper with real bonds in the village.</summary>
    VillageStory = 3,

    /// <summary>Complete the legacy — the village remembers your name.</summary>
    BloomtownLegacy = 4,
}