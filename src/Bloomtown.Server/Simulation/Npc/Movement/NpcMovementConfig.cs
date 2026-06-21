namespace Bloomtown.Server.Simulation.Npc.Movement;

public static class NpcMovementConfig
{
    /// <summary>Default movement speed for NPCs without an explicit override (meters / second).</summary>
    public const float DefaultMoveSpeed = 2f;

    /// <summary>Distance at which an NPC is considered to have reached a waypoint.</summary>
    public const float ArrivalThreshold = 0.2f;

    /// <summary>Minimum interval between AOI position syncs per NPC.</summary>
    public const double AoiSyncIntervalSeconds = 0.5;
}