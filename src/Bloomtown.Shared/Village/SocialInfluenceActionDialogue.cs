using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Dialogue when a Respected or Well-liked player actively calls on focus NPCs for social influence.
/// </summary>
public static class SocialInfluenceActionDialogue
{
    public static string? TryGetSuccessResponseLine(
        uint npcEntityId,
        SocialInfluenceOutcomeKind outcome,
        bool villageNoticedMemory,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (tier < VillageSocialStandingTier.WellLiked)
            return TryGetRespectedSuccessResponseLine(npcEntityId, outcome, villageNoticedMemory, variationSeed);

        string[] lines = (npcEntityId, outcome, villageNoticedMemory) switch
        {
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Harold tips his hat with elder's respect. \"Bloomtown esteems you — so I'll say it plain: the well project moves fastest when stone arrives before noon, and the bridge site needs steady hands at dusk.\"",
                "Harold murmurs quietly. \"Because folk trust you widely, I'll share this — warehouse work counts most when the village board shows a gap in food stores.\"",
            ],
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Harold says with quiet authority. \"Your standing here is real — communal projects at the well and bridge both need patient contributors before the square stirs.\"",
                "Harold nods. \"Neighbors hold you in high regard — Harold's counsel: shared building work goes further when trusted folk speak up at the board.\"",
            ],
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.ProjectBacking, _) =>
            [
                "Harold sets a hand on your shoulder. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next contribution — the village will count it as double progress through your standing.\"",
                "Harold tips his hat. \"Your Well-liked standing carries real weight here. Bring materials to shared work soon — I'll make sure the village counts your effort as +2 progress.\"",
            ],

            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Greta leans in warmly. \"Because you're well-liked, I'll tell you what my guests say — travelers ask after you by name, and folk all over town speak kindly of your company.\"",
                "Greta beams. \"Bloomtown esteems you — between us, the market square hums with talk of who shows up for communal work. You're on every good list.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Greta smiles with real fondness. \"Guests at my table keep hearing your name — always with respect. That's rare, and you've earned it.\"",
                "Greta fusses over your cup. \"Your standing opens doors here — a riverside merchant asked after you yesterday. Bloomtown's proud of folk like you.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Recovery, _) =>
            [
                "Greta squeezes your arm. \"Sit by the hearth a while, love — Well-liked guests get the full inn recovery, not the polite half-measure. My hearth steadies people Bloomtown esteems.\"",
                "Greta waves off your thanks. \"Because the village holds you in high regard, you get the warm corner, a proper meal, and a real breath back. Rest — you've earned more than ordinary folk.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Greta presses food into your hands. \"From my kitchen — guests the village is proud of get the little extras. Take it, love.\"",
                "Greta beams. \"Bloomtown esteems you, so I do too — here's something from the inn for a trusted regular.\"",
            ],

            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Mira grins. \"Because Bloomtown esteems you — real market talk: apples sell fastest before noon, and early vendors remember trusted regulars with better prices.\"",
                "Mira winks. \"Well-liked folk get the good gossip — plank prices firm up after communal projects finish, and Elsie's harvest sets the week's rhythm.\"",
            ],
            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Mira beams. \"Your standing opens doors at my stall — buy before the square fills, sell when folk are feeling generous after shared work.\"",
                "Mira leans in. \"Neighbors hold you in high regard — market mornings reward people the village is proud of.\"",
            ],
            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.TradePrivilege, _) =>
            [
                "Mira taps the counter. \"Because you're well-liked here, I'll mark you for a proper trade favor — your next couple deals at my stall get a real discount through your standing.\"",
                "Mira grins. \"Bloomtown esteems you, so Mira does too — bring goods to my stall soon and I'll sharpen the numbers twice before the favor fades.\"",
            ],
            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Mira slips goods into your hands. \"From the stall — trusted regulars get the little extras when the village speaks well of them.\"",
                "Mira winks. \"A trade favor for someone Bloomtown esteems — useful stock, on the house.\"",
            ],

            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Elsie smiles with quiet pride. \"Because Bloomtown esteems you — the south beds catch morning light best after rain, and food projects move faster when apples arrive early.\"",
                "Elsie leans in gently. \"Neighbors trust you widely — warehouse work counts most when the village board shows a gap in food stores.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Elsie nods warmly. \"Your standing here is real — herb rows and garden beds both yield more generously before the square stirs.\"",
                "Elsie says gently. \"People hold you in high regard — Elsie's counsel: food contributions land best when folk trust who's bringing them.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.GardenBacking, _) =>
            [
                "Elsie sets a hand on your arm. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next food-and-garden contribution — the village will count it as +2 progress.\"",
                "Elsie smiles. \"Your Well-liked standing carries weight in the garden. Bring apples or food work soon — I'll make sure the village counts double through your name.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Elsie presses fruit into your hands. \"From the garden — someone the village esteems deserves a little harvest kindness.\"",
                "Elsie beams. \"Bloomtown esteems you, so the beds do too — take this from Elsie's rows.\"",
            ],

            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Tom wipes his brow and nods. \"Because Bloomtown esteems you — the north grove yields clean timber before noon, and the bridge site always needs another load of wood at dusk.\"",
                "Tom leans on his axe. \"Well-liked folk get the straight talk — warehouse framing moves fastest when wood arrives steady, and the well project drinks lumber like rain.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Tom says plainly. \"Your standing here is real — communal builds at the well and bridge both need patient hands with good timber.\"",
                "Tom nods. \"Neighbors hold you in high regard — Tom's counsel: wood work goes further when trusted folk show up at the lumber yard.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.LumberBacking, _) =>
            [
                "Tom sets a hand on a fresh-cut log. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next wood contribution — the village will count it as +2 progress.\"",
                "Tom tips his cap. \"Your Well-liked standing carries weight in the timber yard. Bring wood to shared work soon — I'll make sure the village counts double through your name.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Tom passes you cut timber. \"From the yard — someone the village esteems gets the useful scraps and the good lengths alike.\"",
                "Tom grins. \"Bloomtown esteems you, so the lumber does too — take this from Tom's stack.\"",
            ],

            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Nora smiles softly. \"Because Bloomtown esteems you — the east herb rows catch morning dew best, and the village board shows when food stores need tending.\"",
                "Nora leans in gently. \"Neighbors trust you widely — Nora's counsel: herb work and apple harvests both land best before the square stirs.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Nora nods warmly. \"Your standing here is real — the herb beds and garden paths both welcome a trusted helper before noon.\"",
                "Nora says quietly. \"People hold you in high regard — health in Bloomtown starts with who tends the growing things.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.HerbalBacking, _) =>
            [
                "Nora sets a bundle of leaves in your hands. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next herb contribution — the village will count it as +2 progress.\"",
                "Nora smiles. \"Your Well-liked standing carries weight in the herb rows. Bring apples or plant work soon — I'll make sure the village counts double through your name.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Nora presses fruit into your hands. \"From the herb rows — someone the village esteems deserves a little harvest kindness.\"",
                "Nora beams gently. \"Bloomtown esteems you, so the beds do too — take this from Nora's tending.\"",
            ],

            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Elias wipes soot from his hands. \"Because Bloomtown esteems you — the forge runs hottest after communal projects finish, and plank stock firms up when trusted folk bring wood to the smithy.\"",
                "Elias nods at the anvil. \"Well-liked regulars get the good counsel — tool work and bridge repairs both reward steady hands at the forge.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Elias says with quiet pride. \"Your standing here is real — smithy help and crafting work both go further when folk trust who's swinging the hammer.\"",
                "Elias leans on the bellows. \"Neighbors hold you in high regard — Elias's counsel: bring materials early and the village remembers.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.SmithingBacking, _) =>
            [
                "Elias sets a finished tool on the bench. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next smithing work — +2 on project contributions or extra wood when you help at the forge.\"",
                "Elias nods at the forge. \"Your Well-liked standing carries weight at the anvil. Help at the smithy or bring plank work soon — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Elias hands you forge stock. \"From the smithy — someone the village esteems gets the useful offcuts and the good steel alike.\"",
                "Elias grins. \"Bloomtown esteems you, so the forge does too — take this from Elias's bench.\"",
            ],

            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Marcus wipes sawdust from his hands. \"Because Bloomtown esteems you — the workshop runs smoothest after communal projects finish, and plank stock firms up when trusted folk bring wood to the bench.\"",
                "Marcus nods at the worktable. \"Well-liked regulars get the good counsel — tool work and bridge repairs both reward steady hands at the workshop.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Marcus says with warm pride. \"Your standing here is real — workshop help and crafting work both go further when folk trust who's shaping the repairs.\"",
                "Marcus leans on the bench. \"Neighbors hold you in high regard — Marcus's counsel: bring materials early and the village remembers.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.CraftingBacking, _) =>
            [
                "Marcus sets a finished plank on the bench. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next crafting work — +2 on project contributions or extra planks when you help at the workshop.\"",
                "Marcus smiles warmly. \"Your Well-liked standing carries weight at the bench. Help at the workshop or bring plank work soon — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Marcus hands you workshop stock. \"From the bench — someone the village esteems gets the useful planks and the good tools alike.\"",
                "Marcus grins. \"Bloomtown esteems you, so the workshop does too — take this from Marcus's table.\"",
            ],

            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Ben scans the lanes. \"Because Bloomtown esteems you — the east path gets loose after market hours, and the well-side needs steady eyes before dusk.\"",
                "Ben nods at the guard post. \"Well-liked regulars get the straight counsel — bridge work and patrol both reward folk who show up before trouble does.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Ben says evenly. \"Your standing here is real — patrol help and communal work both go further when folk trust who's walking the route.\"",
                "Ben leans on his lantern. \"Neighbors hold you in high regard — Ben's counsel: check the lanes early and the village remembers.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.GuardBacking, _) =>
            [
                "Ben sets a tool on the bench. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next well-or-bridge work — +2 on project contributions or extra wood when you help on patrol.\"",
                "Ben nods at the post. \"Your Well-liked standing carries weight on my route. Help patrol or bring stone and planks soon — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Ben hands you guard stock. \"From the post — someone the village esteems gets the useful wood and the good tools alike.\"",
                "Ben says plainly. \"Bloomtown esteems you, so the lanes do too — take this from Ben's kit.\"",
            ],

            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Lila grins at the lanes. \"Because Bloomtown esteems you — the square hums brighter before noon, and the garden path needs friendly eyes before dusk.\"",
                "Lila nods eagerly. \"Well-liked regulars get the straight counsel — warehouse apples and village help both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Lila says warmly. \"Your standing here is real — help around the village and communal work both go further when folk trust who's lending a hand.\"",
                "Lila leans in. \"Neighbors hold you in high regard — Lila's counsel: walk the lanes before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.YouthBacking, _) =>
            [
                "Lila beams. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next warehouse-or-apple work — +2 on project contributions or extra apples when you help around the village.\"",
                "Lila nods along the path. \"Your Well-liked standing carries weight with young folk like me. Help village or bring apples soon — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Lila hands you apples and wood. \"From the lanes — someone the village esteems gets the useful little things and the good company alike.\"",
                "Lila says brightly. \"Bloomtown esteems you, so the square does too — take this from Lila's bundle.\"",
            ],

            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Rowan murmurs at the bench. \"Because Bloomtown esteems you — the inn corner is quietest before noon, and Harold's porch holds tales worth hearing before dusk.\"",
                "Rowan nods slowly. \"Well-liked regulars get the straight counsel — warehouse apples and story listening both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Rowan says warmly. \"Your standing here is real — listen at the bench and communal work both go further when folk trust who's lending an ear.\"",
                "Rowan leans in. \"Neighbors hold you in high regard — Rowan's counsel: sit before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.StoryBacking, _) =>
            [
                "Rowan smiles faintly. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next warehouse-or-apple work — +2 on project contributions or extra wood when you listen to stories.\"",
                "Rowan nods at the bench. \"Your Well-liked standing carries weight with old tales like mine. Listen soon or bring apples — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Rowan hands you wood and an apple. \"From the bench — someone the village esteems gets the useful keepsakes and the good company alike.\"",
                "Rowan says quietly. \"Bloomtown esteems you, so the inn does too — take this from Rowan's bundle.\"",
            ],

            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Eleanor murmurs from the porch. \"Because Bloomtown esteems you — the cottage is quietest before noon, and Harold's well holds tales worth hearing before dusk.\"",
                "Eleanor nods warmly. \"Well-liked regulars get the straight counsel — warehouse apples and porch chats both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Eleanor says warmly. \"Your standing here is real — chat on the porch and communal work both go further when folk trust who's lending an ear.\"",
                "Eleanor leans in. \"Neighbors hold you in high regard — Eleanor's counsel: sit before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.LegacyBacking, _) =>
            [
                "Eleanor smiles warmly. \"Because Bloomtown regards you as one of its own, I'll put my name behind your next warehouse-or-apple work — +2 on project contributions or extra apples when you chat on the porch.\"",
                "Eleanor nods from her chair. \"Your Well-liked standing carries weight with old stories like mine. Chat soon or bring apples — I'll make sure your effort lands twice as hard.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Eleanor hands you an apple and wood. \"From the porch — someone the village esteems gets the useful keepsakes and the good company alike.\"",
                "Eleanor says warmly. \"Bloomtown esteems you, so the cottage does too — take this from Eleanor's basket.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetRespectedSuccessResponseLine(
        uint npcEntityId,
        SocialInfluenceOutcomeKind outcome,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, outcome, villageNoticedMemory) switch
        {
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Harold tips his hat. \"Bloomtown respects you — I'll say it plainly: the well project moves fastest when stone arrives before noon. That's real counsel your standing earned.\"",
                "Harold murmurs quietly. \"Neighbors trust you — warehouse work counts most when the village board shows a gap. Well-liked standing would open even sharper counsel, but this is honest help.\"",
            ],
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Harold listens to your request. \"Your standing here is real — communal projects at the well and bridge both need patient contributors. Respected folk like you still shape shared work.\"",
                "Harold nods. \"Harold hears you out — shared building work rewards trusted folk at the board. Keep deepening your roots here; Well-liked standing opens stronger backing.\"",
            ],
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Harold sets an apple in your hands. \"A small kindness for someone Bloomtown respects — modest, but yours by right of standing.\"",
                "Harold tips his hat. \"Respected standing earns modest favors too — take this from the village stores.\"",
            ],
            (NpcEntityIds.Harold, SocialInfluenceOutcomeKind.ProjectBacking, _) =>
            [
                "Harold sets a hand on your shoulder. \"Because Bloomtown respects you, I'll put a word in for +1 progress on your next communal contribution — real backing, even if Well-liked folk earn double.\"",
                "Harold tips his hat. \"Respected standing carries weight. Bring materials to shared work soon — I'll count your effort as +1 extra progress. Reach Well-liked and that backing doubles.\"",
            ],

            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Greta leans in warmly. \"Because Bloomtown respects you, I'll share what my guests say — folk speak kindly of your company, and that's real standing.\"",
                "Greta beams. \"Neighbors greet you differently now — Respected guests earn honest hospitality. Well-liked regulars get the deepest hearth recovery, but you're on the right path.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Greta listens to your request. \"Guests keep hearing your name with respect — Greta saves a moment for folk Bloomtown trusts.\"",
                "Greta fusses over your cup. \"Your standing opens doors here — a riverside merchant asked after you yesterday. Keep growing those close friendships.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Recovery, _) =>
            [
                "Greta pours you tea. \"Sit a moment, love — Respected guests get a modest breath back. Real warmth, and Well-liked regulars earn the full hearth recovery.\"",
                "Greta squeezes your hand gently. \"Bloomtown respects you, so the inn steadies you a little — honest care for someone the village trusts.\"",
            ],
            (NpcEntityIds.Greta, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Greta presses a small plate into your hands. \"From my kitchen — a modest kindness for someone Respected here. You've earned a place at this table.\"",
                "Greta beams. \"Your standing opens some doors — take this from the inn, love.\"",
            ],

            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Mira grins. \"Because Bloomtown respects you — real market talk: apples sell fastest before noon. That's useful counsel your standing bought.\"",
                "Mira winks. \"Respected regulars get straight talk — plank prices firm up after communal projects. Reach Well-liked and I'll mark you for proper trade favors too.\"",
            ],
            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Mira listens to your request. \"Your standing opens doors at my stall — buy before the square fills, sell when folk feel generous after shared work.\"",
                "Mira leans in. \"Mira hears you out — market mornings reward people Bloomtown respects. Deepen your standing and trade favors unlock at Well-liked.\"",
            ],
            (NpcEntityIds.Mira, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Mira slips a small bundle into your hands. \"A modest trade kindness for someone Respected — useful stock, and proof the village trusts you at my stall.\"",
                "Mira winks. \"Bloomtown respects you, so Mira shares a little from the stall.\"",
            ],

            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Elsie smiles with quiet pride. \"Because Bloomtown respects you — the south beds catch morning light best after rain. Garden backing at your standing adds +1 only.\"",
                "Elsie leans in gently. \"Neighbors trust you — food projects move faster when apples arrive early, though Well-liked backing counts double.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Elsie nods warmly. \"Your standing here is real — herb rows and garden beds both yield more generously before the square stirs.\"",
                "Elsie says gently. \"Elsie listens — because Bloomtown respects you, food contributions still land best when folk trust who's bringing them.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.GardenBacking, _) =>
            [
                "Elsie sets a hand on your arm. \"Elsie listens — because Bloomtown respects you, I'll still put my name behind +1 progress on your next food-and-garden contribution.\"",
                "Elsie smiles. \"Respected standing carries some weight in the garden. Bring apples or food work soon — +1 extra progress. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Elsie, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Elsie presses fruit into your hands. \"A small harvest kindness for someone Respected — modest, but from the heart.\"",
                "Elsie smiles. \"Bloomtown respects you, so the garden shares a little.\"",
            ],

            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Tom wipes his brow and nods. \"Because Bloomtown respects you — the north grove yields clean timber before noon. Lumber backing at your standing adds +1 only.\"",
                "Tom leans on his axe. \"Respected folk get straight talk — the well project drinks lumber like rain, though Well-liked backing counts double.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Tom says plainly. \"Your standing here is real — communal builds at the well and bridge both need patient hands with good timber.\"",
                "Tom nods. \"Tom listens — because Bloomtown respects you, wood work still goes further when trusted folk show up.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.LumberBacking, _) =>
            [
                "Tom sets a hand on a fresh-cut log. \"Tom listens — because Bloomtown respects you, I'll still back your next wood contribution for +1 progress.\"",
                "Tom tips his cap. \"Respected standing carries some weight in the timber yard. Bring wood to shared work soon — +1 extra progress. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Tom, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Tom passes you a short length of timber. \"Useful stock for someone Respected — not the generous haul Well-liked folk get, but it'll serve.\"",
                "Tom grins. \"Bloomtown respects you, so the yard shares a little.\"",
            ],

            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Nora smiles softly. \"Because Bloomtown respects you — the east herb rows catch morning dew best. Herbal backing at your standing adds +1 only.\"",
                "Nora leans in gently. \"Neighbors trust you — herb work and apple harvests both land best before the square stirs.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Nora nods warmly. \"Your standing here is real — the herb beds and garden paths both welcome a trusted helper before noon.\"",
                "Nora says quietly. \"Nora listens — because Bloomtown respects you, health in Bloomtown starts with who tends the growing things.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.HerbalBacking, _) =>
            [
                "Nora sets a bundle of leaves in your hands. \"Nora listens — because Bloomtown respects you, I'll still back your next herb contribution for +1 progress.\"",
                "Nora smiles. \"Respected standing carries some weight in the herb rows. Bring apples or plant work soon — +1 extra progress. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Recovery, _) =>
            [
                "Nora offers a warm cup of herb tea. \"Sit with the beds a moment — Respected folk earn a modest steadiness here, gentler than Well-liked hearth rest but real care.\"",
                "Nora smiles softly. \"Bloomtown respects you, so the herb rows share a little recovery — honest warmth from someone who tends health in this village.\"",
            ],
            (NpcEntityIds.Nora, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Nora offers a small bundle of herbs. \"A modest kindness from the beds — Respected standing earns real care, though not the generous gifts Well-liked folk receive.\"",
                "Nora smiles softly. \"Bloomtown respects you, so the herb rows share a little.\"",
            ],

            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Elias wipes soot from his hands. \"Because Bloomtown respects you — the forge runs hottest after communal projects finish. Smithing backing at your standing adds +1 only.\"",
                "Elias nods at the anvil. \"Respected regulars get good counsel — tool work and bridge repairs both reward steady hands at the forge.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Elias says with quiet pride. \"Your standing here is real — smithy help and crafting work both go further when folk trust who's swinging the hammer.\"",
                "Elias leans on the bellows. \"Elias listens — because Bloomtown respects you, bring materials early and the village remembers.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.SmithingBacking, _) =>
            [
                "Elias sets a finished tool on the bench. \"Elias listens — because Bloomtown respects you, I'll still back your next smithing work for +1 progress or +1 wood at the forge.\"",
                "Elias nods at the forge. \"Respected standing carries some weight at the anvil. Help at the smithy soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Elias, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Elias sets a small piece of wood on the bench. \"Modest stock from the forge — Respected regulars get useful scraps, not the generous favors Well-liked folk earn.\"",
                "Elias nods. \"Bloomtown respects you, so the smithy shares a little.\"",
            ],

            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Marcus wipes sawdust from his hands. \"Because Bloomtown respects you — the workshop runs smoothest after communal projects finish. Crafting backing at your standing adds +1 only.\"",
                "Marcus nods at the worktable. \"Respected regulars get good counsel — tool work and bridge repairs both reward steady hands at the workshop.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Marcus says with warm pride. \"Your standing here is real — workshop help and crafting work both go further when folk trust who's shaping the repairs.\"",
                "Marcus leans on the bench. \"Marcus listens warmly — because Bloomtown respects you, bring materials early and the village remembers.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.CraftingBacking, _) =>
            [
                "Marcus sets a finished plank on the bench. \"Marcus listens warmly — because Bloomtown respects you, I'll still back your next crafting work for +1 progress or +1 plank at the workshop.\"",
                "Marcus smiles. \"Respected standing carries some weight at the bench. Help at the workshop soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Marcus, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Marcus sets a small plank on the bench. \"Modest stock from the workshop — Respected regulars get useful scraps, not the generous favors Well-liked folk earn.\"",
                "Marcus nods warmly. \"Bloomtown respects you, so the workshop shares a little.\"",
            ],

            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Ben scans the lanes. \"Because Bloomtown respects you — the east path gets loose after market hours. Guard backing at your standing adds +1 only.\"",
                "Ben nods at the guard post. \"Respected regulars get straight counsel — bridge work and patrol both reward folk who show up before trouble does.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Ben says evenly. \"Your standing here is real — patrol help and communal work both go further when folk trust who's walking the route.\"",
                "Ben leans on his lantern. \"Ben listens — because Bloomtown respects you, check the lanes early and the village remembers.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.GuardBacking, _) =>
            [
                "Ben sets a tool on the bench. \"Ben listens — because Bloomtown respects you, I'll still back your next well-or-bridge work for +1 progress or +1 wood on patrol.\"",
                "Ben nods at the post. \"Respected standing carries some weight on my route. Help patrol soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Ben, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Ben hands you a small bundle. \"Modest supplies from the post — Respected folk get useful help, not the generous favors Well-liked regulars earn.\"",
                "Ben nods evenly. \"Bloomtown respects you, so the guard route shares a little.\"",
            ],

            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Lila grins at the lanes. \"Because Bloomtown respects you — the square hums brighter before noon. Youth backing at your standing adds +1 only.\"",
                "Lila nods eagerly. \"Respected regulars get straight counsel — warehouse apples and village help both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Lila says warmly. \"Your standing here is real — help around the village and communal work both go further when folk trust who's lending a hand.\"",
                "Lila leans in. \"Lila listens — because Bloomtown respects you, walk the lanes before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.YouthBacking, _) =>
            [
                "Lila beams. \"Lila listens — because Bloomtown respects you, I'll still back your next warehouse-or-apple work for +1 progress or +1 apples on village help.\"",
                "Lila nods along the path. \"Respected standing carries some weight with young folk like me. Help village soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Lila, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Lila hands you a small apple. \"A modest kindness from the lanes — Respected folk get little gifts, not the generous favors Well-liked regulars earn.\"",
                "Lila grins. \"Bloomtown respects you, so young folk like me share a little.\"",
            ],

            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Rowan murmurs at the bench. \"Because Bloomtown respects you — the inn corner is quietest before noon. Story backing at your standing adds +1 only.\"",
                "Rowan nods slowly. \"Respected regulars get straight counsel — warehouse apples and story listening both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Rowan says warmly. \"Your standing here is real — listen at the bench and communal work both go further when folk trust who's lending an ear.\"",
                "Rowan leans in. \"Rowan listens — because Bloomtown respects you, sit before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.StoryBacking, _) =>
            [
                "Rowan smiles faintly. \"Rowan listens — because Bloomtown respects you, I'll still back your next warehouse-or-apple work for +1 progress or +1 wood on story listening.\"",
                "Rowan nods at the bench. \"Respected standing carries some weight with old tales like mine. Listen soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Recovery, _) =>
            [
                "Rowan gestures to the bench. \"Sit a while — Respected listeners earn a modest breath back here, gentler than Well-liked hearth rest but honest care.\"",
                "Rowan smiles faintly. \"Bloomtown respects you, so old tales share a little steadiness — quiet recovery for someone the village trusts.\"",
            ],
            (NpcEntityIds.Rowan, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Rowan sets a small piece of wood on the bench. \"A modest keepsake from old tales — Respected listeners earn small gifts, not the generous favors Well-liked folk receive.\"",
                "Rowan smiles faintly. \"Bloomtown respects you, so the bench shares a little.\"",
            ],

            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Info, true) =>
            [
                "Eleanor murmurs from the porch. \"Because Bloomtown respects you — the cottage is quietest before noon. Legacy backing at your standing adds +1 only.\"",
                "Eleanor nods warmly. \"Respected regulars get straight counsel — warehouse apples and porch chats both reward folk who show up early.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Info, false) =>
            [
                "Eleanor says warmly. \"Your standing here is real — chat on the porch and communal work both go further when folk trust who's lending an ear.\"",
                "Eleanor leans in. \"Eleanor listens — because Bloomtown respects you, sit before the market stirs and Bloomtown remembers.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.LegacyBacking, _) =>
            [
                "Eleanor smiles warmly. \"Eleanor listens — because Bloomtown respects you, I'll still back your next warehouse-or-apple work for +1 progress or +1 apples on porch chats.\"",
                "Eleanor nods from her chair. \"Respected standing carries some weight with old stories like mine. Chat soon — +1 extra. Reach Well-liked and that backing doubles.\"",
            ],
            (NpcEntityIds.Eleanor, SocialInfluenceOutcomeKind.Item, _) =>
            [
                "Eleanor sets a small apple in your hands. \"A modest keepsake from the porch — Respected listeners earn small gifts, not the generous favors Well-liked folk receive.\"",
                "Eleanor smiles warmly. \"Bloomtown respects you, so the cottage shares a little.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetDeclineLine(
        uint npcEntityId,
        uint variationSeed,
        VillageSocialStandingTier tier = VillageSocialStandingTier.WellLiked)
    {
        if (tier < VillageSocialStandingTier.WellLiked)
            return TryGetRespectedDeclineLine(npcEntityId, variationSeed);

        string[] lines = npcEntityId switch
        {
            NpcEntityIds.Harold =>
            [
                "Harold tips his hat politely. \"Not the moment, friend — but your standing still earns you a patient hearing. Come back when the well-side's quieter.\"",
                "Harold murmurs. \"Shared work calls me elsewhere — but Bloomtown esteems you, and I'll make time soon.\"",
            ],
            NpcEntityIds.Greta =>
            [
                "Greta smiles warmly. \"Kitchen's in a whirl just now, love — but honored guests like you always get another turn. Try me again later.\"",
                "Greta fusses over another table. \"Parlor's full — but your standing means I'll save you a moment when things settle.\"",
            ],
            NpcEntityIds.Mira =>
            [
                "Mira grins apologetically. \"Market rush, love — but well-liked regulars always get another turn at my stall. Try me again later.\"",
                "Mira waves a vendor off. \"Square's loud today — but your standing means I'll save you a proper trade favor when things settle.\"",
            ],
            NpcEntityIds.Elsie =>
            [
                "Elsie smiles gently. \"The beds need tending just now — but someone Bloomtown esteems always gets another hearing. Come back when the garden's quieter.\"",
                "Elsie nods warmly. \"Harvest calls me elsewhere — but your standing means I'll make time for you soon.\"",
            ],
            NpcEntityIds.Tom =>
            [
                "Tom tips his cap. \"Timber's moving just now — but well-liked folk always get another turn at the yard. Try me again later.\"",
                "Tom wipes his brow. \"The grove calls — but your standing means I'll save you a proper lumber favor when things settle.\"",
            ],
            NpcEntityIds.Nora =>
            [
                "Nora smiles gently. \"The herb rows need tending just now — but someone Bloomtown esteems always gets another hearing. Come back when the beds are quieter.\"",
                "Nora nods warmly. \"Harvest calls me elsewhere — but your standing means I'll make time for you soon.\"",
            ],
            NpcEntityIds.Elias =>
            [
                "Elias wipes his hands. \"Forge is hot and busy — but well-liked regulars always get another turn at the smithy. Try me again later.\"",
                "Elias nods at the anvil. \"Hammer work calls — but your standing means I'll save you a proper smithing favor when things settle.\"",
            ],
            NpcEntityIds.Marcus =>
            [
                "Marcus wipes his hands. \"Bench is busy and full of orders — but well-liked regulars always get another turn at the workshop. Try me again later.\"",
                "Marcus nods at the worktable. \"Craft work calls — but your standing means I'll save you a proper crafting favor when things settle.\"",
            ],
            NpcEntityIds.Ben =>
            [
                "Ben tips his lantern. \"Patrol calls just now — but well-liked regulars always get another turn at the guard post. Try me again later.\"",
                "Ben nods along the lane. \"Route work calls — but your standing means I'll save you a proper guard favor when things settle.\"",
            ],
            NpcEntityIds.Lila =>
            [
                "Lila smiles apologetically. \"Errands call just now — but well-liked regulars always get another turn on the lanes. Try me again later.\"",
                "Lila waves toward the square. \"Market's loud today — but your standing means I'll save you a proper village favor when things settle.\"",
            ],
            NpcEntityIds.Rowan =>
            [
                "Rowan smiles apologetically. \"A tale needs finishing just now — but well-liked regulars always get another turn at the bench. Try me again later.\"",
                "Rowan nods toward the inn. \"Evening's loud today — but your standing means I'll save you a proper story favor when things settle.\"",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static string? TryGetRespectedDeclineLine(uint npcEntityId, uint variationSeed)
    {
        string[] lines = npcEntityId switch
        {
            NpcEntityIds.Harold =>
            [
                "Harold tips his hat politely. \"Not the moment, friend — but Respected standing still earns you a patient hearing. Come back when the well-side's quieter.\"",
                "Harold murmurs. \"Shared work calls me elsewhere — Bloomtown respects you, and I'll make time soon, though the longer Respected cooldown applies.\"",
            ],
            NpcEntityIds.Greta =>
            [
                "Greta smiles warmly. \"Kitchen's in a whirl just now, love — but folk Respected here always get another turn. Try me again later.\"",
                "Greta fusses over another table. \"Parlor's full — your standing means I'll save you a moment when things settle.\"",
            ],
            NpcEntityIds.Mira =>
            [
                "Mira grins apologetically. \"Market rush, love — but Respected regulars always get another turn at my stall. Try me again later.\"",
                "Mira waves a vendor off. \"Square's loud today — your standing means I'll save you proper counsel when things settle.\"",
            ],
            NpcEntityIds.Elsie =>
            [
                "Elsie smiles gently. \"The beds need tending just now — but someone Bloomtown respects always gets another hearing. Come back when the garden's quieter.\"",
                "Elsie nods warmly. \"Harvest calls me elsewhere — your standing means I'll make time for you soon.\"",
            ],
            NpcEntityIds.Tom =>
            [
                "Tom tips his cap. \"Timber's moving just now — but Respected folk always get another turn at the yard. Try me again later.\"",
                "Tom wipes his brow. \"The grove calls — your standing means I'll save you a modest lumber favor when things settle.\"",
            ],
            NpcEntityIds.Nora =>
            [
                "Nora smiles gently. \"The herb rows need tending just now — but someone Bloomtown respects always gets another hearing. Come back when the beds are quieter.\"",
                "Nora nods warmly. \"Harvest calls me elsewhere — your standing means I'll make time for you soon.\"",
            ],
            NpcEntityIds.Elias =>
            [
                "Elias wipes his hands. \"Forge is hot and busy — but Respected regulars always get another turn at the smithy. Try me again later.\"",
                "Elias nods at the anvil. \"Hammer work calls — your standing means I'll save you a modest smithing favor when things settle.\"",
            ],
            NpcEntityIds.Marcus =>
            [
                "Marcus wipes his hands. \"Bench is busy and full of orders — but Respected regulars always get another turn at the workshop. Try me again later.\"",
                "Marcus nods at the worktable. \"Craft work calls — your standing means I'll save you a modest crafting favor when things settle.\"",
            ],
            NpcEntityIds.Ben =>
            [
                "Ben tips his lantern. \"Patrol calls just now — but Respected regulars always get another turn at the guard post. Try me again later.\"",
                "Ben nods along the lane. \"Route work calls — your standing means I'll save you a modest guard favor when things settle.\"",
            ],
            NpcEntityIds.Lila =>
            [
                "Lila smiles apologetically. \"Errands call just now — but Respected regulars always get another turn on the lanes. Try me again later.\"",
                "Lila waves toward the square. \"Market's loud today — your standing means I'll save you a modest village favor when things settle.\"",
            ],
            NpcEntityIds.Rowan =>
            [
                "Rowan smiles apologetically. \"A tale needs finishing just now — but Respected regulars always get another turn at the bench. Try me again later.\"",
                "Rowan nods toward the inn. \"Evening's loud today — your standing means I'll save you a modest story favor when things settle.\"",
            ],
            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}