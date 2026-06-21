using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct ChestRequest(
    ChestRequestKind Kind,
    ItemType ItemType,
    byte Quantity);