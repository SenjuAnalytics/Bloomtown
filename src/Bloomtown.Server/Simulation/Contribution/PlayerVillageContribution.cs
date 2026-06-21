using Bloomtown.Shared.Contribution;

namespace Bloomtown.Server.Simulation.Contribution;

/// <summary>
/// Persisted village contribution identity for one player.
/// </summary>
public readonly record struct PlayerVillageContribution(
    uint PlayerEntityId,
    int ContributionScore,
    VillageTitle CurrentTitle);