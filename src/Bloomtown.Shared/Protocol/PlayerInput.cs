namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Client → server movement and look input for one simulation step.
/// </summary>
public readonly struct PlayerInput
{
    public uint Seq { get; init; }
    public float MoveDirX { get; init; }
    public float MoveDirY { get; init; }
    public float LookYaw { get; init; }
}