namespace Bloomtown.Shared.Community;

/// <summary>
/// Flavor and NPC acknowledgment lines for community-help activities.
/// </summary>
public static class CommunityActivityDialogue
{
    private static readonly string[][] NpcAcknowledgments =
    [
        // HelpGarden
        [
            "Elsie smiles. \"The garden looks better already — thank you for pitching in.\"",
            "A neighbor waves. \"We appreciate the extra hands around the beds.\"",
            "\"Shared work makes the garden feel like ours,\" someone says warmly.",
        ],
        // HelpMarket
        [
            "A vendor grins. \"Thanks — the square runs smoother with folks like you helping.\"",
            "\"Good to see you lending a hand,\" a local says with a nod.",
            "Someone calls out, \"The market's lucky to have you today!\"",
        ],
        // HelpWell
        [
            "Elsie dips a bucket and smiles. \"The well stays welcoming when we all look after it.\"",
            "\"Handy work around the well — the village notices,\" a passerby remarks.",
            "A villager thanks you quietly for keeping the gathering spot tidy.",
        ],
        // HelpLumber
        [
            "Tom nods. \"Good hands on the woodpile — the yard stays workable because of folk like you.\"",
            "\"Steady help at the lumber yard,\" someone remarks with approval.",
            "A neighbor thanks you for keeping the timber stacks tidy.",
        ],
        // HelpInn
        [
            "Greta beams. \"The parlor looks lovely — thank you for lending a hand at the inn.\"",
            "\"Good help at the hearth,\" a traveler remarks warmly.",
            "Greta fusses fondly. \"You're making my job easier — and the guests feel it too.\"",
        ],
        // HelpHerbGarden
        [
            "Nora smiles softly. \"The herbs look better already — thank you for tending with care.\"",
            "A neighbor murmurs. \"We appreciate the extra hands among the beds.\"",
            "\"Shared work keeps the garden balanced,\" someone says quietly.",
        ],
        // HelpSmithy
        [
            "Elias nods. \"Good hands at the forge — the smithy stays workable because of folk like you.\"",
            "\"Steady help at the anvil,\" someone remarks with approval.",
            "A neighbor thanks you for keeping the tools tidy.",
        ],
        // HelpPatrol
        [
            "Ben nods. \"Good steps on the route — the lanes stay honest because of folk like you.\"",
            "\"Steady help on patrol,\" someone remarks with approval.",
            "A villager thanks you for keeping the paths clear.",
        ],
        // HelpVillage
        [
            "Lila beams. \"Good hands around the village — Bloomtown feels brighter because of folk like you.\"",
            "\"Steady help around the lanes,\" someone remarks with approval.",
            "A villager thanks you for keeping the square welcoming.",
        ],
        // ListenToStories
        [
            "Rowan smiles faintly. \"Good ears at the bench — Bloomtown remembers because of folk like you.\"",
            "\"Steady listening at the story corner,\" someone remarks with approval.",
            "A villager thanks you for keeping the village's tales alive.",
        ],
        // HelpWorkshop
        [
            "Marcus smiles. \"Good hands at the bench — the workshop stays workable because of folk like you.\"",
            "\"Steady help at the worktable,\" someone remarks with approval.",
            "A neighbor thanks you for keeping the tools tidy.",
        ],
        // ChatWithEleanor
        [
            "Eleanor smiles warmly. \"Good ears on the porch — Bloomtown remembers because of folk like you, dear.\"",
            "\"Steady listening at the cottage,\" someone remarks with approval.",
            "A villager thanks you for keeping the old stories alive.",
        ],
    ];

    public static string PickNpcAcknowledgment(CommunityActivityKind kind, uint variationSeed)
    {
        var index = KindToIndex(kind);
        if (index < 0)
            return string.Empty;

        var lines = NpcAcknowledgments[index];
        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static int KindToIndex(CommunityActivityKind kind) =>
        kind switch
        {
            CommunityActivityKind.HelpGarden => 0,
            CommunityActivityKind.HelpMarket => 1,
            CommunityActivityKind.HelpWell => 2,
            CommunityActivityKind.HelpLumber => 3,
            CommunityActivityKind.HelpInn => 4,
            CommunityActivityKind.HelpHerbGarden => 5,
            CommunityActivityKind.HelpSmithy => 6,
            CommunityActivityKind.HelpPatrol => 7,
            CommunityActivityKind.HelpVillage => 8,
            CommunityActivityKind.ListenToStories => 9,
            CommunityActivityKind.HelpWorkshop => 10,
            CommunityActivityKind.ChatWithEleanor => 11,
            _ => -1,
        };
}