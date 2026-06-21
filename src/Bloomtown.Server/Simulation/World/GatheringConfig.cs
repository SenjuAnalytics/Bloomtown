namespace Bloomtown.Server.Simulation.World;

/// <summary>
/// Tunable gathering duration, range, yield, and node cooldown.
/// </summary>
public static class GatheringConfig
{
    /// <summary>Real-time seconds to complete one gather action.</summary>
    public const double GatherDurationRealSeconds = 3.0;

    /// <summary>Cooldown after a node is used, in game minutes (1 real sec = 1 game min).</summary>
    public const double NodeCooldownGameMinutes = 45.0;

    public const float GatherRadiusMeters = 6f;
    public const int DefaultYieldAmount = 2;
}