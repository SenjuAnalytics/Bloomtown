using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Items;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class PlayerInventoryRepository
{
    private readonly string _connectionString;

    public PlayerInventoryRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<PlayerInventoryEntryRecord>> GetByPlayerAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                item_type AS ItemType,
                quantity AS Quantity
            FROM player_inventory
            WHERE player_entity_id = @PlayerEntityId
            ORDER BY item_type
            """;

        var rows = await connection.QueryAsync<PlayerInventoryEntryRecord>(sql, new { PlayerEntityId = playerEntityId });
        return rows.ToList();
    }

    public async Task ReplaceInventoryAsync(uint playerEntityId, IEnumerable<ItemStack> stacks)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM player_inventory WHERE player_entity_id = @PlayerEntityId",
            new { PlayerEntityId = playerEntityId },
            transaction);

        const string insertSql = """
            INSERT INTO player_inventory (player_entity_id, item_type, quantity)
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