using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Movement;

/// <summary>
/// Moves one NPC using either a patrol path or a single go-to destination.
/// </summary>
public sealed class NpcMovementController
{
    private WaypointPath? _patrolPath;
    private int _targetIndex;
    private int _direction = 1;
    private Waypoint? _goToTarget;
    private bool _useGoToMode;

    public float MoveSpeed { get; }

    public NpcMovementController(float moveSpeed = NpcMovementConfig.DefaultMoveSpeed)
    {
        MoveSpeed = moveSpeed;
    }

    public NpcActivityType? ActiveGoToActivity { get; private set; }

    public void SetPatrol(NpcActivityType activity, WaypointPath path)
    {
        _patrolPath = path;
        _useGoToMode = false;
        _goToTarget = null;
        ActiveGoToActivity = null;
        _targetIndex = 0;
        _direction = 1;
    }

    public void SetGoTo(NpcActivityType activity, Waypoint destination)
    {
        _goToTarget = destination;
        _useGoToMode = true;
        ActiveGoToActivity = activity;
    }

    public bool Update(StaticNpc npc, double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return false;

        if (_useGoToMode && _goToTarget.HasValue)
            return MoveTowards(npc, _goToTarget.Value, deltaTimeSeconds, advancePatrolOnArrival: false);

        if (_patrolPath is null)
            return false;

        var target = _patrolPath.Points[_targetIndex];
        return MoveTowards(npc, target, deltaTimeSeconds, advancePatrolOnArrival: true);
    }

    public void SyncTargetToNearestWaypoint(float worldX, float worldZ)
    {
        if (_patrolPath is null)
            return;

        _targetIndex = FindNearestWaypointIndex(worldX, worldZ);
        _direction = 1;
    }

    private bool MoveTowards(
        StaticNpc npc,
        Waypoint target,
        double deltaTimeSeconds,
        bool advancePatrolOnArrival)
    {
        var deltaX = target.X - npc.PositionX;
        var deltaZ = target.Z - npc.PositionZ;
        var distanceToTarget = MathF.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

        if (distanceToTarget <= NpcMovementConfig.ArrivalThreshold)
        {
            npc.PositionX = target.X;
            npc.PositionZ = target.Z;

            if (advancePatrolOnArrival)
                AdvancePatrolTarget();

            return true;
        }

        var step = MoveSpeed * (float)deltaTimeSeconds;
        if (step >= distanceToTarget)
        {
            npc.PositionX = target.X;
            npc.PositionZ = target.Z;

            if (advancePatrolOnArrival)
                AdvancePatrolTarget();

            return true;
        }

        var invDistance = 1f / distanceToTarget;
        npc.PositionX += deltaX * invDistance * step;
        npc.PositionZ += deltaZ * invDistance * step;
        npc.RotationYaw = MathF.Atan2(deltaX, deltaZ);

        return false;
    }

    private int FindNearestWaypointIndex(float worldX, float worldZ)
    {
        if (_patrolPath is null)
            return 0;

        var bestIndex = 0;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < _patrolPath.Points.Count; i++)
        {
            var point = _patrolPath.Points[i];
            var dx = point.X - worldX;
            var dz = point.Z - worldZ;
            var distanceSq = dx * dx + dz * dz;

            if (distanceSq >= bestDistance)
                continue;

            bestDistance = distanceSq;
            bestIndex = i;
        }

        return bestIndex;
    }

    private void AdvancePatrolTarget()
    {
        if (_patrolPath is null)
            return;

        if (_patrolPath.PingPong)
        {
            var nextIndex = _targetIndex + _direction;
            if (nextIndex >= _patrolPath.Points.Count)
            {
                _direction = -1;
                _targetIndex = Math.Max(0, _patrolPath.Points.Count - 2);
                return;
            }

            if (nextIndex < 0)
            {
                _direction = 1;
                _targetIndex = Math.Min(1, _patrolPath.Points.Count - 1);
                return;
            }

            _targetIndex = nextIndex;
            return;
        }

        _targetIndex = (_targetIndex + 1) % _patrolPath.Points.Count;
    }
}