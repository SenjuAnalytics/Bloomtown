namespace Bloomtown.Server.Simulation.Aoi;

public static class AoiConstants
{
    /// <summary>World-space size of one AOI cell in meters.</summary>
    public const float CellSize = 32f;

    /// <summary>
    /// Subscription radius in cells. A value of 1 yields a 3x3 neighborhood
    /// (center cell plus one ring in each direction).
    /// </summary>
    public const int SubscriptionRadius = 1;
}