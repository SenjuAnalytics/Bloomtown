namespace Bloomtown.Server.Simulation.Aoi;

/// <summary>
/// Sparse world grid that maps world positions to AOI cells.
/// </summary>
public sealed class AoiGrid
{
    private readonly Dictionary<CellCoord, AoiCell> _cells = new();

    public CellCoord WorldToCell(float worldX, float worldZ)
    {
        var cellX = (int)MathF.Floor(worldX / AoiConstants.CellSize);
        var cellZ = (int)MathF.Floor(worldZ / AoiConstants.CellSize);
        return new CellCoord(cellX, cellZ);
    }

    public AoiCell GetOrCreateCell(CellCoord coord)
    {
        if (!_cells.TryGetValue(coord, out var cell))
        {
            cell = new AoiCell(coord);
            _cells[coord] = cell;
        }

        return cell;
    }

    public AoiCell? TryGetCell(CellCoord coord)
    {
        return _cells.GetValueOrDefault(coord);
    }

    public IEnumerable<CellCoord> GetCellsInRadius(CellCoord center, int radius)
    {
        for (var offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (var offsetZ = -radius; offsetZ <= radius; offsetZ++)
                yield return new CellCoord(center.X + offsetX, center.Z + offsetZ);
        }
    }
}