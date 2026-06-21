using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Leadership;

/// <summary>
/// Handles client requests for village leadership positions and elections.
/// </summary>
public sealed class VillagePositionHandler
{
    private readonly VillagePositionService _positionService;

    public VillagePositionHandler(VillagePositionService positionService)
    {
        _positionService = positionService;
    }

    public Task<VillagePositionResponse> HandleAsync(uint playerEntityId, VillagePositionRequest request)
    {
        return _positionService.HandleAsync(playerEntityId, request);
    }
}