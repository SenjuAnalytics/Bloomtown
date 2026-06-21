namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Server → client handshake that assigns the local player entity id and spawn point.
/// </summary>
public readonly struct ConnectAccept
{
    public uint EntityId { get; init; }
    public float SpawnX { get; init; }
    public float SpawnY { get; init; }
    public float SpawnZ { get; init; }
}