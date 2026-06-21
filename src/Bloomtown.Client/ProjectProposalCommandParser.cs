using Bloomtown.Shared.Community;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses village project proposal commands: propose, proposals.
/// </summary>
public static class ProjectProposalCommandParser
{
    public static bool TryParse(string commandLine, out ProjectProposalRequest request, out string errorMessage)
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

        if (normalized is "proposals" or "proposal-list")
        {
            request = new ProjectProposalRequest(
                ProjectProposalRequestKind.ListProposals,
                string.Empty,
                0,
                0,
                0,
                0);
            return true;
        }

        if (normalized.StartsWith("vote "))
            return TryParseVote(trimmed, out request, out errorMessage);

        if (!normalized.StartsWith("propose "))
        {
            return false;
        }

        var argumentText = trimmed["propose ".Length..].TrimStart();
        if (!TryParseProjectName(argumentText, out var projectName, out var resourceText))
        {
            errorMessage = "Usage: propose \"Project Name\" wood 40 stone 30";
            return false;
        }

        if (string.IsNullOrWhiteSpace(projectName))
        {
            errorMessage = "Project name is required.";
            return false;
        }

        if (projectName.Length > PacketSerializer.MaxProjectProposalNameBytes)
        {
            errorMessage = $"Project name must be at most {PacketSerializer.MaxProjectProposalNameBytes} characters.";
            return false;
        }

        byte wood = 0;
        byte stone = 0;
        byte apple = 0;
        byte tool = 0;

        if (!TryParseResourcePairs(resourceText, ref wood, ref stone, ref apple, ref tool, out errorMessage))
            return false;

        if (wood + stone + apple + tool == 0)
        {
            errorMessage = "Specify at least one resource with quantity 1–99, e.g. wood 40 stone 30.";
            return false;
        }

        request = new ProjectProposalRequest(
            ProjectProposalRequestKind.Propose,
            projectName,
            wood,
            stone,
            apple,
            tool);
        return true;
    }

    private static bool TryParseProjectName(string input, out string projectName, out string remainder)
    {
        projectName = string.Empty;
        remainder = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.TrimStart();

        if (input.StartsWith('"'))
        {
            var endQuote = input.IndexOf('"', 1);
            if (endQuote < 0)
                return false;

            projectName = input[1..endQuote];
            remainder = input[(endQuote + 1)..].Trim();
            return true;
        }

        var spaceIndex = input.IndexOf(' ');
        if (spaceIndex < 0)
        {
            projectName = input;
            remainder = string.Empty;
            return true;
        }

        projectName = input[..spaceIndex];
        remainder = input[spaceIndex..].Trim();
        return true;
    }

    private static bool TryParseResourcePairs(
        string resourceText,
        ref byte wood,
        ref byte stone,
        ref byte apple,
        ref byte tool,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(resourceText))
            return true;

        var parts = resourceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length % 2 != 0)
        {
            errorMessage = "Resource requirements must be item/quantity pairs, e.g. wood 40 stone 30.";
            return false;
        }

        for (var index = 0; index < parts.Length; index += 2)
        {
            var itemName = parts[index];
            if (!ItemNameLookup.TryResolve(itemName, out var itemType))
            {
                errorMessage = $"Unknown item '{itemName}'. Known items: {ItemNameLookup.KnownItemsList}.";
                return false;
            }

            if (!byte.TryParse(parts[index + 1], out var quantity) || quantity is < 1 or > 99)
            {
                errorMessage = "Quantity must be a number between 1 and 99.";
                return false;
            }

            switch (itemType)
            {
                case ItemType.Wood:
                    wood = quantity;
                    break;
                case ItemType.Stone:
                    stone = quantity;
                    break;
                case ItemType.Apple:
                    apple = quantity;
                    break;
                case ItemType.Tool:
                    tool = quantity;
                    break;
                default:
                    errorMessage = $"Item '{itemName}' cannot be used in project proposals.";
                    return false;
            }
        }

        return true;
    }

    private static bool TryParseVote(string trimmed, out ProjectProposalRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        // vote yes on 5
        // vote no on "Bangun Pagar Desa"
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4 || !parts[2].Equals("on", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "Usage: vote yes on 5  or  vote no on \"Project Name\"";
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
            errorMessage = "Vote must be 'yes' or 'no'. Usage: vote yes on 5";
            return false;
        }

        var targetText = trimmed[(trimmed.IndexOf(" on ", StringComparison.OrdinalIgnoreCase) + 4)..].TrimStart();
        int proposalId = 0;
        string projectName = string.Empty;

        if (targetText.StartsWith('"'))
        {
            if (!TryParseProjectName(targetText, out projectName, out _))
            {
                errorMessage = "Invalid quoted project name after 'on'.";
                return false;
            }
        }
        else if (int.TryParse(targetText.Split(' ')[0], out proposalId) && proposalId > 0)
        {
            projectName = string.Empty;
        }
        else
        {
            errorMessage = "Specify a proposal id or quoted project name after 'on'.";
            return false;
        }

        request = new ProjectProposalRequest(
            ProjectProposalRequestKind.Vote,
            projectName,
            0,
            0,
            0,
            0,
            proposalId,
            voteChoice);
        return true;
    }

    public static string BuildUsageHint()
    {
        return "Proposals: proposals, propose \"Name\" wood 40, vote yes on 5 (weight by title)";
    }
}