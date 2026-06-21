using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerHomeStorageEntryRecord
{
    public uint PlayerEntityId { get; init; }
    public ItemType ItemType { get; init; }
    public int Quantity { get; init; }
}