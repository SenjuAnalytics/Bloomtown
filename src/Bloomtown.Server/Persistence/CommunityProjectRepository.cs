using Bloomtown.Server.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists communal project status, progress, and contributor totals.
/// </summary>
public sealed class CommunityProjectRepository
{
    private readonly string _connectionString;

    public CommunityProjectRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<CommunityProjectStatusRecord>> GetAllStatusAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                project_id AS ProjectId,
                status AS Status,
                completed_at AS CompletedAtUtc
            FROM community_project_status
            ORDER BY project_id
            """;

        var rows = await connection.QueryAsync<CommunityProjectStatusRecord>(sql);
        return rows.ToList();
    }

    public async Task<IReadOnlyList<CommunityProjectProgressRecord>> GetAllProgressAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                project_id AS ProjectId,
                item_type AS ItemType,
                current_quantity AS CurrentQuantity
            FROM community_project_progress
            ORDER BY project_id, item_type
            """;

        var rows = await connection.QueryAsync<CommunityProjectProgressRecord>(sql);
        return rows.ToList();
    }

    public async Task<IReadOnlyList<CommunityProjectContributorRecord>> GetAllContributorsAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                project_id AS ProjectId,
                player_entity_id AS PlayerEntityId,
                total_contributed AS TotalContributed
            FROM community_project_contributors
            ORDER BY project_id, player_entity_id
            """;

        var rows = await connection.QueryAsync<CommunityProjectContributorRecord>(sql);
        return rows.ToList();
    }

    public async Task SaveProjectAsync(
        byte projectId,
        int status,
        IEnumerable<(int ItemType, int CurrentQuantity)> progress,
        IEnumerable<(uint PlayerEntityId, int TotalContributed)> contributors,
        DateTime? completedAtUtc = null)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            """
            INSERT INTO community_project_status (project_id, status, completed_at)
            VALUES (@ProjectId, @Status, @CompletedAtUtc)
            ON CONFLICT(project_id) DO UPDATE SET
                status = excluded.status,
                completed_at = COALESCE(excluded.completed_at, community_project_status.completed_at)
            """,
            new { ProjectId = projectId, Status = status, CompletedAtUtc = completedAtUtc?.ToString("O") },
            transaction);

        await connection.ExecuteAsync(
            "DELETE FROM community_project_progress WHERE project_id = @ProjectId",
            new { ProjectId = projectId },
            transaction);

        const string progressSql = """
            INSERT INTO community_project_progress (project_id, item_type, current_quantity)
            VALUES (@ProjectId, @ItemType, @CurrentQuantity)
            """;

        foreach (var (itemType, currentQuantity) in progress.Where(entry => entry.CurrentQuantity > 0))
        {
            await connection.ExecuteAsync(
                progressSql,
                new { ProjectId = projectId, ItemType = itemType, CurrentQuantity = currentQuantity },
                transaction);
        }

        await connection.ExecuteAsync(
            "DELETE FROM community_project_contributors WHERE project_id = @ProjectId",
            new { ProjectId = projectId },
            transaction);

        const string contributorSql = """
            INSERT INTO community_project_contributors (project_id, player_entity_id, total_contributed)
            VALUES (@ProjectId, @PlayerEntityId, @TotalContributed)
            """;

        foreach (var (playerEntityId, totalContributed) in contributors.Where(entry => entry.TotalContributed > 0))
        {
            await connection.ExecuteAsync(
                contributorSql,
                new { ProjectId = projectId, PlayerEntityId = playerEntityId, TotalContributed = totalContributed },
                transaction);
        }

        await transaction.CommitAsync();
    }
}