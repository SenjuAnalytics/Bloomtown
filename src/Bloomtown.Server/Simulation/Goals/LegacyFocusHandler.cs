using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Goals;

/// <summary>
/// Handles conscious legacy-focus commands from the client console.
/// </summary>
public sealed class LegacyFocusHandler
{
    private const float NearbyNpcRadiusMeters = LegacyArchetypeFocusConfig.ConnectorNpcRadiusMeters;

    private readonly PlayerLongTermGoalService _longTermGoalService;
    private readonly NpcManager _npcManager;
    private readonly AoiSystem _aoiSystem;

    public LegacyFocusHandler(
        PlayerLongTermGoalService longTermGoalService,
        NpcManager npcManager,
        AoiSystem aoiSystem)
    {
        _longTermGoalService = longTermGoalService;
        _npcManager = npcManager;
        _aoiSystem = aoiSystem;
    }

    public LegacyFocusResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        LegacyFocusRequest request)
    {
        var hasNearbyNpc = HasNearbyNpc(playerEntityId, playerX, playerZ);
        var variationSeed = playerEntityId + (uint)request.Path + (uint)(playerX * 10) + (uint)(playerZ * 10);

        return _longTermGoalService
            .PerformConsciousFocusAsync(
                playerEntityId,
                playerX,
                playerZ,
                hasNearbyNpc,
                request,
                variationSeed)
            .GetAwaiter()
            .GetResult();
    }

    private bool HasNearbyNpc(uint playerEntityId, float playerX, float playerZ)
    {
        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (!_aoiSystem.IsEntityVisibleToPlayer(simulation.Npc.EntityId, playerEntityId))
                continue;

            var dx = playerX - simulation.Npc.PositionX;
            var dz = playerZ - simulation.Npc.PositionZ;
            if (dx * dx + dz * dz <= NearbyNpcRadiusMeters * NearbyNpcRadiusMeters)
                return true;
        }

        return false;
    }
}