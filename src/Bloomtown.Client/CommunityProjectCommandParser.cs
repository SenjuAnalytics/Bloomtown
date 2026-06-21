using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses community project commands: projects, contribute.
/// </summary>
public static class CommunityProjectCommandParser
{
    public static bool TryParse(string commandLine, out CommunityProjectRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var normalized = commandLine.Trim().ToLowerInvariant();
        if (normalized.StartsWith('/'))
            normalized = normalized[1..];

        if (string.IsNullOrWhiteSpace(normalized))
        {
            errorMessage = "Empty command.";
            return false;
        }

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts[0] is "projects" or "project")
        {
            request = new CommunityProjectRequest(CommunityProjectRequestKind.List, 0, 0, 0);
            return true;
        }

        if (parts[0] == "contribute")
            return TryParseContribute(parts, out request, out errorMessage);

        return false;
    }

    private static bool TryParseContribute(string[] parts, out CommunityProjectRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        // contribute wood 10 to well
        // contribute wood to well
        if (parts.Length < 4)
        {
            errorMessage = "Usage: contribute <item> [qty] to <project>  e.g. contribute wood 10 to well";
            return false;
        }

        var toIndex = Array.IndexOf(parts, "to");
        if (toIndex < 0)
        {
            errorMessage = "Contribute command must include 'to', e.g. contribute wood 10 to well.";
            return false;
        }

        if (toIndex + 1 >= parts.Length)
        {
            errorMessage = "Missing project name after 'to'.";
            return false;
        }

        var projectSlug = parts[toIndex + 1];
        if (!CommunityProjectNameLookup.TryResolve(projectSlug, out var projectId))
        {
            errorMessage = $"Unknown project '{projectSlug}'. Known projects: {CommunityProjectNameLookup.KnownProjectsList}.";
            return false;
        }

        byte quantity = 1;
        string itemName;

        if (toIndex == 2)
        {
            itemName = parts[1];
        }
        else if (toIndex == 3)
        {
            if (!byte.TryParse(parts[1], out quantity) || quantity is < 1 or > 99)
            {
                errorMessage = "Quantity must be a number between 1 and 99.";
                return false;
            }

            itemName = parts[2];
        }
        else
        {
            errorMessage = "Usage: contribute <item> [qty] to <project>";
            return false;
        }

        if (!ItemNameLookup.TryResolve(itemName, out var itemType))
        {
            errorMessage = $"Unknown item '{itemName}'. Known items: {ItemNameLookup.KnownItemsList}.";
            return false;
        }

        request = new CommunityProjectRequest(CommunityProjectRequestKind.Contribute, projectId, itemType, quantity);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Projects: projects, contribute wood 10 to well";
    }
}