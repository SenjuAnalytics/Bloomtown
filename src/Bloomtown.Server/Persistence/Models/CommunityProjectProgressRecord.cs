using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Persistence.Models;

public sealed class CommunityProjectProgressRecord
{
    public byte ProjectId { get; init; }
    public ItemType ItemType { get; init; }
    public int CurrentQuantity { get; init; }
}