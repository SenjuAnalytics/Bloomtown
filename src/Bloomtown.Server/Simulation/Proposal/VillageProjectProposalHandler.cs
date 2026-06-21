using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Handles client requests to propose and list village project proposals.
/// </summary>
public sealed class VillageProjectProposalHandler
{
    private readonly VillageProjectProposalService _proposalService;

    public VillageProjectProposalHandler(VillageProjectProposalService proposalService)
    {
        _proposalService = proposalService;
    }

    public Task<ProjectProposalResponse> HandleAsync(uint playerEntityId, ProjectProposalRequest request)
    {
        return _proposalService.HandleAsync(playerEntityId, request);
    }
}