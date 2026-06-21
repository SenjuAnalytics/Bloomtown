namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerNpcRelationshipRecord
{
    public uint PlayerEntityId { get; init; }
    public uint NpcEntityId { get; init; }
    public int AffinityValue { get; init; }
    public int LastInteractionGameDay { get; init; }
    public DateTime LastInteractionUtc { get; init; } = DateTime.UtcNow;
}