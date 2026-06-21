using System.Text.Json;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Shared.Items;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Persists player-proposed community project definitions.
/// </summary>
public sealed class CommunityProjectDefinitionRepository
{
    private readonly string _connectionString;

    public CommunityProjectDefinitionRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<IReadOnlyList<CommunityProjectDefinitionRecord>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        const string sql = """
            SELECT
                project_id AS ProjectId,
                slug AS Slug,
                name AS Name,
                description AS Description,
                requirements_json AS RequirementsJson,
                is_builtin AS IsBuiltin
            FROM community_project_definitions
            ORDER BY project_id
            """;

        var rows = await connection.QueryAsync<CommunityProjectDefinitionRecord>(sql);
        return rows.ToList();
    }

    public async Task SaveAsync(CommunityProjectDefinition definition, bool isBuiltin)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var requirementsJson = SerializeRequirements(definition.Requirements);

        await connection.ExecuteAsync(
            """
            INSERT INTO community_project_definitions (
                project_id, slug, name, description, requirements_json, is_builtin
            )
            VALUES (
                @ProjectId, @Slug, @Name, @Description, @RequirementsJson, @IsBuiltin
            )
            ON CONFLICT(project_id) DO UPDATE SET
                slug = excluded.slug,
                name = excluded.name,
                description = excluded.description,
                requirements_json = excluded.requirements_json,
                is_builtin = excluded.is_builtin
            """,
            new
            {
                definition.ProjectId,
                definition.Slug,
                definition.Name,
                definition.Description,
                RequirementsJson = requirementsJson,
                IsBuiltin = isBuiltin ? 1 : 0,
            });
    }

    public static Dictionary<ItemType, int> DeserializeRequirements(string requirementsJson)
    {
        var raw = JsonSerializer.Deserialize<Dictionary<int, int>>(requirementsJson) ?? new Dictionary<int, int>();
        return raw.ToDictionary(pair => (ItemType)pair.Key, pair => pair.Value);
    }

    private static string SerializeRequirements(IReadOnlyDictionary<ItemType, int> requirements)
    {
        var raw = requirements.ToDictionary(pair => (int)pair.Key, pair => pair.Value);
        return JsonSerializer.Serialize(raw);
    }
}