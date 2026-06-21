using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Milestone;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists unlocked village milestones (world progression state).
/// </summary>
public sealed class VillageMilestoneRepository
{
    private readonly string _connectionString;

    public VillageMilestoneRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<VillageMilestoneRecord>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT milestone_id AS MilestoneId, unlocked_at AS UnlockedAtUtc
            FROM village_milestones
            ORDER BY milestone_id
            """;

        var rows = await connection.QueryAsync<VillageMilestoneRow>(sql);
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task UpsertAsync(VillageMilestone milestoneId, DateTime unlockedAtUtc)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO village_milestones (milestone_id, unlocked_at)
            VALUES (@MilestoneId, @UnlockedAtUtc)
            ON CONFLICT(milestone_id) DO UPDATE SET unlocked_at = excluded.unlocked_at
            """;

        await connection.ExecuteAsync(sql, new
        {
            MilestoneId = (int)milestoneId,
            UnlockedAtUtc = unlockedAtUtc.ToString("O"),
        });
    }

    private sealed class VillageMilestoneRow
    {
        public int MilestoneId { get; init; }
        public string UnlockedAtUtc { get; init; } = string.Empty;

        public VillageMilestoneRecord ToRecord()
        {
            return new VillageMilestoneRecord
            {
                MilestoneId = (VillageMilestone)MilestoneId,
                UnlockedAtUtc = DateTime.Parse(UnlockedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}