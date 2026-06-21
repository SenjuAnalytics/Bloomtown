namespace Bloomtown.Shared.Protocol;

public readonly record struct LegacyFocusResponse(
    bool Success,
    LegacyFocusRequestKind Kind,
    LegacyFocusFailureReason FailureReason,
    string Message);