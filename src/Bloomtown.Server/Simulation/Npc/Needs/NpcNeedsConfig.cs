namespace Bloomtown.Server.Simulation.Npc.Needs;

public static class NpcNeedsConfig
{
    public const float MinValue = 0f;
    public const float MaxValue = 100f;

    public const float DefaultHunger = 35f;
    public const float DefaultEnergy = 80f;
    public const float DefaultSocial = 30f;

    /// <summary>Hunger at or above this value triggers Eat activity.</summary>
    public const float HungerCriticalThreshold = 70f;

    /// <summary>Energy at or below this value triggers Rest activity.</summary>
    public const float EnergyCriticalThreshold = 30f;

    public const float HungerDecayPerGameMinute = 0.8f;
    public const float EnergyDecayPerGameMinute = 0.5f;

    public const float EatHungerReduction = 45f;
    public const float RestEnergyRecovery = 40f;

    /// <summary>Social need at or above this value is considered elevated.</summary>
    public const float SocialElevatedThreshold = 55f;

    public const float SocialRisePerGameMinute = 0.35f;
    public const float GreetSocialReduction = 8f;
    public const float TalkSocialReduction = 15f;
}