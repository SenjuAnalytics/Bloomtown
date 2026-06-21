using Bloomtown.Shared.Milestone;

namespace Bloomtown.Server.Simulation.Milestone;

/// <summary>
/// Static definition linking a community project to its village milestone effect.
/// </summary>
public sealed class VillageMilestoneDefinition
{
    public required VillageMilestone Milestone { get; init; }
    public required byte ProjectId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UnlockAnnouncement { get; init; }
    public required string InteractionHint { get; init; }
    public required MilestoneInteractionKind Interaction { get; init; }
    public required float WorldX { get; init; }
    public required float WorldZ { get; init; }
}