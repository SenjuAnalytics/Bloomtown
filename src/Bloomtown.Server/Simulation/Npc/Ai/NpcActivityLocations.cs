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

    public static WaypointPath GetPatrolPath(uint entityId)
    {
        return entityId switch
        {
            NpcEntityIds.Elsie => CreateElsiePatrol(),
            NpcEntityIds.Tom => CreateTomPatrol(),
            NpcEntityIds.Mira => CreateMiraPatrol(),
            NpcEntityIds.Harold => CreateHaroldPatrol(),
            NpcEntityIds.Greta => CreateGretaPatrol(),
            NpcEntityIds.Nora => CreateNoraPatrol(),
            NpcEntityIds.Elias => CreateEliasPatrol(),
            NpcEntityIds.Marcus => CreateMarcusPatrol(),
            NpcEntityIds.Ben => CreateBenPatrol(),
            NpcEntityIds.Lila => CreateLilaPatrol(),
            NpcEntityIds.Rowan => CreateRowanPatrol(),
            NpcEntityIds.Eleanor => CreateEleanorPatrol(),
            _ => new WaypointPath([new Waypoint(0f, 0f), new Waypoint(1f, 1f)]),
        };
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

    private static WaypointPath CreateElsiePatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(12f, 12f),
            new Waypoint(20f, 12f),
            new Waypoint(20f, 20f),
            new Waypoint(12f, 20f),
        ],
            pingPong: true);
    }

    private static WaypointPath CreateTomPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(100f, 100f),
            new Waypoint(115f, 100f),
            new Waypoint(115f, 115f),
            new Waypoint(100f, 115f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateMiraPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(16f, 6f),
            new Waypoint(20f, 6f),
            new Waypoint(20f, 10f),
            new Waypoint(16f, 10f),
        ],
            pingPong: true);
    }

    private static WaypointPath CreateHaroldPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(12f, 10f),
            new Waypoint(16f, 10f),
            new Waypoint(16f, 14f),
            new Waypoint(12f, 14f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateGretaPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(20f, 14f),
            new Waypoint(24f, 14f),
            new Waypoint(24f, 18f),
            new Waypoint(20f, 18f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateNoraPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(14f, 16f),
            new Waypoint(18f, 16f),
            new Waypoint(18f, 20f),
            new Waypoint(14f, 20f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateEliasPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(10f, 12f),
            new Waypoint(14f, 12f),
            new Waypoint(14f, 16f),
            new Waypoint(10f, 16f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateMarcusPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(8f, 14f),
            new Waypoint(12f, 14f),
            new Waypoint(12f, 18f),
            new Waypoint(8f, 18f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateBenPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(13f, 10f),
            new Waypoint(17f, 10f),
            new Waypoint(17f, 14f),
            new Waypoint(13f, 14f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateLilaPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(16f, 8f),
            new Waypoint(20f, 8f),
            new Waypoint(20f, 12f),
            new Waypoint(16f, 12f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateRowanPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(9f, 7f),
            new Waypoint(13f, 7f),
            new Waypoint(13f, 11f),
            new Waypoint(9f, 11f),
        ],
            pingPong: true);
    }

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

    private static WaypointPath CreateEleanorPatrol()
    {
        return new WaypointPath(
        [
            new Waypoint(11f, 10f),
            new Waypoint(15f, 10f),
            new Waypoint(15f, 14f),
            new Waypoint(11f, 14f),
        ],
            pingPong: true);
    }
}