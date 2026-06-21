using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Community;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists village project proposals submitted by players.
/// </summary>
public sealed class VillageProjectProposalRepository
{
    private readonly string _connectionString;

    public VillageProjectProposalRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<int> InsertAsync(VillageProjectProposalRecord record)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            INSERT INTO village_project_proposals (
                proposed_by_player_id,
                project_name,
                project_slug,
                required_resources_json,
                status,
                project_tier,
                created_project_id,
                voting_end_total_minutes,
                created_at
            )
            VALUES (
                @ProposedByPlayerId,
                @ProjectName,
                @ProjectSlug,
                @RequiredResourcesJson,
                @Status,
                @ProjectTier,
                @CreatedProjectId,
                @VotingEndTotalMinutes,
                @CreatedAtUtc
            );
            SELECT last_insert_rowid();
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            record.ProposedByPlayerId,
            record.ProjectName,
            record.ProjectSlug,
            record.RequiredResourcesJson,
            Status = (int)record.Status,
            ProjectTier = (int)record.ProjectTier,
            record.CreatedProjectId,
            record.VotingEndTotalMinutes,
            CreatedAtUtc = record.CreatedAtUtc.ToString("O"),
        });
    }

    public async Task UpdateStatusAsync(int proposalId, ProposalStatus status, byte? createdProjectId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE village_project_proposals
            SET status = @Status, created_project_id = @CreatedProjectId
            WHERE id = @ProposalId
            """,
            new
            {
                ProposalId = proposalId,
                Status = (int)status,
                CreatedProjectId = createdProjectId,
            });
    }

    public async Task UpdateVotingStateAsync(int proposalId, ProposalStatus status, long votingEndTotalMinutes)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            """
            UPDATE village_project_proposals
            SET status = @Status, voting_end_total_minutes = @VotingEndTotalMinutes
            WHERE id = @ProposalId
            """,
            new
            {
                ProposalId = proposalId,
                Status = (int)status,
                VotingEndTotalMinutes = votingEndTotalMinutes,
            });
    }

    public async Task<VillageProjectProposalRecord?> GetByIdAsync(int proposalId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE id = @ProposalId
            """;

        var row = await connection.QuerySingleOrDefaultAsync<ProposalRow>(sql, new { ProposalId = proposalId });
        return row?.ToRecord();
    }

    public async Task<VillageProjectProposalRecord?> GetByProjectNameAsync(string projectName)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE LOWER(project_name) = LOWER(@ProjectName)
            ORDER BY created_at DESC, id DESC
            LIMIT 1
            """;

        var row = await connection.QuerySingleOrDefaultAsync<ProposalRow>(sql, new { ProjectName = projectName.Trim() });
        return row?.ToRecord();
    }

    public async Task<IReadOnlyList<VillageProjectProposalRecord>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            ORDER BY created_at DESC, id DESC
            """;

        var rows = await connection.QueryAsync<ProposalRow>(sql);
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<VillageProjectProposalRecord>> GetVotingAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE status = @VotingStatus
            ORDER BY created_at ASC, id ASC
            """;

        var rows = await connection.QueryAsync<ProposalRow>(sql, new { VotingStatus = (int)ProposalStatus.Voting });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<VillageProjectProposalRecord>> GetCouncilVotingAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE status = @CouncilVotingStatus
            ORDER BY created_at ASC, id ASC
            """;

        var rows = await connection.QueryAsync<ProposalRow>(sql, new { CouncilVotingStatus = (int)ProposalStatus.CouncilVoting });
        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<VillageProjectProposalRecord>> GetExpiredCouncilVotingAsync(long currentTotalMinutes)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE status = @CouncilVotingStatus
              AND voting_end_total_minutes IS NOT NULL
              AND voting_end_total_minutes <= @CurrentTotalMinutes
            ORDER BY created_at ASC, id ASC
            """;

        var rows = await connection.QueryAsync<ProposalRow>(sql, new
        {
            CouncilVotingStatus = (int)ProposalStatus.CouncilVoting,
            CurrentTotalMinutes = currentTotalMinutes,
        });

        return rows.Select(row => row.ToRecord()).ToList();
    }

    public async Task<IReadOnlyList<VillageProjectProposalRecord>> GetExpiredVotingAsync(long currentTotalMinutes)
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                id AS Id,
                proposed_by_player_id AS ProposedByPlayerId,
                project_name AS ProjectName,
                project_slug AS ProjectSlug,
                required_resources_json AS RequiredResourcesJson,
                status AS Status,
                project_tier AS ProjectTier,
                created_project_id AS CreatedProjectId,
                voting_end_total_minutes AS VotingEndTotalMinutes,
                created_at AS CreatedAtUtc
            FROM village_project_proposals
            WHERE status = @VotingStatus
              AND voting_end_total_minutes IS NOT NULL
              AND voting_end_total_minutes <= @CurrentTotalMinutes
            ORDER BY created_at ASC, id ASC
            """;

        var rows = await connection.QueryAsync<ProposalRow>(sql, new
        {
            VotingStatus = (int)ProposalStatus.Voting,
            CurrentTotalMinutes = currentTotalMinutes,
        });

        return rows.Select(row => row.ToRecord()).ToList();
    }

    private sealed class ProposalRow
    {
        public int Id { get; init; }
        public uint ProposedByPlayerId { get; init; }
        public string ProjectName { get; init; } = string.Empty;
        public string ProjectSlug { get; init; } = string.Empty;
        public string RequiredResourcesJson { get; init; } = string.Empty;
        public int Status { get; init; }
        public int ProjectTier { get; init; } = 1;
        public byte? CreatedProjectId { get; init; }
        public long? VotingEndTotalMinutes { get; init; }
        public string CreatedAtUtc { get; init; } = string.Empty;

        public VillageProjectProposalRecord ToRecord()
        {
            return new VillageProjectProposalRecord
            {
                Id = Id,
                ProposedByPlayerId = ProposedByPlayerId,
                ProjectName = ProjectName,
                ProjectSlug = ProjectSlug,
                RequiredResourcesJson = RequiredResourcesJson,
                Status = (ProposalStatus)Status,
                ProjectTier = (ProjectImportanceTier)ProjectTier,
                CreatedProjectId = CreatedProjectId,
                VotingEndTotalMinutes = VotingEndTotalMinutes,
                CreatedAtUtc = DateTime.Parse(CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            };
        }
    }
}