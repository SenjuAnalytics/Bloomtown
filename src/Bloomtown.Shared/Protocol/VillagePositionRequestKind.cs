namespace Bloomtown.Shared.Protocol;

public enum VillagePositionRequestKind : byte
{
    List = 1,
    RunFor = 2,
    Vote = 3,
    CouncilVote = 4,
    ChiefApprove = 5,
    ChiefVeto = 6,
}