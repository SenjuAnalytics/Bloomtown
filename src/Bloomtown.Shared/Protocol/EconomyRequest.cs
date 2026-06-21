using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct EconomyRequest(
    EconomyRequestKind Kind,
    ItemType ItemType,
    byte Quantity,
    uint NpcEntityId);