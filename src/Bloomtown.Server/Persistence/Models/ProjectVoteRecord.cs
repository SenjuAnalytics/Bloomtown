using Bloomtown.Shared.Community;

namespace Bloomtown.Server.Persistence.Models;

public sealed class ProjectVoteRecord
{
    public int ProposalId { get; init; }
    public uint PlayerEntityId { get; init; }
    public ProjectVoteChoice Vote { get; init; }
    public int VoteWeight { get; init; }
    public DateTime VotedAtUtc { get; init; }
}