using Bloomtown.Shared.Memory;

namespace Bloomtown.Server.Simulation.Memory;

/// <summary>
/// Runtime record of one NPC memory about a player.
/// </summary>
public sealed class PlayerNpcMemory
{
    public required uint PlayerEntityId { get; init; }
    public required uint NpcEntityId { get; init; }
    public required NpcMemoryType MemoryType { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
}