using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Client;

/// <summary>
/// Parses active social-standing favor commands for focus NPCs when the player has Respected+ standing.
/// </summary>
public static class SocialStandingCommandParser
{
    public static bool TryParse(string commandLine, out EmotionalBondRequest request, out string errorMessage)
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

        if (!TryParseNpcName(normalized, out var npcName, out errorMessage))
            return false;

        if (!NpcNameLookup.TryResolveEntityId(npcName, out var npcEntityId))
        {
            errorMessage = $"Unknown NPC '{npcName}'. Social favors are with elsie, harold, mira, tom, greta, nora, elias, ben, or lila.";
            return false;
        }

        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
        {
            errorMessage = "Social favors are only available with Elsie, Harold, Mira, Tom, Greta, Nora, Elias, or Ben.";
            return false;
        }

        request = new EmotionalBondRequest(
            EmotionalBondRequestKind.RequestStandingFavor,
            EmotionalBondActionKind.None,
            npcEntityId);
        return true;
    }

    public static string BuildUsageHint() =>
        SocialStandingActionConfig.BuildUsageHint();

    private static bool TryParseNpcName(
        string normalized,
        out string npcName,
        out string errorMessage)
    {
        npcName = string.Empty;
        errorMessage = string.Empty;

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && (normalized.EndsWith(" for help", StringComparison.Ordinal)
                || normalized.EndsWith(" for advice", StringComparison.Ordinal)))
        {
            var endIndex = normalized.LastIndexOf(" for ", StringComparison.Ordinal);
            npcName = normalized["ask ".Length..endIndex].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("request favor from ", StringComparison.Ordinal))
        {
            npcName = normalized["request favor from ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("request help from ", StringComparison.Ordinal))
        {
            npcName = normalized["request help from ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        return false;
    }

    private static bool ValidateNpcName(string npcName, out string errorMessage)
    {
        if (npcName is "elsie" or "harold" or "mira" or "tom" or "greta" or "nora" or "elias" or "ben" or "lila")
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = "Social favors are only with elsie, harold, mira, tom, greta, nora, elias, ben, or lila. "
            + BuildUsageHint();
        return false;
    }
}