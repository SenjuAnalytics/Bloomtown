using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Handles client requests to list and perform daily village leisure activities.
/// </summary>
public sealed class DailyVillageActivityHandler
{
    private readonly DailyVillageActivityService _activityService;

    public DailyVillageActivityHandler(DailyVillageActivityService activityService)
    {
        _activityService = activityService;
    }

    public DailyVillageActivityResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        DailyVillageActivityRequest request) =>
        _activityService.Handle(playerEntityId, playerX, playerZ, request);
}