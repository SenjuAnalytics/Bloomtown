using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses community-help commands: community, help garden, help market, help well, help lumber, help inn, help herb garden, help smithy, help workshop, help patrol, help village, listen to stories, chat with eleanor.
/// </summary>
public static class CommunityActivityCommandParser
{
    public static bool TryParse(string commandLine, out CommunityActivityRequest request, out string errorMessage)
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

        if (normalized is "community" or "community help" or "community activities")
        {
            request = new CommunityActivityRequest(CommunityActivityRequestKind.List, CommunityActivityKind.None);
            return true;
        }

        var activity = normalized switch
        {
            "help garden" or "garden help" => CommunityActivityKind.HelpGarden,
            "help market" or "market help" => CommunityActivityKind.HelpMarket,
            "help well" or "well help" => CommunityActivityKind.HelpWell,
            "help lumber" or "lumber help" => CommunityActivityKind.HelpLumber,
            "help inn" or "inn help" => CommunityActivityKind.HelpInn,
            "help herb garden" or "help herbs" or "tend herbs" or "herb garden help" => CommunityActivityKind.HelpHerbGarden,
            "help smithy" or "smithy help" or "work at forge" or "forge help" => CommunityActivityKind.HelpSmithy,
            "help workshop" or "workshop help" or "work at workshop" or "bench help" => CommunityActivityKind.HelpWorkshop,
            "help patrol" or "patrol help" or "assist guard" or "guard help" => CommunityActivityKind.HelpPatrol,
            "help village" or "village help" or "help around village" or "assist around village" or "tend village" => CommunityActivityKind.HelpVillage,
            "listen to stories" or "listen stories" or "hear stories" or "story bench" => CommunityActivityKind.ListenToStories,
            "chat with eleanor" or "listen to old stories" or "listen old stories" or "eleanor porch" => CommunityActivityKind.ChatWithEleanor,
            _ => CommunityActivityKind.None,
        };

        if (activity == CommunityActivityKind.None)
            return false;

        request = new CommunityActivityRequest(CommunityActivityRequestKind.Perform, activity);
        return true;
    }

    public static string BuildUsageHint() =>
        "Community help: community | help garden | help market | help well | help lumber | help inn | help herb garden | help smithy | help workshop | help patrol | help village | listen to stories | chat with eleanor";
}