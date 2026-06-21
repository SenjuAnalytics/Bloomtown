namespace Bloomtown.Shared.Community;

/// <summary>
/// Case-insensitive project slug parsing for console commands.
/// </summary>
public static class CommunityProjectNameLookup
{
    private static readonly Dictionary<string, byte> IdsBySlug = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<byte, string> SlugsById = new();

    static CommunityProjectNameLookup()
    {
        RegisterSlug(1, "well");
        RegisterSlug(2, "bridge");
        RegisterSlug(3, "warehouse");
    }

    public static string KnownProjectsList => string.Join(", ", SlugsById.Values.OrderBy(name => name));

    public static void RegisterSlug(byte projectId, string slug)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        IdsBySlug[normalizedSlug] = projectId;
        SlugsById[projectId] = normalizedSlug;
    }

    public static bool TryResolve(string slug, out byte projectId)
    {
        return IdsBySlug.TryGetValue(slug.Trim(), out projectId);
    }

    public static string GetSlugOrDefault(byte projectId)
    {
        return SlugsById.TryGetValue(projectId, out var slug) ? slug : $"project-{projectId}";
    }
}