namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Fixed home locations, access range, and sleep-at-home bonuses.
/// </summary>
public static class PlayerHousingConfig
{
    public const float AccessRadiusMeters = 8f;

    /// <summary>Energy restored by resting anywhere (weaker than home sleep).</summary>
    public const float RestEnergyRecovery = 40f;

    public const int RestDurationGameMinutes = 30;
    public const int HomeSleepDurationGameMinutes = 60;

    private const int HomeGridColumns = 4;
    private const float HomeOriginX = 12f;
    private const float HomeOriginZ = 18f;
    private const float HomeSpacing = 6f;

    /// <summary>
    /// Assigns each player a permanent home plot based on entity id (persisted on first connect).
    /// </summary>
    public static (float WorldX, float WorldZ) ComputeDefaultLocation(uint playerEntityId)
    {
        var index = Math.Max(0, (int)playerEntityId - 1);
        var row = index / HomeGridColumns;
        var col = index % HomeGridColumns;
        return (HomeOriginX + col * HomeSpacing, HomeOriginZ + row * HomeSpacing);
    }

    public static float GetDistance(float playerX, float playerZ, float houseX, float houseZ)
    {
        var dx = houseX - playerX;
        var dz = houseZ - playerZ;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public static bool IsWithinHome(float playerX, float playerZ, float houseX, float houseZ)
    {
        return GetDistance(playerX, playerZ, houseX, houseZ) <= AccessRadiusMeters;
    }
}