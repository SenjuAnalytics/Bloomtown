using Bloomtown.Shared.Housing;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerHousingRecord
{
    public uint PlayerEntityId { get; init; }
    public float HouseX { get; init; }
    public float HouseZ { get; init; }
    public HouseTier HouseTier { get; init; } = HouseTier.Basic;
}