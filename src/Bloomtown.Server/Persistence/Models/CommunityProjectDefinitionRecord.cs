namespace Bloomtown.Server.Persistence.Models;

public sealed class CommunityProjectDefinitionRecord
{
    public byte ProjectId { get; init; }
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string RequirementsJson { get; init; }
    public bool IsBuiltin { get; init; }
}