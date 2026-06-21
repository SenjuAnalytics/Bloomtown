using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Community moments and community-help ambient reactions for village social fabric flavor.
/// </summary>
internal static class VillageSocialFabricDialogue
{
    internal static string? TryGetCommunityMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        momentKind = (VillageCommunityMomentKind)(variationSeed % 4);
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;

        var lines = momentKind switch
        {
            VillageCommunityMomentKind.WarmGathering => GetWarmGatheringLines(timeOfDay, developmentLevel, friendly),
            VillageCommunityMomentKind.SmallPreparation => GetSmallPreparationLines(timeOfDay, developmentLevel, friendly),
            VillageCommunityMomentKind.DistantCluster => GetDistantClusterLines(developmentLevel, friendly),
            VillageCommunityMomentKind.NeighborlyPause => GetNeighborlyPauseLines(timeOfDay, developmentLevel, friendly),
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    internal static string? TryGetCommunityHelpFollowUp(
        CommunityActivityKind activity,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed)
    {
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden => friendly
                ?
                [
                    "Word'll spread — the garden looks cared for by someone who belongs here.",
                    "That's the kind of help neighbors remember at supper.",
                ]
                :
                [
                    "The village notices steady hands on shared work.",
                    "Good help — people will mention it quietly.",
                ],
            CommunityActivityKind.HelpMarket => friendly
                ?
                [
                    "The square feels friendlier already — gossip will be kind today.",
                    "Bloomtown likes volunteers who show up without being asked.",
                ]
                :
                [
                    "Practical help — the market runs better when folks pitch in.",
                    "Someone will pass that along before the day ends.",
                ],
            CommunityActivityKind.HelpWell => friendly
                ?
                [
                    "Shared spots stay welcoming because people like you tend them.",
                    "The village will hear about this — the good kind of gossip.",
                ]
                :
                [
                    "Useful work around the well — neighbors appreciate upkeep.",
                    "That kind of care keeps the village feeling together.",
                ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    internal static string? TryGetCommunityHelpAmbientReaction(
        CommunityActivityKind activity,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint npcEntityId,
        uint variationSeed)
    {
        var lines = activity switch
        {
            CommunityActivityKind.HelpGarden => GetGardenHelpReactions(interpersonalRelationship, npcEntityId),
            CommunityActivityKind.HelpMarket => GetMarketHelpReactions(interpersonalRelationship, npcEntityId),
            CommunityActivityKind.HelpWell => GetWellHelpReactions(interpersonalRelationship, npcEntityId),
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string[] GetWarmGatheringLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        bool friendly)
    {
        var lines = new List<string>
        {
            friendly
                ? "A small cluster of neighbors laughs over shared chores — Elsie and Tom's voices blend into the rest."
                : "A few villagers compare notes by a doorway — practical talk, but it warms the lane.",
            "Someone sets out an extra stool without being asked — the village expects company.",
        };

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add("From across the square, familiar faces linger longer than their errands require — Bloomtown enjoys itself.");

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add("Lantern light gathers a handful of neighbors — supper stories traded before anyone heads home.");

        if (timeOfDay == GameTimeOfDay.Morning && friendly)
            lines.Add("Morning greetings pile up near the well — everyone seems to know whose turn it is to smile first.");

        return lines.ToArray();
    }

    private static string[] GetSmallPreparationLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        bool friendly)
    {
        var lines = new List<string>
        {
            friendly
                ? "Elsie straightens a bench while Tom counts baskets — small preparations that keep the village welcoming."
                : "Tom stacks crates while Elsie checks a list — quiet prep work holding the day together.",
            "Someone sweeps a path that was already tidy — the village likes to look ready for company.",
        };

        if (developmentLevel >= VillageDevelopmentLevel.Bustling)
            lines.Add("Extra hands appear for a five-minute chore — no announcement, just neighbors showing up.");

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add("A table is wiped down in the square — not for trade yet, just because the village cares how it looks.");

        return lines.ToArray();
    }

    private static string[] GetNeighborlyPauseLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        bool friendly)
    {
        var lines = new List<string>
        {
            friendly
                ? "Elsie and Tom pause mid-errand to trade a story — the village slows down around them, briefly and kindly."
                : "Elsie and Tom pause to compare notes on the afternoon list — brief, practical, enough.",
            friendly
                ? "Someone laughs at Elsie and Tom's easy bickering; neither minds the audience."
                : "Two neighbors watch Elsie and Tom coordinate a chore, then return to their own work.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add(friendly
                ? "Morning light catches Elsie handing Tom a basket without a word — an old rhythm, comfortable."
                : "Morning exchange: Tom confirms a delivery; Elsie marks it and moves on.");

        if (developmentLevel >= VillageDevelopmentLevel.Bustling)
            lines.Add("A small group lingers near shared work — Bloomtown enjoys these unplanned pauses.");

        return lines.ToArray();
    }

    private static string[] GetDistantClusterLines(VillageDevelopmentLevel developmentLevel, bool friendly)
    {
        if (developmentLevel == VillageDevelopmentLevel.Quiet)
        {
            return
            [
                friendly
                    ? "Two silhouettes talk by a fence — Elsie and Tom, maybe, keeping the village clock steady."
                    : "Distant voices trade short updates between errands — too far to catch every word.",
                "You glimpse neighbors paused in conversation, then returning to their work — a living village.",
            ];
        }

        return
        [
            friendly
                ? "From the lane you hear Elsie and Tom finishing each other's sentences — old rhythm, good company."
                : "Across the way, villagers cluster briefly around shared work, then drift back to their routes.",
            "Laughter carries from a knot of neighbors you cannot quite see — Bloomtown sounds inhabited.",
            "A pair of friends compare the day's gossip at a distance — the village circulates its own stories.",
        ];
    }

    private static string[] GetGardenHelpReactions(
        NpcInterpersonalRelationship interpersonalRelationship,
        uint npcEntityId)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Elsie calls over, \"The beds look loved — Tom will pretend he noticed before I did.\"",
                    "Elsie wipes her hands and smiles. \"That's the sort of help that makes a garden feel shared.\"",
                ]
                :
                [
                    "Elsie nods approvingly. \"Good work on the beds — the village will notice.\"",
                    "Elsie says quietly, \"Thank you. Shared gardens need steady hands.\"",
                ];
        }

