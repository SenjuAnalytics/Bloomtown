using Bloomtown.Server.Simulation.Npc.Movement;
using Bloomtown.Server.Simulation.Npc.Schedule;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Npc.Ai;

/// <summary>
/// World anchors and patrol paths used by schedule/needs decisions.
/// </summary>
public static class NpcActivityLocations
{
    public static Waypoint GetDestination(uint entityId, NpcActivityType activity)
    {
        return entityId switch
        {
            NpcEntityIds.Elsie => GetElsieDestination(activity),
            NpcEntityIds.Tom => GetTomDestination(activity),
            NpcEntityIds.Mira => GetMiraDestination(activity),
            NpcEntityIds.Harold => GetHaroldDestination(activity),
            NpcEntityIds.Greta => GetGretaDestination(activity),
            NpcEntityIds.Nora => GetNoraDestination(activity),
            NpcEntityIds.Elias => GetEliasDestination(activity),
            NpcEntityIds.Marcus => GetMarcusDestination(activity),
            NpcEntityIds.Ben => GetBenDestination(activity),
            NpcEntityIds.Lila => GetLilaDestination(activity),
            NpcEntityIds.Rowan => GetRowanDestination(activity),
            NpcEntityIds.Eleanor => GetEleanorDestination(activity),
            _ => new Waypoint(0f, 0f),
        };
    }

    public static WaypointPath GetActivityPath(uint entityId, NpcActivityType activity)
    {
        if (activity == NpcActivityType.Patrol)
            return GetPatrolPath(entityId);

        var center = GetDestination(entityId, activity);
        var (radiusX, radiusZ, segments) = activity switch
        {
            NpcActivityType.Work   => (2.0f, 2.0f, 6),
            NpcActivityType.Social => (2.5f, 2.5f, 8),
            NpcActivityType.Eat    => (1.5f, 1.5f, 6),
            NpcActivityType.Rest   => (1.2f, 1.2f, 6),
            _                      => (2.0f, 2.0f, 6),
        };

        return CreateCircleLoop(center.X, center.Z, radiusX, radiusZ, segments);
    }

    public static WaypointPath GetPatrolPath(uint entityId)
    {
        return entityId switch
        {
            NpcEntityIds.Elsie   => CreateElsiePatrol(),
            NpcEntityIds.Tom     => CreateTomPatrol(),
            NpcEntityIds.Mira    => CreateMiraPatrol(),
            NpcEntityIds.Harold  => CreateHaroldPatrol(),
            NpcEntityIds.Greta   => CreateGretaPatrol(),
            NpcEntityIds.Nora    => CreateNoraPatrol(),
            NpcEntityIds.Elias   => CreateEliasPatrol(),
            NpcEntityIds.Marcus  => CreateMarcusPatrol(),
            NpcEntityIds.Ben     => CreateBenPatrol(),
            NpcEntityIds.Lila    => CreateLilaPatrol(),
            NpcEntityIds.Rowan   => CreateRowanPatrol(),
            NpcEntityIds.Eleanor => CreateEleanorPatrol(),
            _ => CreateCircleLoop(14f, 12f, 3f, 3f),
        };
    }

    /// <summary>Loop tertutup — NPC berjalan mutar terus tanpa berhenti.</summary>
    public static WaypointPath CreateCircleLoop(
        float centerX,
        float centerZ,
        float radiusX,
        float radiusZ,
        int segments = 8)
    {
        segments = Math.Clamp(segments, 4, 16);
        var points = new Waypoint[segments];

        for (var i = 0; i < segments; i++)
        {
            var angle = MathF.PI * 2f * i / segments;
            points[i] = new Waypoint(
                centerX + MathF.Cos(angle) * radiusX,
                centerZ + MathF.Sin(angle) * radiusZ);
        }

        return new WaypointPath(points, pingPong: false);
    }

