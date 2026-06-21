using Bloomtown.Server.Simulation.Leadership;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Classifies proposals as small (citizen vote / Chief fast-track) or important (Council only).
/// </summary>
public static class ProjectTierClassifier
{
    public static ProjectImportanceTier Classify(IReadOnlyDictionary<ItemType, int> requirements)
    {
        var total = requirements.Values.Sum();

        if (total > VillageCouncilConfig.ImportantProjectTotalThreshold)
            return ProjectImportanceTier.Important;

        foreach (var quantity in requirements.Values)
        {
            if (quantity >= VillageCouncilConfig.ImportantSingleItemThreshold)
                return ProjectImportanceTier.Important;
        }

        if (requirements.ContainsKey(ItemType.Tool))
            return ProjectImportanceTier.Important;

        return ProjectImportanceTier.Small;
    }
}