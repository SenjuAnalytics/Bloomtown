namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Server-side NPC state. Position is advanced by <see cref="Movement.NpcMovementController"/>.
/// </summary>
public sealed class StaticNpc
{
    public required uint EntityId { get; init; }
    public required string Name { get; init; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationYaw { get; set; }
}