namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Server → client position update for a single entity.
/// </summary>
public readonly struct EntityDelta
{
    public uint EntityId { get; init; }
    public uint Seq { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public float PositionZ { get; init; }
    public float RotationYaw { get; init; }
}