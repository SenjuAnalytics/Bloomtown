namespace Bloomtown.Server.Simulation.Aoi;

/// <summary>
/// One grid cell containing entities and players subscribed to it.
/// </summary>
public sealed class AoiCell
{
    private readonly HashSet<uint> _entities = new();
    private readonly HashSet<uint> _subscribedPlayers = new();

    public CellCoord Coord { get; }

    public IReadOnlyCollection<uint> Entities => _entities;
    public IReadOnlyCollection<uint> SubscribedPlayers => _subscribedPlayers;

    public AoiCell(CellCoord coord)
    {
        Coord = coord;
    }

    public void AddEntity(uint entityId)
    {
        _entities.Add(entityId);
    }

    public void RemoveEntity(uint entityId)
    {
        _entities.Remove(entityId);
    }

    public void AddSubscriber(uint playerEntityId)
    {
        _subscribedPlayers.Add(playerEntityId);
    }

    public void RemoveSubscriber(uint playerEntityId)
    {
        _subscribedPlayers.Remove(playerEntityId);
    }
}