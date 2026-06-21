namespace Bloomtown.Server.Simulation.Npc.Interaction;

/// <summary>
/// Finds NPCs within interaction range of a world position.
/// </summary>
public static class NpcProximityDetector
{
    public static NpcSimulationState? FindNearestInRange(
        float worldX,
        float worldZ,
        IEnumerable<NpcSimulationState> npcs,
        float radiusMeters)
    {
        NpcSimulationState? nearest = null;
        var bestDistance = float.MaxValue;

        foreach (var simulation in npcs)
        {
            var distance = GetDistance(
                worldX,
                worldZ,
                simulation.Npc.PositionX,
                simulation.Npc.PositionZ);

            if (distance > radiusMeters || distance >= bestDistance)
                continue;

            bestDistance = distance;
            nearest = simulation;
        }

        return nearest;
    }

    public static bool IsWithinRange(
        float worldX,
        float worldZ,
        NpcSimulationState simulation,
        float radiusMeters)
    {
        return GetDistance(worldX, worldZ, simulation.Npc.PositionX, simulation.Npc.PositionZ) <= radiusMeters;
    }

    public static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}