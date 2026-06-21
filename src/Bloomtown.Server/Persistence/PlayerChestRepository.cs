using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Items;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists per-player personal chest item stacks.
/// </summary>
public sealed class PlayerChestRepository
{
    private readonly string _connectionString;

    public PlayerChestRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<PlayerChestEntryRecord>> GetByPlayerAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                item_type AS ItemType,
                quantity AS Quantity
            FROM player_chest
            WHERE player_entity_id = @PlayerEntityId
            ORDER BY item_type
            """;

        var rows = await connection.QueryAsync<PlayerChestEntryRecord>(sql, new { PlayerEntityId = playerEntityId });
        return rows.ToList();
    }

    public async Task ReplaceChestAsync(uint playerEntityId, IEnumerable<ItemStack> stacks)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM player_chest WHERE player_entity_id = @PlayerEntityId",
            new { PlayerEntityId = playerEntityId },
            transaction);

        const string insertSql = """
            INSERT INTO player_chest (player_entity_id, item_type, quantity)
            VALUES (@PlayerEntityId, @ItemType, @Quantity)
            """;

        foreach (var stack in stacks.Where(stack => stack.Quantity > 0))
        {
            await connection.ExecuteAsync(
                insertSql,
                new
                {
                    PlayerEntityId = playerEntityId,
                    ItemType = (int)stack.ItemType,
                    stack.Quantity,
                },
                transaction);
        }

        await transaction.CommitAsync();
    }
}