using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Milestone recognition, NPC archetype recognition, and personal aligned-action feedback.
/// Variants reflect the player's detected legacy archetype — builder, caretaker, or connector.
/// </summary>
internal static class PlayerLongTermGoalDialogue
{
    /// <summary>NPC-specific recognition — Elsie, Harold, and Mira speak differently per archetype.</summary>
    internal static string? TryGetNpcArchetypeRecognitionLine(
        uint npcEntityId,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        var lines = (npcEntityId, archetype) switch
        {
            (NpcEntityIds.Elsie, LegacyArchetype.Builder) =>
            [
                "Elsie studies the work nearby. \"You're not just helping — you're building Bloomtown's tomorrow.\"",
                "Elsie smiles. \"I hear your name beside every project now. That's a builder's story.\"",
            ],
            (NpcEntityIds.Elsie, LegacyArchetype.Caretaker) =>
            [
                "Elsie touches your arm gently. \"When things get difficult, folk think of you first — that's rare.\"",
                "Elsie says warmly, \"You're becoming the kind of neighbor Bloomtown leans on without asking.\"",
            ],
            (NpcEntityIds.Elsie, LegacyArchetype.Connector) =>
            [
                "Elsie laughs. \"Half the village met someone new through you this week — that's connector magic.\"",
                "Elsie says, \"You remember names the way old friends do. Bloomtown feels smaller and warmer for it.\"",
            ],

            (NpcEntityIds.Harold, LegacyArchetype.Builder) =>
            [
                "Harold nods at the timbers. \"Steady hands, shared work — builders earn their place that way.\"",
                "Harold murmurs, \"Folk already point to what you've raised. Good legacy for a good season.\"",
            ],
            (NpcEntityIds.Harold, LegacyArchetype.Caretaker) =>
            [
                "Harold says quietly, \"Caretakers don't shout — but everyone knows who kept the well tidy.\"",
                "Harold tips his hat. \"You're the steady help this village needed. Don't underestimate that.\"",
            ],
            (NpcEntityIds.Harold, LegacyArchetype.Connector) =>
            [
                "Harold chuckles. \"You know everyone's business — and somehow make it feel like belonging.\"",
                "Harold says, \"Introductions flow through you. Connectors hold villages together that way.\"",
            ],

            (NpcEntityIds.Mira, LegacyArchetype.Builder) =>
            [
                "Mira grins. \"You look at half-finished work like it's already a landmark — builder's eyes.\"",
                "Mira says, \"People talk about what you're raising. That's how builders become village history.\"",
            ],
            (NpcEntityIds.Mira, LegacyArchetype.Caretaker) =>
            [
                "Mira says softly, \"The garden feels safer when you're around — caretaker energy, plain and true.\"",
                "Mira smiles. \"Small chores, big trust. That's how caretakers root in Bloomtown.\"",
            ],
            (NpcEntityIds.Mira, LegacyArchetype.Connector) =>
            [
                "Mira winks. \"You greeted three people I hadn't met yet — all before lunch. Classic connector.\"",
                "Mira says, \"Your name travels kindly. That's how someone becomes part of the village story.\"",
            ],

            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    /// <summary>Personal narrative feedback when an action aligns with the player's emerging archetype.</summary>
    internal static string? TryGetPersonalAlignedActionFeedback(
        LegacyArchetype archetype,
        LegacyAlignedActionKind action,
        uint variationSeed)
    {
        var lines = (archetype, action) switch
        {
            (LegacyArchetype.Builder, LegacyAlignedActionKind.ProjectContribution) =>
            [
                "Another gift toward shared work — your builder story grows sturdier with every contribution.",
                "Bloomtown reads this as building, not visiting. Your legacy leans into what endures.",
            ],
            (LegacyArchetype.Builder, LegacyAlignedActionKind.ConsciousFocus) =>
            [
                "You choose to build deliberately — the village feels someone shaping its future, not just passing through.",
            ],

            (LegacyArchetype.Caretaker, LegacyAlignedActionKind.CommunityHelp) =>
            [
                "Steady help again — you're writing a caretaker chapter in Bloomtown's daily rhythm.",
                "Small upkeep, growing trust. The village remembers who tends, not just who arrives.",
            ],
            (LegacyArchetype.Caretaker, LegacyAlignedActionKind.ConsciousFocus) =>
            [
                "You pause to tend what matters — caretaker focus, and Bloomtown feels warmer for it.",
            ],

            (LegacyArchetype.Connector, LegacyAlignedActionKind.NpcInteraction) =>
            [
                "Another greeting, another thread — your connector story weaves Bloomtown a little tighter.",
                "Faces remembered, doors opened. That's how a village learns your name with affection.",
            ],
            (LegacyArchetype.Connector, LegacyAlignedActionKind.CommunityHelp) =>
            [
                "Helping at the square turns into introductions — your connector path shows in every chat.",
            ],
            (LegacyArchetype.Connector, LegacyAlignedActionKind.ConsciousFocus) =>
            [
                "You listen for names and belonging — connector focus, and the village feels more like home.",
            ],

            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    /// <summary>Personal milestone flavor text for goal detail — all milestones when archetype is known.</summary>
    internal static string? TryGetMilestonePersonalFlavor(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype)
    {
        return (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Builder) =>
                "Putting Down Roots — your first marks on Bloomtown's shared work.",
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Caretaker) =>
                "Putting Down Roots — steady help before anyone had to ask twice.",
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Connector) =>
                "Putting Down Roots — faces learned, introductions begun.",

            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Builder) =>
                "Trusted Neighbor — the village trusts your hands on shared projects.",
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Caretaker) =>
                "Trusted Neighbor — someone Bloomtown counts on when routines slip.",
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Connector) =>
                "Trusted Neighbor — your name travels warmly through the lanes.",

            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
                "Village Story — your name beside finished work.",
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
                "Village Story — your name when someone needs steady help.",
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
                "Village Story — your name in every warm introduction.",

            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Builder) =>
                "Bloomtown Legacy — what you raised endures.",
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Caretaker) =>
                "Bloomtown Legacy — who you cared for remembers.",
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Connector) =>
                "Bloomtown Legacy — the bonds you built endure.",

            _ => null,
        };
    }

    /// <summary>Narrative direction hint — personal phrasing instead of mechanical leaning text.</summary>
    internal static string? TryGetNarrativeDirectionHint(
        LegacyArchetype detectedArchetype,
        LegacyArchetype? leadingPath,
        LegacyArchetypeInfluence influence,
        uint variationSeed)
    {
        if (detectedArchetype != LegacyArchetype.None)
        {
            var deepen = detectedArchetype switch
            {
                LegacyArchetype.Builder =>
                    "Keep building — village projects carry your story forward.",
                LegacyArchetype.Caretaker =>
                    "Keep tending — garden and well work deepen the trust you've earned.",
                LegacyArchetype.Connector =>
                    "Keep connecting — greetings and market help weave you into every circle.",
                _ => string.Empty,
            };

            if (leadingPath is not null && leadingPath != detectedArchetype)
            {
                var also = leadingPath.Value switch
                {
                    LegacyArchetype.Builder => "you're also pushing a builder's path",
                    LegacyArchetype.Caretaker => "you're also nurturing a caretaker's path",
                    LegacyArchetype.Connector => "you're also growing a connector's path",
                    _ => string.Empty,
                };

                return $"Your story leans {LegacyArchetypeConfig.GetDisplayName(detectedArchetype).ToLowerInvariant()}, though {also}. {deepen}";
            }

            return deepen;
        }

        if (leadingPath is null)
            return "Your story is still open — build, tend, or connect, and Bloomtown will answer in kind.";

        return leadingPath.Value switch
        {
            LegacyArchetype.Builder =>
                PickLine(
                [
                    "Bloomtown senses a builder's patience in you — project work would make it official.",
                    "You're leaning toward building — shared projects are how that story takes root.",
                ],
                variationSeed),
            LegacyArchetype.Caretaker =>
                PickLine(
                [
                    "Bloomtown senses a caretaker's steadiness in you — garden and well work would deepen it.",
                    "You're leaning toward caretaking — steady village help is writing that chapter.",
                ],
                variationSeed),
            LegacyArchetype.Connector =>
                PickLine(
                [
                    "Bloomtown senses a connector's warmth in you — greetings and market help would seal it.",
                    "You're leaning toward connection — every face remembered is another line in your story.",
                ],
                variationSeed),
            _ => null,
        };
    }
    internal static string? TryGetMilestoneFeedbackLine(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        var lines = (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Builder) =>
            [
                "Elsie nods. \"You're already leaving marks on Bloomtown — good builders start with showing up.\"",
                "Harold says, \"First timbers of a legacy — I see someone who builds more than they take.\"",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Caretaker) =>
            [
                "Elsie smiles. \"Three helps and the village feels your care already — that's how caretakers begin.\"",
                "Harold murmurs, \"Steady hands, steady heart — you're tending Bloomtown, not just visiting.\"",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Connector) =>
            [
                "Mira waves. \"You're weaving in nicely — connectors always find their people first.\"",
                "Elsie says warmly, \"Roots grow through faces remembered — you're planting yours well.\"",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, _) =>
            [
                "Elsie smiles. \"You're putting down roots here — Bloomtown notices when someone keeps showing up.\"",
                "Harold nods. \"Three helps and you're no longer just passing through. Good start.\"",
            ],

            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Builder) =>
            [
                "Elsie says, \"The village trusts your hands now — builders earn that before the big projects land.\"",
                "Harold tips his hat. \"Trusted neighbor who builds — Bloomtown's lucky to have you.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Caretaker) =>
            [
                "Elsie says warmly, \"They call you reliable now — caretakers become the village's quiet backbone.\"",
                "Mira grins. \"Folks count on your usual help. That's caretaker trust, plain and simple.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Connector) =>
            [
                "Mira laughs softly. \"Trusted connector — people greet you before they greet the weather.\"",
                "Elsie says, \"Your name travels kindly through the lanes. That's rare and worth keeping.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Elsie says warmly, \"The village has a name for you now — a trusted neighbor, not a stranger.\"",
                "Mira grins. \"Folks talk about your usual spot. That's how neighbors become family here.\"",
            ],

            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
            [
                "Elsie looks pleased. \"Your story here is built in timber and stone — people notice what you've raised.\"",
                "Harold murmurs, \"Village tale with a builder's chapter — Bloomtown grows sturdier for it.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
            [
                "Elsie says softly, \"Your story's written in every small kindness — caretakers shape a village's warmth.\"",
                "Harold nods. \"Helper, steady friend, keeper of routine — you're part of the village tale now.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Mira beams. \"Your story's all introductions and laughter — connectors stitch Bloomtown together.\"",
                "Elsie says, \"People speak your name like they speak old friends — that's a village story worth having.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, _) =>
            [
                "Elsie looks pleased. \"Your story's woven into Bloomtown now — people speak your name with gratitude.\"",
                "Harold murmurs, \"Helper, friend, builder of trust — you're becoming part of the village tale.\"",
            ],

            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Builder) =>
            [
                "Elsie speaks softly. \"A builder's legacy — what you raised here will outlast any single season.\"",
                "Harold tips his hat. \"Living legacy in every beam and path — Bloomtown stands taller for you.\"",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Caretaker) =>
            [
                "Elsie says warmly, \"A caretaker's legacy — Bloomtown will remember who kept showing up when it mattered.\"",
                "Harold murmurs, \"Living legacy of steady care — not every village earns someone like you.\"",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Connector) =>
            [
                "Mira says quietly, \"A connector's legacy — the bonds you built will keep Bloomtown feeling like home.\"",
                "Elsie speaks softly. \"Living legacy woven through friendships — that's how villages endure.\"",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, _) =>
            [
                "Elsie speaks softly. \"You've built a legacy here. Bloomtown will remember what you gave this place.\"",
                "Harold tips his hat. \"Living legacy — not every traveler earns that. The village is richer for you.\"",
            ],

            _ => Array.Empty<string>(),
        };

        return PickLine(lines, variationSeed);
    }

    internal static string? TryGetMilestoneAmbientLine(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        var lines = (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Builder) =>
            [
                "Village talk: someone new is already shaping Bloomtown — a builder's touch, early but real.",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Caretaker) =>
            [
                "Someone mentions you've been helping around the village — caretaker energy, they say.",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Connector) =>
            [
                "Soft gossip: a newcomer is learning every face fast — connector spirit, the square agrees.",
            ],
            (PlayerLongTermGoalMilestone.PuttingDownRoots, _) =>
            [
                "Someone mentions you've been helping around the village — a good sign you're settling in.",
            ],

            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Builder) =>
            [
                "Village talk: the builder-in-the-making is trusted now — folks save them a spot near the work.",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Caretaker) =>
            [
                "Village talk: you're known as a regular helper — the kind people count on without asking twice.",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, LegacyArchetype.Connector) =>
            [
                "Village talk: everyone seems to know your name already — connectors earn that kind of warmth.",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Village talk: you're known as a regular helper now — the kind people count on.",
            ],

            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
            [
                "Neighbors compare notes — your name comes up beside finished work and sturdy progress.",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
            [
                "Neighbors compare notes — your name comes up when they list who keeps Bloomtown cared for.",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Neighbors compare notes — your name comes up in every circle, like you've always belonged.",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, _) =>
            [
                "Neighbors compare notes — your name comes up when they list who keeps Bloomtown going.",
            ],

            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Builder) =>
            [
                "Quiet pride in the lanes: Bloomtown has a builder's legacy now, solid and lasting.",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Caretaker) =>
            [
                "Quiet pride in the lanes: Bloomtown has a caretaker's legacy — steady care remembered kindly.",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Connector) =>
            [
                "Quiet pride in the lanes: Bloomtown has a connector's legacy — friendships that outlast seasons.",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, _) =>
            [
                "Quiet pride in the lanes: Bloomtown has a new legacy, and it wears your name kindly.",
            ],

            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return PickLine(lines, variationSeed);
    }

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}