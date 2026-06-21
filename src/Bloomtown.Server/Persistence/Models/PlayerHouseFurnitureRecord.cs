using Bloomtown.Shared.Housing;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerHouseFurnitureRecord
{
    public uint PlayerEntityId { get; init; }
    public FurnitureType FurnitureType { get; init; }
    public int Quantity { get; init; } = 1;
}