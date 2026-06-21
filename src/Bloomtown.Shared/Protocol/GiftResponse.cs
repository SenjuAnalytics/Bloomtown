using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct GiftResponse(
    bool Success,
    uint NpcEntityId,
    ItemType ItemType,
    byte Quantity,
    int AffinityGained,
    int NewAffinity,
    GiftFailureReason FailureReason,
    string NpcDialogue,
    string Message);