using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Dialogue and ambient lines for scheduled village events.
/// </summary>
public static class VillageEventDialogue
{
    public static string? TryGetVillagerAmbientComment(
        VillageEventKind eventKind,
        uint variationSeed)
    {
        string[] lines = eventKind switch
        {
            VillageEventKind.MarketDay =>
            [
                "A neighbor calls cheerfully, \"Market Day's lively — good day to trade at the square.\"",
                "Overheard: \"Vendors are out early — Bloomtown's market feels twice as busy today.\"",
                "Someone at the lane edge says, \"Market Day bustle — even quiet folk are heading to Mira's corner.\"",
                "A villager smiles. \"Trade day's in full swing — the square hums with friendly barter.\"",
            ],
            VillageEventKind.CommunityWorkDay =>
            [
                "A neighbor calls warmly, \"Community Work Day — everyone's lending a hand around the village.\"",
                "Overheard: \"Gotong royong today — folk are pitching in at the garden, forge, and lanes.\"",
                "Someone remarks, \"Work Day spirit — even small help counts when the whole village moves together.\"",
                "A villager nods. \"Bloomtown's busy with shared chores today — good day to help out.\"",
            ],
            VillageEventKind.RainyDay =>
            [
                "A neighbor murmurs softly, \"Rainy Day — Bloomtown's gone quiet; good weather for a porch and warm company.\"",
                "Overheard: \"Soft rain all day — folk are staying indoors where the hearths are warm.\"",
                "Someone pulls their cloak tighter. \"Wet lanes today — even the square feels half asleep.\"",
                "A villager smiles faintly. \"Rain on the roof — Bloomtown sounds peaceful when the village slows down.\"",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetNpcInteractionLine(
        uint playerEntityId,
        uint npcEntityId,
        int gameDay,
        uint variationSeed)
    {
        if (!VillageEventConfig.ShouldTriggerEventInteractionLine(playerEntityId, npcEntityId, gameDay, variationSeed))
            return null;

        var events = VillageEventConfig.GetActiveEvents(gameDay);
        if (events.Count == 0)
            return null;

        var primaryEvent = events[(int)(variationSeed % (uint)events.Count)];
        return TryGetNpcEventLine(npcEntityId, primaryEvent, variationSeed);
    }

    public static string? TryGetNpcEventLine(
        uint npcEntityId,
        VillageEventKind eventKind,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, eventKind) switch
        {
            (NpcEntityIds.Mira, VillageEventKind.MarketDay) =>
            [
                "Mira grins. \"Market Day, love — stall's buzzing and I've sharpened my prices a touch for the crowd.\"",
                "Mira waves a vendor over. \"Busy square today — good day to buy or sell while the market hums.\"",
                "Mira leans on the counter. \"Trade day's lively — folk know Market Day means fairer deals at my stall.\"",
            ],
            (NpcEntityIds.Mira, VillageEventKind.CommunityWorkDay) =>
            [
                "Mira smiles. \"Work Day in Bloomtown — I'll keep the square running while neighbors pitch in elsewhere.\"",
                "Mira nods toward the lanes. \"Gotong royong spirit today — even market folk feel the village pulling together.\"",
            ],

            (NpcEntityIds.Harold, VillageEventKind.MarketDay) =>
            [
                "Harold tips his hat. \"Market Day brings folk to the square — good for trade and neighborly talk.\"",
                "Harold murmurs. \"Busy market today — Bloomtown feels awake when trade stirs.\"",
            ],
            (NpcEntityIds.Harold, VillageEventKind.CommunityWorkDay) =>
            [
                "Harold says quietly. \"Community Work Day — shared chores keep a village honest and strong.\"",
                "Harold tips his hat. \"Good day to lend a hand — Bloomtown moves faster when everyone pitches in.\"",
            ],

            (NpcEntityIds.Elsie, VillageEventKind.CommunityWorkDay) =>
            [
                "Elsie smiles. \"Work Day at the garden — extra hands make the beds feel cared for by the whole village.\"",
                "Elsie says gently. \"Gotong royong today — even weeding shared beds feels lighter together.\"",
            ],
            (NpcEntityIds.Elsie, VillageEventKind.MarketDay) =>
            [
                "Elsie nods toward the square. \"Market Day bustle — I'll send a few apples that way when the beds allow.\"",
            ],

            (NpcEntityIds.Greta, VillageEventKind.MarketDay) =>
            [
                "Greta beams. \"Market Day — travelers and locals alike fill my tables before the square quiets.\"",
            ],
            (NpcEntityIds.Greta, VillageEventKind.CommunityWorkDay) =>
            [
                "Greta fusses over the hearth. \"Work Day spirit — I'll keep soup ready for folk coming back from shared chores.\"",
            ],

            (NpcEntityIds.Tom, VillageEventKind.CommunityWorkDay) =>
            [
                "Tom wipes his brow. \"Work Day — woodpile's busy, but folk lending a hand makes the yard feel lighter.\"",
            ],
            (NpcEntityIds.Tom, VillageEventKind.MarketDay) =>
            [
                "Tom nods toward the square. \"Market Day — even the lumber yard hears the bustle from here.\"",
            ],

            (NpcEntityIds.Elias, VillageEventKind.CommunityWorkDay) =>
            [
                "Elias says plainly. \"Work Day at the forge — extra help keeps the village tools honest.\"",
            ],
            (NpcEntityIds.Marcus, VillageEventKind.CommunityWorkDay) =>
            [
                "Marcus smiles warmly. \"Work Day at the workshop — shared repairs go smoother when neighbors show up.\"",
            ],
            (NpcEntityIds.Nora, VillageEventKind.CommunityWorkDay) =>
            [
                "Nora says quietly. \"Work Day in the herb rows — many hands make the garden paths feel welcoming.\"",
            ],
            (NpcEntityIds.Ben, VillageEventKind.CommunityWorkDay) =>
            [
                "Ben says evenly. \"Work Day on the lanes — folk are out helping; the village feels steadier for it.\"",
            ],
            (NpcEntityIds.Lila, VillageEventKind.CommunityWorkDay) =>
            [
                "Lila grins. \"Work Day — young folk like me are running errands while neighbors pitch in everywhere.\"",
            ],
            (NpcEntityIds.Rowan, VillageEventKind.CommunityWorkDay) =>
            [
                "Rowan smiles faintly. \"Work Day — old tales say villages grow when everyone lends a hand on the same day.\"",
            ],

            (NpcEntityIds.Eleanor, VillageEventKind.RainyDay) =>
            [
                "Eleanor watches the rain from her porch. \"Soft weather, dear — good day to sit still and let the village rest.\"",
                "Eleanor murmurs warmly. \"Rainy days teach patience. Bloomtown feels wiser when the lanes go quiet.\"",
            ],
            (NpcEntityIds.Greta, VillageEventKind.RainyDay) =>
            [
                "Greta beams at the window. \"Rainy Day, love — my parlor's full and the soup's on. Stay awhile where it's dry.\"",
                "Greta fusses over the hearth. \"Wet weather keeps folk indoors — good day for warm company and quiet talk.\"",
            ],
            (NpcEntityIds.Harold, VillageEventKind.RainyDay) =>
            [
                "Harold tips his hat against the drizzle. \"Rainy Day — even the well sounds softer. Bloomtown earns its rest.\"",
                "Harold murmurs. \"Wet lanes slow everyone down — wise folk stay dry and keep their kindness close.\"",
            ],
            (NpcEntityIds.Rowan, VillageEventKind.RainyDay) =>
            [
                "Rowan smiles faintly from the story bench. \"Rainy Day — old tales sound truer when the square is quiet.\"",
                "Rowan says softly. \"Soft rain on the roof — good weather for listening, not rushing.\"",
            ],
            (NpcEntityIds.Marcus, VillageEventKind.RainyDay) =>
            [
                "Marcus glances at the wet workshop yard. \"Rainy Day — outdoor repairs can wait. Bench work feels better indoors.\"",
                "Marcus says warmly. \"Wet timber and wet lanes — Bloomtown's wiser when folk slow down a little.\"",
            ],
            (NpcEntityIds.Elsie, VillageEventKind.RainyDay) =>
            [
                "Elsie smiles at the rain on the beds. \"Rainy Day — the garden drinks, and folk like us can rest a while.\"",
            ],
            (NpcEntityIds.Tom, VillageEventKind.RainyDay) =>
            [
                "Tom wipes rain from his brow. \"Wet day at the yard — wood can wait till the lanes clear.\"",
            ],
            (NpcEntityIds.Mira, VillageEventKind.RainyDay) =>
            [
                "Mira shrugs cheerfully. \"Rainy Day — quieter market, but my stall's dry and the tea's hot.\"",
            ],

            (_, VillageEventKind.RainyDay) =>
            [
                "A villager nearby mentions the rain has slowed Bloomtown to a gentler pace today.",
            ],

            (_, VillageEventKind.MarketDay) =>
            [
                "A villager nearby mentions the market square is especially lively today.",
            ],
            (_, VillageEventKind.CommunityWorkDay) =>
            [
                "A neighbor remarks that Bloomtown is busy with shared work today.",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetCommunityWorkHelpAcknowledgment(
        uint playerEntityId,
        uint npcEntityId,
        CommunityActivityKind activity,
        int gameDay,
        uint variationSeed)
    {
        if (!VillageEventConfig.ShouldTriggerCommunityWorkHelpAcknowledgment(
                playerEntityId,
                activity,
                gameDay,
                variationSeed))
            return null;

        string[] lines = (npcEntityId, activity) switch
        {
            (NpcEntityIds.Elsie, CommunityActivityKind.HelpGarden) =>
            [
                "Elsie beams. \"Work Day at the beds — your hands make the whole village feel cared for.\"",
            ],
            (NpcEntityIds.Harold, CommunityActivityKind.HelpWell) =>
            [
                "Harold tips his hat. \"Community Work Day — shared upkeep at the well is exactly what Bloomtown needs.\"",
            ],
            (NpcEntityIds.Mira, CommunityActivityKind.HelpMarket) =>
            [
                "Mira grins. \"Work Day and Market Day together — you're keeping the square worthy of the bustle.\"",
            ],
            (NpcEntityIds.Tom, CommunityActivityKind.HelpLumber) =>
            [
                "Tom nods. \"Work Day at the yard — good timber and good neighbors, both stacking up.\"",
            ],
            (NpcEntityIds.Greta, CommunityActivityKind.HelpInn) =>
            [
                "Greta fusses over you. \"Work Day at the inn — folk like you keep the hearth honest, love.\"",
            ],
            (NpcEntityIds.Elias, CommunityActivityKind.HelpSmithy) =>
            [
                "Elias grunts approvingly. \"Work Day at the forge — steady hands keep the village running.\"",
            ],
            (NpcEntityIds.Marcus, CommunityActivityKind.HelpWorkshop) =>
            [
                "Marcus smiles. \"Work Day at the bench — shared repairs feel lighter with you here.\"",
            ],
            (NpcEntityIds.Nora, CommunityActivityKind.HelpHerbGarden) =>
            [
                "Nora smiles softly. \"Work Day in the herb rows — the village feels healthier when we tend together.\"",
            ],
            (NpcEntityIds.Ben, CommunityActivityKind.HelpPatrol) =>
            [
                "Ben says evenly. \"Work Day on the route — neighbors walking the lanes together keeps Bloomtown honest.\"",
            ],
            (NpcEntityIds.Lila, CommunityActivityKind.HelpVillage) =>
            [
                "Lila waves. \"Work Day — young folk notice when someone shows up for the whole village.\"",
            ],
            (NpcEntityIds.Rowan, CommunityActivityKind.ListenToStories) =>
            [
                "Rowan nods. \"Work Day — even listening counts when the village gathers its stories together.\"",
            ],
            (_, _) when VillageEventConfig.IsHelpActivity(activity) =>
            [
                "A neighbor calls warmly, \"Community Work Day — Bloomtown notices when you pitch in.\"",
                "Someone nearby says, \"Good Work Day help — the village feels closer for it.\"",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetRainyDayActivityAcknowledgment(
        uint playerEntityId,
        uint npcEntityId,
        CommunityActivityKind activity,
        int gameDay,
        uint variationSeed)
    {
        if (!VillageEventConfig.ShouldTriggerRainyDayActivityAcknowledgment(
                playerEntityId,
                activity,
                gameDay,
                variationSeed))
            return null;

        string[] lines = (npcEntityId, activity) switch
        {
            (NpcEntityIds.Eleanor, CommunityActivityKind.ChatWithEleanor) =>
            [
                "Eleanor smiles at the rain on the porch. \"Good weather for quiet talk, dear — I'm glad you stayed indoors with me.\"",
            ],
            (NpcEntityIds.Rowan, CommunityActivityKind.ListenToStories) =>
            [
                "Rowan nods toward the rain. \"Soft weather for old tales — the bench feels warmer when the square is quiet.\"",
            ],
            (NpcEntityIds.Greta, CommunityActivityKind.HelpInn) =>
            [
                "Greta beams. \"Rainy Day at the inn — folk shelter here, and helpers like you make the hearth feel doubly welcome.\"",
            ],
            (NpcEntityIds.Elsie, CommunityActivityKind.HelpGarden) =>
            [
                "Elsie says gently. \"Brave of you to help in the rain — the beds appreciate it, even if the drizzle doesn't.\"",
            ],
            (NpcEntityIds.Tom, CommunityActivityKind.HelpLumber) =>
            [
                "Tom wipes his brow. \"Wet yard work's honest but tiring — thanks for showing up anyway.\"",
            ],
            (NpcEntityIds.Ben, CommunityActivityKind.HelpPatrol) =>
            [
                "Ben says evenly. \"Rainy patrol's no picnic — Bloomtown notices when someone walks the lanes anyway.\"",
            ],
            (_, _) when VillageEventConfig.IsIndoorCalmCommunityActivity(activity) =>
            [
                "A neighbor murmurs warmly, \"Rainy Day — good choice staying indoors where it's dry and friendly.\"",
                "Someone nearby says, \"Soft rain outside — quiet company indoors feels just right today.\"",
            ],
            (_, _) when VillageEventConfig.IsOutdoorCommunityActivity(activity) =>
            [
                "A villager calls through the drizzle, \"Rainy Day — thanks for braving the wet to help anyway.\"",
                "Someone under a cloak nods. \"Wet chores aren't easy — Bloomtown sees the effort.\"",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}