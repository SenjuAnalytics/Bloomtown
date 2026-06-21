using Bloomtown.Shared.Community;

namespace Bloomtown.Server.Persistence.Models;

public sealed class VillageProjectProposalRecord
{
    public int Id { get; init; }
    public uint ProposedByPlayerId { get; init; }
    public required string ProjectName { get; init; }
    public required string ProjectSlug { get; init; }
    public required string RequiredResourcesJson { get; init; }
    public ProposalStatus Status { get; init; }
    public ProjectImportanceTier ProjectTier { get; init; } = ProjectImportanceTier.Small;
    public byte? CreatedProjectId { get; init; }
    public long? VotingEndTotalMinutes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}