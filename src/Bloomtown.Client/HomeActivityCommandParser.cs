using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses cozy home activity commands: relax, read, sit, tea, nap.
/// </summary>
public static class HomeActivityCommandParser
{
    private static readonly Dictionary<string, HomeActivityType> NamesByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["relax"] = HomeActivityType.Relax,
        ["read"] = HomeActivityType.ReadBook,
        ["read book"] = HomeActivityType.ReadBook,
        ["sit"] = HomeActivityType.SitByTable,
        ["sit by table"] = HomeActivityType.SitByTable,
        ["tea"] = HomeActivityType.EnjoyTea,
        ["enjoy tea"] = HomeActivityType.EnjoyTea,
        ["nap"] = HomeActivityType.Nap,
        ["take a nap"] = HomeActivityType.Nap,
    };

    public static string KnownActivitiesList => string.Join(", ", NamesByKey.Keys.OrderBy(name => name));

    public static bool TryParse(string commandLine, out HomeRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var normalized = commandLine.Trim().ToLowerInvariant();
        if (normalized.StartsWith('/'))
            normalized = normalized[1..];

        if (!NamesByKey.TryGetValue(normalized, out var activityType))
            return false;

        request = new HomeRequest(HomeRequestKind.Activity, 0, 0, 0, activityType);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Cozy home activities (must be at home): relax | read | sit | tea | nap";
    }
}