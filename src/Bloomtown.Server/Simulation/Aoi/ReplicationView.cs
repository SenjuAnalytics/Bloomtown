namespace Bloomtown.Server.Simulation.Aoi;

/// <summary>
/// Tracks which players currently have this entity inside their AOI subscription.
/// </summary>
public sealed class ReplicationView
{
    private readonly HashSet<uint> _viewingPlayers = new();

    public uint EntityId { get; }

    public IReadOnlyCollection<uint> ViewingPlayers => _viewingPlayers;

    public ReplicationView(uint entityId)
    {
        EntityId = entityId;
    }

    public void Clear()
    {
        _viewingPlayers.Clear();
    }

    public void AddViewer(uint playerEntityId)
    {
        _viewingPlayers.Add(playerEntityId);
    }

    public void RemoveViewer(uint playerEntityId)
    {
        _viewingPlayers.Remove(playerEntityId);
    }
}