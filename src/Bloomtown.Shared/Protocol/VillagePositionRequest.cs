using Bloomtown.Shared.Community;
using Bloomtown.Shared.Leadership;

namespace Bloomtown.Shared.Protocol;

public readonly record struct VillagePositionRequest(
    VillagePositionRequestKind Kind,
    VillagePosition Position,
    ProjectVoteChoice VoteChoice = ProjectVoteChoice.None,
    int ProposalId = 0);