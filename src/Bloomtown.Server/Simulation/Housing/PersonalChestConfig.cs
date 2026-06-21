namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Fixed world location for each player's personal storage chest.
/// </summary>
public static class PersonalChestConfig
{
    public const float WorldX = 5f;
    public const float WorldZ = 5f;
    public const float AccessRadiusMeters = 8f;

    public static float GetDistance(float playerX, float playerZ)
    {
        var dx = WorldX - playerX;
        var dz = WorldZ - playerZ;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public static bool IsWithinRange(float playerX, float playerZ)
    {
        return GetDistance(playerX, playerZ) <= AccessRadiusMeters;
    }
}