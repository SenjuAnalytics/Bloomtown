namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerContributionSummaryRecord
{
    public uint EntityId { get; init; }
    public int VillageContributionScore { get; init; }
    public int VillageTitleId { get; init; }
    public int VillagePositionId { get; init; }
}