using Serilog;

namespace Bloomtown.Server.Simulation.Aoi;

/// <summary>
/// Maintains AOI cell membership, player subscriptions, and replication visibility.
/// </summary>
public sealed class AoiSystem : ISimulationSystem
{
    private readonly AoiGrid _grid = new();
    private readonly Dictionary<uint, TrackedEntity> _entities = new();

    public void RegisterEntity(uint entityId, EntityKind kind, float worldX, float worldZ)
    {
        if (_entities.ContainsKey(entityId))
            throw new InvalidOperationException($"Entity {entityId} is already registered.");

        var cell = _grid.WorldToCell(worldX, worldZ);
        var record = new TrackedEntity
        {
            EntityId = entityId,
            Kind = kind,
            WorldX = worldX,
            WorldZ = worldZ,
            Cell = cell,
            ReplicationView = new ReplicationView(entityId),
            Subscription = kind == EntityKind.Player ? new AoiSubscription(entityId) : null,
        };

        _entities[entityId] = record;
        _grid.GetOrCreateCell(cell).AddEntity(entityId);

        if (record.Subscription is not null)
            RefreshPlayerSubscription(record);
    }

    public void RemoveEntity(uint entityId)
    {
        if (!_entities.Remove(entityId, out var record))
            return;

        _grid.TryGetCell(record.Cell)?.RemoveEntity(entityId);

        if (record.Subscription is not null)
        {
            foreach (var subscribedCell in record.Subscription.SubscribedCells)
                _grid.TryGetCell(subscribedCell)?.RemoveSubscriber(entityId);
        }
    }

    public void SetEntityPosition(uint entityId, float worldX, float worldZ)
    {
        if (!_entities.TryGetValue(entityId, out var record))
            return;

        record.WorldX = worldX;
        record.WorldZ = worldZ;
    }

    public ReplicationView? GetReplicationView(uint entityId)
    {
        return _entities.TryGetValue(entityId, out var record) ? record.ReplicationView : null;
    }

    /// <summary>
    /// Returns true when the player currently has this entity inside their AOI subscription.
    /// </summary>
    public bool IsEntityVisibleToPlayer(uint entityId, uint playerEntityId)
    {
        var replicationView = GetReplicationView(entityId);
        return replicationView is not null && replicationView.ViewingPlayers.Contains(playerEntityId);
    }

    /// <summary>
    /// Returns true when a world position lies inside the player's subscribed AOI cells.
    /// </summary>
    public bool IsPositionVisibleToPlayer(uint playerEntityId, float worldX, float worldZ)
    {
        if (!_entities.TryGetValue(playerEntityId, out var player) || player.Subscription is null)
            return false;

        var targetCell = _grid.WorldToCell(worldX, worldZ);
        return player.Subscription.SubscribedCells.Contains(targetCell);
    }

    public void Update(double deltaTimeSeconds)
    {
        UpdateEntityCells();
        UpdatePlayerSubscriptions();
        UpdateReplicationViews();
    }

    private void UpdateEntityCells()
    {
        foreach (var record in _entities.Values)
        {
            var newCell = _grid.WorldToCell(record.WorldX, record.WorldZ);
            if (newCell == record.Cell)
                continue;

            _grid.TryGetCell(record.Cell)?.RemoveEntity(record.EntityId);
            _grid.GetOrCreateCell(newCell).AddEntity(record.EntityId);
            record.Cell = newCell;
        }
    }

    private void UpdatePlayerSubscriptions()
    {
        foreach (var record in _entities.Values)
        {
            if (record.Subscription is null)
                continue;

            RefreshPlayerSubscription(record);
        }
    }

    private void RefreshPlayerSubscription(TrackedEntity player)
    {
        var subscription = player.Subscription!;
        var centerCell = _grid.WorldToCell(player.WorldX, player.WorldZ);

        if (centerCell.X == subscription.CenterCellX && centerCell.Z == subscription.CenterCellZ)
            return;

        var desiredCells = _grid
            .GetCellsInRadius(centerCell, AoiConstants.SubscriptionRadius)
            .ToHashSet();

        foreach (var oldCell in subscription.SubscribedCells)
        {
            if (desiredCells.Contains(oldCell))
                continue;

            _grid.TryGetCell(oldCell)?.RemoveSubscriber(player.EntityId);
        }

        foreach (var newCell in desiredCells)
        {
            if (subscription.SubscribedCells.Contains(newCell))
                continue;

            _grid.GetOrCreateCell(newCell).AddSubscriber(player.EntityId);
        }

        subscription.SetSubscription(centerCell.X, centerCell.Z, desiredCells);

        Log.Information(
            "AOI player {PlayerId} subscription updated: center cell ({CellX},{CellZ}), subscribed cells={CellCount}",
            player.EntityId,
            centerCell.X,
            centerCell.Z,
            desiredCells.Count);
    }

    private void UpdateReplicationViews()
    {
        foreach (var record in _entities.Values)
        {
            record.ReplicationView.Clear();

            var cell = _grid.TryGetCell(record.Cell);
            if (cell is null)
                continue;

            foreach (var viewerId in cell.SubscribedPlayers)
                record.ReplicationView.AddViewer(viewerId);
        }
    }

    private sealed class TrackedEntity
    {
        public required uint EntityId { get; init; }
        public required EntityKind Kind { get; init; }
        public float WorldX { get; set; }
        public float WorldZ { get; set; }
        public CellCoord Cell { get; set; }
        public required ReplicationView ReplicationView { get; init; }
        public AoiSubscription? Subscription { get; init; }
    }
}