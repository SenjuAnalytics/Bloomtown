namespace Bloomtown.Server.Simulation.Aoi;

/// <summary>
/// Stores the 3x3 AOI cell neighborhood currently subscribed by one player.
/// </summary>
public sealed class AoiSubscription
{
    private readonly HashSet<CellCoord> _subscribedCells = new();

    public uint PlayerEntityId { get; }

    public int CenterCellX { get; private set; }
    public int CenterCellZ { get; private set; }

    public IReadOnlyCollection<CellCoord> SubscribedCells => _subscribedCells;

    public AoiSubscription(uint playerEntityId)
    {
        PlayerEntityId = playerEntityId;
        CenterCellX = int.MinValue;
        CenterCellZ = int.MinValue;
    }

    public void SetSubscription(int centerCellX, int centerCellZ, IEnumerable<CellCoord> cells)
    {
        CenterCellX = centerCellX;
        CenterCellZ = centerCellZ;

        _subscribedCells.Clear();
        foreach (var cell in cells)
            _subscribedCells.Add(cell);
    }
}