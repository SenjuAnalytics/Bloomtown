namespace Bloomtown.Shared.Protocol;

public enum VillagePositionFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    UnknownPosition = 2,
    NotEligible = 3,
    AlreadyHoldingPosition = 4,
    ElectionInProgress = 5,
    NoActiveElection = 6,
    AlreadyVoted = 7,
    InvalidVote = 8,
    VotingClosed = 9,
    EconomyUnavailable = 10,
    PositionFilled = 11,
    NotCouncilMember = 12,
    NotChief = 13,
    UnknownProposal = 14,
    NotInCouncilVoting = 15,
    NotSmallProject = 16,
    ChiefAuthorityLimitReached = 17,
    AlreadyVotedCouncil = 18,
    CouncilVotingClosed = 19,
    NotInCitizenVoting = 20,
}