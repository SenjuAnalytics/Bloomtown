using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists player–NPC affinity and last interaction timestamps.
/// </summary>
public sealed class PlayerNpcRelationshipRepository
{
    private readonly string _connectionString;

    public PlayerNpcRelationshipRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<PlayerNpcRelationshipRecord>> GetByPlayerAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                npc_entity_id AS NpcEntityId,
                affinity_value AS AffinityValue,
                last_interaction_game_day AS LastInteractionGameDay,
                last_interaction AS LastInteractionUtc
            FROM player_npc_relationship
            WHERE player_entity_id = @PlayerEntityId
            ORDER BY affinity_value DESC, npc_entity_id ASC
            """;

        var rows = await connection.QueryAsync<RelationshipRow>(sql, new { PlayerEntityId = playerEntityId });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task UpsertAsync(PlayerNpcRelationshipRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO player_npc_relationship (
                player_entity_id,
                npc_entity_id,
                affinity_value,
                last_interaction_game_day,
                last_interaction)
            VALUES (
                @PlayerEntityId,
                @NpcEntityId,
                @AffinityValue,
                @LastInteractionGameDay,
                @LastInteractionUtc)
            ON CONFLICT(player_entity_id, npc_entity_id) DO UPDATE SET
                affinity_value = excluded.affinity_value,
                last_interaction_game_day = excluded.last_interaction_game_day,
                last_interaction = excluded.last_interaction
            """;

        await connection.ExecuteAsync(sql, new
        {
            record.PlayerEntityId,
            record.NpcEntityId,
            record.AffinityValue,
            record.LastInteractionGameDay,
            LastInteractionUtc = record.LastInteractionUtc.ToString("O"),
        });
    }

    private sealed class RelationshipRow
    {
        public uint PlayerEntityId { get; init; }
        public uint NpcEntityId { get; init; }
        public int AffinityValue { get; init; }
        public int LastInteractionGameDay { get; init; }
        public string LastInteractionUtc { get; init; } = string.Empty;

        public PlayerNpcRelationshipRecord ToRecord()
        {
            return new PlayerNpcRelationshipRecord
            {
                PlayerEntityId = PlayerEntityId,
                NpcEntityId = NpcEntityId,
                AffinityValue = AffinityValue,
                LastInteractionGameDay = LastInteractionGameDay,
                LastInteractionUtc = DateTime.Parse(LastInteractionUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}