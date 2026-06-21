using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses player-initiated emotional bonding commands for Elsie, Harold, Mira, Tom, Greta, Nora, Elias, Ben, Lila, Rowan, Marcus, and Eleanor.
/// </summary>
public static class EmotionalBondCommandParser
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

        if (!TryParseAction(normalized, out var action, out var npcName, out errorMessage))
            return false;

        if (!NpcNameLookup.TryResolveEntityId(npcName, out var npcEntityId))
        {
            errorMessage = $"Unknown NPC '{npcName}'. Emotional bonding is with elsie, harold, mira, tom, greta, nora, elias, ben, lila, rowan, marcus, or eleanor.";
            return false;
        }

        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
        {
            errorMessage = "Emotional bonding is only available with Elsie, Harold, Mira, Tom, Greta, Nora, Elias, Ben, Lila, Rowan, Marcus, or Eleanor.";
            return false;
        }

        request = new EmotionalBondRequest(EmotionalBondRequestKind.Perform, action, npcEntityId);
        return true;
    }

    public static string BuildUsageHint() =>
        "Bonding (near NPC): check on elsie | spend time with greta | share moment with mira | help harold | help ben | help lila | help rowan | help marcus";

    private static bool TryParseAction(
        string normalized,
        out EmotionalBondActionKind action,
        out string npcName,
        out string errorMessage)
    {
        action = EmotionalBondActionKind.None;
        npcName = string.Empty;
        errorMessage = string.Empty;

        if (normalized.StartsWith("check on ", StringComparison.Ordinal))
        {
            action = EmotionalBondActionKind.CheckOn;
            npcName = normalized["check on ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("spend time with ", StringComparison.Ordinal)
            || normalized.StartsWith("sit with ", StringComparison.Ordinal))
        {
            action = EmotionalBondActionKind.SpendTime;
            npcName = normalized.StartsWith("spend time with ", StringComparison.Ordinal)
                ? normalized["spend time with ".Length..].Trim()
                : normalized["sit with ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("share moment with ", StringComparison.Ordinal))
        {
            action = EmotionalBondActionKind.ShareMoment;
            npcName = normalized["share moment with ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("help ", StringComparison.Ordinal)
            && !normalized.StartsWith("help garden", StringComparison.Ordinal)
            && !normalized.StartsWith("help market", StringComparison.Ordinal)
            && !normalized.StartsWith("help well", StringComparison.Ordinal)
            && !normalized.StartsWith("help lumber", StringComparison.Ordinal)
            && !normalized.StartsWith("help inn", StringComparison.Ordinal)
            && !normalized.StartsWith("help herb", StringComparison.Ordinal)
            && !normalized.StartsWith("help smithy", StringComparison.Ordinal)
            && !normalized.StartsWith("help workshop", StringComparison.Ordinal)
            && !normalized.StartsWith("help patrol", StringComparison.Ordinal)
            && !normalized.StartsWith("help village", StringComparison.Ordinal)
            && !normalized.StartsWith("listen to stories", StringComparison.Ordinal)
            && !normalized.StartsWith("listen stories", StringComparison.Ordinal)
            && !normalized.StartsWith("chat with eleanor", StringComparison.Ordinal)
            && !normalized.StartsWith("listen to old stories", StringComparison.Ordinal)
            && !normalized.StartsWith("assist guard", StringComparison.Ordinal)
            && !normalized.StartsWith("assist around village", StringComparison.Ordinal)
            && !normalized.StartsWith("work at forge", StringComparison.Ordinal)
            && !normalized.StartsWith("tend herbs", StringComparison.Ordinal))
        {
            action = EmotionalBondActionKind.HelpWith;
            npcName = normalized["help ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        return false;
    }

    private static bool ValidateNpcName(string npcName, out string errorMessage)
    {
        if (npcName is "elsie" or "harold" or "mira" or "tom" or "greta" or "nora" or "elias" or "ben" or "lila" or "rowan" or "marcus" or "eleanor")
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = "Emotional bonding is only with elsie, harold, mira, tom, greta, nora, elias, ben, lila, rowan, marcus, or eleanor. "
            + BuildUsageHint();
        return false;
    }
}