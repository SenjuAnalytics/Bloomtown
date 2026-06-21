using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Milestone;

/// <summary>
/// Handles client requests to list milestones and interact with them.
/// </summary>
public sealed class MilestoneInteractionHandler
{
    private readonly VillageMilestoneService _milestoneService;

    public MilestoneInteractionHandler(VillageMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    public MilestoneResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        MilestoneRequest request)
    {
        return request.Kind switch
        {
            MilestoneRequestKind.List => HandleList(playerEntityId),
            MilestoneRequestKind.Interact => _milestoneService.Interact(
                playerEntityId,
                playerX,
                playerZ,
                request.Interaction),
            _ => new MilestoneResponse(
                false,
                request.Kind,
                MilestoneFailureReason.UnknownRequest,
                "Unknown milestone request."),
        };
    }

    private MilestoneResponse HandleList(uint playerEntityId)
    {
        var message = _milestoneService.FormatMilestoneList();
        Log.Information("Player {PlayerId} viewed village milestones.", playerEntityId);

        return new MilestoneResponse(
            true,
            MilestoneRequestKind.List,
            MilestoneFailureReason.None,
            message);
    }
}