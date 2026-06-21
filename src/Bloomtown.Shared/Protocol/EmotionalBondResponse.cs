namespace Bloomtown.Shared.Protocol;

public readonly record struct EmotionalBondResponse(
    bool Success,
    EmotionalBondRequestKind Kind,
    EmotionalBondActionKind Action,
    EmotionalBondFailureReason FailureReason,
    string Message);