using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Protocol;

public readonly record struct ProjectProposalRequest(
    ProjectProposalRequestKind Kind,
    string ProjectName,
    byte WoodQuantity,
    byte StoneQuantity,
    byte AppleQuantity,
    byte ToolQuantity,
    int ProposalId = 0,
    ProjectVoteChoice VoteChoice = ProjectVoteChoice.None);