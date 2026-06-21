using Bloomtown.Shared.Contribution;

namespace Bloomtown.Server.Simulation.Contribution;

/// <summary>
/// Maps village contribution score to a recognition title.
/// </summary>
public static class VillageTitleCalculator
{
    public static VillageTitle GetTitle(int contributionScore)
    {
        if (contributionScore >= VillageContributionConfig.ElderCandidateThreshold)
            return VillageTitle.ElderCandidate;

        if (contributionScore >= VillageContributionConfig.RespectedVillagerThreshold)
            return VillageTitle.RespectedVillager;

        if (contributionScore >= VillageContributionConfig.BuilderThreshold)
            return VillageTitle.Builder;

        if (contributionScore >= VillageContributionConfig.HelperThreshold)
            return VillageTitle.Helper;

        return VillageTitle.Newcomer;
    }
}