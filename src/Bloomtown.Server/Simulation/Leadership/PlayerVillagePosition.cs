using Bloomtown.Shared.Leadership;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// A player's assigned village leadership role.
/// </summary>
public sealed class PlayerVillagePosition
{
    public uint PlayerEntityId { get; init; }
    public VillagePosition Position { get; init; }
    public DateTime? AssignedAtUtc { get; init; }
}