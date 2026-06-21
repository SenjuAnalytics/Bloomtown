using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class CommunityReputationRepository
{
    private readonly string _connectionString;

    public CommunityReputationRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<PlayerCommunityReputationRecord?> GetAsync(uint playerEntityId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                player_entity_id AS PlayerEntityId,
                help_garden_count AS HelpGardenCount,
                help_market_count AS HelpMarketCount,
                help_well_count AS HelpWellCount
            FROM player_community_reputation
            WHERE player_entity_id = @PlayerEntityId
            """;

        return await connection.QuerySingleOrDefaultAsync<PlayerCommunityReputationRecord>(
            sql,
            new { PlayerEntityId = playerEntityId });
    }

    public async Task UpsertAsync(PlayerCommunityReputationRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO player_community_reputation (
                player_entity_id,
                help_garden_count,
                help_market_count,
                help_well_count)
            VALUES (
                @PlayerEntityId,
                @HelpGardenCount,
                @HelpMarketCount,
                @HelpWellCount)
            ON CONFLICT(player_entity_id) DO UPDATE SET
                help_garden_count = excluded.help_garden_count,
                help_market_count = excluded.help_market_count,
                help_well_count = excluded.help_well_count
            """;

        await connection.ExecuteAsync(sql, record);
    }
}