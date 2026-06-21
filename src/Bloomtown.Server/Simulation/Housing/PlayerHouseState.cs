using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Housing;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// In-memory home location, tier, and home storage for one player.
/// </summary>
public sealed class PlayerHouseState
{
    public required uint PlayerEntityId { get; init; }
    public float HouseX { get; set; }
    public float HouseZ { get; set; }
    public HouseTier HouseTier { get; set; } = HouseTier.Basic;
    public Dictionary<FurnitureType, int> PlacedFurniture { get; } = new();
    public Inventory HomeStorage { get; } = new();

    public int ComfortScore => FurnitureComfortConfig.CalculateComfortScore(PlacedFurniture);
}