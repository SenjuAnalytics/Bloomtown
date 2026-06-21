using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Handles client requests to list projects and contribute resources.
/// </summary>
public sealed class CommunityProjectHandler
{
    private readonly CommunityProjectService _projectService;

    public CommunityProjectHandler(CommunityProjectService projectService)
    {
        _projectService = projectService;
    }

    public CommunityProjectResponse Handle(uint playerEntityId, CommunityProjectRequest request)
    {
        return request.Kind switch
        {
            CommunityProjectRequestKind.List => HandleList(playerEntityId),
            CommunityProjectRequestKind.Contribute => _projectService.Contribute(
                playerEntityId,
                request.ProjectId,
                request.ItemType,
                request.Quantity),
            _ => new CommunityProjectResponse(
                false,
                request.Kind,
                CommunityProjectFailureReason.UnknownRequest,
                "Unknown community project request."),
        };
    }

    private CommunityProjectResponse HandleList(uint playerEntityId)
    {
        var message = _projectService.FormatProjectList();
        Log.Information("Player {PlayerId} viewed community projects.", playerEntityId);

        return new CommunityProjectResponse(
            true,
            CommunityProjectRequestKind.List,
            CommunityProjectFailureReason.None,
            message);
    }
}