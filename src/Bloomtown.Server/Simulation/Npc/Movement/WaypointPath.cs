namespace Bloomtown.Server.Simulation.Npc.Movement;

/// <summary>
/// Ordered list of waypoints. Supports looping or ping-pong traversal.
/// </summary>
public sealed class WaypointPath
{
    public WaypointPath(IEnumerable<Waypoint> points, bool pingPong = true)
    {
        var pointList = points.ToList();
        if (pointList.Count < 2)
            throw new ArgumentException("A movement path requires at least two waypoints.", nameof(points));

        Points = pointList;
        PingPong = pingPong;
    }

    public IReadOnlyList<Waypoint> Points { get; }

    public bool PingPong { get; }
}