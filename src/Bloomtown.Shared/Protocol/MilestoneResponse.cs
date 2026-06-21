namespace Bloomtown.Shared.Protocol;

public readonly record struct MilestoneResponse(
    bool Success,
    MilestoneRequestKind Kind,
    MilestoneFailureReason FailureReason,
    string Message);