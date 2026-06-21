using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Leadership;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists village leadership elections.
/// </summary>
public sealed class PositionElectionRepository
{
    private readonly string _connectionString;

    public PositionElectionRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<int> InsertAsync(PositionElectionRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO village_position_elections (
                position, candidate_player_id, status, voting_end_total_minutes, created_at
            )
            VALUES (
                @Position, @CandidatePlayerId, @Status, @VotingEndTotalMinutes, @CreatedAtUtc
            );
            SELECT last_insert_rowid();
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            Position = (int)record.Position,
            record.CandidatePlayerId,
            Status = (int)record.Status,
            record.VotingEndTotalMinutes,
            CreatedAtUtc = record.CreatedAtUtc.ToString("O"),
        });
    }

    public async Task UpdateStatusAsync(int electionId, PositionElectionStatus status)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE village_position_elections
            SET status = @Status
            WHERE id = @ElectionId
            """,
            new { ElectionId = electionId, Status = (int)status });
    }

    public async Task<PositionElectionRecord?> GetActiveByPositionAsync(VillagePosition position)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                position AS Position,
                candidate_player_id AS CandidatePlayerId,
                status AS Status,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_position_elections
            WHERE position = @Position AND status = @VotingStatus
            ORDER BY created_at DESC, id DESC
            LIMIT 1
            """;

        var row = await connection.QuerySingleOrDefaultAsync<ElectionRow>(sql, new
        {
            Position = (int)position,
            VotingStatus = (int)PositionElectionStatus.Voting,
        });

        return row?.ToRecord();
    }

    public async Task<PositionElectionRecord?> GetByIdAsync(int electionId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                position AS Position,
                candidate_player_id AS CandidatePlayerId,
                status AS Status,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_position_elections
            WHERE id = @ElectionId
            """;

        var row = await connection.QuerySingleOrDefaultAsync<ElectionRow>(sql, new { ElectionId = electionId });
        return row?.ToRecord();
    }

    public async Task<IReadOnlyList<PositionElectionRecord>> GetExpiredVotingAsync(long currentTotalMinutes)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                position AS Position,
                candidate_player_id AS CandidatePlayerId,
                status AS Status,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_position_elections
            WHERE status = @VotingStatus
              AND voting_end_total_minutes <= @CurrentTotalMinutes
            ORDER BY created_at ASC, id ASC
            """;

        var rows = await connection.QueryAsync<ElectionRow>(sql, new
        {
            VotingStatus = (int)PositionElectionStatus.Voting,
            CurrentTotalMinutes = currentTotalMinutes,
        });

        return rows.Select(row => row.ToRecord()).ToList();
    }

    private sealed class ElectionRow
    {
        public int Id { get; init; }
        public int Position { get; init; }
        public uint CandidatePlayerId { get; init; }
        public int Status { get; init; }
        public long VotingEndTotalMinutes { get; init; }
        public string CreatedAtUtc { get; init; } = string.Empty;

        public PositionElectionRecord ToRecord()
        {
            return new PositionElectionRecord
            {
                Id = Id,
                Position = (VillagePosition)Position,
                CandidatePlayerId = CandidatePlayerId,
                Status = (PositionElectionStatus)Status,
                VotingEndTotalMinutes = VotingEndTotalMinutes,
                CreatedAtUtc = DateTime.Parse(CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}