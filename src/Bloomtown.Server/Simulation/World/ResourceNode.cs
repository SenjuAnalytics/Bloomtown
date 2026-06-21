using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.World;

/// <summary>
/// A fixed world location where players can gather a specific resource.
/// </summary>
public sealed class ResourceNode
{
    public required int NodeId { get; init; }
    public required string Name { get; init; }
    public required float WorldX { get; init; }
    public required float WorldZ { get; init; }
    public required ItemType OutputItem { get; init; }
    public int YieldAmount { get; init; } = GatheringConfig.DefaultYieldAmount;
}