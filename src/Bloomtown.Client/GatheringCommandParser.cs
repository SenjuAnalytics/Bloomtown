using Bloomtown.Shared.Gathering;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses gathering console commands such as "gather wood" or "chop tree".
/// </summary>
public static class GatheringCommandParser
{
    public static bool TryParse(string commandLine, out GatheringRequest request, out string errorMessage)
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
        if (parts.Length < 2)
        {
            errorMessage = BuildUsageHint();
            return false;
        }

        var verb = parts[0];
        var target = parts[1];

        if (verb is not ("gather" or "chop" or "mine"))
        {
            return false;
        }

        if (verb == "chop" && target != "tree")
        {
            errorMessage = "Use: chop tree";
            return false;
        }

        if (verb == "mine" && target is not ("stone" or "rock"))
        {
            errorMessage = "Use: mine stone";
            return false;
        }

        var lookupWord = verb switch
        {
            "chop" => "tree",
            "mine" => "stone",
            _ => target,
        };

        if (!GatheringActionLookup.TryResolve(lookupWord, out var resourceType))
        {
            errorMessage = $"Unknown resource '{target}'. {GatheringActionLookup.KnownActionsList}.";
            return false;
        }

        request = new GatheringRequest(resourceType);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Gathering: gather wood, chop tree, mine stone (near resource nodes)";
    }
}