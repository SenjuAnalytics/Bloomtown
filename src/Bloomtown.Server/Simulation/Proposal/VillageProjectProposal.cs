using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// A player-submitted proposal for a new communal village project.
/// </summary>
public sealed class VillageProjectProposal
{
    public int Id { get; init; }
    public uint ProposedByPlayerId { get; init; }
    public required string ProjectName { get; init; }
    public required string ProjectSlug { get; init; }
    public required IReadOnlyDictionary<ItemType, int> RequiredResources { get; init; }
    public ProposalStatus Status { get; init; }
    public byte? CreatedProjectId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}