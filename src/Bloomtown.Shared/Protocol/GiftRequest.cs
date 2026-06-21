using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct GiftRequest(ItemType ItemType, byte Quantity, uint NpcEntityId);