namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Simple majority voting rules for village project proposals.
/// </summary>
public static class ProjectVotingConfig
{
    /// <summary>How long players have to vote, measured in game minutes.</summary>
    public const int DurationGameMinutes = 10;

    /// <summary>Minimum distinct voters required for a valid outcome.</summary>
    public const int MinimumVoterCount = 2;
}