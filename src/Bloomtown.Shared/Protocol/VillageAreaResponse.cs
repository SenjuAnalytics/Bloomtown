namespace Bloomtown.Shared.Protocol;

public readonly record struct VillageAreaResponse(
    bool Success,
    VillageAreaRequestKind Kind,
    VillageAreaFailureReason FailureReason,
    string Message);