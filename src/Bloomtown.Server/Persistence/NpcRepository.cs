using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class NpcRepository
{
    private readonly string _connectionString;

    public NpcRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<NpcRecord>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                entity_id AS EntityId,
                name AS Name,
                position_x AS PositionX,
                position_y AS PositionY,
                position_z AS PositionZ,
                hunger AS Hunger,
                energy AS Energy,
                social AS Social
            FROM npcs
            ORDER BY entity_id
            """;

        var rows = await connection.QueryAsync<NpcRecord>(sql);
        return rows.ToList();
    }

    public async Task UpsertAsync(NpcRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO npcs (
                entity_id, name, position_x, position_y, position_z, hunger, energy, social)
            VALUES (
                @EntityId, @Name, @PositionX, @PositionY, @PositionZ, @Hunger, @Energy, @Social)
            ON CONFLICT(entity_id) DO UPDATE SET
                name = excluded.name,
                position_x = excluded.position_x,
                position_y = excluded.position_y,
                position_z = excluded.position_z,
                hunger = excluded.hunger,
                energy = excluded.energy,
                social = excluded.social
            """;

        await connection.ExecuteAsync(sql, record);
    }
}