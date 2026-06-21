using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Scores how well an activity matches the NPC's daily schedule at the current game time.
/// </summary>
public sealed class ScheduleFitFactor : IUtilityFactor
{
    public string Name => "ScheduleFit";

    public float Score(NpcActivityType activity, in UtilityEvaluationContext context)
    {
        return context.Schedule.GetScheduleFit(activity, context.GameMinute);
    }
}