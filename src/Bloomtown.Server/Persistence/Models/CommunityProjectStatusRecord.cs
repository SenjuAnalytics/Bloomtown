namespace Bloomtown.Server.Persistence.Models;

public sealed class CommunityProjectStatusRecord
{
    public byte ProjectId { get; init; }
    public int Status { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}