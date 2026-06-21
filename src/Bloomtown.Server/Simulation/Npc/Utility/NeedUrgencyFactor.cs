using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Scores activities by how well they address current Hunger and Energy levels.
/// </summary>
public sealed class NeedUrgencyFactor : IUtilityFactor
{
    public string Name => "NeedUrgency";

    public float Score(NpcActivityType activity, in UtilityEvaluationContext context)
    {
        var hunger = context.Needs.Hunger;
        var energy = context.Needs.Energy;

        return activity switch
        {
            NpcActivityType.Eat => ScaleHungerUrgency(hunger),
            NpcActivityType.Rest => ScaleEnergyUrgency(energy),
            NpcActivityType.Work => ScaleWorkViability(energy, hunger),
            NpcActivityType.Patrol => ScaleNeutralActivity(energy, hunger),
            NpcActivityType.Social => ScaleNeutralActivity(energy, hunger),
            _ => 0f,
        };
    }

    private static float ScaleHungerUrgency(float hunger)
    {
        // Hunger rises over time; higher hunger means Eat is more urgent.
        var normalized = (hunger - 20f) / (NpcNeedsConfig.HungerCriticalThreshold - 20f);
        return Clamp01(normalized) * 100f;
    }

    private static float ScaleEnergyUrgency(float energy)
    {
        // Lower energy means Rest is more urgent.
        var normalized = (NpcNeedsConfig.EnergyCriticalThreshold + 30f - energy) / 60f;
        return Clamp01(normalized) * 100f;
    }

    private static float ScaleWorkViability(float energy, float hunger)
    {
        if (hunger >= NpcNeedsConfig.HungerCriticalThreshold ||
            energy <= NpcNeedsConfig.EnergyCriticalThreshold)
        {
            return 15f;
        }

        return 35f + energy * 0.45f;
    }

    private static float ScaleNeutralActivity(float energy, float hunger)
    {
        if (hunger >= NpcNeedsConfig.HungerCriticalThreshold ||
            energy <= NpcNeedsConfig.EnergyCriticalThreshold)
        {
            return 20f;
        }

        return 55f;
    }

    private static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
}