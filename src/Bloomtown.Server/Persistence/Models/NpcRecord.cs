namespace Bloomtown.Server.Persistence.Models;

public sealed class NpcRecord
{
    public uint EntityId { get; init; }
    public required string Name { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public float PositionZ { get; init; }
    public float Hunger { get; init; } = NpcNeedsDefaults.Hunger;
    public float Energy { get; init; } = NpcNeedsDefaults.Energy;
    public float Social { get; init; } = NpcNeedsDefaults.Social;
}

/// <summary>Default need values for DB columns when no runtime state exists yet.</summary>
public static class NpcNeedsDefaults
{
    public const float Hunger = 35f;
    public const float Energy = 80f;
    public const float Social = 30f;
}