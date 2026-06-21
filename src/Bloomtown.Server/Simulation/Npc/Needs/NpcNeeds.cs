namespace Bloomtown.Server.Simulation.Npc.Needs;

/// <summary>
/// Basic NPC needs. Hunger rises over time; energy falls over time.
/// </summary>
public sealed class NpcNeeds
{
    public float Hunger { get; private set; } = NpcNeedsConfig.DefaultHunger;
    public float Energy { get; private set; } = NpcNeedsConfig.DefaultEnergy;
    public float Social { get; private set; } = NpcNeedsConfig.DefaultSocial;

    public float HungerValue => Hunger;
    public float EnergyValue => Energy;

    public bool IsHungerCritical => Hunger >= NpcNeedsConfig.HungerCriticalThreshold;
    public bool IsEnergyCritical => Energy <= NpcNeedsConfig.EnergyCriticalThreshold;
    public bool IsSocialNeedElevated => Social >= NpcNeedsConfig.SocialElevatedThreshold;

    public void Load(float hunger, float energy, float social = NpcNeedsConfig.DefaultSocial)
    {
        Hunger = Clamp(hunger);
        Energy = Clamp(energy);
        Social = Clamp(social);
    }

    public void Decay(double deltaGameMinutes)
    {
        if (deltaGameMinutes <= 0)
            return;

        Hunger += NpcNeedsConfig.HungerDecayPerGameMinute * (float)deltaGameMinutes;
        Energy -= NpcNeedsConfig.EnergyDecayPerGameMinute * (float)deltaGameMinutes;
        Social += NpcNeedsConfig.SocialRisePerGameMinute * (float)deltaGameMinutes;

        Hunger = Clamp(Hunger);
        Energy = Clamp(Energy);
        Social = Clamp(Social);
    }

    public void SatisfyHunger(float amount)
    {
        Hunger = Clamp(Hunger - amount);
    }

    public void RestoreEnergy(float amount)
    {
        Energy = Clamp(Energy + amount);
    }

    public void SatisfySocial(float amount)
    {
        Social = Clamp(Social - amount);
    }

    private static float Clamp(float value)
    {
        return Math.Clamp(value, NpcNeedsConfig.MinValue, NpcNeedsConfig.MaxValue);
    }
}