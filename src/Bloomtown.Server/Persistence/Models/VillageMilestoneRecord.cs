using Bloomtown.Shared.Milestone;

namespace Bloomtown.Server.Persistence.Models;

public sealed class VillageMilestoneRecord
{
    public VillageMilestone MilestoneId { get; init; }
    public DateTime UnlockedAtUtc { get; init; }
}