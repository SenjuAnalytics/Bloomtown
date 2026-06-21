using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists which village areas have been unlocked.
/// </summary>
public sealed class VillageAreaRepository
{
    private readonly string _connectionString;

    public VillageAreaRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<VillageAreaRecord>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT area_id AS AreaId, unlocked_at AS UnlockedAtUtc
            FROM unlocked_village_areas
            ORDER BY area_id
            """;

        var rows = await connection.QueryAsync<VillageAreaRow>(sql);
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task UpsertAsync(byte areaId, DateTime unlockedAtUtc)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO unlocked_village_areas (area_id, unlocked_at)
            VALUES (@AreaId, @UnlockedAtUtc)
            ON CONFLICT(area_id) DO UPDATE SET unlocked_at = excluded.unlocked_at
            """;

        await connection.ExecuteAsync(sql, new
        {
            AreaId = areaId,
            UnlockedAtUtc = unlockedAtUtc.ToString("O"),
        });
    }

    private sealed class VillageAreaRow
    {
        public byte AreaId { get; init; }
        public string UnlockedAtUtc { get; init; } = string.Empty;

        public VillageAreaRecord ToRecord() => new()
        {
            AreaId = AreaId,
            UnlockedAtUtc = DateTime.Parse(UnlockedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
    }
}