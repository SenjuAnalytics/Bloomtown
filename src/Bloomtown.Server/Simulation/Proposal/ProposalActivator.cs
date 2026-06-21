using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Shared.Community;
using Serilog;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Shared activation path for proposals approved by citizens, Council, or Chief authority.
/// </summary>
public static class ProposalActivator
{
    public static async Task<ProposalActivationResult> TryActivateAsync(
        CommunityProjectService communityProjectService,
        VillageProjectProposalRepository proposalRepository,
        VillageProjectProposalRecord proposal)
    {
        var requirements = CommunityProjectDefinitionRepository.DeserializeRequirements(proposal.RequiredResourcesJson);

        try
        {
            var createdProjectId = await communityProjectService.RegisterDynamicProjectAsync(
                proposal.ProjectSlug,
                proposal.ProjectName,
                $"Player-proposed village project: {proposal.ProjectName}",
                requirements);

            await proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.Approved, createdProjectId);

            return new ProposalActivationResult(true, createdProjectId, null);
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to create community project from approved proposal #{ProposalId}.",
                proposal.Id);

            await proposalRepository.UpdateStatusAsync(proposal.Id, ProposalStatus.Rejected, null);
            return new ProposalActivationResult(false, null, ex.Message);
        }
    }
}

public readonly record struct ProposalActivationResult(bool Success, byte? CreatedProjectId, string? ErrorMessage);