using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Derived player state used to evaluate milestone eligibility.
/// Built from legacy, reputation, contribution, and relationship systems.
/// </summary>
public readonly record struct PlayerLongTermGoalSnapshot(
    int TotalHelpCount,
    CommunitySocialRole SocialRole,
    VillageTitle VillageTitle,
    int VillageContributionScore,
    int FriendCount,
    int AcquaintanceCount,
    int CloseFriendCount,
    int CompletedProjectContributions,
    bool HasLegacyRecognition);