using Bloomtown.Shared.Community;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// A single player vote on a village project proposal.
/// </summary>
public sealed class ProjectVote
{
    public int ProposalId { get; init; }
    public uint PlayerEntityId { get; init; }
    public ProjectVoteChoice Vote { get; init; }
    public int VoteWeight { get; init; }
    public DateTime VotedAtUtc { get; init; }
}