        if (npcEntityId == NpcEntityIds.Tom)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Tom grins. \"Elsie will claim she planned those weeds out herself — don't let her.\"",
                    "Tom leans on a rake. \"Garden help like that? Bloomtown remembers.\"",
                ]
                :
                [
                    "Tom tips his hat. \"Solid work — keeps the place welcoming.\"",
                    "Tom says, \"Appreciated. The garden's a village face, not just dirt and herbs.\"",
                ];
        }

        return
        [
            "A neighbor pauses. \"The garden looks better already — thanks for pitching in.\"",
            "Someone nearby murmurs approval — your help did not go unnoticed.",
        ];
    }

    private static string[] GetMarketHelpReactions(
        NpcInterpersonalRelationship interpersonalRelationship,
        uint npcEntityId)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Elsie waves from a stall. \"You make the square feel like it belongs to all of us.\"",
                    "Elsie laughs softly. \"Tom said the market ran smoother — I told him you deserved the credit.\"",
                ]
                :
                [
                    "Elsie says, \"Efficient help — the square works better when people step in.\"",
                    "Elsie nods. \"Good timing. Trade days need extra hands.\"",
                ];
        }

        if (npcEntityId == NpcEntityIds.Tom)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Tom calls out, \"Market's lucky to have you — Elsie already told half the lane.\"",
                    "Tom stacks a crate you straightened. \"That's the kind of help gossip loves — the good kind.\"",
                ]
                :
                [
                    "Tom says, \"Handy work. The square runs cleaner when folks pitch in.\"",
                    "Tom nods toward the stalls. \"Appreciated — makes the day easier for everyone.\"",
                ];
        }

        return
        [
            "A vendor smiles. \"The market feels friendlier when neighbors help set the tone.\"",
            "Someone nearby thanks you — the square seems to carry the goodwill forward.",
        ];
    }

    private static string[] GetWellHelpReactions(
        NpcInterpersonalRelationship interpersonalRelationship,
        uint npcEntityId)
    {
        if (npcEntityId == NpcEntityIds.Elsie)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Elsie sets down a bucket. \"The well's the village's front door — thank you for polishing it.\"",
                    "Elsie smiles. \"Tom will brag that the gathering spot looks sharp. He's not wrong today.\"",
                ]
                :
                [
                    "Elsie says, \"Tidy work around the well — everyone benefits.\"",
                    "Elsie nods. \"That kind of upkeep keeps the village feeling cared for.\"",
                ];
        }

        if (npcEntityId == NpcEntityIds.Tom)
        {
            return interpersonalRelationship == NpcInterpersonalRelationship.Friendly
                ?
                [
                    "Tom wipes his hands. \"Elsie said the well looked welcoming — I told her you earned that.\"",
                    "Tom grins. \"Good help. Folks gather here because someone always tends the details.\"",
                ]
                :
                [
                    "Tom says, \"Solid chore — the well stays important when people look after it.\"",
                    "Tom nods toward the rim. \"Appreciated. Shared spots need shared care.\"",
                ];
        }

        return
        [
            "A villager thanks you — the well feels more welcoming already.",
            "Someone nearby notes your help aloud — small upkeep, village-sized gratitude.",
        ];
    }
}