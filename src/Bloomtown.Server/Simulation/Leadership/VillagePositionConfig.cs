using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// Requirements and election rules for village leadership positions.
/// </summary>
public static class VillagePositionConfig
{
    public const int ElectionDurationGameMinutes = 10;
    public const int MinimumVoterCount = 2;

    public readonly record struct PositionRequirement(VillageTitle MinimumTitle, int MinimumContributionScore);

    /// <summary>
    /// Returns title and contribution thresholds required to run for a position.
    /// </summary>
    public static PositionRequirement GetRequirement(VillagePosition position)
    {
        return position switch
        {
            // Ketua Proyek: Builder + 150 contribution score.
            VillagePosition.ProjectLeader => new(VillageTitle.Builder, 150),
            // Penasehat Desa: Respected Villager, no extra score gate.
            VillagePosition.Advisor => new(VillageTitle.RespectedVillager, 0),
            VillagePosition.DeputyChief => new(VillageTitle.RespectedVillager, 350),
            // Kepala Desa: Elder Candidate + 500 contribution score.
            VillagePosition.Chief => new(VillageTitle.ElderCandidate, 500),
            _ => new(VillageTitle.ElderCandidate, int.MaxValue),
        };
    }

    public static bool CanViewVillageOverview(VillagePosition position)
    {
        return position is VillagePosition.Advisor or VillagePosition.DeputyChief or VillagePosition.Chief;
    }
}