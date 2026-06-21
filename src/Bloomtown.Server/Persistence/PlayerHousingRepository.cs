using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Housing;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists each player's fixed home location and improvement tier.
/// </summary>
public sealed class PlayerHousingRepository
{
    private readonly string _connectionString;

    public PlayerHousingRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<PlayerHousingRecord?> GetAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                house_x AS HouseX,
                house_z AS HouseZ,
                house_tier AS HouseTier
            FROM player_housing
            WHERE player_entity_id = @PlayerEntityId
            """;

        return await connection.QuerySingleOrDefaultAsync<PlayerHousingRecord>(
            sql,
            new { PlayerEntityId = playerEntityId });
    }

    public async Task UpsertAsync(PlayerHousingRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO player_housing (player_entity_id, house_x, house_z, house_tier)
            VALUES (@PlayerEntityId, @HouseX, @HouseZ, @HouseTier)
            ON CONFLICT(player_entity_id) DO UPDATE SET
                house_x = excluded.house_x,
                house_z = excluded.house_z,
                house_tier = excluded.house_tier
            """,
            new
            {
                record.PlayerEntityId,
                record.HouseX,
                record.HouseZ,
                HouseTier = (int)record.HouseTier,
            });
    }
}