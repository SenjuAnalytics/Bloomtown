using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Ambient flavor lines for village life: time-of-day, location, emergent events, and NPC-to-NPC chatter.
/// </summary>
public static class VillageLifeDialogue
{
    public static string? TryGetLocationTimeComment(
        VillageAmbientLocation location,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed)
    {
        // Prefer location+time lines; fall back to location-only, then general time-of-day.
        var specific = TryGetLocationTimeSpecific(location, timeOfDay, developmentLevel, variationSeed);
        if (!string.IsNullOrWhiteSpace(specific))
            return specific;

        var locationOnly = TryGetLocationOnly(location, variationSeed);
        if (!string.IsNullOrWhiteSpace(locationOnly))
            return locationOnly;

        if (location == VillageAmbientLocation.General)
            return TryGetGeneralTimeComment(timeOfDay, developmentLevel, variationSeed);

        return null;
    }

    public static string? TryGetEmergentEvent(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed,
        out VillageEmergentEventKind eventKind) =>
        TryGetEmergentEvent(
            timeOfDay,
            developmentLevel,
            NpcInterpersonalRelationshipConfig.DefaultRelationship,
            variationSeed,
            out eventKind);

    /// <summary>
    /// Rare emergent village flavor — resting villagers, distant chatter, mood shifts, and small social moments.
    /// Elsie–Tom relationship tone shapes distant conversation and social gathering lines.
    /// </summary>
    public static string? TryGetEmergentEvent(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageEmergentEventKind eventKind)
    {
        eventKind = (VillageEmergentEventKind)(variationSeed % 4);

        var lines = eventKind switch
        {
            VillageEmergentEventKind.NpcResting => GetNpcRestingLines(timeOfDay),
            VillageEmergentEventKind.DistantConversation => GetDistantConversationLines(
                developmentLevel,
                interpersonalRelationship),
            VillageEmergentEventKind.VillageMoodShift => GetVillageMoodLines(timeOfDay, developmentLevel),
            VillageEmergentEventKind.SmallSocialGathering => GetSmallSocialGatheringLines(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship),
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetNpcToNpcComment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed)
    {
        return VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            timeOfDay,
            developmentLevel,
            Array.Empty<byte>(),
            NpcInterpersonalRelationshipConfig.DefaultRelationship,
            variationSeed,
            out _,
            out _);
    }

