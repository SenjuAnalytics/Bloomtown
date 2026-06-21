using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct GatheringResponse(
    GatheringResponseKind Kind,
    ItemType ResourceType,
    int Quantity,
    int NodeId,
    GatheringFailureReason FailureReason,
    string Message);