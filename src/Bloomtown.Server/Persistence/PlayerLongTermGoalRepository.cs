using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class PlayerLongTermGoalRepository
{
    private readonly string _connectionString;

    public PlayerLongTermGoalRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<PlayerLongTermGoalRecord?> GetAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                goal_kind AS GoalKind,
                highest_completed_milestone AS HighestCompletedMilestone,
                goal_completed_at AS GoalCompletedAtUtc,
                legacy_archetype AS LegacyArchetype,
                builder_influence AS BuilderInfluence,
                caretaker_influence AS CaretakerInfluence,
                connector_influence AS ConnectorInfluence,
                legacy_focus AS LegacyFocus
            FROM player_long_term_goal
            WHERE player_entity_id = @PlayerEntityId
            """;

        return await connection.QuerySingleOrDefaultAsync<PlayerLongTermGoalRecord>(
            sql,
            new { PlayerEntityId = playerEntityId });
    }

    public async Task UpsertAsync(PlayerLongTermGoalRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO player_long_term_goal (
                player_entity_id,
                goal_kind,
                highest_completed_milestone,
                goal_completed_at,
                legacy_archetype,
                builder_influence,
                caretaker_influence,
                connector_influence,
                legacy_focus)
            VALUES (
                @PlayerEntityId,
                @GoalKind,
                @HighestCompletedMilestone,
                @GoalCompletedAtUtc,
                @LegacyArchetype,
                @BuilderInfluence,
                @CaretakerInfluence,
                @ConnectorInfluence,
                @LegacyFocus)
            ON CONFLICT(player_entity_id) DO UPDATE SET
                goal_kind = excluded.goal_kind,
                highest_completed_milestone = excluded.highest_completed_milestone,
                goal_completed_at = excluded.goal_completed_at,
                legacy_archetype = excluded.legacy_archetype,
                builder_influence = excluded.builder_influence,
                caretaker_influence = excluded.caretaker_influence,
                connector_influence = excluded.connector_influence,
                legacy_focus = excluded.legacy_focus
            """;

        await connection.ExecuteAsync(sql, new
        {
            record.PlayerEntityId,
            record.GoalKind,
            record.HighestCompletedMilestone,
            GoalCompletedAtUtc = record.GoalCompletedAtUtc?.ToString("O"),
            record.LegacyArchetype,
            record.BuilderInfluence,
            record.CaretakerInfluence,
            record.ConnectorInfluence,
            record.LegacyFocus,
        });
    }
}