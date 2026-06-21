using Bloomtown.Shared.Memory;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerNpcMemoryRecord
{
    public uint PlayerEntityId { get; init; }
    public uint NpcEntityId { get; init; }
    public NpcMemoryType MemoryType { get; init; }
    public DateTime OccurredAtUtc { get; init; }
}