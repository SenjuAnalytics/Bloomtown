using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Client;

/// <summary>
/// Parses daily village activity commands: leisure, social, community, and light practice.
/// </summary>
public static class DailyVillageActivityCommandParser
{
    public static bool TryParse(string commandLine, out DailyVillageActivityRequest request, out string errorMessage)
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

        if (normalized is "daily" or "daily leisure" or "daily activities" or "village leisure" or "leisure")
        {
            request = new DailyVillageActivityRequest(DailyVillageActivityRequestKind.List, DailyVillageActivityKind.None);
            return true;
        }

        var activity = normalized switch
        {
            "sit bench" or "bench sit" or "sit on bench" => DailyVillageActivityKind.SitOnBench,
            "watch village" or "village watch" or "observe village" => DailyVillageActivityKind.WatchVillage,
            "chat locals" or "chat local" or "village chat" => DailyVillageActivityKind.ChatWithLocals,
            "tend public garden" or "public garden" or "garden tend" => DailyVillageActivityKind.TendPublicGarden,
            "practice workshop" or "workshop practice" or "workshop drill" => DailyVillageActivityKind.PracticeWorkshop,
            _ => DailyVillageActivityKind.None,
        };

        if (activity == DailyVillageActivityKind.None)
            return false;

        request = new DailyVillageActivityRequest(DailyVillageActivityRequestKind.Perform, activity);
        return true;
    }

    public static string BuildUsageHint() =>
        "Daily village activities: daily | sit bench | watch village | chat locals | tend public garden | practice workshop";
}