    private static string? TryGetLocationTimeSpecific(
        VillageAmbientLocation location,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed)
    {
        string[]? lines = (location, timeOfDay) switch
        {
            (VillageAmbientLocation.MarketSquare, GameTimeOfDay.Morning) =>
            [
                "Stalls are being set up — the square smells of bread and fresh herbs.",
                "Early shoppers greet each other; the market is waking up.",
            ],
            (VillageAmbientLocation.MarketSquare, GameTimeOfDay.Afternoon) =>
            [
                "The square hums with midday trade and easy laughter.",
                "Afternoon sun warms the cobbles; bargaining feels unhurried.",
            ],
            (VillageAmbientLocation.MarketSquare, GameTimeOfDay.Evening) =>
            [
                "Vendors pack up slowly, swapping stories from the day.",
                "The square softens at dusk — a few last deals, then home.",
            ],
            (VillageAmbientLocation.MarketSquare, GameTimeOfDay.Night) =>
            [
                "Empty stalls and quiet cobbles — only a lantern still glows.",
                "The market sleeps, but you catch a distant door closing.",
            ],

            (VillageAmbientLocation.CommunityGarden, GameTimeOfDay.Morning) =>
            [
                "Dew clings to the leaves; the garden feels freshly awake.",
                "Morning light catches the herbs — a calm start to the day.",
            ],
            (VillageAmbientLocation.CommunityGarden, GameTimeOfDay.Afternoon) =>
            [
                "Bees work the beds in the warm afternoon stillness.",
                "Someone left a watering can by the bench — a cared-for place.",
            ],
            (VillageAmbientLocation.CommunityGarden, GameTimeOfDay.Evening) =>
            [
                "Long shadows stretch across the beds; crickets begin nearby.",
                "The garden exhales at dusk — peaceful, unhurried.",
            ],
            (VillageAmbientLocation.CommunityGarden, GameTimeOfDay.Night) =>
            [
                "The garden rests under starlight; leaves whisper softly.",
                "A cool night breeze carries the scent of lavender.",
            ],

            (VillageAmbientLocation.RiversideWalk, GameTimeOfDay.Morning) =>
            [
                "Mist lifts off the water; the path feels fresh and new.",
                "Birdsong and the river set a gentle morning rhythm.",
            ],
            (VillageAmbientLocation.RiversideWalk, GameTimeOfDay.Afternoon) =>
            [
                "Sunlight dances on the current — an easy place to linger.",
                "The afternoon walk feels bright and unhurried.",
            ],
            (VillageAmbientLocation.RiversideWalk, GameTimeOfDay.Evening) =>
            [
                "The river turns gold at sunset; footsteps slow naturally.",
                "Evening light on the water makes the village feel connected.",
            ],
            (VillageAmbientLocation.RiversideWalk, GameTimeOfDay.Night) =>
            [
                "The river murmurs in the dark — steady, reassuring.",
                "Fireflies drift near the bank; the path feels intimate.",
            ],

            (VillageAmbientLocation.VillageWell, GameTimeOfDay.Morning) =>
            [
                "Neighbors gather at the well to start the day — quiet, friendly ritual.",
                "Morning buckets and greetings; the well is the village's first heartbeat.",
            ],
            (VillageAmbientLocation.VillageWell, GameTimeOfDay.Afternoon) =>
            [
                "A steady trickle of visitors at the well — life feels dependable.",
                "Someone refills a jug and waves; the well keeps everyone moving.",
            ],
            (VillageAmbientLocation.VillageWell, GameTimeOfDay.Evening) =>
            [
                "Last trips to the well before supper — unhurried, familiar.",
                "The well catches the last warm light of the day.",
            ],
            (VillageAmbientLocation.VillageWell, GameTimeOfDay.Night) =>
            [
                "The well is quiet now, but water still sounds comforting.",
                "A lone lantern near the well sways in the night breeze.",
            ],

            (VillageAmbientLocation.RepairedBridge, GameTimeOfDay.Morning) =>
            [
                "Early travelers cross the bridge — a safe start to the day.",
                "Morning mist clings to the river below the repaired planks.",
            ],
            (VillageAmbientLocation.RepairedBridge, GameTimeOfDay.Afternoon) =>
            [
                "Footsteps cross the bridge at an easy afternoon pace.",
                "From the bridge you can see the village stirring on both banks.",
            ],
            (VillageAmbientLocation.RepairedBridge, GameTimeOfDay.Evening) =>
            [
                "Someone pauses mid-bridge to watch the sunset on the water.",
                "The bridge feels like a meeting place as the day winds down.",
            ],

            (VillageAmbientLocation.VillageWarehouse, GameTimeOfDay.Morning) =>
            [
                "Crates shift inside as helpers start the morning sort.",
                "The warehouse doors open — shared supplies, shared purpose.",
            ],
            (VillageAmbientLocation.VillageWarehouse, GameTimeOfDay.Afternoon) =>
            [
                "Afternoon light through the warehouse windows — orderly and calm.",
                "A neighbor signs for a delivery; the village feels organized.",
            ],
            (VillageAmbientLocation.VillageWarehouse, GameTimeOfDay.Evening) =>
            [
                "The day's tally is done; the warehouse settles for the night.",
                "Supplies stacked and secure — a quiet sense of readiness.",
            ],

            _ => null,
        };

        if (lines is null || lines.Length == 0)
            return null;

        if (developmentLevel == VillageDevelopmentLevel.Bustling && variationSeed % 4 == 0)
        {
            return location switch
            {
                VillageAmbientLocation.MarketSquare => "The busy square never seems to empty anymore — Bloomtown has grown.",
                VillageAmbientLocation.CommunityGarden => "More hands tend the garden now; everyone shares the harvest.",
                VillageAmbientLocation.RiversideWalk => "The riverside path sees more walkers since the village flourished.",
                VillageAmbientLocation.VillageWell => "Laughter at the well carries farther these days.",
                _ => lines[(int)(variationSeed % (uint)lines.Length)],
            };
        }

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetLocationOnly(VillageAmbientLocation location, uint variationSeed)
    {
        var lines = location switch
        {
            VillageAmbientLocation.MarketSquare => VillageAreaNpcDialogue.GetMarketSquareLines(),
            VillageAmbientLocation.CommunityGarden => VillageAreaNpcDialogue.GetCommunityGardenLines(),
            VillageAmbientLocation.RiversideWalk => VillageAreaNpcDialogue.GetRiversideWalkLines(),
            VillageAmbientLocation.VillageWell =>
            [
                "Fresh water draws a steady flow of neighbors throughout the day.",
                "The well remains the village's simplest comfort.",
            ],
            VillageAmbientLocation.RepairedBridge =>
            [
                "Safe planks and steady rails — the crossing feels trustworthy again.",
                "The bridge links both sides of Bloomtown with quiet confidence.",
            ],
            VillageAmbientLocation.VillageWarehouse =>
            [
                "Shared storage gives the village a backbone.",
                "Crates and shelves tell the story of a growing town.",
            ],
            _ => null,
        };

        if (lines is null || lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetGeneralTimeComment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed)
    {
        var lines = timeOfDay switch
        {
            GameTimeOfDay.Morning =>
            [
                "Bloomtown wakes gently — smoke rises from chimneys and doors creak open.",
                "Morning light softens the edges of the village; a new day begins.",
                developmentLevel >= VillageDevelopmentLevel.Lively
                    ? "The village stirs earlier now — a sign of how much it has grown."
                    : "A peaceful morning hush still holds the village.",
            ],
            GameTimeOfDay.Afternoon =>
            [
                "Afternoon warmth settles over the lanes; life moves at a steady pace.",
                "You hear distant chores and friendly calls between neighbors.",
                developmentLevel == VillageDevelopmentLevel.Bustling
                    ? "The afternoon bustle feels like the village's natural heartbeat."
                    : "The village hums quietly through the afternoon.",
            ],
            GameTimeOfDay.Evening =>
            [
                "Lanterns begin to glow as the village eases toward suppertime.",
                "Evening brings softer voices and the smell of cooking nearby.",
                "The day's work slows; Bloomtown feels homely at dusk.",
            ],
            GameTimeOfDay.Night =>
            [
                "Night folds over Bloomtown — most windows dark, a few still lit.",
                "Crickets and a distant dog; the village rests.",
                developmentLevel >= VillageDevelopmentLevel.Lively
                    ? "Even at night you sense neighbors close by — comforting."
                    : "A deep calm holds the village after dark.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string[] GetNpcRestingLines(GameTimeOfDay timeOfDay) =>
        timeOfDay switch
        {
            GameTimeOfDay.Morning =>
            [
                "You spot a villager resting on a porch stoop, sipping tea before the day picks up.",
                "Someone leans against a fence post, watching the village wake.",
            ],
            GameTimeOfDay.Afternoon =>
            [
                "A villager sits in the shade, fanning themselves after midday chores.",
                "You notice someone resting on a bench, content to watch the afternoon pass.",
            ],
            GameTimeOfDay.Evening =>
            [
                "A neighbor rocks slowly on a porch, enjoying the cool of evening.",
                "Someone rests by a doorway, greeting passersby without getting up.",
            ],
            _ => 
            [
                "A lone figure sits by a dim window, still up though the village sleeps.",
                "You glimpse someone resting near a lantern — a quiet night moment.",
            ],
        };

    private static string[] GetDistantConversationLines(
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship)
    {
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;

        if (developmentLevel == VillageDevelopmentLevel.Quiet)
        {
            return friendly
                ?
                [
                    "Soft voices drift from a cottage — Elsie and Tom, maybe, trading the day's small stories.",
                    "Two silhouettes talk by a fence; the village feels intimate and lived-in.",
                ]
                :
                [
                    "Soft voices drift from a cottage — too far to catch the words.",
                    "Two silhouettes talk by a fence; the village feels intimate.",
                ];
        }

        return friendly
            ?
            [
                "From across the lane, Elsie and Tom chat easily — warmth in the distance.",
                "You hear laughter between neighbors you cannot quite see — fond, unhurried.",
                "A pair of friends compare notes on the day's work, voices carrying on the breeze.",
                "Distant voices: someone teases Tom; Elsie answers without missing a beat.",
            ]
            :
            [
                "From across the lane, two villagers chat easily — warmth in the distance.",
                "You hear practical updates between neighbors you cannot quite see.",
                "A pair of villagers compare notes on the day's work, voices carrying on the breeze.",
            ];
    }

    private static string[] GetSmallSocialGatheringLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship)
    {
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;
        var lines = new List<string>
        {
            friendly
                ? "A handful of neighbors cluster near the lane — Elsie and Tom's voices set the easy tone."
                : "A small group pauses near shared work — short talk, then back to chores.",
            friendly
                ? "Someone sets out an extra cup without asking — the village expects company today."
                : "Two villagers compare a chore list aloud — practical, brief, enough.",
        };

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add(friendly
                ? "Lantern light gathers neighbors on a porch — supper stories traded before anyone heads home."
                : "Evening voices trade tomorrow's errands — quiet planning, shared load.");

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add("Bloomtown hums with small conversations that overlap and fade — a living village.");

        return lines.ToArray();
    }

    private static string[] GetVillageMoodLines(GameTimeOfDay timeOfDay, VillageDevelopmentLevel developmentLevel)
    {
        if (developmentLevel == VillageDevelopmentLevel.Bustling)
        {
            return
            [
                "For a moment the village feels especially alive — footsteps, voices, purpose.",
                "A cheerful bustle moves through the lanes; Bloomtown feels like it belongs to everyone.",
            ];
        }

        if (developmentLevel == VillageDevelopmentLevel.Lively)
        {
            return timeOfDay == GameTimeOfDay.Night
                ?
                [
                    "The village is quieter tonight, but not lonely — a gentle calm.",
                ]
                :
                [
                    "The village carries a hopeful energy today — not loud, but present.",
                    "Something about Bloomtown feels a little busier than it used to.",
                ];
        }

        return
        [
            "The village feels especially peaceful right now — unhurried, unguarded.",
            "A hush settles over the lanes; even the air seems to slow down.",
        ];
    }

}

/// <summary>Lightweight emergent flavor events — text only, no gameplay impact.</summary>
public enum VillageEmergentEventKind : byte
{
    NpcResting = 0,
    DistantConversation = 1,
    VillageMoodShift = 2,
    SmallSocialGathering = 3,
}