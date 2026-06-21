namespace Bloomtown.Shared.Contribution;

/// <summary>
/// Maps village titles to proposal vote weights for weighted voting.
/// </summary>
public static class VoteWeightCalculator
{
    public static int GetWeight(VillageTitle title)
    {
        return title switch
        {
            VillageTitle.Builder => 2,
            VillageTitle.RespectedVillager => 3,
            VillageTitle.ElderCandidate => 4,
            VillageTitle.Helper => 1,
            _ => 1,
        };
    }
}