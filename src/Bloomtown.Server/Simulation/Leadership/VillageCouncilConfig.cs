using Bloomtown.Shared.Leadership;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// Village Council membership, review timing, and Chief authority limits.
/// </summary>
public static class VillageCouncilConfig
{
    public static readonly VillagePosition[] CouncilPositions =
    [
        VillagePosition.Advisor,
        VillagePosition.DeputyChief,
        VillagePosition.Chief,
    ];

    /// <summary>Total resource quantity above this marks a proposal as important (Council-only).</summary>
    public const int ImportantProjectTotalThreshold = 80;

    /// <summary>Any single resource at or above this quantity marks a proposal as important.</summary>
    public const int ImportantSingleItemThreshold = 50;

    public const int ReviewDurationGameMinutes = 10;
    public const int MinimumCouncilVotes = 2;
    public const int ChiefDirectApprovalsPerGameDay = 1;
    public const int ChiefVetoesPerGameDay = 1;

    public static bool IsCouncilPosition(VillagePosition position)
    {
        return position is VillagePosition.Advisor or VillagePosition.DeputyChief or VillagePosition.Chief;
    }
}