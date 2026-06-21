namespace Bloomtown.Shared.Protocol;

public readonly record struct VillagePositionResponse(
    bool Success,
    VillagePositionRequestKind Kind,
    VillagePositionFailureReason FailureReason,
    string Message);