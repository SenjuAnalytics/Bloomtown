using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// One scoring dimension that contributes to an activity's utility (0–100).
/// </summary>
public interface IUtilityFactor
{
    string Name { get; }

    float Score(NpcActivityType activity, in UtilityEvaluationContext context);
}