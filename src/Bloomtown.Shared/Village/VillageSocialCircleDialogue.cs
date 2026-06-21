using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Social lines and gossip for the expanded village circle — Mira (merchant) and Harold (elder).
/// Static pair tones differ from Elsie–Tom: Elsie–Mira is warm; Tom–Harold stays practical.
/// </summary>
internal static class VillageSocialCircleDialogue
{
    internal static string? TryGetExpandedNpcToNpcLine(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId,
        out VillageSocialPair? socialPair)
    {
        preferredSpeakerNpcEntityId = 0;
        socialPair = null;

        // Alternate between the two new static pairs so the circle feels wider, not Elsie–Tom only.
        var pair = variationSeed % 2 == 0
            ? VillageSocialPair.ElsieMira
            : VillageSocialPair.TomHarold;

        socialPair = pair;
        var relationship = VillageSocialCircleConfig.GetPairRelationship(pair);
        var lines = pair switch
        {
            VillageSocialPair.ElsieMira => GetElsieMiraLines(relationship, timeOfDay, developmentLevel),
            VillageSocialPair.TomHarold => GetTomHaroldLines(relationship, timeOfDay, developmentLevel),
            _ => Array.Empty<SocialLineEntry>(),
        };

        if (lines.Length == 0)
            return null;

        _ = elsieTomRelationship;
        var entry = lines[(int)(variationSeed % (uint)lines.Length)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    internal static string? TryGetSocialCircleGossip(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind,
        out VillageSocialPair? socialPair)
    {
        gossipKind = VillageGossipKind.WiderSocialCircle;
        socialPair = null;

        var pool = variationSeed % 3;
        var lines = pool switch
        {
            0 => GetElsieMiraGossipLines(),
            1 => GetTomHaroldGossipLines(),
            _ => GetMixedCircleGossipLines(elsieTomRelationship),
        };

        if (lines.Length == 0)
            return null;

        socialPair = pool switch
        {
            0 => VillageSocialPair.ElsieMira,
            1 => VillageSocialPair.TomHarold,
            _ => null,
        };

        _ = developmentLevel;
        _ = timeOfDay;
        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    internal static string? TryGetGroupSocialMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        momentKind = VillageCommunityMomentKind.GroupGathering;
        var friendlyElsieTom = elsieTomRelationship == NpcInterpersonalRelationship.Friendly;
        var lines = GetGroupMomentLines(timeOfDay, developmentLevel, friendlyElsieTom);

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static SocialLineEntry[] GetElsieMiraLines(
        NpcInterpersonalRelationship relationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        if (relationship == NpcInterpersonalRelationship.Friendly)
        {
            var friendly = new List<SocialLineEntry>
            {
                new(NpcEntityIds.Elsie, "Mira saved me the best stall spot again — she says it's nothing, but the market feels brighter for it."),
                new(NpcEntityIds.Mira, "Elsie brought garden herbs to trade before sunrise. Old friends don't wait for opening bell."),
                new(NpcEntityIds.Elsie, "Mira already knows who's buying apples today. I just help carry the gossip."),
                new(NpcEntityIds.Mira, "Elsie fusses over my ledger like it's her own garden — rude, but accurate."),
            };

            if (timeOfDay == GameTimeOfDay.Morning)
                friendly.Add(new(NpcEntityIds.Mira, "Elsie and I still compare the morning list over tea — market days start better that way."));

            if (developmentLevel >= VillageDevelopmentLevel.Lively)
                friendly.Add(new(NpcEntityIds.Elsie, "Mira says the square's busier every week. I told her Bloomtown finally sounds like itself."));

            return friendly.ToArray();
        }

        return
        [
            new(NpcEntityIds.Elsie, "Mira asked about garden surplus for the stalls. Straightforward trade talk."),
            new(NpcEntityIds.Mira, "Elsie noted which herbs sell fastest. Useful information, nothing more."),
        ];
    }

    private static SocialLineEntry[] GetTomHaroldLines(
        NpcInterpersonalRelationship relationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        if (relationship == NpcInterpersonalRelationship.Friendly)
        {
            return
            [
                new(NpcEntityIds.Tom, "Harold told me which fence posts need replacing — elder eyes catch what I miss."),
                new(NpcEntityIds.Harold, "Tom still pretends he doesn't ask for advice. He asks every week."),
            ];
        }

        var neutral = new List<SocialLineEntry>
        {
            new(NpcEntityIds.Tom, "Harold asked about timber for the lane repairs. I said I'd measure tomorrow."),
            new(NpcEntityIds.Harold, "Tom reported the woods path is clear. Brief update, dependable as always."),
            new(NpcEntityIds.Tom, "Harold keeps the afternoon errands organized. I handle what he points at."),
            new(NpcEntityIds.Harold, "Tom mentioned the bridge timbers again. Practical concern — I'll pass it along."),
        };

        if (timeOfDay == GameTimeOfDay.Afternoon)
            neutral.Add(new(NpcEntityIds.Harold, "Tom checked the supply count at noon. Elder habit: listen first, advise second."));

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            neutral.Add(new(NpcEntityIds.Tom, "Harold says the village lanes feel busier. He's usually right about that."));

        return neutral.ToArray();
    }

    private static string[] GetElsieMiraGossipLines() =>
    [
        "Market gossip: Mira says Elsie knows every regular by name before breakfast — Elsie says Mira exaggerates.",
        "You overhear someone say Elsie and Mira plan trade days like gardeners plan seasons — different work, same care.",
        "Soft rumor: Mira saved Elsie the last basket of late apples. The village heard about it before lunch.",
        "A neighbor swears Mira and Elsie argue over prices like sisters and settle like partners.",
    ];

    private static string[] GetTomHaroldGossipLines() =>
    [
        "Village talk: Harold asked Tom to check the old bridge timbers again — Tom grumbled, then went.",
        "Someone says Tom and Harold compare notes on lane repairs every week — not close, but steady.",
        "Quiet gossip: Harold still remembers when Tom fixed the well ropes without being asked.",
        "A villager notes Tom passes Harold the afternoon tallies — elder habit, woodsman's respect.",
    ];

    private static string[] GetMixedCircleGossipLines(NpcInterpersonalRelationship elsieTomRelationship)
    {
        if (elsieTomRelationship == NpcInterpersonalRelationship.Friendly)
        {
            return
            [
                "Bloomtown gossip: Mira says the square's fuller since Harold started his evening porch rounds.",
                "You overhear four names in one breath — Elsie, Tom, Mira, Harold — like the village finally has a chorus.",
                "Someone laughs that Mira knows the market, Harold knows the lanes, and Elsie knows everyone else's business.",
            ];
        }

        return
        [
            "Village rumor: Mira, Harold, Elsie, and Tom keep different corners of Bloomtown running — quiet division of labor.",
            "You catch gossip about the market, the woods, and the garden — four neighbors, four steady roles.",
            "Someone says Bloomtown feels less like two voices and more like a proper circle lately.",
        ];
    }

    private static string[] GetGroupMomentLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        bool friendlyElsieTom)
    {
        var lines = new List<string>
        {
            friendlyElsieTom
                ? "Near the square, Elsie, Mira, and Tom compare the afternoon list — easy overlap, shared village work."
                : "Near the square, Elsie, Mira, and Tom trade short updates — practical voices, shared errands.",
            friendlyElsieTom
                ? "Harold settles on a bench while Mira counts crates and Elsie teases Tom about fence posts — Bloomtown sounds inhabited."
                : "Harold listens while Mira and Tom sort deliveries — elder patience, merchant pace, woodsman's hands.",
            "A small knot of neighbors lingers longer than their chores require — four familiar faces, one gentle bustle.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add("Morning greetings ripple from stall to porch — Mira, Elsie, Harold, and Tom all in the same breath.");

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add("Lantern light gathers a handful of villagers — supper talk passes between Elsie, Tom, Mira, and Harold.");

        if (developmentLevel >= VillageDevelopmentLevel.Bustling)
            lines.Add("The lane hums with overlapping conversations — Bloomtown finally sounds like more than a pair of old friends.");

        return lines.ToArray();
    }

    private readonly record struct SocialLineEntry(uint SpeakerNpcEntityId, string Text);
}