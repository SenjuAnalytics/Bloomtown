using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Handles client requests to list and perform community-help activities.
/// </summary>
public sealed class CommunityActivityHandler
{
    private readonly CommunityActivityService _activityService;

    public CommunityActivityHandler(CommunityActivityService activityService)
    {
        _activityService = activityService;
    }

    public CommunityActivityResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        CommunityActivityRequest request) =>
        _activityService.Handle(playerEntityId, playerX, playerZ, request);
}