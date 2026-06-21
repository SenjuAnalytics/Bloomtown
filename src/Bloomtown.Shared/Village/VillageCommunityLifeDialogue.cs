using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// NPC-to-NPC social lines and village gossip templates for community life flavor.
/// </summary>
internal static class VillageCommunityLifeDialogue
{
    internal static string? TryGetGeneralNpcToNpcLine(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        preferredSpeakerNpcEntityId = 0;
        var lines = NpcInterpersonalDialogue.GetGeneralLines(
            interpersonalRelationship,
            timeOfDay,
            developmentLevel);

        if (lines.Length == 0)
            return null;

        var entry = lines[(int)(variationSeed % (uint)lines.Length)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    internal static string? TryGetProjectNpcToNpcLine(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        preferredSpeakerNpcEntityId = 0;
        var lines = NpcInterpersonalDialogue.GetProjectLines(
            interpersonalRelationship,
            completedProjectIds,
            developmentLevel);

        if (lines.Count == 0)
            return null;

        _ = timeOfDay;
        var entry = lines[(int)(variationSeed % (uint)lines.Count)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    internal static string? TryGetDevelopmentSocialLine(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        preferredSpeakerNpcEntityId = 0;
        var lines = NpcInterpersonalDialogue.GetDevelopmentLines(
            interpersonalRelationship,
            timeOfDay,
            developmentLevel);

        if (lines.Length == 0)
            return null;

        var entry = lines[(int)(variationSeed % (uint)lines.Length)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    /// <summary>Warm Elsie–Tom bond lines when their relationship has grown friendly.</summary>
    internal static string? TryGetInterpersonalBondLine(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        preferredSpeakerNpcEntityId = 0;
        var lines = NpcInterpersonalDialogue.GetBondLines(timeOfDay, developmentLevel);

        if (lines.Length == 0)
            return null;

        var entry = lines[(int)(variationSeed % (uint)lines.Length)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    internal static string? TryGetNeighborlyGossip(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind)
    {
        gossipKind = VillageGossipKind.NeighborlyRumor;

        if (interpersonalRelationship == NpcInterpersonalRelationship.Friendly)
        {
            var lines = new List<string>
            {
                "You catch a fragment of gossip: Elsie and Tom argued fondly over whose turn it is to check the garden beds.",
                "Someone whispers that Tom told Elsie a joke so bad she pretended to swat him with a dish towel.",
                "A neighbor insists Elsie already knows everyone's business before breakfast — Tom swears it's true.",
                "Village talk: Tom claims Elsie fusses too much; Elsie says that's why the lanes stay friendly.",
                "Soft gossip: someone saw Tom save Elsie the last seat at supper — neither mentioned it aloud.",
                "A neighbor laughs that Elsie and Tom finish each other's sentences when chores pile up.",
            };

            if (timeOfDay == GameTimeOfDay.Morning)
                lines.Add("Morning gossip: Tom promised Elsie he'd fix a loose fence post before noon. Nobody's betting against him.");

            if (timeOfDay == GameTimeOfDay.Evening)
                lines.Add("Evening rumor: Elsie and Tom are planning tomorrow's chores aloud on someone's porch.");

            if (developmentLevel >= VillageDevelopmentLevel.Lively)
                lines.Add("Soft chatter nearby: folks say Elsie and Tom are the village's unofficial clock — steady, reliable.");

            if (timeOfDay == GameTimeOfDay.Afternoon)
                lines.Add("Afternoon gossip: someone swears Elsie already knows who helped in the garden — Tom says she's never wrong.");

            return lines[(int)(variationSeed % (uint)lines.Count)];
        }

        var neutralLines = new List<string>
        {
            "You overhear someone say Elsie and Tom coordinated the morning chores without much fuss.",
            "A neighbor mentions Tom checked in with Elsie about supplies — practical, routine.",
            "Soft gossip: Elsie and Tom keep to their own errands, but they stay in step when work needs doing.",
            "A villager notes Elsie and Tom trade updates at the well — not close friends, but reliable neighbors.",
            "Quiet rumor: Tom passed a message to Elsie about tomorrow's fence post — efficient, no small talk.",
            "Someone says Elsie and Tom work well side by side when the village needs it, then go their separate ways.",
            "You catch a fragment: Elsie asked if Tom covered the afternoon run. He had. That was the whole conversation.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            neutralLines.Add("Morning rumor: Tom said he'd handle the fence post. Elsie nodded and moved on.");

        if (timeOfDay == GameTimeOfDay.Evening)
            neutralLines.Add("Evening rumor: Tom and Elsie compared tomorrow's chores — brief, efficient, dependable.");

        return neutralLines[(int)(variationSeed % (uint)neutralLines.Count)];
    }

    internal static string? TryGetProjectGossip(
        GameTimeOfDay timeOfDay,
        IReadOnlyCollection<byte> completedProjectIds,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind)
    {
        gossipKind = VillageGossipKind.ProjectPride;
        var lines = new List<string>();
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
        {
            lines.Add("Village gossip: someone swears the well water tastes better since the whole town pitched in.");
            lines.Add("You overhear pride in a neighbor's voice — the finished well still feels like a shared victory.");
            if (friendly)
                lines.Add("Someone repeats Elsie's joke — Tom finished the well, but she still claims she kept the queue polite.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
        {
            lines.Add("Rumor on the breeze: Tom told half the village the bridge held firm in last week's rain.");
            lines.Add("Someone repeats what Elsie said — crossing the bridge feels like trusting your neighbors again.");
            if (friendly)
                lines.Add("Friendly gossip: Elsie and Tom still brag about the bridge like proud siblings.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId))
        {
            lines.Add("Warehouse gossip: deliveries are steadier now, and everyone credits the shared stores.");
            lines.Add("A villager mutters that Elsie already sorted the afternoon crates before anyone asked.");
            if (friendly)
                lines.Add("You overhear Tom teasing Elsie for knowing every crate by heart — she doesn't deny it.");
        }

        if (lines.Count == 0)
            return null;

        _ = timeOfDay;
        return lines[(int)(variationSeed % (uint)lines.Count)];
    }

    /// <summary>Gossip that ties completed village work to Elsie–Tom's social bond.</summary>
    internal static string? TryGetInterpersonalProjectGossip(
        IReadOnlyCollection<byte> completedProjectIds,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind)
    {
        gossipKind = VillageGossipKind.SocialConnection;
        var friendly = interpersonalRelationship == NpcInterpersonalRelationship.Friendly;
        var lines = new List<string>();

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
        {
            lines.Add(friendly
                ? "Village talk: Elsie still thanks Tom for the well ropes every morning — Tom pretends to be embarrassed."
                : "Quiet rumor: Tom checks the well because Elsie asked once. He never stopped.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
        {
            lines.Add(friendly
                ? "Someone says Elsie and Tom crossed the bridge together just to hear it stay quiet — old friends celebrating small wins."
                : "You overhear that Tom and Elsie still compare notes whenever the bridge gets busy.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId))
        {
            lines.Add(friendly
                ? "Warehouse gossip: Elsie and Tom argue fondly over who organizes faster — the village enjoys listening."
                : "A neighbor says the warehouse runs smoother when Tom and Elsie are both in earshot.");
        }

        if (lines.Count == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Count)];
    }

    internal static string? TryGetCommunityMoodGossip(
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay,
        uint variationSeed,
        out VillageGossipKind gossipKind)
    {
        gossipKind = VillageGossipKind.CommunityMood;

        var lines = developmentLevel switch
        {
            VillageDevelopmentLevel.Bustling =>
            [
                "Community talk: folks say Bloomtown hasn't felt this warmly busy in years.",
                "You overhear someone say the village finally sounds like a place with a shared future.",
                "A neighbor laughs that every lane has a familiar face now — the gossip is affectionate, not sharp.",
            ],
            VillageDevelopmentLevel.Lively =>
            [
                "Soft rumor: the village feels a little more connected each week — not louder, just closer.",
                "Someone says Bloomtown is finding its rhythm again; the tone is hopeful.",
            ],
            _ => Array.Empty<string>(),
        };

        if (lines.Length == 0)
            return null;

        if (timeOfDay == GameTimeOfDay.Night && developmentLevel == VillageDevelopmentLevel.Bustling)
        {
            gossipKind = VillageGossipKind.CommunityMood;
            return "Night gossip: even after dark, neighbors check each other's lanterns — a quiet kind of bustle.";
        }

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    /// <summary>Small emergent social moments — village life glimpses tied to Elsie–Tom relationship tone.</summary>
    internal static string? TryGetEmergentSocialMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        momentKind = VillageCommunityMomentKind.NeighborlyPause;
        var lines = NpcInterpersonalDialogue.GetEmergentSocialMomentLines(
            interpersonalRelationship,
            timeOfDay,
            developmentLevel);

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    internal static string? TryGetSocialConnectionGossip(
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind)
    {
        gossipKind = VillageGossipKind.SocialConnection;

        if (interpersonalRelationship == NpcInterpersonalRelationship.Friendly)
        {
            var friendlyLines = new[]
            {
                "You overhear Elsie's name and Tom's in the same breath — old friends, still keeping the village stitched together.",
                "Someone says Tom and Elsie argue like siblings and mend fences like neighbors. Everyone smiles at that.",
                "Village gossip, fond and plain: if one of them knows your name, the other probably does too.",
                "A neighbor swears Elsie already heard today's gossip from Tom before lunch — and improved it.",
                "Soft village talk: Tom and Elsie still check on each other like it's the most ordinary kindness in Bloomtown.",
                "Someone repeats what Elsie said — Tom keeps the village steady, and she keeps him fed. Tom pretends not to hear.",
                "Friendly rumor: the whole lane knows Elsie and Tom's bickering is just another way of looking out for each other.",
            };

            return friendlyLines[(int)(variationSeed % (uint)friendlyLines.Length)];
        }

        var neutralLines = new[]
        {
            "You overhear a neighbor note that Elsie and Tom keep each other informed — not close, but dependable.",
            "Someone says Tom and Elsie work well side by side when the village needs it. That's enough for most folks.",
            "Quiet gossip: Elsie and Tom aren't inseparable, but Bloomtown counts on both of them.",
            "A villager mentions Tom passed a message to Elsie about tomorrow's errands — routine, reliable.",
            "You catch a fragment: Elsie and Tom coordinate without fuss, the way long neighbors do.",
        };

        return neutralLines[(int)(variationSeed % (uint)neutralLines.Length)];
    }
}