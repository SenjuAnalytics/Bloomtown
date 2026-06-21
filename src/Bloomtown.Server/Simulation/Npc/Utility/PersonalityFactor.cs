using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Applies per-NPC personality biases to activity preferences.
/// </summary>
public sealed class PersonalityFactor : IUtilityFactor
{
    public string Name => "Personality";

    public float Score(NpcActivityType activity, in UtilityEvaluationContext context)
    {
        return context.Personality switch
        {
            NpcPersonalityTrait.Diligent => ScoreDiligent(activity),
            NpcPersonalityTrait.Social => ScoreSocial(activity),
            NpcPersonalityTrait.Lazy => ScoreLazy(activity),
            _ => 50f,
        };
    }

    private static float ScoreDiligent(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => 95f,
            NpcActivityType.Patrol => 70f,
            NpcActivityType.Eat => 55f,
            NpcActivityType.Social => 45f,
            NpcActivityType.Rest => 25f,
            _ => 50f,
        };
    }

    private static float ScoreSocial(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Social => 95f,
            NpcActivityType.Patrol => 75f,
            NpcActivityType.Eat => 60f,
            NpcActivityType.Work => 40f,
            NpcActivityType.Rest => 50f,
            _ => 50f,
        };
    }

    private static float ScoreLazy(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Rest => 95f,
            NpcActivityType.Eat => 70f,
            NpcActivityType.Social => 60f,
            NpcActivityType.Patrol => 35f,
            NpcActivityType.Work => 20f,
            _ => 50f,
        };
    }
}