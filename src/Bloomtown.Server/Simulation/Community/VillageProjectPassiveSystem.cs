using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Community;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Applies passive village-project benefits each tick (e.g. bridge-area fatigue relief).
/// </summary>
public sealed class VillageProjectPassiveSystem : ISimulationSystem
{
    private readonly VillageProjectStateService _projectState;
    private readonly PlayerNeedsService _needsService;
    private readonly Func<IReadOnlyList<(uint EntityId, float X, float Z)>> _getPlayerPositions;

    public VillageProjectPassiveSystem(
        VillageProjectStateService projectState,
        PlayerNeedsService needsService,
        Func<IReadOnlyList<(uint EntityId, float X, float Z)>> getPlayerPositions)
    {
        _projectState = projectState;
        _needsService = needsService;
        _getPlayerPositions = getPlayerPositions;
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        if (!_projectState.IsCompleted(VillageProjectBenefitConfig.BridgeProjectId))
            return;

        foreach (var (playerEntityId, playerX, playerZ) in _getPlayerPositions())
        {
            if (!_projectState.IsNearBridge(playerX, playerZ))
                continue;

            // Passive benefit: gentle fatigue relief while lingering near the repaired bridge.
            _needsService.ApplyBridgePassiveRelief(
                playerEntityId,
                VillageProjectBenefitConfig.BridgePassiveFatigueReliefPerSecond * (float)deltaTimeSeconds);
        }
    }
}