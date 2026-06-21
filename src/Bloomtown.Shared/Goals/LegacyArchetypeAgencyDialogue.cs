using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Goals;

/// <summary>Archetype-specific action feedback — shows the village notices how the player shapes their legacy.</summary>
internal static class LegacyArchetypeAgencyDialogue
{
    internal static string? TryGetCommunityHelpFeedback(
        LegacyArchetype archetype,
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var lines = (archetype, activity) switch
        {
            (LegacyArchetype.Caretaker, CommunityActivityKind.HelpGarden) =>
            [
                "The garden feels steadier when you help — caretaker work, and the village feels it.",
                "Elsie catches your eye. \"This is how caretakers leave their mark — one bed at a time.\"",
            ],
            (LegacyArchetype.Caretaker, CommunityActivityKind.HelpWell) =>
            [
                "Your well-side chores land gently — caretakers keep the village's daily rhythm honest.",
                "Harold nods. \"Small upkeep, big trust — that's caretaker legacy in motion.\"",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your help carries extra warmth today — Bloomtown reads you as someone who tends, not just visits.",
            ],
            (LegacyArchetype.Connector, CommunityActivityKind.HelpMarket) =>
            [
                "Market chatter opens around you — connectors turn errands into introductions.",
                "Mira winks. \"You don't just help the square — you help people find each other.\"",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Someone introduces themselves while you work — your connector path is showing.",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetProjectContributionFeedback(LegacyArchetype archetype, uint variationSeed)
    {
        if (archetype != LegacyArchetype.Builder)
            return null;

        var lines = new[]
        {
            "Your builder's touch — villagers notice how carefully you place each gift toward the project's future.",
            "Harold murmurs, \"Steady hands on shared work. That's how builders become part of the skyline.\"",
            "Elsie smiles. \"Every timber you add tells the village you're here to build, not just pass through.\"",
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetConnectorSocialInsight(uint variationSeed)
    {
        var lines = new[]
        {
            "Mira leans in. \"If you're looking for company, Harold's loop near noon — he hears everything before supper.\"",
            "Elsie adds quietly, \"Tom's out past the woods most afternoons — good soul when work needs doing.\"",
            "A villager mentions the square fills fastest after breakfast — connectors always know when faces gather.",
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetInfluenceGainFeedback(LegacyArchetype influencedPath, uint variationSeed)
    {
        var lines = influencedPath switch
        {
            LegacyArchetype.Builder =>
            [
                "Your legacy leans toward building — project work strengthens that path.",
            ],
            LegacyArchetype.Caretaker =>
            [
                "Your legacy leans toward caretaking — steady village help strengthens that path.",
            ],
            LegacyArchetype.Connector =>
            [
                "Your legacy leans toward connection — social bonds strengthen that path.",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}