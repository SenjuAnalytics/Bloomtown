using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Ambient village lines, cross-NPC bond awareness, appreciation, and status summaries.
/// </summary>
public static class VillageBondRecognitionDialogue
{
    public static string? TryGetVillageAmbientComment(
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (focusCloseFriendNpcIds.Count == 0)
            return null;

        string[] lines;
        if (focusCloseFriendNpcIds.Count >= 2)
        {
            lines = villageNoticedMemory
                ? MultiBondNoticedAmbientLines
                : MultiBondWarmAmbientLines;
        }
        else
        {
            var name = NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0]);
            lines =
            [
                $"People say you and {name} have been spending a lot of time together lately.",
                $"I heard {name} lights up whenever you're nearby.",
                $"Someone mentioned you and {name} look awfully comfortable together these days.",
                $"Folks have started pairing your name with {name}'s around town.",
            ];
        }

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetCrossNpcRecognitionLine(
        uint speakingNpcEntityId,
        uint otherCloseFriendNpcId,
        uint variationSeed)
    {
        var otherName = NpcNameLookup.GetDisplayNameOrDefault(otherCloseFriendNpcId);
        string[] lines = (speakingNpcEntityId, otherCloseFriendNpcId) switch
        {
            (NpcEntityIds.Elsie, NpcEntityIds.Tom) =>
            [
                $"Tom mentioned you've been a steady friend to him too — I'm glad he has you.",
                $"I see you with Tom at the lumber yard sometimes. He talks about you fondly.",
            ],
            (NpcEntityIds.Elsie, NpcEntityIds.Harold) =>
            [
                $"Harold says you're the sort who shows up when it counts. I believe him.",
                $"Word reaches the garden that you and Harold have grown close. That warms me.",
            ],
            (NpcEntityIds.Elsie, NpcEntityIds.Mira) =>
            [
                $"Mira says the square feels brighter when you're around. I notice the same.",
                $"I hear you and Mira have your own easy rhythm at the market.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Elsie) =>
            [
                $"Elsie speaks highly of you — says you've got a gentle way about you.",
                $"The garden folk mention how close you and Elsie have become. Good to hear.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Tom) =>
            [
                $"Tom trusts few people quickly. The fact that he trusts you says plenty.",
                $"I see you lending a hand at the woodpile. Tom doesn't let just anyone in.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Mira) =>
            [
                $"Mira tells me you're a familiar face at the square. She doesn't say that lightly.",
                $"Market gossip reaches the well — you and Mira seem genuinely fond of each other.",
            ],
            (NpcEntityIds.Mira, NpcEntityIds.Elsie) =>
            [
                $"Elsie told me you help her like family. That's rare praise from her.",
                $"I catch you near the garden now and then. Elsie smiles more when you're around.",
            ],
            (NpcEntityIds.Mira, NpcEntityIds.Harold) =>
            [
                $"Harold says you're reliable in the quiet ways that matter. I trust his judgment.",
                $"Word from the well is that you and Harold have grown quite close.",
            ],
            (NpcEntityIds.Mira, NpcEntityIds.Tom) =>
            [
                $"Tom doesn't chatter much, but he speaks well of you when he does.",
                $"I hear you spend time at the lumber yard. Tom considers you a real friend.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Elsie) =>
            [
                $"Elsie mentioned you check on her. Means a lot — she's picky about who she lets close.",
                $"Garden talk drifts to the yard sometimes. Folks say you and Elsie are thick as thieves.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Harold) =>
            [
                $"Harold vouches for you. Coming from him, that's not small talk.",
                $"I see you at the well with Harold. He doesn't waste warmth on strangers.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Mira) =>
            [
                $"Mira says you're a regular at the square. She notices who keeps showing up.",
                $"Market folk mention you and Mira in the same breath lately. Makes sense to me.",
            ],
            (NpcEntityIds.Greta, NpcEntityIds.Elsie) =>
            [
                $"Elsie told me you help at the garden like family. She doesn't say that to everyone.",
                $"Garden folk mention you and Elsie together — I hear it over morning tea.",
            ],
            (NpcEntityIds.Greta, NpcEntityIds.Mira) =>
            [
                $"Mira says the square's brighter when you're around. I believe her — you brighten my parlor too.",
                $"Market gossip reaches the inn. You and Mira seem genuinely fond of each other.",
            ],
            (NpcEntityIds.Elsie, NpcEntityIds.Greta) =>
            [
                $"Greta says you're a regular at the hearth now. That's high praise from our innkeeper.",
                $"Inn talk drifts to the garden sometimes. Greta speaks warmly of you.",
            ],
            (NpcEntityIds.Mira, NpcEntityIds.Greta) =>
            [
                $"Greta told me you make folk feel welcome at the inn. She fusses over people she trusts.",
                $"Travelers mention you at the hearth — Greta pairs your name with kindness.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Greta) =>
            [
                $"Greta says you lend a hand at the inn without being asked. She notices that sort of thing.",
                $"Word from the parlor is that you and Greta have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Greta) =>
            [
                $"Greta hears everything — she says you're steady at the woodpile too. Tom agrees, quietly.",
                $"Inn chatter reaches the yard sometimes. Greta speaks well of you.",
            ],
            (NpcEntityIds.Nora, NpcEntityIds.Elsie) =>
            [
                $"Elsie told me you help among the herbs like family. She doesn't say that to everyone.",
                $"Garden folk mention you and Elsie together — I hear it in the morning breeze.",
            ],
            (NpcEntityIds.Elsie, NpcEntityIds.Nora) =>
            [
                $"Nora says you're steady among the herb rows now. That's high praise from our herbalist.",
                $"Herb talk drifts to the garden sometimes. Nora speaks warmly of you.",
            ],
            (NpcEntityIds.Nora, NpcEntityIds.Greta) =>
            [
                $"Greta told me you make folk feel welcome at the inn. Nora notices who tends people gently.",
                $"Inn gossip reaches the herb patch. You and Greta seem genuinely fond of each other.",
            ],
            (NpcEntityIds.Greta, NpcEntityIds.Nora) =>
            [
                $"Nora says you help among the herbs without hurry. She notices that sort of care.",
                $"Word from the garden is that you and Nora have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Nora) =>
            [
                $"Nora says you lend a hand among the beds without being asked. She remembers steady folk.",
                $"Quiet talk from the herb rows — Nora speaks well of you.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Nora) =>
            [
                $"Nora hears the village's rhythm — she says you're steady at the woodpile too. Tom agrees, quietly.",
                $"Herb-garden chatter reaches the yard sometimes. Nora speaks well of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Tom) =>
            [
                $"Tom says you're steady at the woodpile — Elias notices the same hands at the forge.",
                $"Yard talk drifts to the smithy sometimes. Tom speaks well of you.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Elias) =>
            [
                $"Elias says you help at the forge without fanfare. Tom trusts folk like that.",
                $"Forge chatter reaches the yard sometimes. Elias speaks well of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Harold) =>
            [
                $"Harold says you're reliable in the quiet ways that matter. Elias notices the same at the forge.",
                $"Well-side talk drifts to the smithy sometimes. Harold speaks well of you.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Elias) =>
            [
                $"Elias says you lend a hand at the forge without being asked. Harold remembers steady folk.",
                $"Word from the anvil is that you and Elias have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Greta) =>
            [
                $"Greta told me you make folk feel welcome at the inn. Elias notices who tends people honestly.",
                $"Inn gossip reaches the forge. You and Greta seem genuinely fond of each other.",
            ],
            (NpcEntityIds.Greta, NpcEntityIds.Elias) =>
            [
                $"Elias says you help at the forge with steady hands. Greta fusses over people she trusts.",
                $"Word from the smithy is that you and Elias have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Nora) =>
            [
                $"Nora says you help among the herbs without hurry. Elias notices that sort of care.",
                $"Herb talk drifts to the forge sometimes. Nora speaks well of you.",
            ],
            (NpcEntityIds.Nora, NpcEntityIds.Elias) =>
            [
                $"Elias says you lend a hand at the forge without fanfare. Nora remembers patient folk.",
                $"Quiet talk from the anvil — Elias speaks well of you.",
            ],
            (NpcEntityIds.Ben, NpcEntityIds.Harold) =>
            [
                $"Harold says you're reliable in the quiet ways that matter. Ben notices the same on patrol.",
                $"Well-side talk drifts to the guard post sometimes. Harold speaks well of you.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Ben) =>
            [
                $"Ben says you walk the lanes without being asked. Harold remembers steady folk.",
                $"Word from the guard post is that you and Ben have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Ben, NpcEntityIds.Elias) =>
            [
                $"Elias says you help at the forge with steady hands. Ben trusts folk who show up on patrol too.",
                $"Forge chatter reaches the guard post. Elias speaks well of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Ben) =>
            [
                $"Ben says you lend a hand on patrol without fanfare. Elias notices that sort of reliability.",
                $"Quiet talk from the lanes — Ben speaks well of you.",
            ],
            (NpcEntityIds.Marcus, NpcEntityIds.Elias) =>
            [
                $"Elias says you help at the forge with steady hands. Marcus notices the same care at the workshop.",
                $"Forge chatter reaches the bench sometimes. Elias speaks well of you.",
            ],
            (NpcEntityIds.Elias, NpcEntityIds.Marcus) =>
            [
                $"Marcus says you lend a hand at the workshop without fanfare. Elias notices that sort of craft.",
                $"Workshop talk drifts to the smithy sometimes. Marcus speaks well of you.",
            ],
            (NpcEntityIds.Marcus, NpcEntityIds.Tom) =>
            [
                $"Tom says you're steady at the woodpile — Marcus notices the same hands at the bench.",
                $"Yard chatter reaches the workshop sometimes. Tom speaks well of you.",
            ],
            (NpcEntityIds.Tom, NpcEntityIds.Marcus) =>
            [
                $"Marcus says you help at the workshop without fanfare. Tom trusts folk like that.",
                $"Bench talk reaches the yard sometimes. Marcus speaks well of you.",
            ],
            (NpcEntityIds.Marcus, NpcEntityIds.Harold) =>
            [
                $"Harold says you're reliable in the quiet ways that matter. Marcus notices the same at the workshop.",
                $"Well-side talk reaches the bench sometimes. Harold speaks well of you.",
            ],
            (NpcEntityIds.Harold, NpcEntityIds.Marcus) =>
            [
                $"Marcus says you lend a hand at the workshop without being asked. Harold remembers steady folk.",
                $"Word from the bench is that you and Marcus have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Lila, NpcEntityIds.Mira) =>
            [
                $"Mira says you greet folk at the square with real warmth. Lila notices that sort of kindness.",
                $"Market chatter reaches the lanes sometimes. Mira speaks well of you.",
            ],
            (NpcEntityIds.Mira, NpcEntityIds.Lila) =>
            [
                $"Lila says you help around the village without being asked. Mira remembers friendly folk.",
                $"Word from the lanes is that you and Lila have grown close. Good company, both of you.",
            ],
            (NpcEntityIds.Lila, NpcEntityIds.Ben) =>
            [
                $"Ben says you walk the lanes without being asked. Lila notices folk who show up around the village too.",
                $"Patrol talk drifts to the square sometimes. Ben speaks well of you.",
            ],
            (NpcEntityIds.Ben, NpcEntityIds.Lila) =>
            [
                $"Lila says you lend a hand around Bloomtown without fanfare. Ben trusts folk who show up on patrol too.",
                $"Quiet talk from the lanes — Lila speaks well of you.",
            ],
            (NpcEntityIds.Rowan, NpcEntityIds.Greta) =>
            [
                $"Greta says guests ask after you by name at the hearth. Rowan notices that sort of warmth.",
                $"Inn talk drifts to the story bench sometimes. Greta speaks well of you.",
            ],
            (NpcEntityIds.Greta, NpcEntityIds.Rowan) =>
            [
                $"Rowan says you listen at the bench without hurry. Greta remembers patient folk at the inn too.",
                $"Word from the story corner is that you and Rowan have grown close. Good company, both of you.",
            ],
            _ =>
            [
                $"I hear you've grown close with {otherName} too. Bloomtown notices that sort of thing.",
                $"{otherName} speaks warmly of you — it's good to see you finding your people here.",
            ],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetVillageAppreciationLine(
        uint npcEntityId,
        int focusCloseFriendCount,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, villageNoticedMemory, focusCloseFriendCount >= 3) switch
        {
            (NpcEntityIds.Elsie, true, true) =>
            [
                "You've woven yourself into Bloomtown's heart — I hope you feel how glad we are you're here.",
                "The garden folk talk about you like family now. That matters to me.",
            ],
            (NpcEntityIds.Elsie, true, false) =>
            [
                "People notice how many close friends you've made here. I'm proud of that for you.",
                "Bloomtown feels warmer when you're walking through — don't think that goes unseen.",
            ],
            (NpcEntityIds.Elsie, false, _) =>
            [
                "Neighbors speak well of the company you keep. I think they're right.",
                "You've found your people here. I'm glad Bloomtown gets to see that.",
            ],

            (NpcEntityIds.Harold, true, true) =>
            [
                "Folks at the well say you're one of ours now. I won't argue with them.",
                "You've earned a place in this town's quiet trust. That doesn't happen often.",
            ],
            (NpcEntityIds.Harold, true, false) =>
            [
                "Word travels when someone builds real friendships here. You've done that.",
                "Bloomtown regards you fondly — I wanted you to hear it straight.",
            ],
            (NpcEntityIds.Harold, false, _) =>
            [
                "People notice who you spend time with. You've chosen good company.",
                "The village sees how you show up for folks. That stays remembered.",
            ],

            (NpcEntityIds.Mira, true, true) =>
            [
                "Market regulars list you among the people who make the square feel alive.",
                "You've become part of the rhythm here — I hope that feels as good as it looks.",
            ],
            (NpcEntityIds.Mira, true, false) =>
            [
                "People say you've grown close with more than one of us. Bloomtown appreciates that.",
                "I hear your name with warmth around town. You've earned that tone.",
            ],
            (NpcEntityIds.Mira, false, _) =>
            [
                "The square feels steadier when you're around — neighbors mention it too.",
                "You've made yourself known in the best way. I'm glad you're here.",
            ],

            (NpcEntityIds.Tom, true, true) =>
            [
                "Even quiet folks at the yard talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Tom, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Tom, false, _) =>
            [
                "Folks mention how at home you look with the regulars. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            (NpcEntityIds.Greta, true, true) =>
            [
                "Travelers list you among the people who make Bloomtown feel like home — I agree wholeheartedly.",
                "You've woven yourself into village life — my hearth remembers, and so does the town.",
            ],
            (NpcEntityIds.Greta, true, false) =>
            [
                "People say you've grown close with more than one of us. The inn hears those stories warmly.",
                "Bloomtown regards you fondly — I wanted you to hear it from my parlor too.",
            ],
            (NpcEntityIds.Greta, false, _) =>
            [
                "Neighbors mention how welcome you look at the hearth. I see it every time you visit.",
                "You've made yourself known in the best way — regular kindness, not loud gestures.",
            ],

            (NpcEntityIds.Nora, true, true) =>
            [
                "Neighbors list you among the people who keep Bloomtown balanced — I agree, softly.",
                "You've woven yourself into village life — the herb rows remember, and so does the town.",
            ],
            (NpcEntityIds.Nora, true, false) =>
            [
                "People say you've grown close with more than one of us. The garden hears those stories warmly.",
                "Bloomtown regards you fondly — I wanted you to hear it among the herbs too.",
            ],
            (NpcEntityIds.Nora, false, _) =>
            [
                "Neighbors mention how at home you look among the beds. I see it every time you visit.",
                "You've made yourself known in the best way — patient care, not loud gestures.",
            ],

            (NpcEntityIds.Elias, true, true) =>
            [
                "Even quiet folk at the forge talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Elias, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Elias, false, _) =>
            [
                "Folks mention how at home you look at the anvil. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            (NpcEntityIds.Ben, true, true) =>
            [
                "Even quiet folk on patrol talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Ben, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Ben, false, _) =>
            [
                "Folks mention how at home you look on my route. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            (NpcEntityIds.Lila, true, true) =>
            [
                "Even young folk around the lanes talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Lila, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Lila, false, _) =>
            [
                "Folks mention how at home you look on the village paths. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            (NpcEntityIds.Rowan, true, true) =>
            [
                "Even old tales around the bench talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Rowan, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Rowan, false, _) =>
            [
                "Folks mention how at home you look at the story bench. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            (NpcEntityIds.Marcus, true, true) =>
            [
                "Even quiet folk at the workshop talk about you with respect. That means something.",
                "Bloomtown knows who belongs here. I reckon you do now.",
            ],
            (NpcEntityIds.Marcus, true, false) =>
            [
                "People notice your friendships. I do too — and I think well of it.",
                "You've put down real roots here. The town sees that.",
            ],
            (NpcEntityIds.Marcus, false, _) =>
            [
                "Folks mention how at home you look at the bench. I see it as well.",
                "You've found your footing in Bloomtown. Good — we needed you to.",
            ],

            _ =>
            [
                "Bloomtown regards you warmly — your friendships haven't gone unnoticed.",
                "People speak of you with fondness around town. You've earned that.",
            ],
        };

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string FormatRecognitionStatus(
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory)
    {
        if (focusCloseFriendNpcIds.Count == 0)
            return string.Empty;

        if (focusCloseFriendNpcIds.Count == 1)
        {
            var name = NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0]);
            return villageNoticedMemory
                ? $"Village bonds: people have noticed how close you are with {name}."
                : $"Village bonds: neighbors are starting to notice your closeness with {name}.";
        }

        var names = FormatNameList(focusCloseFriendNpcIds);
        return focusCloseFriendNpcIds.Count switch
        {
            2 when villageNoticedMemory =>
                $"Village bonds: the village has noticed your close bond with {names} — Bloomtown is beginning to regard you warmly.",
            2 =>
                $"Village bonds: people say you and {names} have grown quite close.",
            3 when villageNoticedMemory =>
                $"Village bonds: the village regards you warmly — your friendships with {names} are part of town life now.",
            3 =>
                $"Village bonds: neighbors speak fondly of how close you've become with {names}.",
            _ when villageNoticedMemory =>
                $"Village bonds: Bloomtown knows who you belong with — {names} — and regards you with real warmth.",
            _ =>
                $"Village bonds: the whole village seems to notice your closeness with {names}.",
        };
    }

    public static string? FormatPassiveBenefitStatusHint(int focusCloseFriendCount, bool villageNoticedMemory)
    {
        if (focusCloseFriendCount < VillageBondRecognitionConfig.MinCloseFriendsForPassiveBenefit)
            return null;

        return villageNoticedMemory
            ? "A gentle warmth follows you through town."
            : focusCloseFriendCount >= 3
                ? "Being out in Bloomtown steadies your mood a little — the village feels like yours."
                : "Being out in Bloomtown lifts your mood slightly — people here know who matters to you.";
    }

    private static readonly string[] MultiBondWarmAmbientLines =
    [
        "You've got a real circle here — folks appreciate seeing someone who shows up for people.",
        "Neighbors say you belong with the regulars now. That's a compliment in Bloomtown.",
        "I keep hearing good things about the company you keep. The village values that.",
        "People talk about how at home you look with Elsie, Harold, Mira, Tom, Greta, and Nora.",
        "Word is you've become someone the town quietly counts on being around.",
        "Folks don't just notice you anymore — they notice who you care about here.",
    ];

    private static readonly string[] MultiBondNoticedAmbientLines =
    [
        "The whole village seems to know who you're close with — and they speak warmly of it.",
        "Bloomtown regards you fondly these days. People still mention your friendships.",
        "Word around town is that your bonds run deep with the people everyone trusts.",
        "Neighbors say the village feels warmer when you're out among friends.",
        "Folks appreciate how you've woven yourself into the heart of Bloomtown.",
        "People still talk about how many close friendships you've built. It hasn't faded.",
    ];

    private static string FormatNameList(IReadOnlyList<uint> npcIds)
    {
        var names = npcIds.Select(NpcNameLookup.GetDisplayNameOrDefault).ToList();
        return names.Count switch
        {
            2 => $"{names[0]} and {names[1]}",
            _ => string.Join(", ", names.Take(names.Count - 1)) + $", and {names[^1]}",
        };
    }
}