    private static Waypoint GetElsieDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(20f, 20f),
            NpcActivityType.Eat => new Waypoint(12f, 12f),
            NpcActivityType.Rest => new Waypoint(8f, 8f),
            NpcActivityType.Patrol => new Waypoint(16f, 16f),
            NpcActivityType.Social => new Waypoint(18f, 14f),
            _ => new Waypoint(8f, 8f),
        };
    }

    private static Waypoint GetTomDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(115f, 115f),
            NpcActivityType.Eat => new Waypoint(105f, 105f),
            NpcActivityType.Rest => new Waypoint(100f, 100f),
            NpcActivityType.Patrol => new Waypoint(110f, 110f),
            NpcActivityType.Social => new Waypoint(108f, 112f),
            _ => new Waypoint(100f, 100f),
        };
    }

    private static WaypointPath CreateElsiePatrol() =>
        CreateCircleLoop(16f, 16f, 4f, 4f, 8);

    private static WaypointPath CreateTomPatrol() =>
        CreateCircleLoop(107.5f, 107.5f, 7.5f, 7.5f, 8);

    private static Waypoint GetMiraDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(18f, 6f),
            NpcActivityType.Eat => new Waypoint(16f, 8f),
            NpcActivityType.Rest => new Waypoint(20f, 4f),
            NpcActivityType.Patrol => new Waypoint(18f, 10f),
            NpcActivityType.Social => new Waypoint(18f, 6f),
            _ => new Waypoint(18f, 6f),
        };
    }

    private static Waypoint GetHaroldDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(14f, 10f),
            NpcActivityType.Eat => new Waypoint(12f, 12f),
            NpcActivityType.Rest => new Waypoint(14f, 10f),
            NpcActivityType.Patrol => new Waypoint(16f, 12f),
            NpcActivityType.Social => new Waypoint(14f, 10f),
            _ => new Waypoint(14f, 10f),
        };
    }

    private static WaypointPath CreateMiraPatrol() =>
        CreateCircleLoop(18f, 8f, 2.5f, 2.5f, 8);

    private static WaypointPath CreateHaroldPatrol() =>
        CreateCircleLoop(14f, 12f, 3f, 3f, 8);

    private static Waypoint GetGretaDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(22f, 16f),
            NpcActivityType.Eat => new Waypoint(20f, 18f),
            NpcActivityType.Rest => new Waypoint(24f, 14f),
            NpcActivityType.Patrol => new Waypoint(22f, 16f),
            NpcActivityType.Social => new Waypoint(22f, 16f),
            _ => new Waypoint(22f, 16f),
        };
    }

    private static WaypointPath CreateGretaPatrol() =>
        CreateCircleLoop(22f, 16f, 2.5f, 2.5f, 8);

    private static Waypoint GetNoraDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(16f, 18f),
            NpcActivityType.Eat => new Waypoint(14f, 16f),
            NpcActivityType.Rest => new Waypoint(16f, 20f),
            NpcActivityType.Patrol => new Waypoint(18f, 18f),
            NpcActivityType.Social => new Waypoint(16f, 18f),
            _ => new Waypoint(16f, 18f),
        };
    }

    private static WaypointPath CreateNoraPatrol() =>
        CreateCircleLoop(16f, 18f, 2.5f, 2.5f, 8);

    private static Waypoint GetEliasDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(12f, 14f),
            NpcActivityType.Eat => new Waypoint(10f, 12f),
            NpcActivityType.Rest => new Waypoint(12f, 16f),
            NpcActivityType.Patrol => new Waypoint(14f, 14f),
            NpcActivityType.Social => new Waypoint(12f, 14f),
            _ => new Waypoint(12f, 14f),
        };
    }

    private static WaypointPath CreateEliasPatrol() =>
        CreateCircleLoop(12f, 14f, 2.5f, 2.5f, 8);

    private static Waypoint GetMarcusDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(10f, 16f),
            NpcActivityType.Eat => new Waypoint(8f, 14f),
            NpcActivityType.Rest => new Waypoint(10f, 18f),
            NpcActivityType.Patrol => new Waypoint(12f, 16f),
            NpcActivityType.Social => new Waypoint(10f, 16f),
            _ => new Waypoint(10f, 16f),
        };
    }

    private static WaypointPath CreateMarcusPatrol() =>
        CreateCircleLoop(10f, 16f, 2.5f, 2.5f, 8);

    private static Waypoint GetBenDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(15f, 11f),
            NpcActivityType.Eat => new Waypoint(14f, 12f),
            NpcActivityType.Rest => new Waypoint(15f, 13f),
            NpcActivityType.Patrol => new Waypoint(17f, 11f),
            NpcActivityType.Social => new Waypoint(15f, 11f),
            _ => new Waypoint(15f, 11f),
        };
    }

    private static WaypointPath CreateBenPatrol() =>
        CreateCircleLoop(15f, 12f, 2.5f, 2.5f, 8);

    private static Waypoint GetLilaDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(19f, 10f),
            NpcActivityType.Eat => new Waypoint(18f, 12f),
            NpcActivityType.Rest => new Waypoint(20f, 12f),
            NpcActivityType.Patrol => new Waypoint(19f, 8f),
            NpcActivityType.Social => new Waypoint(18f, 6f),
            _ => new Waypoint(19f, 10f),
        };
    }

    private static WaypointPath CreateLilaPatrol() =>
        CreateCircleLoop(18f, 10f, 2.5f, 2.5f, 8);

    private static Waypoint GetRowanDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(11f, 9f),
            NpcActivityType.Eat => new Waypoint(12f, 10f),
            NpcActivityType.Rest => new Waypoint(10f, 10f),
            NpcActivityType.Patrol => new Waypoint(11f, 7f),
            NpcActivityType.Social => new Waypoint(11f, 9f),
            _ => new Waypoint(11f, 9f),
        };
    }

    private static WaypointPath CreateRowanPatrol() =>
        CreateCircleLoop(11f, 9f, 2.5f, 2.5f, 8);

    private static Waypoint GetEleanorDestination(NpcActivityType activity)
    {
        return activity switch
        {
            NpcActivityType.Work => new Waypoint(13f, 12f),
            NpcActivityType.Eat => new Waypoint(14f, 11f),
            NpcActivityType.Rest => new Waypoint(12f, 13f),
            NpcActivityType.Patrol => new Waypoint(13f, 10f),
            NpcActivityType.Social => new Waypoint(13f, 12f),
            _ => new Waypoint(13f, 12f),
        };
    }

    private static WaypointPath CreateEleanorPatrol() =>
        CreateCircleLoop(13f, 12f, 2.5f, 2.5f, 8);
}