using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses conscious legacy-focus commands: focus build | focus tend | focus connect.
/// </summary>
public static class LegacyFocusCommandParser
{
    public static bool TryParse(string commandLine, out LegacyFocusRequest request, out string errorMessage)
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

        if (!normalized.StartsWith("focus ", StringComparison.Ordinal)
            && !normalized.StartsWith("legacy focus ", StringComparison.Ordinal))
        {
            return false;
        }

        var pathToken = normalized.StartsWith("legacy focus ", StringComparison.Ordinal)
            ? normalized["legacy focus ".Length..].Trim()
            : normalized["focus ".Length..].Trim();

        var path = pathToken switch
        {
            "build" or "builder" => LegacyArchetype.Builder,
            "tend" or "caretaker" or "care" => LegacyArchetype.Caretaker,
            "connect" or "connector" or "social" => LegacyArchetype.Connector,
            _ => LegacyArchetype.None,
        };

        if (path == LegacyArchetype.None)
        {
            errorMessage = "Usage: focus build | focus tend | focus connect";
            return false;
        }

        request = new LegacyFocusRequest(LegacyFocusRequestKind.Perform, path);
        return true;
    }

    public static string BuildUsageHint() =>
        "Conscious legacy focus: focus build | focus tend | focus connect (at the right location)";
}