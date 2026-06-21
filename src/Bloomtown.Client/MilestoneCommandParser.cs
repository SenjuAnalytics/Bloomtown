using Bloomtown.Shared.Milestone;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses milestone commands: milestones, drink well, cross bridge, collect stipend.
/// </summary>
public static class MilestoneCommandParser
{
    public static bool TryParse(string commandLine, out MilestoneRequest request, out string errorMessage)
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

        if (normalized is "milestones" or "milestone")
        {
            request = new MilestoneRequest(MilestoneRequestKind.List, MilestoneInteractionKind.None);
            return true;
        }

        var interaction = normalized switch
        {
            "drink well" or "drink from well" => MilestoneInteractionKind.DrinkWell,
            "cross bridge" => MilestoneInteractionKind.CrossBridge,
            "collect stipend" or "warehouse stipend" => MilestoneInteractionKind.CollectStipend,
            _ => MilestoneInteractionKind.None,
        };

        if (interaction == MilestoneInteractionKind.None)
            return false;

        request = new MilestoneRequest(MilestoneRequestKind.Interact, interaction);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Milestones: milestones, drink well, cross bridge, collect stipend";
    }
}