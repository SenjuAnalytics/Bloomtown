using Bloomtown.Shared.Milestone;

namespace Bloomtown.Server.Simulation.Milestone;

/// <summary>
/// Hardcoded milestone definitions tied to community projects.
/// </summary>
public static class VillageMilestoneRegistry
{
    private static readonly VillageMilestoneDefinition[] Milestones =
    [
        new()
        {
            Milestone = VillageMilestone.VillageWell,
            ProjectId = 1,
            Name = "Village Well",
            Description = "Fresh water is available for everyone.",
            UnlockAnnouncement = "The village well is now operational! Type 'drink well' nearby to restore energy.",
            InteractionHint = "drink well",
            Interaction = MilestoneInteractionKind.DrinkWell,
            WorldX = 5f,
            WorldZ = 5f,
        },
        new()
        {
            Milestone = VillageMilestone.RepairedBridge,
            ProjectId = 2,
            Name = "Repaired Bridge",
            Description = "Travelers can cross the river safely again.",
            UnlockAnnouncement = "The bridge has been repaired! Type 'cross bridge' to regain energy on your journey.",
            InteractionHint = "cross bridge",
            Interaction = MilestoneInteractionKind.CrossBridge,
            WorldX = 12f,
            WorldZ = 8f,
        },
        new()
        {
            Milestone = VillageMilestone.VillageWarehouse,
            ProjectId = 3,
            Name = "Village Warehouse",
            Description = "Shared supplies support the growing town.",
            UnlockAnnouncement = "The village warehouse is open! Type 'collect stipend' for a small coin stipend.",
            InteractionHint = "collect stipend",
            Interaction = MilestoneInteractionKind.CollectStipend,
            WorldX = 6f,
            WorldZ = 12f,
        },
    ];

    public static IReadOnlyList<VillageMilestoneDefinition> All => Milestones;

    public static bool TryGet(VillageMilestone milestone, out VillageMilestoneDefinition definition)
    {
        definition = Milestones.FirstOrDefault(entry => entry.Milestone == milestone)!;
        return definition is not null;
    }

    public static bool TryGetForProject(byte projectId, out VillageMilestoneDefinition definition)
    {
        definition = Milestones.FirstOrDefault(entry => entry.ProjectId == projectId)!;
        return definition is not null;
    }

    public static bool TryGetForInteraction(MilestoneInteractionKind interaction, out VillageMilestoneDefinition definition)
    {
        definition = Milestones.FirstOrDefault(entry => entry.Interaction == interaction)!;
        return definition is not null;
    }
}