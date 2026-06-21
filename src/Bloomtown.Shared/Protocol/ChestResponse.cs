namespace Bloomtown.Shared.Protocol;

public readonly record struct ChestResponse(
    bool Success,
    ChestRequestKind Kind,
    ChestFailureReason FailureReason,
    string Message);