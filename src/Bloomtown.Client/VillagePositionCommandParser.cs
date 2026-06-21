using Bloomtown.Shared.Community;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses village leadership commands: positions, run for, elect.
/// </summary>
public static class VillagePositionCommandParser
{
    public static bool TryParse(string commandLine, out VillagePositionRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var trimmed = commandLine.Trim();
        if (trimmed.StartsWith('/'))
            trimmed = trimmed[1..];

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            errorMessage = "Empty command.";
            return false;
        }

        var normalized = trimmed.ToLowerInvariant();

        if (normalized is "positions" or "position-list" or "leadership")
        {
            request = new VillagePositionRequest(VillagePositionRequestKind.List, VillagePosition.None);
            return true;
        }

        if (normalized.StartsWith("run for "))
        {
            var slug = trimmed["run for ".Length..].Trim();
            if (!VillagePositionDisplay.TryParseSlug(slug, out var position))
            {
                errorMessage = "Unknown position. Use: chief, deputy, advisor, or project-leader";
                return false;
            }

            request = new VillagePositionRequest(VillagePositionRequestKind.RunFor, position);
            return true;
        }

        if (normalized.StartsWith("elect "))
            return TryParseElect(trimmed, out request, out errorMessage);

        if (normalized.StartsWith("council "))
            return TryParseCouncil(trimmed, out request, out errorMessage);

        if (normalized.StartsWith("chief "))
            return TryParseChief(trimmed, out request, out errorMessage);

        return false;
    }

    private static bool TryParseElect(string trimmed, out VillagePositionRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        // elect yes chief
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            errorMessage = "Usage: elect yes chief  or  elect no deputy";
            return false;
        }

        var voteChoice = parts[1].ToLowerInvariant() switch
        {
            "yes" => ProjectVoteChoice.Yes,
            "no" => ProjectVoteChoice.No,
            _ => ProjectVoteChoice.None,
        };

        if (voteChoice == ProjectVoteChoice.None)
        {
            errorMessage = "Vote must be yes or no. Usage: elect yes chief";
            return false;
        }

        if (!VillagePositionDisplay.TryParseSlug(parts[2], out var position))
        {
            errorMessage = "Unknown position. Use: chief, deputy, advisor, or project-leader";
            return false;
        }

        request = new VillagePositionRequest(VillagePositionRequestKind.Vote, position, voteChoice);
        return true;
    }

    private static bool TryParseCouncil(string trimmed, out VillagePositionRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        // council yes 5
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            errorMessage = "Usage: council yes 5  or  council no 5";
            return false;
        }

        var voteChoice = parts[1].ToLowerInvariant() switch
        {
            "yes" => ProjectVoteChoice.Yes,
            "no" => ProjectVoteChoice.No,
            _ => ProjectVoteChoice.None,
        };

        if (voteChoice == ProjectVoteChoice.None)
        {
            errorMessage = "Vote must be yes or no. Usage: council yes 5";
            return false;
        }

        if (!int.TryParse(parts[2], out var proposalId) || proposalId <= 0)
        {
            errorMessage = "Specify a proposal id. Usage: council yes 5";
            return false;
        }

        request = new VillagePositionRequest(
            VillagePositionRequestKind.CouncilVote,
            VillagePosition.None,
            voteChoice,
            proposalId);
        return true;
    }

    private static bool TryParseChief(string trimmed, out VillagePositionRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        // chief approve 5 / chief veto 5
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            errorMessage = "Usage: chief approve 5  or  chief veto 5";
            return false;
        }

        if (!int.TryParse(parts[2], out var proposalId) || proposalId <= 0)
        {
            errorMessage = "Specify a proposal id. Usage: chief approve 5";
            return false;
        }

        var kind = parts[1].ToLowerInvariant() switch
        {
            "approve" => VillagePositionRequestKind.ChiefApprove,
            "veto" => VillagePositionRequestKind.ChiefVeto,
            _ => VillagePositionRequestKind.List,
        };

        if (kind is VillagePositionRequestKind.List)
        {
            errorMessage = "Chief action must be approve or veto. Usage: chief approve 5";
            return false;
        }

        request = new VillagePositionRequest(kind, VillagePosition.None, ProjectVoteChoice.None, proposalId);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Leadership: positions, run for chief, elect yes chief, council yes 5, chief approve 5";
    }
}