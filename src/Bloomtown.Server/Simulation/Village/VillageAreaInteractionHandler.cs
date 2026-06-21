using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Handles client requests to list village areas and interact with them.
/// </summary>
public sealed class VillageAreaInteractionHandler
{
    private readonly VillageAreaService _areaService;

    public VillageAreaInteractionHandler(VillageAreaService areaService)
    {
        _areaService = areaService;
    }

    public VillageAreaResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        VillageAreaRequest request)
    {
        return request.Kind switch
        {
            VillageAreaRequestKind.List => HandleList(playerEntityId),
            VillageAreaRequestKind.Interact => _areaService.Interact(
                playerEntityId,
                playerX,
                playerZ,
                request.Interaction),
            _ => new VillageAreaResponse(
                false,
                request.Kind,
                VillageAreaFailureReason.UnknownRequest,
                "Unknown village area request."),
        };
    }

    private VillageAreaResponse HandleList(uint playerEntityId)
    {
        var message = _areaService.FormatAreaList();
        Log.Information("Player {PlayerId} viewed village areas.", playerEntityId);

        return new VillageAreaResponse(
            true,
            VillageAreaRequestKind.List,
            VillageAreaFailureReason.None,
            message);
    }
}