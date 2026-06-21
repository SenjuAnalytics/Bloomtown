namespace Bloomtown.Shared.Contribution;

/// <summary>
/// Human-readable village title labels for console output.
/// </summary>
public static class VillageTitleDisplay
{
    public static string GetName(VillageTitle title)
    {
        return title switch
        {
            VillageTitle.Helper => "Helper",
            VillageTitle.Builder => "Builder",
            VillageTitle.RespectedVillager => "Respected Villager",
            VillageTitle.ElderCandidate => "Elder Candidate",
            _ => "Newcomer",
        };
    }
}