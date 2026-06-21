namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Basic player hunger — rises over game time (higher = hungrier).
/// Mirrors <see cref="Npc.Needs.NpcNeedsConfig"/> hunger semantics for players.
/// </summary>
public static class PlayerHungerConfig
{
    public const float MinValue = 0f;
    public const float MaxValue = 100f;
    public const float DefaultHunger = 20f;

    /// <summary>Hunger gained per game minute while awake.</summary>
    public const float HungerRisePerGameMinute = 0.5f;

    /// <summary>At or above this value the player feels noticeably hungry.</summary>
    public const float HighHungerThreshold = 70f;

    public static bool IsHungry(float hunger) => hunger >= HighHungerThreshold;

    public static float Clamp(float value)
    {
        return Math.Clamp(value, MinValue, MaxValue);
    }

    public static string FormatStatusLabel(float hunger)
    {
        if (IsHungry(hunger))
            return "hungry";

        if (hunger <= DefaultHunger + 5f)
            return "satisfied";

        return "okay";
    }
}