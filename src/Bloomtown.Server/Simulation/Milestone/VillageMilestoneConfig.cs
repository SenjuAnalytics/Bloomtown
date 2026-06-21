namespace Bloomtown.Server.Simulation.Milestone;

/// <summary>
/// Tunable values for milestone interactions and player energy.
/// </summary>
public static class VillageMilestoneConfig
{
    public const float MaxPlayerEnergy = 100f;
    public const float DefaultPlayerEnergy = 100f;
    public const float WellDrinkEnergyRestore = 15f;
    public const float BridgeCrossEnergyRestore = 8f;
    public const int WarehouseStipendCoins = 5;

    public const float InteractionRadiusMeters = 10f;

    public static readonly TimeSpan WellDrinkCooldown = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan WarehouseStipendCooldown = TimeSpan.FromMinutes(5);
}