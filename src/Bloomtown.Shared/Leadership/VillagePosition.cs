namespace Bloomtown.Shared.Leadership;

/// <summary>
/// Formal village leadership roles earned through contribution and election.
/// </summary>
public enum VillagePosition : byte
{
    None = 0,
    ProjectLeader = 1,
    Advisor = 2,
    DeputyChief = 3,
    Chief = 4,
}