using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Registry of built-in and player-proposed communal projects.
/// </summary>
public static class CommunityProjectRegistry
{
    private static readonly List<CommunityProjectDefinition> Projects = [];

    static CommunityProjectRegistry()
    {
        RegisterBuiltins();
    }

    public static IReadOnlyList<CommunityProjectDefinition> All => Projects;

    public static bool TryGet(byte projectId, out CommunityProjectDefinition definition)
    {
        definition = Projects.FirstOrDefault(project => project.ProjectId == projectId)!;
        return definition is not null;
    }

    public static bool TryGetBySlug(string slug, out CommunityProjectDefinition definition)
    {
        definition = Projects.FirstOrDefault(
            project => string.Equals(project.Slug, slug, StringComparison.OrdinalIgnoreCase))!;
        return definition is not null;
    }

    public static byte GetNextProjectId()
    {
        if (Projects.Count == 0)
            return 1;

        return (byte)(Projects.Max(project => project.ProjectId) + 1);
    }

    /// <summary>
    /// Registers a dynamic project proposed by a player (or loaded from persistence).
    /// </summary>
    public static void Register(CommunityProjectDefinition definition)
    {
        if (Projects.Any(project => project.ProjectId == definition.ProjectId))
            throw new InvalidOperationException($"Project id {definition.ProjectId} is already registered.");

        if (Projects.Any(project => string.Equals(project.Slug, definition.Slug, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Project slug '{definition.Slug}' is already registered.");

        Projects.Add(definition);
        CommunityProjectNameLookup.RegisterSlug(definition.ProjectId, definition.Slug);
    }

    private static void RegisterBuiltins()
    {
        Register(new CommunityProjectDefinition
        {
            ProjectId = 1,
            Slug = "well",
            Name = "Build Village Well",
            Description = "Collect wood and stone to build a shared well near the village center.",
            Requirements = new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 30,
                [ItemType.Stone] = 15,
            },
        });

        Register(new CommunityProjectDefinition
        {
            ProjectId = 2,
            Slug = "bridge",
            Name = "Repair the Bridge",
            Description = "Restore the river crossing so villagers can travel safely.",
            Requirements = new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 20,
                [ItemType.Stone] = 25,
            },
        });

        Register(new CommunityProjectDefinition
        {
            ProjectId = 3,
            Slug = "warehouse",
            Name = "Village Warehouse",
            Description = "Expand communal storage for harvested goods and tools.",
            Requirements = new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 50,
                [ItemType.Stone] = 30,
            },
        });
    }
}