using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Ai;

/// <summary>
/// Legacy priority-based activity selection from Spike 7.
/// Superseded by <see cref="Utility.NpcUtilityScorer"/> in Spike 8; kept for reference and tests.
/// </summary>
public static class NpcActivityDecider
{
    public static NpcActivityType Decide(NpcNeeds needs, NpcDailySchedule schedule, int gameMinute)
    {
        if (needs.IsHungerCritical)
            return NpcActivityType.Eat;

        if (needs.IsEnergyCritical)
            return NpcActivityType.Rest;

        return schedule.GetActivityAt(gameMinute);
    }

    public static string DescribeReason(NpcActivityType activity, NpcNeeds needs, NpcDailySchedule schedule, int gameMinute)
    {
        return activity switch
        {
            NpcActivityType.Eat when needs.IsHungerCritical =>
                $"Hunger is high ({needs.Hunger:F0})",
            NpcActivityType.Rest when needs.IsEnergyCritical =>
                $"Energy is low ({needs.Energy:F0})",
            _ => $"schedule at {gameMinute / 60:D2}:{gameMinute % 60:D2}",
        };
    }
}