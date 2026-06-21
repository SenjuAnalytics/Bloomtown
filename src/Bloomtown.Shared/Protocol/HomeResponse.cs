namespace Bloomtown.Shared.Protocol;

public readonly record struct HomeResponse(
    bool Success,
    HomeRequestKind Kind,
    HomeFailureReason FailureReason,
    string Message);