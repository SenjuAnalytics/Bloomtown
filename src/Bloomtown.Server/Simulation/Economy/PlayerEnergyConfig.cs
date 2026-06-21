using Bloomtown.Server.Simulation.Milestone;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Tunable player energy decay, rest recovery, and low-energy penalties.
/// Mirrors the simplicity of <see cref="Npc.Needs.NpcNeedsConfig"/> for players.
/// </summary>
public static class PlayerEnergyConfig
{
    public const float MinValue = 0f;
    public const float MaxValue = VillageMilestoneConfig.MaxPlayerEnergy;
    public const float DefaultEnergy = VillageMilestoneConfig.DefaultPlayerEnergy;

    /// <summary>Base energy lost per game minute while awake.</summary>
    public const float EnergyDecayPerGameMinute = 0.4f;

    /// <summary>Energy at or below this value triggers penalties and warnings.</summary>
    public const float LowEnergyThreshold = 30f;

    /// <summary>Extra decay multiplier when already tired (creates a downward spiral).</summary>
    public const float LowEnergyDecayMultiplier = 1.5f;

    /// <summary>Gathering takes longer when energy is low (1.5 = 50% slower).</summary>
    public const double LowEnergyGatherDurationMultiplier = 1.5;

    /// <summary>Energy restored by the rest/sleep command (anywhere for this spike).</summary>
    public const float RestEnergyRecovery = 40f;

    /// <summary>Flavor text for how long the rest break lasts in game time.</summary>
    public const int RestDurationGameMinutes = 30;

    public static bool IsLowEnergy(float energy) => energy <= LowEnergyThreshold;

    /// <summary>
    /// Decay rate scales up when the player is already below the low-energy threshold.
    /// </summary>
    public static float GetDecayPerGameMinute(float energy)
    {
        var rate = EnergyDecayPerGameMinute;
        if (IsLowEnergy(energy))
            rate *= LowEnergyDecayMultiplier;

        return rate;
    }

    /// <summary>
    /// Low energy slows gathering actions to make fatigue feel tangible.
    /// </summary>
    public static double GetGatherDurationMultiplier(float energy)
    {
        return IsLowEnergy(energy) ? LowEnergyGatherDurationMultiplier : 1.0;
    }

    public static float Clamp(float value)
    {
        return Math.Clamp(value, MinValue, MaxValue);
    }

    public static string FormatStatusLabel(float energy)
    {
        if (IsLowEnergy(energy))
            return "low — gathering is slower, energy drains faster";

        if (energy >= MaxValue - 0.5f)
            return "refreshed";

        return "okay";
    }
}