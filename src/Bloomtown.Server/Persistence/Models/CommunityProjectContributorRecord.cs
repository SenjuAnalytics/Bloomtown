namespace Bloomtown.Server.Persistence.Models;

public sealed class CommunityProjectContributorRecord
{
    public byte ProjectId { get; init; }
    public uint PlayerEntityId { get; init; }
    public int TotalContributed { get; init; }
}