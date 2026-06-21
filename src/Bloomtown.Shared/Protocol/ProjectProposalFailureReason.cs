namespace Bloomtown.Shared.Protocol;

public enum ProjectProposalFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    InsufficientTitle = 2,
    InvalidProjectName = 3,
    InvalidRequirements = 4,
    EconomyUnavailable = 5,
    DatabaseError = 6,
    UnknownProposal = 7,
    NotInVoting = 8,
    AlreadyVoted = 9,
    InvalidVote = 10,
    VotingClosed = 11,
}