using Bloomtown.Server.Simulation.Npc.Ai;
using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Prefers activities whose destinations are closer to the NPC's current position.
/// </summary>
public sealed class DistanceFactor : IUtilityFactor
{
    public string Name => "Distance";

    public float Score(NpcActivityType activity, in UtilityEvaluationContext context)
    {
        var distance = GetDistanceToActivity(context.EntityId, activity, context.PositionX, context.PositionZ);
        var normalized = 1f - Math.Clamp(distance / UtilityScoringConfig.MaxDistanceForScoring, 0f, 1f);
        return normalized * 100f;
    }

    private static float GetDistanceToActivity(uint entityId, NpcActivityType activity, float posX, float posZ)
    {
        if (activity == NpcActivityType.Patrol)
            return GetDistanceToNearestPatrolWaypoint(entityId, posX, posZ);

        var destination = NpcActivityLocations.GetDestination(entityId, activity);
        return Distance(posX, posZ, destination.X, destination.Z);
    }

    private static float GetDistanceToNearestPatrolWaypoint(uint entityId, float posX, float posZ)
    {
        var path = NpcActivityLocations.GetPatrolPath(entityId);
        var best = float.MaxValue;

        foreach (var point in path.Points)
        {
            var distance = Distance(posX, posZ, point.X, point.Z);
            if (distance < best)
                best = distance;
        }

        return best;
    }

    private static float Distance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}