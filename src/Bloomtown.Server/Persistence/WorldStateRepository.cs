using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

public sealed class WorldStateRepository
{
    private readonly string _connectionString;

    public WorldStateRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<WorldStateRecord?> GetAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                game_day AS GameDay,
                game_minute AS GameMinute,
                last_saved AS LastSavedUtc,
                village_development_level AS VillageDevelopmentLevel
            FROM world_state
            WHERE id = 1
            """;

        var row = await connection.QuerySingleOrDefaultAsync<WorldStateRow>(sql);
        return row?.ToRecord();
    }

    public async Task UpsertAsync(WorldStateRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO world_state (id, game_day, game_minute, last_saved, village_development_level)
            VALUES (@Id, @GameDay, @GameMinute, @LastSavedUtc, @VillageDevelopmentLevel)
            ON CONFLICT(id) DO UPDATE SET
                game_day = excluded.game_day,
                game_minute = excluded.game_minute,
                last_saved = excluded.last_saved,
                village_development_level = excluded.village_development_level
            """;

        await connection.ExecuteAsync(sql, new
        {
            record.Id,
            record.GameDay,
            record.GameMinute,
            LastSavedUtc = record.LastSavedUtc.ToString("O"),
            VillageDevelopmentLevel = (int)record.VillageDevelopmentLevel,
        });
    }

    private sealed class WorldStateRow
    {
        public int Id { get; init; }
        public int GameDay { get; init; }
        public int GameMinute { get; init; }
        public string LastSavedUtc { get; init; } = string.Empty;

        public WorldStateRecord ToRecord()
        {
            return new WorldStateRecord
            {
                Id = Id,
                GameDay = GameDay,
                GameMinute = GameMinute,
                LastSavedUtc = DateTime.Parse(LastSavedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}