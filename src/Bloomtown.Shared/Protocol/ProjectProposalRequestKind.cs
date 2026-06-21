namespace Bloomtown.Shared.Protocol;

public enum ProjectProposalRequestKind : byte
{
    Propose = 1,
    ListProposals = 2,
    Vote = 3,
}