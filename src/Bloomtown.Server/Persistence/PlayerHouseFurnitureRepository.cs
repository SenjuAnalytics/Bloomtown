using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Housing;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists furniture placed in each player's home.
/// </summary>
public sealed class PlayerHouseFurnitureRepository
{
    private readonly string _connectionString;

    public PlayerHouseFurnitureRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<PlayerHouseFurnitureRecord>> GetByPlayerAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                furniture_type AS FurnitureType,
                quantity AS Quantity
            FROM player_house_furniture
            WHERE player_entity_id = @PlayerEntityId
            ORDER BY furniture_type ASC
            """;

        var rows = await connection.QueryAsync<PlayerHouseFurnitureRecord>(sql, new { PlayerEntityId = playerEntityId });
        return rows.ToList();
    }

    public async Task ReplaceFurnitureAsync(uint playerEntityId, IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM player_house_furniture WHERE player_entity_id = @PlayerEntityId",
            new { PlayerEntityId = playerEntityId },
            transaction);

        foreach (var (furnitureType, quantity) in placedFurniture.OrderBy(pair => pair.Key))
        {
            if (quantity <= 0)
                continue;

            await connection.ExecuteAsync(
                """
                INSERT INTO player_house_furniture (player_entity_id, furniture_type, quantity)
                VALUES (@PlayerEntityId, @FurnitureType, @Quantity)
                """,
                new
                {
                    PlayerEntityId = playerEntityId,
                    FurnitureType = (int)furnitureType,
                    Quantity = quantity,
                },
                transaction);
        }

        await transaction.CommitAsync();
    }
}