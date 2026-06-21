namespace Bloomtown.Shared.Community;

/// <summary>
/// Lifecycle status for a village project proposal.
/// </summary>
public enum ProposalStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Voting = 4,
    CouncilVoting = 5,
}