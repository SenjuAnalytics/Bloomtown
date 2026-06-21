using Bloomtown.Server.Simulation.Economy;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// In-memory personal chest storage for one player.
/// </summary>
public sealed class PlayerChestState
{
    public required uint PlayerEntityId { get; init; }

    public Inventory Storage { get; } = new();
}