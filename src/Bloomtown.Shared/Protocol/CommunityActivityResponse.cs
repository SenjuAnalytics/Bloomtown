namespace Bloomtown.Shared.Protocol;

public readonly record struct CommunityActivityResponse(
    bool Success,
    CommunityActivityRequestKind Kind,
    CommunityActivityFailureReason FailureReason,
    string Message);