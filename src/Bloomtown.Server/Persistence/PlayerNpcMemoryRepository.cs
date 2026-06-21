using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Memory;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists player–NPC memory records.
/// </summary>
public sealed class PlayerNpcMemoryRepository
{
    private readonly string _connectionString;

    public PlayerNpcMemoryRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<PlayerNpcMemoryRecord>> GetByPlayerAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                npc_entity_id AS NpcEntityId,
                memory_type AS MemoryTypeId,
                occurred_at AS OccurredAtUtc
            FROM player_npc_memories
            WHERE player_entity_id = @PlayerEntityId
            ORDER BY occurred_at ASC
            """;

        var rows = await connection.QueryAsync<MemoryRow>(sql, new { PlayerEntityId = playerEntityId });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task InsertAsync(PlayerNpcMemoryRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT OR IGNORE INTO player_npc_memories (
                player_entity_id,
                npc_entity_id,
                memory_type,
                occurred_at)
            VALUES (
                @PlayerEntityId,
                @NpcEntityId,
                @MemoryType,
                @OccurredAtUtc)
            """;

        await connection.ExecuteAsync(sql, new
        {
            record.PlayerEntityId,
            record.NpcEntityId,
            MemoryType = (int)record.MemoryType,
            OccurredAtUtc = record.OccurredAtUtc.ToString("O"),
        });
    }

    private sealed class MemoryRow
    {
        public uint PlayerEntityId { get; init; }
        public uint NpcEntityId { get; init; }
        public int MemoryTypeId { get; init; }
        public string OccurredAtUtc { get; init; } = string.Empty;

        public PlayerNpcMemoryRecord ToRecord()
        {
            return new PlayerNpcMemoryRecord
            {
                PlayerEntityId = PlayerEntityId,
                NpcEntityId = NpcEntityId,
                MemoryType = (NpcMemoryType)MemoryTypeId,
                OccurredAtUtc = DateTime.Parse(OccurredAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}