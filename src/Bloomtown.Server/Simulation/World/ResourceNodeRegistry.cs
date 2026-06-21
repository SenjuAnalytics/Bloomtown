using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.World;

/// <summary>
/// Static resource node placements for the Bloomtown prototype map.
/// </summary>
public static class ResourceNodeRegistry
{
    private static readonly IReadOnlyList<ResourceNode> Nodes =
    [
        new ResourceNode
        {
            NodeId = 1,
            Name = "Tree Grove",
            WorldX = 14f,
            WorldZ = 10f,
            OutputItem = ItemType.Wood,
        },
        new ResourceNode
        {
            NodeId = 2,
            Name = "Forest Edge",
            WorldX = 10f,
            WorldZ = 14f,
            OutputItem = ItemType.Wood,
        },
        new ResourceNode
        {
            NodeId = 3,
            Name = "Rocky Patch",
            WorldX = 16f,
            WorldZ = 16f,
            OutputItem = ItemType.Stone,
        },
        new ResourceNode
        {
            NodeId = 4,
            Name = "Far Quarry",
            WorldX = 108f,
            WorldZ = 108f,
            OutputItem = ItemType.Stone,
        },
        new ResourceNode
        {
            NodeId = 5,
            Name = "Pine Stand",
            WorldX = 104f,
            WorldZ = 102f,
            OutputItem = ItemType.Wood,
        },
    ];

    public static IReadOnlyList<ResourceNode> All => Nodes;

    public static ResourceNode? GetById(int nodeId)
    {
        return Nodes.FirstOrDefault(node => node.NodeId == nodeId);
    }
}