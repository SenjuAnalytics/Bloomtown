using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class PlayerMilestoneRepository
{
    private readonly string _connectionString;

    public PlayerMilestoneRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<PlayerMilestoneRecord?> GetAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                completed_bitmask AS CompletedBitmask,
                rhythm_agency_days AS RhythmAgencyDays,
                daily_activity_count AS DailyActivityCount,
                daily_activity_days AS DailyActivityDays
            FROM player_personal_milestone
            WHERE player_entity_id = @PlayerEntityId
            """;

        return await connection.QuerySingleOrDefaultAsync<PlayerMilestoneRecord>(
            sql,
            new { PlayerEntityId = playerEntityId });
    }

    public async Task UpsertAsync(PlayerMilestoneRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO player_personal_milestone (
                player_entity_id,
                completed_bitmask,
                rhythm_agency_days,
                daily_activity_count,
                daily_activity_days)
            VALUES (
                @PlayerEntityId,
                @CompletedBitmask,
                @RhythmAgencyDays,
                @DailyActivityCount,
                @DailyActivityDays)
            ON CONFLICT(player_entity_id) DO UPDATE SET
                completed_bitmask = excluded.completed_bitmask,
                rhythm_agency_days = excluded.rhythm_agency_days,
                daily_activity_count = excluded.daily_activity_count,
                daily_activity_days = excluded.daily_activity_days
            """;

        await connection.ExecuteAsync(sql, record);
    }
}