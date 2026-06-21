namespace Bloomtown.Shared.Protocol;

public readonly record struct CommunityProjectResponse(
    bool Success,
    CommunityProjectRequestKind Kind,
    CommunityProjectFailureReason FailureReason,
    string Message);