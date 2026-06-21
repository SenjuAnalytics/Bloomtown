using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Ambient villager lines and status summaries reflecting the player's social standing in Bloomtown.
/// </summary>
public static class VillageSocialStandingDialogue
{
    public static string? TryGetVillagerAmbientComment(
        VillageSocialStandingTier tier,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (tier == VillageSocialStandingTier.Stranger || focusCloseFriendNpcIds.Count == 0)
            return null;

        string[] lines = (tier, villageNoticedMemory, focusCloseFriendNpcIds.Count >= 3) switch
        {
            (VillageSocialStandingTier.WellLiked, true, true) =>
            [
                "A neighbor murmurs with quiet respect, \"People say you've become one of the most honored residents in Bloomtown — and they mean it.\"",
                "Someone at the lane edge says, \"Folk speak your name with real fondness now. That's earned, not given.\"",
                "Overheard: \"You've got real roots here — neighbors treat you like someone the village is proud of.\"",
                "A villager tips their head respectfully. \"Morning — folk say you're one of Bloomtown's most respected neighbors now.\"",
                $"Someone says warmly, \"{FormatSampleName(focusCloseFriendNpcIds, variationSeed)} isn't the only one — half the village counts you as family.\"",
                "A neighbor adds softly, \"We hold you in high regard here — even quiet folk who barely know the regulars notice your standing.\"",
                "Overheard at the lane: \"Bloomtown regards you as one of their most trusted residents. Everyone's noticed — not just the usual circle.\"",
                "A passerby greets you by name before you speak. \"Good to see you — the whole village speaks well of you these days.\"",
                "Someone waves from across the lane. \"Folk are glad you're here — you're one of the faces Bloomtown is proud of.\"",
                "Overheard: \"Even folk who keep to themselves know your name now — and say it with respect. That's rare here.\"",
                "A villager by the well murmurs, \"Harold says you're one of the good ones — folk around here agree wholeheartedly.\"",
                "A villager you've only nodded to before stops and smiles. \"People say you're among the most respected here — I wanted to say I agree.\"",
                "Overheard from a porch: \"She's not just close with the regulars — the whole village holds her in esteem now.\"",
                "Someone calls softly across the square. \"Bloomtown's lucky to have you — folk say that openly these days.\"",
            ],
            (VillageSocialStandingTier.WellLiked, true, false) =>
            [
                "A villager remarks with respect, \"People say you're one of the most honored residents in Bloomtown — the village is lucky to have you.\"",
                "Overheard nearby: \"You're someone the whole village counts on — folk speak of you with real esteem.\"",
                "A neighbor adds, \"You're a face Bloomtown is proud of — everyone says so with genuine warmth.\"",
                "Someone calls warmly, \"Neighbors hold you in high regard — the village values people like you.\"",
                "A villager nods as you pass. \"Good to see you — folk greet you like someone who truly belongs here.\"",
                "Overheard: \"Bloomtown feels more complete when you're walking the lanes — people notice that.\"",
                "A villager smiles. \"Greta says you're one of her most trusted regulars — we all hear those stories.\"",
                "A quiet neighbor tips their head. \"I don't know the regulars well — but I know your name, and I know it's spoken with respect.\"",
                "Overheard: \"She's become one of the most respected folk here — not just among the inner circle.\"",
            ],
            (VillageSocialStandingTier.WellLiked, false, _) =>
            [
                "Someone calls softly with respect, \"Folk say you've become one of the most honored residents in Bloomtown.\"",
                "A passerby says, \"Bloomtown feels like yours now — people speak of you with real pride.\"",
                "Overheard: \"People say you're very close with many folk here — the whole village holds you in esteem.\"",
                "A villager nods your way. \"You're well-liked here — folk greet you like someone the village is proud of.\"",
                "A neighbor smiles. \"You're one of the faces people look for in the square — and greet with respect.\"",
                "Overheard at the well: \"Half the village knows your name — and says it with genuine fondness.\"",
                "A villager by the forge murmurs, \"Elias says you're steady and trusted — folk around here agree.\"",
                "A villager by the workshop murmurs, \"Marcus says you're steady and trusted — folk around here agree.\"",
                "Someone near the herb rows smiles. \"Nora told us you're one of the good ones — I believe her completely.\"",
                "A neighbor at the inn lane adds, \"Greta speaks of you like family — that tells you everything about your standing here.\"",
                "A villager you barely know tips their cap. \"People say you're among the most respected neighbors here — good to finally put a face to the name.\"",
                "Overheard from the market edge: \"Bloomtown knows her now — not just the regulars, but all of us.\"",
                "Someone murmurs with quiet pride, \"Folk say you're one of the most trusted people in the village — I hear it everywhere I walk.\"",
                "A villager you've never spoken to greets you by name. \"Bloomtown knows you now — and speaks of you with real fondness.\"",
                "Overheard at the forge: \"That neighbor's become part of how this village feels — folk notice when they're around.\"",
            ],

            (VillageSocialStandingTier.Respected, true, _) =>
            [
                $"A villager nods. \"{FormatSampleName(focusCloseFriendNpcIds, variationSeed)} isn't the only one who trusts you — word gets around.\"",
                "Overheard: \"People trust you more these days. The village sees who you show up for.\"",
                "Someone says warmly, \"You're not a newcomer anymore — neighbors speak well of your company.\"",
                "A neighbor murmurs, \"People treat you a little warmer these days — you've earned it.\"",
                "Someone remarks, \"Bloomtown's starting to count on your presence — that's respect, that is.\"",
                "A passerby tips their head. \"Morning — folk mention you kindly these days.\"",
                "Overheard: \"More neighbors know your face now — and greet you like you belong.\"",
                "A villager by the market edge says, \"People are starting to notice you around town — in a good way.\"",
                "Someone calls softly, \"Good to see a familiar face — neighbors talk about you more kindly lately.\"",
                "Overheard: \"You're not just close with the regulars anymore — ordinary folk are beginning to know your name.\"",
                "A neighbor smiles. \"Bloomtown's warming to you — I can tell by how folk greet you at the square.\"",
            ],
            (VillageSocialStandingTier.Respected, false, _) =>
            [
                "A neighbor remarks, \"Folks are starting to pair your name with trust around here.\"",
                $"Someone mentions, \"{FormatPairNames(focusCloseFriendNpcIds)} both speak warmly of you — people notice.\"",
                "Overheard: \"Bloomtown is beginning to see you as one of their own.\"",
                "A villager says quietly, \"You're becoming a familiar face — folk greet you differently now.\"",
                "A passerby adds, \"Neighbors trust you more — you can hear it in how they talk about you.\"",
                "Someone at the lane edge murmurs, \"People are starting to know you beyond the regulars — that's something.\"",
                "A villager smiles briefly. \"Good to see you around — you're not a stranger to many folk now.\"",
                "Overheard from the garden path: \"More folk are starting to notice who you spend time with — and they speak well of it.\"",
                "A quiet villager nods as you pass. \"People are beginning to recognize you here — that takes time, and you've earned it.\"",
                "Someone murmurs, \"The village is paying attention to you lately — neighbors mention your name with growing trust.\"",
            ],

            (VillageSocialStandingTier.Known, true, _) =>
            [
                $"A villager says, \"{FormatSampleName(focusCloseFriendNpcIds, variationSeed)} talks about you fondly — others are starting to listen.\"",
                "Overheard: \"People are beginning to notice how close you've become with the regulars.\"",
            ],
            (VillageSocialStandingTier.Known, false, _) =>
            [
                $"Someone remarks, \"{FormatSampleName(focusCloseFriendNpcIds, variationSeed)} lights up when you're nearby — word travels in a small village.\"",
                "A neighbor says quietly, \"You're becoming someone people recognize around Bloomtown.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetFocusNpcStandingWarmthLine(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (tier < VillageSocialStandingTier.Respected)
            return null;

        string[] lines = (npcEntityId, tier, villageNoticedMemory) switch
        {
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Elsie smiles with quiet pride. \"Folk say you're one of the most trusted people in Bloomtown — I felt that at the garden long before the lanes named it.\"",
                "Elsie adds warmly, \"The village speaks of you like family now. I'm honored I knew you first.\"",
                "Elsie says gently, \"Neighbors hold you in real esteem — the beds have heard that praise for a while.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Elsie nods with fondness. \"People say you're one of Bloomtown's most cherished neighbors — I see why they trust you.\"",
                "Elsie beams. \"The village regards you with real pride now — the garden's glad of it.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.Respected, _) =>
            [
                "Elsie smiles. \"Neighbors mention how many folk trust you — the garden hears the same stories.\"",
                "Elsie says gently, \"Word reaches the beds that you're becoming one of Bloomtown's own.\"",
                "Elsie adds, \"People greet you warmer these days — I notice it when you pass the gate.\"",
                "Elsie leans on the gate. \"People are starting to notice you around town — the village sees who you show up for.\"",
            ],

            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Harold tips his hat with quiet pride. \"The village regards you as one of its own — I told them they were right to.\"",
                "Harold murmurs, \"Folk trust you widely now. In Bloomtown, that's the highest compliment an elder can hear.\"",
                "Harold says quietly, \"Neighbors speak your name with fondness — the well hears that kind of talk every morning.\"",
                "Harold adds, \"You've earned real standing here — I count you among the folk this village is proud of.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Harold says quietly, \"People speak of you with real esteem all over town. I agree with every word.\"",
                "Harold nods. \"Bloomtown holds you in high regard now — I hear it every morning by the well.\"",
                "Harold tips his hat. \"You're one of the trusted ones now — folk like you shape what this village becomes.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.Respected, _) =>
            [
                "Harold tips his hat. \"The village speaks well of you. So do I.\"",
                "Harold adds, \"Neighbors say you're trusted here — I hear it by the well most days.\"",
                "Harold murmurs, \"People treat you warmer — you've earned that from Bloomtown.\"",
                "Harold says quietly, \"Folk are starting to notice you more — people pay attention to neighbors who keep showing up.\"",
            ],

            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Mira grins. \"Market gossip says you're close with everyone worth knowing — I started that rumor, by the way.\"",
                "Mira laughs warmly. \"The whole square talks about you like family. Glad I'm in the circle.\"",
                "Mira beams. \"Folk trust you all over Bloomtown — the square's been saying so for weeks.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Mira beams. \"Everyone's talking about how close you've become with the regulars — glad I'm one of them.\"",
                "Mira winks. \"Bloomtown regards you warmly — I make sure the market hears the good stories.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.Respected, _) =>
            [
                "Mira grins. \"Folk at the square say you're trusted around town — I already knew that.\"",
                "Mira adds, \"Word travels fast when someone becomes a familiar face — you're there now.\"",
                "Mira says cheerfully, \"Neighbors greet you warmer — the market notices who Bloomtown trusts.\"",
            ],

            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Tom gives a rare smile. \"Bloomtown talks about you like one of theirs. I reckon they're right.\"",
                "Tom nods slowly. \"Folk trust you all over — even quiet yards hear that kind of talk.\"",
                "Tom says plainly, \"The village regards you warmly. Means something, coming from here.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Tom allows a half-smile. \"People say you're close with the regulars. Good — means you belong.\"",
                "Tom murmurs, \"Neighbors trust you now — I hear it when folk pass the woodpile.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.Respected, _) =>
            [
                "Tom gives a rare half-smile. \"Word's out you're one of ours. Good.\"",
                "Tom says plainly, \"Neighbors trust you — I hear it at the woodpile. Means something.\"",
                "Tom adds quietly, \"People treat you warmer these days — Bloomtown's noticed.\"",
            ],

            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Greta beams with pride. \"Everyone in Bloomtown knows your name now — guests ask after you before I even serve their tea.\"",
                "Greta squeezes your arm warmly. \"The whole village talks about you like family — and my parlor holds you in the highest regard.\"",
                "Greta beams. \"Folk say you're one of the most trusted people in Bloomtown. I see why, love.\"",
                "Greta fusses over you. \"You're one of my most honored regulars — guests like you are why this inn feels like home.\"",
                "Greta saves you the warm seat. \"For someone the village esteems — you always get the best corner by the hearth.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Greta smiles with real warmth. \"People say you've become one of Bloomtown's most cherished neighbors — I felt it at my table first.\"",
                "Greta waves you closer. \"Neighbors hold you in high regard — my inn treats you as a guest of honor.\"",
                "Greta fusses over your cup. \"Regulars the village is proud of get the little extras — and you've earned every one.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.Respected, true) =>
            [
                "Greta says warmly, \"Neighbors mention how many close friends you've made — my hearth's glad of it too.\"",
                "Greta fusses over you. \"Bloomtown trusts you now. Regulars like you keep an inn honest.\"",
                "Greta beams. \"People treat you warmer — I save the good corner for folk the village respects.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.Respected, false) =>
            [
                "Greta says cheerfully, \"Word reaches the inn that you're close with the regulars — I believe it.\"",
                "Greta waves you closer. \"Folk trust you around here now. Sit — you look like you belong.\"",
                "Greta adds, \"Neighbors speak well of you — my parlor hears every story.\"",
            ],

            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Nora smiles softly. \"The village speaks of you like family now — the herb beds felt that before the lanes named it.\"",
                "Nora says gently, \"Folk trust you widely — even quiet gardens hear that kind of talk.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Nora nods. \"People say you're close with half the village — I see why they trust you.\"",
                "Nora murmurs, \"Bloomtown regards you warmly now — the herbs are glad of it.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.Respected, _) =>
            [
                "Nora says quietly, \"Neighbors mention how many folk trust you — the garden hears the same stories.\"",
                "Nora adds, \"Word reaches the beds that you're becoming one of Bloomtown's own.\"",
                "Nora smiles. \"People greet you warmer these days — I notice it when you pass the rows.\"",
            ],

            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Elias nods slowly. \"The village speaks of you like family now — the forge felt that before the lanes named it.\"",
                "Elias says plainly, \"Folk trust you widely — even quiet smithies hear that kind of talk.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Elias grunts softly. \"People say you're close with half the village — I see why they trust you.\"",
                "Elias adds, \"Bloomtown regards you warmly now — the tools are glad of it.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.Respected, _) =>
            [
                "Elias says plainly, \"Neighbors mention how many folk trust you — the forge hears the same stories.\"",
                "Elias adds, \"Word reaches the anvil that you're becoming one of Bloomtown's own.\"",
                "Elias nods. \"People greet you warmer these days — I notice it when you pass the smithy.\"",
            ],

            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Marcus nods slowly. \"The village speaks of you like family now — the workshop felt that before the lanes named it.\"",
                "Marcus says warmly, \"Folk trust you widely — even quiet workshops hear that kind of talk.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Marcus smiles. \"People say you're close with half the village — I see why they trust you.\"",
                "Marcus adds, \"Bloomtown regards you warmly now — the bench is glad of it.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.Respected, _) =>
            [
                "Marcus says plainly, \"Neighbors mention how many folk trust you — the workshop hears the same stories.\"",
                "Marcus adds, \"Word reaches the bench that you're becoming one of Bloomtown's own.\"",
                "Marcus nods warmly. \"People greet you warmer these days — I notice it when you pass the workshop.\"",
            ],

            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Ben nods slowly. \"The village speaks of you like family now — the lanes felt that before the square named it.\"",
                "Ben says evenly, \"Folk trust you widely — even quiet patrols hear that kind of talk.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Ben says plainly. \"People say you're close with half the village — I see why they trust you.\"",
                "Ben adds, \"Bloomtown regards you warmly now — the guard post is glad of it.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.Respected, _) =>
            [
                "Ben says evenly, \"Neighbors mention how many folk trust you — the lanes hear the same stories.\"",
                "Ben adds, \"Word reaches the guard post that you're becoming one of Bloomtown's own.\"",
                "Ben nods. \"People greet you warmer these days — I notice it when you pass my route.\"",
            ],

            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Lila beams. \"The village speaks of you like family now — the lanes felt that before the square named it.\"",
                "Lila says warmly, \"Folk trust you widely — even young helpers like me hear that kind of talk.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Lila grins. \"People say you're close with half the village — I see why they trust you.\"",
                "Lila adds, \"Bloomtown regards you warmly now — I'm glad of it.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.Respected, _) =>
            [
                "Lila says cheerfully, \"Neighbors mention how many folk trust you — the square hears the same stories.\"",
                "Lila adds, \"Word reaches the lanes that you're becoming one of Bloomtown's own.\"",
                "Lila waves. \"People greet you warmer these days — I notice it when you pass through.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Rowan smiles faintly. \"The village speaks of you like family now — the bench felt that before the square named it.\"",
                "Rowan says warmly, \"Folk trust you widely — even old storytellers like me hear that kind of talk.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Rowan nods. \"People say you're close with half the village — I see why they trust you.\"",
                "Rowan adds, \"Bloomtown regards you warmly now — I'm glad of it.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.Respected, _) =>
            [
                "Rowan says quietly, \"Neighbors mention how many folk trust you — the inn hears the same stories.\"",
                "Rowan adds, \"Word reaches the bench that you're becoming one of Bloomtown's own.\"",
                "Rowan waves. \"People greet you warmer these days — I notice it when you pass through.\"",
                "Rowan murmurs, \"People are starting to notice you around town — old tales hear that kind of talk first.\"",
            ],

            (NpcEntityIds.Eleanor, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Eleanor smiles warmly from her porch. \"The whole village speaks of you like family now — I've watched that happen season by season.\"",
                "Eleanor says softly, \"Folk trust you widely — even quiet porches hear neighbors say your name with fondness.\"",
            ],
            (NpcEntityIds.Eleanor, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Eleanor nods from her chair. \"People say you're close with half the village — I see why Bloomtown esteems you.\"",
                "Eleanor murmurs, \"Neighbors hold you in high regard — the cottage porch hears every kind word.\"",
            ],
            (NpcEntityIds.Eleanor, VillageSocialStandingTier.Respected, _) =>
            [
                "Eleanor says warmly, \"Neighbors mention how many folk trust you — the porch hears the same stories.\"",
                "Eleanor adds, \"Word reaches the cottage that you're becoming one of Bloomtown's own.\"",
                "Eleanor smiles. \"People are starting to notice you around town — I notice it when you pass the well.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetWellLikedPrivilegeLine(
        uint npcEntityId,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, villageNoticedMemory) switch
        {
            (NpcEntityIds.Elsie, true) =>
            [
                "Elsie leans in. \"Since Bloomtown trusts you, I'll share this — the south beds catch morning light best after rain.\"",
                "Elsie hands you a ripe apple from her basket. \"For someone the village speaks of fondly — the garden's small thank-you.\"",
            ],
            (NpcEntityIds.Elsie, false) =>
            [
                "Elsie smiles. \"You're well-liked here — take this tip: check the herb rows before noon, they're generous lately.\"",
            ],

            (NpcEntityIds.Harold, true) =>
            [
                "Harold murmurs with elder's respect, \"Bloomtown holds you in high regard — so I'll say it plain: the well runs clearest at first light, and communal work counts more when you're there.\"",
                "Harold sets a stick of wood by your feet. \"For someone the village esteems — a small favor from your elder, nothing more.\"",
                "Harold tips his hat. \"Your standing here is real — folk listen when you lend a hand at shared work. I notice, and so does the village.\"",
            ],
            (NpcEntityIds.Harold, false) =>
            [
                "Harold tips his hat. \"Folk hold you in high regard around here — the lane by the well's quieter before the square wakes.\"",
                "Harold says quietly, \"As one of Bloomtown's trusted neighbors, you ought to know — project work goes smoother when folk like you show up.\"",
            ],

            (NpcEntityIds.Mira, true) =>
            [
                "Mira winks. \"Because Bloomtown likes you, here's real gossip — Elsie's apples sell out fastest on market mornings.\"",
                "Mira slips you an apple. \"A little market favor — well-liked folk get looked after, you know.\"",
            ],
            (NpcEntityIds.Mira, false) =>
            [
                "Mira grins. \"You're trusted here — Mira's tip: greet the early vendors, they remember kindness.\"",
            ],

            (NpcEntityIds.Tom, true) =>
            [
                "Tom sets a plank aside for you. \"Village trusts you — take it. Good wood shouldn't go to waste on strangers.\"",
                "Tom says plainly, \"Bloomtown regards you well. The shed out back stays dry — useful if you're working late.\"",
            ],
            (NpcEntityIds.Tom, false) =>
            [
                "Tom nods. \"Neighbors speak well of you — I'll mention the woodpile's stocked on the east side.\"",
            ],

            (NpcEntityIds.Greta, true) =>
            [
                "Greta fusses over your cup. \"For a guest Bloomtown esteems — second helping's on the house, love.\"",
                "Greta presses an apple into your hand. \"Kitchen had one spare — honored guests get the little extras.\"",
                "Greta beams. \"The village speaks of you with real pride — so I saved you the warm seat by the hearth.\"",
                "Greta leans in warmly. \"Between us — the traveling merchant's been asking after you. Says folk all over town speak your name kindly.\"",
            ],
            (NpcEntityIds.Greta, false) =>
            [
                "Greta smiles. \"Folk hold you in high regard — sit, I'll bring something from the kitchen while it's fresh.\"",
                "Greta waves off your thanks. \"Guests the village is proud of get fussed over — it's the innkeeper's privilege, love.\"",
                "Greta adds softly. \"Travelers ask after you sometimes — word of your standing reaches even strangers at my table.\"",
            ],

            (NpcEntityIds.Nora, true) =>
            [
                "Nora leans in. \"Since Bloomtown trusts you — the south herb rows catch morning light best after rain.\"",
                "Nora hands you an apple from her basket. \"For someone the village speaks of fondly — the garden's small thank-you.\"",
            ],
            (NpcEntityIds.Nora, false) =>
            [
                "Nora smiles. \"You're well-liked here — check the herb rows before noon, they're generous lately.\"",
            ],

            (NpcEntityIds.Elias, true) =>
            [
                "Elias leans in. \"Since Bloomtown trusts you — timber near the bridge stacks higher before dusk.\"",
                "Elias hands you a plank from his bench. \"For someone the village speaks of fondly — the forge's small thank-you.\"",
            ],
            (NpcEntityIds.Elias, false) =>
            [
                "Elias nods. \"You're well-liked here — check the forge before noon, the tools are ready lately.\"",
            ],

            (NpcEntityIds.Marcus, true) =>
            [
                "Marcus leans in. \"Since Bloomtown trusts you — plank stock near the bridge firms up before dusk.\"",
                "Marcus hands you a plank from his bench. \"For someone the village speaks of fondly — the workshop's small thank-you.\"",
            ],
            (NpcEntityIds.Marcus, false) =>
            [
                "Marcus nods warmly. \"You're well-liked here — check the workshop before noon, the tools are ready lately.\"",
            ],

            (NpcEntityIds.Ben, true) =>
            [
                "Ben leans in. \"Since Bloomtown trusts you — the east lane gets restless after market hours.\"",
                "Ben hands you a stick of wood from his kit. \"For someone the village speaks of fondly — the post's small thank-you.\"",
            ],
            (NpcEntityIds.Ben, false) =>
            [
                "Ben nods. \"You're well-liked here — walk the route before dusk, the paths stay honest lately.\"",
            ],

            (NpcEntityIds.Lila, true) =>
            [
                "Lila leans in. \"Since Bloomtown trusts you — the square hums brightest before noon, and the garden path needs friendly eyes before dusk.\"",
                "Lila hands you an apple from her bundle. \"For someone the village speaks of fondly — a young villager's small thank-you.\"",
            ],
            (NpcEntityIds.Lila, false) =>
            [
                "Lila smiles. \"You're well-liked here — help around the village before the market stirs, the lanes stay welcoming lately.\"",
            ],
            (NpcEntityIds.Rowan, true) =>
            [
                "Rowan leans in. \"Since Bloomtown trusts you — the bench is quietest before noon, and Harold's porch holds tales the square never tells.\"",
                "Rowan hands you wood from his bundle. \"For someone the village speaks of fondly — a storyteller's small thank-you.\"",
            ],
            (NpcEntityIds.Rowan, false) =>
            [
                "Rowan smiles faintly. \"You're well-liked here — listen at the story bench before the market stirs, the inn corner stays welcoming lately.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? FormatBroaderVillageRecognitionHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        return (tier, villageNoticedMemory) switch
        {
            (VillageSocialStandingTier.WellLiked, true) =>
                "Broader village recognition: ordinary neighbors — not just the focus regulars — greet you by name, speak of you with respect, and treat your presence as part of what makes Bloomtown feel whole.",
            (VillageSocialStandingTier.WellLiked, false) =>
                "Broader village recognition: folk beyond your close circle know who you are now — quiet villagers, market passersby, and lane neighbors all hold you in genuine esteem.",
            _ => null,
        };
    }

    public static string? FormatStandingImpactHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        return (tier, villageNoticedMemory) switch
        {
            (VillageSocialStandingTier.WellLiked, true) =>
                "Village treatment: Bloomtown holds you in high regard — Harold, Greta, Nora, and Elias treat you as an honored regular, and ordinary folk across the lanes speak your name with personal respect.",
            (VillageSocialStandingTier.WellLiked, false) =>
                "Village treatment: you're one of the village's most trusted faces — focus regulars welcome you warmly, and neighbors you've barely met already know your standing.",
            (VillageSocialStandingTier.Respected, true) =>
                "Village treatment: neighbors treat you warmer and more openly — focus regulars greet you differently, ordinary folk are starting to recognize you, and the square feels a little friendlier when you pass.",
            (VillageSocialStandingTier.Respected, false) =>
                "Village treatment: people trust you more around Bloomtown — Harold, Greta, Nora, and Elias welcome you with clearer warmth, and neighbors beyond your close circle are beginning to know your name.",
            _ => null,
        };
    }

    public static string? FormatVillageAtmosphereHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        return (tier, villageNoticedMemory) switch
        {
            (VillageSocialStandingTier.WellLiked, true) =>
                "How Bloomtown sees you: you're among its most honored residents — ordinary villagers greet you by name, focus NPCs treat you as family, and the whole community speaks of your presence with quiet pride.",
            (VillageSocialStandingTier.WellLiked, false) =>
                "How Bloomtown sees you: one of its most cherished neighbors — folk beyond your close circle know who you are, greet you with genuine esteem, and the lanes feel warmer when you walk them.",
            (VillageSocialStandingTier.Respected, true) =>
                "How Bloomtown sees you: a trusted neighbor whose name is spreading — people beyond your close friends are starting to recognize you, and ordinary villagers greet you with growing warmth.",
            (VillageSocialStandingTier.Respected, false) =>
                "How Bloomtown sees you: someone the village is beginning to count as their own — neighbors notice your presence at the square, forge, and garden, and word of your standing travels a little farther each day.",
            _ => null,
        };
    }

    public static string FormatStandingStatus(
        VillageSocialStandingTier tier,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory)
    {
        return (tier, villageNoticedMemory, focusCloseFriendNpcIds.Count >= 3) switch
        {
            (VillageSocialStandingTier.WellLiked, true, true) =>
                "Well-liked — Bloomtown regards you as one of its most honored residents. Ordinary villagers and focus regulars alike hold you in high esteem, and your presence shapes how the whole community feels.",
            (VillageSocialStandingTier.WellLiked, true, false) =>
                "Well-liked — the village holds you in genuine esteem. Neighbors beyond your close friends speak of you with respect, and ordinary folk greet you like family.",
            (VillageSocialStandingTier.WellLiked, false, _) =>
                "Well-liked — you're one of Bloomtown's most cherished neighbors. Folk across the lanes greet you with real warmth, and even quiet villagers know your name.",

            (VillageSocialStandingTier.Respected, true, _) =>
                "Respected — neighbors trust you more these days. Word has spread through Bloomtown, ordinary folk greet you with real warmth, and people are starting to notice you around town.",
            (VillageSocialStandingTier.Respected, false, _) =>
                "Respected — Bloomtown is starting to see you as one of their own. Neighbors beyond your close friends treat you with growing trust, and your name travels a little farther each week.",

            (VillageSocialStandingTier.Known, true, _) =>
                $"Known — villagers notice your closeness with {FormatSampleName(focusCloseFriendNpcIds, 0)}.",
            (VillageSocialStandingTier.Known, false, _) =>
                "Known — you're becoming someone people recognize around the village.",

            _ => string.Empty,
        };
    }

    /// <summary>
    /// Rare appendix when a focus NPC notes that the wider village is beginning to recognize the player.
    /// </summary>
    public static string? TryGetVillageSocialStandingAwarenessLine(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (tier < VillageSocialStandingTier.Respected)
            return null;

        string[] lines = (npcEntityId, tier, villageNoticedMemory) switch
        {
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Elsie adds softly, \"Folk all over Bloomtown know your name now — the garden's only one place that noticed first.\"",
                "Elsie smiles. \"Ordinary neighbors mention you warmly now — not just the regulars who've known you longest.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Elsie smiles. \"Neighbors talk about you like you're one of theirs — I hear it from more than just the regulars.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.Respected, _) =>
            [
                "Elsie says gently, \"More people in the village are starting to know you — I can tell by how they greet you at the gate.\"",
            ],

            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Harold murmurs, \"Half the village speaks your name with trust now — the well hears that kind of talk every morning.\"",
                "Harold tips his hat. \"Even quiet folk who keep to themselves know who you are — Bloomtown regards you widely now.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Harold tips his hat. \"Folk beyond the regulars know you now — Bloomtown doesn't hand that out lightly.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.Respected, _) =>
            [
                "Harold says quietly, \"Neighbors are starting to recognize you — I hear it when folk pass the well.\"",
            ],

            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Mira grins. \"The whole village knows you now — I didn't even have to spread all the good stories myself.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Mira winks. \"People all over town greet you warmly — market gossip's been very kind lately.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.Respected, _) =>
            [
                "Mira adds cheerfully, \"More folk in the square are starting to know you — word travels fast in a village this size.\"",
            ],

            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Tom says plainly, \"Bloomtown knows you now — even quiet yards hear neighbors speak your name with trust.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Tom nods. \"Folk beyond the regulars recognize you — that means something in a place this small.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.Respected, _) =>
            [
                "Tom murmurs, \"More neighbors know your face now — I hear it when folk pass the woodpile.\"",
            ],

            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Greta beams. \"The whole village talks about you warmly — my parlor's only one room that heard it first.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Greta fusses over you. \"Neighbors all over Bloomtown know you now — regulars like you change how a village feels.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.Respected, _) =>
            [
                "Greta says warmly, \"More people in the village are starting to recognize you — my hearth hears every story.\"",
            ],

            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Nora smiles softly. \"Folk all over Bloomtown know your name now — the herb beds felt that before the lanes named it.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Nora nods. \"Neighbors talk about you like you're one of theirs — even quiet gardens hear that kind of talk.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.Respected, _) =>
            [
                "Nora says quietly, \"More people in the village are starting to know you — I notice it when you pass the rows.\"",
            ],

            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Elias nods slowly. \"Folk all over Bloomtown know your name now — the forge felt that before the lanes named it.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Elias grunts softly. \"Neighbors talk about you like you're one of theirs — even quiet smithies hear that kind of talk.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.Respected, _) =>
            [
                "Elias says plainly, \"More people in the village are starting to know you — I notice it when you pass the forge.\"",
            ],

            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Marcus nods slowly. \"Folk all over Bloomtown know your name now — the workshop felt that before the lanes named it.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Marcus smiles. \"Neighbors talk about you like you're one of theirs — even quiet workshops hear that kind of talk.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.Respected, _) =>
            [
                "Marcus says plainly, \"More people in the village are starting to know you — I notice it when you pass the workshop.\"",
            ],

            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Ben nods slowly. \"Folk all over Bloomtown know your name now — the lanes felt that before the square named it.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Ben says plainly. \"Neighbors talk about you like you're one of theirs — even quiet patrols hear that kind of talk.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.Respected, _) =>
            [
                "Ben says evenly, \"More people in the village are starting to know you — I notice it when you pass my route.\"",
            ],

            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Lila beams. \"Folk all over Bloomtown know your name now — the lanes felt that before the square named it.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Lila says warmly. \"Neighbors talk about you like you're one of theirs — even young folk hear that kind of talk.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.Respected, _) =>
            [
                "Lila says cheerfully, \"More people in the village are starting to know you — I notice it when you pass through.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Rowan smiles faintly. \"Folk all over Bloomtown know your name now — the bench felt that before the square named it.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Rowan says warmly. \"Neighbors talk about you like you're one of theirs — even old tales hear that kind of talk.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.Respected, _) =>
            [
                "Rowan says quietly, \"More people in the village are starting to know you — I notice it when you pass through.\"",
            ],
            (NpcEntityIds.Eleanor, VillageSocialStandingTier.WellLiked, true) =>
            [
                "Eleanor murmurs from her porch. \"Folk all over Bloomtown know your name now — the cottage felt that before the lanes named it.\"",
            ],
            (NpcEntityIds.Eleanor, VillageSocialStandingTier.WellLiked, false) =>
            [
                "Eleanor says warmly. \"Neighbors talk about you like you're one of theirs — even quiet porches hear that kind of talk.\"",
            ],
            (NpcEntityIds.Eleanor, VillageSocialStandingTier.Respected, _) =>
            [
                "Eleanor says gently, \"More people in the village are starting to know you — I notice it when you pass the well.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    /// <summary>Extracts direct speech from narrative ambient lines for delivery by a named villager.</summary>
    public static string FormatVillagerAmbientCommentForSpeaker(string ambientComment)
    {
        var start = ambientComment.IndexOf('"');
        var end = ambientComment.LastIndexOf('"');
        if (start >= 0 && end > start)
            return ambientComment[(start + 1)..end];

        return ambientComment;
    }

    public static string? FormatPrestigeStatusHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.WellLiked =>
                "Social prestige: ordinary villagers speak of you with personal respect, Harold and Greta offer exclusive kindnesses, "
                + "and the whole community treats you as one of Bloomtown's most honored neighbors.",
            _ => null,
        };

    /// <summary>
    /// Rare extra recognition when a focus NPC greets a Well-liked player — warmer and more personal than Respected warmth.
    /// </summary>
    public static string? TryGetWellLikedPrestigeRecognitionLine(
        uint npcEntityId,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, villageNoticedMemory) switch
        {
            (NpcEntityIds.Harold, true) =>
            [
                "Harold says with quiet authority, \"Bloomtown esteems you — and as your elder, I do too.\"",
                "Harold tips his hat. \"Folk look to neighbors like you — your standing here is well earned.\"",
            ],
            (NpcEntityIds.Harold, false) =>
            [
                "Harold nods with respect. \"You're one of the trusted ones now — the village is better for it.\"",
            ],

            (NpcEntityIds.Greta, true) =>
            [
                "Greta beams. \"You're one of my most honored guests — the whole village says so, and I agree.\"",
                "Greta fusses over you. \"Bloomtown holds you in high regard, love — my parlor's proud to have you.\"",
            ],
            (NpcEntityIds.Greta, false) =>
            [
                "Greta smiles warmly. \"Guests the village esteems get treated like family here — and you've earned that.\"",
            ],

            (NpcEntityIds.Elsie, _) =>
            [
                "Elsie smiles with quiet pride. \"The village speaks of you with real fondness — I'm glad the garden knew you first.\"",
            ],
            (NpcEntityIds.Mira, _) =>
            [
                "Mira grins. \"Everyone in the square knows your name now — and says it with respect. I love that for you.\"",
            ],
            (NpcEntityIds.Tom, _) =>
            [
                "Tom nods slowly. \"Bloomtown holds you in high regard. Means something, coming from folk who don't hand that out lightly.\"",
            ],
            (NpcEntityIds.Nora, _) =>
            [
                "Nora smiles softly. \"Neighbors speak of you with genuine esteem — even the quiet gardens hear that kind of talk.\"",
            ],
            (NpcEntityIds.Elias, _) =>
            [
                "Elias says plainly. \"The village trusts you widely now — the forge respects that kind of standing.\"",
            ],
            (NpcEntityIds.Marcus, _) =>
            [
                "Marcus says warmly. \"The village trusts you widely now — the workshop respects that kind of standing.\"",
            ],
            (NpcEntityIds.Ben, _) =>
            [
                "Ben says evenly. \"The village trusts you widely now — the guard post respects that kind of standing.\"",
            ],
            (NpcEntityIds.Lila, _) =>
            [
                "Lila says brightly. \"The village trusts you widely now — young folk like me respect that kind of standing.\"",
            ],
            (NpcEntityIds.Rowan, _) =>
            [
                "Rowan says quietly. \"The village trusts you widely now — old stories like mine respect that kind of standing.\"",
            ],
            (NpcEntityIds.Eleanor, true) =>
            [
                "Eleanor smiles from her porch. \"Bloomtown esteems you, dear — folk speak your name with the kind of fondness that lasts.\"",
                "Eleanor murmurs warmly. \"Neighbors look to people like you — your standing here is something the whole village feels.\"",
            ],
            (NpcEntityIds.Eleanor, false) =>
            [
                "Eleanor nods with quiet pride. \"You're one of the trusted ones now — even quiet porches hear neighbors speak well of you.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    /// <summary>Well-liked exclusive: Greta shares useful guest gossip from the inn.</summary>
    public static string? TryGetGretaWellLikedInnGuestInfoLine(uint variationSeed)
    {
        string[] lines =
        [
            "Greta leans in. \"Between us, love — the traveling merchant mentioned your name. Folk all over town speak well of you.\"",
            "Greta whispers warmly. \"A guest from the riverside path asked after you — said Bloomtown's lucky to have someone so trusted.\"",
            "Greta smiles. \"Travelers at my table keep hearing your name — always spoken with respect. That's rare, and you earned it.\"",
        ];

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    /// <summary>Well-liked exclusive: Harold acknowledges the player's influence on village projects.</summary>
    public static string? TryGetHaroldWellLikedProjectAcknowledgmentLine(uint variationSeed)
    {
        string[] lines =
        [
            "Harold murmurs with elder's pride, \"Your standing here carries weight — communal work counts more when folk like you lend a hand.\"",
            "Harold tips his hat. \"Bloomtown esteems you — and shared projects move a little faster when trusted neighbors like you show up.\"",
            "Harold says quietly, \"As one of our most trusted folk, your contributions shape what this village becomes. I notice, and so does everyone.\"",
        ];

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string GetTierDisplayName(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.Known => "known around the village",
            VillageSocialStandingTier.Respected => "respected by neighbors",
            VillageSocialStandingTier.WellLiked => "one of Bloomtown's most trusted residents",
            _ => "still finding your place",
        };

    public static string GetStandingTierLabel(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.Known => "Known",
            VillageSocialStandingTier.Respected => "Respected",
            VillageSocialStandingTier.WellLiked => "Well-liked",
            _ => "Stranger",
        };

    public static string FormatFocusCloseFriendsLabel(IReadOnlyList<uint> focusCloseFriendNpcIds) =>
        focusCloseFriendNpcIds.Count switch
        {
            0 => "no focus close friends yet",
            1 => $"1 focus close friend ({NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0])})",
            2 =>
                $"2 focus close friends ({NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0])}, "
                + $"{NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[1])})",
            _ => $"{focusCloseFriendNpcIds.Count} focus close friends ({FormatNameList(focusCloseFriendNpcIds)})",
        };

    public static string? FormatCurrentTierActionsHint(VillageSocialStandingTier tier) =>
        FormatCompactTierActionsHint(tier);

    public static string? FormatCompactTierActionsHint(VillageSocialStandingTier tier) =>
        tier switch
        {
            VillageSocialStandingTier.Respected =>
                "Village favors · call-on (counsel, +1 backing, recovery, small gifts) · warmer NPC treatment · "
                + "Mira discount · help bonuses · reach Well-liked for +2 backing, trade favors, and legacy.",
            VillageSocialStandingTier.WellLiked =>
                "Full call-on (+2 backing, trade favors, recovery, gifts) · legacy effects · exclusive kindnesses from Harold, Greta, and villagers.",
            VillageSocialStandingTier.Known =>
                "Deepen focus friendships via bonding actions. Reach 2 focus close friends for Respected standing.",
            _ => null,
        };

    public static string? FormatCompactVillageTreatmentHint(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        return (tier, villageNoticedMemory) switch
        {
            (VillageSocialStandingTier.WellLiked, true) =>
                "Bloomtown treats you as family — focus regulars and ordinary villagers greet you with personal respect.",
            (VillageSocialStandingTier.WellLiked, false) =>
                "You're one of Bloomtown's most cherished neighbors — folk across the lanes know your name warmly.",
            (VillageSocialStandingTier.Respected, true) =>
                "Neighbors trust you more openly — word has spread beyond your close circle, and ordinary folk are beginning to greet you by name.",
            (VillageSocialStandingTier.Respected, false) =>
                "Bloomtown is starting to see you as one of their own — warmer greetings at the square, forge, and garden, with more villagers noticing your presence.",
            _ => null,
        };
    }

    public static string? FormatTierPromotionFeedback(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory)
    {
        return tier switch
        {
            VillageSocialStandingTier.Respected =>
                "[Social standing — Respected! Unlocked: village favors, call-on (counsel, +1 backing, recovery, small gifts), "
                + "Mira discount, and help bonuses. Next: Well-liked for +2 backing, trade favors, and legacy.]",
            VillageSocialStandingTier.WellLiked when villageNoticedMemory =>
                "[Social standing — Well-liked! Unlocked: full call-on (+2 backing, trade favors, recovery), "
                + "legacy effects (passive mood, richer recognition, journey lines), and broader village esteem.]",
            VillageSocialStandingTier.WellLiked =>
                "[Social standing — Well-liked! Unlocked: full call-on, legacy effects, and warmer treatment across Bloomtown. "
                + "Your name already travels farther than you know.]",
            _ => null,
        };
    }

    private static string FormatNameList(IReadOnlyList<uint> npcIds)
    {
        if (npcIds.Count == 0)
            return string.Empty;

        if (npcIds.Count <= 3)
        {
            return string.Join(
                ", ",
                npcIds.Select(id => NpcNameLookup.GetDisplayNameOrDefault(id)));
        }

        return string.Join(
            ", ",
            npcIds.Take(2).Select(id => NpcNameLookup.GetDisplayNameOrDefault(id)))
            + $" and {npcIds.Count - 2} more";
    }

    private static string FormatSampleName(IReadOnlyList<uint> npcIds, uint variationSeed)
    {
        if (npcIds.Count == 0)
            return "someone";

        return NpcNameLookup.GetDisplayNameOrDefault(npcIds[(int)(variationSeed % (uint)npcIds.Count)]);
    }

    private static string FormatPairNames(IReadOnlyList<uint> npcIds)
    {
        if (npcIds.Count == 0)
            return "the regulars";

        if (npcIds.Count == 1)
            return NpcNameLookup.GetDisplayNameOrDefault(npcIds[0]);

        return $"{NpcNameLookup.GetDisplayNameOrDefault(npcIds[0])} and {NpcNameLookup.GetDisplayNameOrDefault(npcIds[1])}";
    }
}