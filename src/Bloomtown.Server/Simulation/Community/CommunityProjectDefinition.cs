using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Static definition for a communal village project.
/// </summary>
public sealed class CommunityProjectDefinition
{
    public required byte ProjectId { get; init; }
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyDictionary<ItemType, int> Requirements { get; init; }
}