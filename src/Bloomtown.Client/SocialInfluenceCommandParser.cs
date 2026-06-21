using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;

namespace Bloomtown.Client;

/// <summary>
/// Parses active social-influence commands for Well-liked players calling on focus NPCs.
/// </summary>
public static class SocialInfluenceCommandParser
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
            errorMessage = $"Unknown NPC '{npcName}'. Social influence is with focus NPCs: harold, greta, mira, elsie, tom, nora, elias, ben, lila, rowan, marcus, or eleanor.";
            return false;
        }

        if (!SocialInfluenceActionConfig.IsSupportedNpc(npcEntityId))
        {
            errorMessage = "Social influence is only available with focus NPCs at Well-liked standing.";
            return false;
        }

        request = new EmotionalBondRequest(
            EmotionalBondRequestKind.RequestSocialInfluence,
            EmotionalBondActionKind.None,
            npcEntityId);
        return true;
    }

    public static string BuildUsageHint() =>
        SocialInfluenceActionConfig.BuildUsageHint();

    private static bool TryParseNpcName(
        string normalized,
        out string npcName,
        out string errorMessage)
    {
        npcName = string.Empty;
        errorMessage = string.Empty;

        if (normalized.StartsWith("call on ", StringComparison.Ordinal))
        {
            npcName = normalized["call on ".Length..].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for a favor", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for a favor".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for a trade favor", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for a trade favor".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for garden support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for garden support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for lumber support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for lumber support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for herbal support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for herbal support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for smithing support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for smithing support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for crafting support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for crafting support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized.StartsWith("ask ", StringComparison.Ordinal)
            && normalized.EndsWith(" for guard support", StringComparison.Ordinal))
        {
            npcName = normalized["ask ".Length..^" for guard support".Length].Trim();
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized is "ask lila for help")
        {
            npcName = "lila";
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized is "ask rowan for help")
        {
            npcName = "rowan";
            return ValidateNpcName(npcName, out errorMessage);
        }

        if (normalized is "ask eleanor for help")
        {
            npcName = "eleanor";
            return ValidateNpcName(npcName, out errorMessage);
        }

        return false;
    }

    private static bool ValidateNpcName(string npcName, out string errorMessage)
    {
        if (npcName is "harold" or "greta" or "mira" or "elsie" or "tom" or "nora" or "elias" or "ben" or "lila" or "rowan" or "marcus" or "eleanor")
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = "Social influence is with harold, greta, mira, elsie, tom, nora, elias, ben, lila, rowan, marcus, or eleanor. " + BuildUsageHint();
        return false;
    }
}