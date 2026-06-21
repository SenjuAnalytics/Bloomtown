using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Runtime progress for one communal project.
/// </summary>
public sealed class CommunityProjectState
{
    public required byte ProjectId { get; init; }
    public CommunityProjectStatus Status { get; set; } = CommunityProjectStatus.Active;
    public DateTime? CompletedAtUtc { get; set; }
    public Dictionary<ItemType, int> Progress { get; } = new();
    public Dictionary<uint, int> ContributorTotals { get; } = new();
}