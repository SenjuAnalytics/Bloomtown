using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Npc;

/// <summary>
/// Elsie–Tom NPC-to-NPC lines grouped by interpersonal relationship tone.
/// </summary>
internal static class NpcInterpersonalDialogue
{
    /// <summary>
    /// General neighbor chatter — Neutral is practical; Friendly is warm and familiar.
    /// </summary>
    internal static SocialLineEntry[] GetGeneralLines(
        NpcInterpersonalRelationship relationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        if (relationship == NpcInterpersonalRelationship.Friendly)
            return BuildFriendlyGeneralPool(timeOfDay, developmentLevel);

        return BuildNeutralGeneralPool(timeOfDay);
    }

    /// <summary>
    /// Project-aware lines reference shared village work; tone follows relationship status.
    /// </summary>
    internal static IReadOnlyList<SocialLineEntry> GetProjectLines(
        NpcInterpersonalRelationship relationship,
        IReadOnlyCollection<byte> completedProjectIds,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<SocialLineEntry>();

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
            lines.AddRange(relationship == NpcInterpersonalRelationship.Friendly
                ? FriendlyWellLines
                : NeutralWellLines);

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
            lines.AddRange(relationship == NpcInterpersonalRelationship.Friendly
                ? FriendlyBridgeLines
                : NeutralBridgeLines);

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId))
            lines.AddRange(relationship == NpcInterpersonalRelationship.Friendly
                ? FriendlyWarehouseLines
                : NeutralWarehouseLines);

        if (developmentLevel >= VillageDevelopmentLevel.Lively
            && relationship == NpcInterpersonalRelationship.Friendly
            && lines.Count > 0)
        {
            lines.Add(new SocialLineEntry(
                NpcEntityIds.Elsie,
                "Tom and I still compare notes on the projects we finished — old habits, good ones."));
        }

        return lines;
    }

    internal static SocialLineEntry[] GetDevelopmentLines(
        NpcInterpersonalRelationship relationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        if (relationship != NpcInterpersonalRelationship.Friendly)
            return Array.Empty<SocialLineEntry>();

        var lines = developmentLevel switch
        {
            VillageDevelopmentLevel.Bustling =>
            [
                new(NpcEntityIds.Elsie, "Tom says the lanes feel fuller every season — I told him that's Bloomtown growing into itself."),
                new(NpcEntityIds.Tom, "Elsie claims she knows half the village by footsteps now. I don't think she's joking."),
                new(NpcEntityIds.Elsie, "Tom and I hardly get a quiet porch anymore. Honestly? I don't mind the company."),
                new(NpcEntityIds.Tom, "Elsie says the village sounds like a family supper now — noisy, warm, ours."),
            ],
            VillageDevelopmentLevel.Lively =>
            [
                new(NpcEntityIds.Tom, "Elsie thinks the village hums louder than it used to. She's probably right."),
                new(NpcEntityIds.Elsie, "Tom caught me swapping stories with two neighbors at once — felt like old times, but busier."),
                new(NpcEntityIds.Tom, "Elsie waved three people into conversation before breakfast. The village is finding its voice."),
            ],
            _ => Array.Empty<SocialLineEntry>(),
        };

        if (lines.Length == 0)
            return lines;

        if (timeOfDay == GameTimeOfDay.Evening && developmentLevel == VillageDevelopmentLevel.Bustling)
            return [new(NpcEntityIds.Tom, "Elsie saved me a seat for supper talk — evenings are when this village feels smallest and kindest.")];

        if (timeOfDay == GameTimeOfDay.Morning && developmentLevel >= VillageDevelopmentLevel.Lively)
            lines = [..lines, new(NpcEntityIds.Elsie, "Tom greeted half the lane before I finished my tea — Bloomtown is waking up social.")];

        return lines;
    }

    /// <summary>
    /// Small emergent social moments — overheard village life shaped by Elsie–Tom relationship tone.
    /// Neutral moments are practical; Friendly moments feel warmer and more familiar.
    /// </summary>
    internal static string[] GetEmergentSocialMomentLines(
        NpcInterpersonalRelationship relationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        if (relationship == NpcInterpersonalRelationship.Friendly)
            return BuildFriendlyEmergentMoments(timeOfDay, developmentLevel);

        return BuildNeutralEmergentMoments(timeOfDay, developmentLevel);
    }

    /// <summary>Extra warm Elsie–Tom lines when their bond has grown friendly.</summary>
    internal static SocialLineEntry[] GetBondLines(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<SocialLineEntry>
        {
            new(NpcEntityIds.Elsie, "Tom still tells the village my gossip before I do. Rude, but accurate."),
            new(NpcEntityIds.Tom, "Elsie fusses over everyone else's chores and mine too. Wouldn't trade it."),
            new(NpcEntityIds.Elsie, "Tom and I have argued over whose turn it is to be helpful since before Bloomtown had a name."),
            new(NpcEntityIds.Tom, "Elsie says I'm predictable. I say that's why the village trusts us."),
            new(NpcEntityIds.Elsie, "Tom brought me tea without being asked — he calls it nothing. I call it kindness."),
            new(NpcEntityIds.Tom, "Elsie still scolds me for working through supper. She's usually right, which annoys us both."),
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add(new(NpcEntityIds.Elsie, "Tom promised breakfast gossip and village errands — same schedule, every good morning."));

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add(new(NpcEntityIds.Tom, "Elsie is already planning tomorrow's neighborly chaos. I brought the list."));

        if (developmentLevel >= VillageDevelopmentLevel.Bustling)
            lines.Add(new(NpcEntityIds.Elsie, "Tom says the village grew up around our bickering. I think he's proud of that."));

        return lines.ToArray();
    }

    private static SocialLineEntry[] BuildFriendlyGeneralPool(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<SocialLineEntry>
        {
            new(NpcEntityIds.Elsie, "Tom was busy in the woods again — I told him not to skip supper."),
            new(NpcEntityIds.Tom, "Elsie had the garden beds looking wonderful. I said so twice; she still acted surprised."),
            new(NpcEntityIds.Elsie, "I asked Tom about his latest errand and he pretended it was nothing. Typical."),
            new(NpcEntityIds.Tom, "Elsie checked on me this morning like clockwork. The village runs on that sort of care."),
            new(NpcEntityIds.Elsie, "Tom swears he doesn't gossip. The village disagrees, affectionately."),
            new(NpcEntityIds.Tom, "Elsie says I worry too much. I say someone has to keep the lanes friendly."),
            new(NpcEntityIds.Elsie, "Tom saved me the last ripe tomato again — he acts like it's nothing, but I notice."),
            new(NpcEntityIds.Tom, "Elsie fussed over my scarf like winter already arrived. Can't say I minded."),
            new(NpcEntityIds.Elsie, "Tom and I still finish each other's chore lists. Old habit, good one."),
            new(NpcEntityIds.Tom, "Elsie laughed at my worst joke before noon. That's how I know the day's going well."),
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add(new(NpcEntityIds.Elsie, "Tom promised he'd be out early — I already told the neighbors, just in case."));

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add(new(NpcEntityIds.Tom, "Elsie waved me over to compare notes on the afternoon chores — easy habit, good one."));

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add(new(NpcEntityIds.Tom, "Elsie fussed over tomorrow's work again. I told her we'll manage, as always."));

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add(new(NpcEntityIds.Elsie, "Tom and I still trade stories like we did when the village was smaller — steadier now, though."));

        return lines.ToArray();
    }

    private static SocialLineEntry[] BuildNeutralGeneralPool(GameTimeOfDay timeOfDay)
    {
        var lines = new List<SocialLineEntry>
        {
            new(NpcEntityIds.Elsie, "Tom said he'd handle the fence post today. He usually does."),
            new(NpcEntityIds.Tom, "Elsie asked about the garden schedule. Straightforward enough."),
            new(NpcEntityIds.Elsie, "I passed Tom on his errand run. Brief hello, nothing more."),
            new(NpcEntityIds.Tom, "Elsie keeps the morning chores organized. I follow the list."),
            new(NpcEntityIds.Elsie, "Tom mentioned the afternoon deliveries. I noted it and moved on."),
            new(NpcEntityIds.Tom, "Elsie tracks who needs help in the village. I handle what she points at."),
            new(NpcEntityIds.Elsie, "Tom confirmed he'd cover the afternoon run. Reliable, if quiet about it."),
            new(NpcEntityIds.Tom, "Elsie passed along who needed help today. I took the list and went."),
            new(NpcEntityIds.Elsie, "Saw Tom near the lane — nodded, kept moving. That's enough some days."),
            new(NpcEntityIds.Tom, "Elsie had the morning schedule ready. I stuck to my part."),
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add(new(NpcEntityIds.Tom, "Elsie noted I'd be out early. Fine — I had planned to anyway."));

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add(new(NpcEntityIds.Elsie, "Tom checked the supply count at noon. Brief update, then back to work."));

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add(new(NpcEntityIds.Elsie, "Tom mentioned tomorrow's work. We'll see how the day goes."));

        return lines.ToArray();
    }

    private static string[] BuildFriendlyEmergentMoments(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<string>
        {
            "Elsie and Tom trade quiet jokes while sorting baskets — the lane feels warmer for it.",
            "You catch Elsie handing Tom a list and Tom handing it back corrected — an old rhythm, easy and fond.",
            "Tom calls out a teasing warning; Elsie answers without looking up. The village smiles at both of them.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add("Morning bustle: Elsie and Tom compare the day's errands like they've done it a thousand times — because they have.");

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add("Afternoon pause: Elsie and Tom share a bench for a minute, then return to work — no fuss, good company.");

        if (timeOfDay == GameTimeOfDay.Evening)
            lines.Add("Evening light: Elsie and Tom linger by a doorway, trading tomorrow's plans aloud.");

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add("A small knot of neighbors gathers around Elsie and Tom's easy back-and-forth — Bloomtown likes listening in.");

        return lines.ToArray();
    }

    private static string[] BuildNeutralEmergentMoments(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel)
    {
        var lines = new List<string>
        {
            "Elsie and Tom compare a short chore list — practical voices, no ceremony.",
            "Tom passes Elsie a delivery note; she nods once and files it away. Routine, reliable.",
            "You overhear Elsie and Tom coordinate afternoon errands without much small talk.",
        };

        if (timeOfDay == GameTimeOfDay.Morning)
            lines.Add("Morning exchange: Tom confirms the fence post; Elsie marks it done on her list.");

        if (timeOfDay == GameTimeOfDay.Afternoon)
            lines.Add("Afternoon check-in: Elsie asks about supplies; Tom answers in three words and keeps moving.");

        if (developmentLevel >= VillageDevelopmentLevel.Lively)
            lines.Add("Neighbors notice Elsie and Tom keeping the village schedule straight — quiet leadership, shared load.");

        return lines.ToArray();
    }

    private static readonly SocialLineEntry[] FriendlyWellLines =
    [
        new(NpcEntityIds.Tom, "Elsie was telling everyone the well's been busy since we finished it — good problem to have."),
        new(NpcEntityIds.Elsie, "Tom checked the well ropes this morning. He still fusses over village work like family."),
        new(NpcEntityIds.Tom, "Elsie says the morning queue at the well feels like the whole village saying hello at once."),
    ];

    private static readonly SocialLineEntry[] NeutralWellLines =
    [
        new(NpcEntityIds.Tom, "Elsie mentioned the well's been busy since we finished it. Makes sense."),
        new(NpcEntityIds.Elsie, "Tom checked the well ropes this morning. Reliable habit."),
    ];

    private static readonly SocialLineEntry[] FriendlyBridgeLines =
    [
        new(NpcEntityIds.Elsie, "Tom swears the bridge creaks less every week. I think he's just proud we mended it."),
        new(NpcEntityIds.Tom, "Elsie waved at me from the bridge earlier — we still celebrate that crossing like it was yesterday."),
        new(NpcEntityIds.Elsie, "Tom told me foot traffic doubled since the bridge came back. Bloomtown feels connected again."),
    ];

    private static readonly SocialLineEntry[] NeutralBridgeLines =
    [
        new(NpcEntityIds.Elsie, "Tom says the bridge creaks less now. Good enough for daily crossings."),
        new(NpcEntityIds.Tom, "Elsie noted more people use the bridge since we repaired it."),
    ];

    private static readonly SocialLineEntry[] FriendlyWarehouseLines =
    [
        new(NpcEntityIds.Tom, "Elsie already knows what arrived at the warehouse before I do — village eyes everywhere."),
        new(NpcEntityIds.Elsie, "Tom sorted the afternoon delivery without being asked. That's how we keep the stores steady."),
        new(NpcEntityIds.Tom, "Elsie says the warehouse made supper conversations calmer. I believe her."),
    ];

    private static readonly SocialLineEntry[] NeutralWarehouseLines =
    [
        new(NpcEntityIds.Tom, "Elsie tracks warehouse deliveries closely. Keeps things orderly."),
        new(NpcEntityIds.Elsie, "Tom handled the afternoon crates. No fuss, which I appreciate."),
    ];

    internal readonly record struct SocialLineEntry(uint SpeakerNpcEntityId, string Text);
}