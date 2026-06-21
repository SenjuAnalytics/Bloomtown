namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Vote counts and weighted totals for a proposal tally.
/// </summary>
public readonly record struct WeightedVoteTally(
    int YesCount,
    int NoCount,
    int YesWeight,
    int NoWeight)
{
    public int TotalVoters => YesCount + NoCount;

    /// <summary>
    /// Weighted majority: Yes weight must exceed No weight with enough distinct voters.
    /// </summary>
    public bool WouldApprove(int minimumVoterCount)
    {
        return TotalVoters >= minimumVoterCount && YesWeight > NoWeight;
    }

    public string FormatCountsAndWeights()
    {
        return $"Yes {YesCount} (weight {YesWeight}), No {NoCount} (weight {NoWeight})";
    }
}