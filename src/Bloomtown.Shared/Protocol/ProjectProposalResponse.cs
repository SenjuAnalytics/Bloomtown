namespace Bloomtown.Shared.Protocol;

public readonly record struct ProjectProposalResponse(
    bool Success,
    ProjectProposalRequestKind Kind,
    ProjectProposalFailureReason FailureReason,
    string Message);