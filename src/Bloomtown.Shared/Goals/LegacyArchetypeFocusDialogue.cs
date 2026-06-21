using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Conscious legacy-focus action feedback and village recognition of the player's emerging identity.
/// </summary>
internal static class LegacyArchetypeFocusDialogue
{
    internal static string? TryGetConsciousActionFeedback(LegacyArchetype path, uint variationSeed)
    {
        var lines = path switch
        {
            LegacyArchetype.Builder =>
            [
                "You study the site with a builder's eye — Bloomtown feels your intent to leave something lasting.",
                "Harold watches the timbers. \"That's builder focus. The village remembers who shows up for shared work.\"",
                "You picture the finished project — a deliberate choice that strengthens your builder legacy.",
            ],
            LegacyArchetype.Caretaker =>
            [
                "You pause to tend what others rely on — caretaker focus, quiet but unmistakable.",
                "Elsie smiles. \"That's how caretakers root here — small upkeep, steady trust.\"",
                "You choose care over hurry — Bloomtown reads it as someone who keeps the village warm.",
            ],
            LegacyArchetype.Connector =>
            [
                "You listen for names and faces — connector focus turns errands into belonging.",
                "Mira nods. \"You don't just pass through — you help people find each other.\"",
                "You make room for introductions — a deliberate step toward a connector legacy.",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetFocusTradeoffHint(LegacyArchetype focusedPath, uint variationSeed)
    {
        var lines = focusedPath switch
        {
            LegacyArchetype.Builder =>
            [
                "Focusing on building nudges other legacy paths to grow more slowly — a tradeoff, not a lock.",
            ],
            LegacyArchetype.Caretaker =>
            [
                "Focusing on caretaking nudges other legacy paths to grow more slowly — a tradeoff, not a lock.",
            ],
            LegacyArchetype.Connector =>
            [
                "Focusing on connection nudges other legacy paths to grow more slowly — a tradeoff, not a lock.",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetIdentityRecognitionLine(LegacyArchetype archetype, uint variationSeed)
    {
        var lines = archetype switch
        {
            LegacyArchetype.Builder =>
            [
                "People already describe you as someone who builds — not just visits, but raises things up.",
                "Elsie mentions your name beside village projects — builder identity, earned in timber and stone.",
            ],
            LegacyArchetype.Caretaker =>
            [
                "Neighbors call you steady help — caretaker identity, earned in garden soil and well-side chores.",
                "Harold says quietly, \"When something needs tending, folk think of you first.\"",
            ],
            LegacyArchetype.Connector =>
            [
                "Introductions flow through you now — connector identity, earned in greetings and market chatter.",
                "Mira laughs softly. \"You know everyone's name before supper — that's connector legacy showing.\"",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetIdentityAmbientLine(LegacyArchetype archetype, uint variationSeed)
    {
        var lines = archetype switch
        {
            LegacyArchetype.Builder =>
            [
                "Overheard: \"They're one of our builders now — you can tell by how they study every project site.\"",
            ],
            LegacyArchetype.Caretaker =>
            [
                "Overheard: \"Caretaker through and through — the garden and well both feel safer when they're around.\"",
            ],
            LegacyArchetype.Connector =>
            [
                "Overheard: \"Connector, I'd say — half the square knows them by name and the other half will by week's end.\"",
            ],
            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string FormatLocationHint(LegacyArchetype path) =>
        path switch
        {
            LegacyArchetype.Builder =>
                "Move within 14m of a village project site (well, bridge, or warehouse).",
            LegacyArchetype.Caretaker =>
                "Move within 10m of the Community Garden (20, 14) or Village Well (5, 5).",
            LegacyArchetype.Connector =>
                "Move within 10m of Market Square (18, 6) or within 24m of a villager.",
            _ => "Move to a location that fits your chosen legacy path.",
        };

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}