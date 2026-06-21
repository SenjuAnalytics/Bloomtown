using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Logs Chief direct approvals and vetoes for daily limit enforcement and auditing.
/// </summary>
public sealed class ChiefAuthorityLogRepository
{
    private readonly string _connectionString;

    public ChiefAuthorityLogRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InsertAsync(ChiefAuthorityLogRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            INSERT INTO chief_authority_log (chief_player_id, action_type, proposal_id, game_day, created_at)
            VALUES (@ChiefPlayerId, @ActionType, @ProposalId, @GameDay, @CreatedAtUtc)
            """,
            new
            {
                record.ChiefPlayerId,
                ActionType = (int)record.ActionType,
                record.ProposalId,
                record.GameDay,
                CreatedAtUtc = record.CreatedAtUtc.ToString("O"),
            });
    }

    public async Task<int> CountActionsOnGameDayAsync(ChiefAuthorityAction actionType, int gameDay)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(1)
            FROM chief_authority_log
            WHERE action_type = @ActionType AND game_day = @GameDay
            """,
            new { ActionType = (int)actionType, GameDay = gameDay });
    }

    public async Task<IReadOnlyList<ChiefAuthorityLogRecord>> GetRecentAsync(int limit = 10)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                chief_player_id AS ChiefPlayerId,
                action_type AS ActionType,
                proposal_id AS ProposalId,
                game_day AS GameDay,
                created_at AS CreatedAtUtc
            FROM chief_authority_log
            ORDER BY created_at DESC, id DESC
            LIMIT @Limit
            """;

        var rows = await connection.QueryAsync<LogRow>(sql, new { Limit = limit });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    private sealed class LogRow
    {
        public int Id { get; init; }
        public uint ChiefPlayerId { get; init; }
        public int ActionType { get; init; }
        public int ProposalId { get; init; }
        public int GameDay { get; init; }
        public string CreatedAtUtc { get; init; } = string.Empty;

        public ChiefAuthorityLogRecord ToRecord()
        {
            return new ChiefAuthorityLogRecord
            {
                Id = Id,
                ChiefPlayerId = ChiefPlayerId,
                ActionType = (ChiefAuthorityAction)ActionType,
                ProposalId = ProposalId,
                GameDay = GameDay,
                CreatedAtUtc = DateTime.Parse(CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}