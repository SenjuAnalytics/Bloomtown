using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// NPC dialogue for active social-standing favor requests — personality-specific help, advice, and polite declines.
/// </summary>
public static class SocialStandingActionDialogue
{
    public static string? TryGetSuccessResponseLine(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        SocialStandingFavorOutcomeKind outcome,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, tier, outcome, villageNoticedMemory) switch
        {
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Elsie nods warmly. \"Bloomtown trusts you — so I'll say it: the south beds yield best after rain, and wood east of the well refreshes at dawn.\"",
                "Elsie leans in. \"Since neighbors speak well of you — check the community board by the garden early. Caretakers who read it first find the best work.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Elsie smiles. \"People trust you here — try the herb rows before noon. They're generous when folk are kind to the garden.\"",
                "Elsie adds quietly, \"Word reaches the beds that you're one of ours — Mira's market is gentler before the square fills.\"",
            ],
            (NpcEntityIds.Elsie, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Elsie presses fruit into your hands. \"From the garden — you've earned a little kindness from Bloomtown.\"",
            ],
            (NpcEntityIds.Elsie, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Elsie fusses over you. \"Sit a moment by the beds — trusted folk deserve a breath and something steadying.\"",
            ],

            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Harold murmurs, \"The village regards you well — the well runs clearest at first light, and folk gather there before the square stirs.\"",
                "Harold tips his hat. \"Neighbors trust you — Harold's plain advice: greet the regulars at dawn. Bloomtown remembers who shows up.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Harold says quietly, \"You're well-liked here — the lane by the well stays peaceful before noon. Good for thinking.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Harold nods. \"People speak well of you — the well's busiest at midday, but mornings are best for honest talk.\"",
                "Harold adds, \"Bloomtown's starting to count on you — wood piles near the lumber yard restock after quiet hours.\"",
            ],
            (NpcEntityIds.Harold, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Harold sets wood aside. \"For someone the village respects — take it. Good timber shouldn't wait on strangers.\"",
            ],
            (NpcEntityIds.Harold, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Harold offers a steady nod. \"Rest a moment by the well — folk who belong here don't have to rush.\"",
            ],

            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Mira grins. \"Because Bloomtown likes you — real gossip: early vendors remember kindness, and Elsie's apples vanish by noon.\"",
                "Mira winks. \"Trusted regulars get the good tips — the square's warmest after the first hour, when half the village passes through.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Mira beams. \"Neighbors trust you more — market mornings reward folk who greet vendors by name.\"",
                "Mira adds, \"Word's out you're one of ours — project donations at the board draw extra goodwill if you speak up first.\"",
            ],
            (NpcEntityIds.Mira, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Mira slips you something from her stall. \"A little market favor — well-liked folk get looked after, you know.\"",
            ],
            (NpcEntityIds.Mira, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Mira laughs warmly. \"Sit — even busy regulars deserve a moment. The square can wait.\"",
            ],

            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Tom says plainly, \"Bloomtown trusts you — east woodpile's stocked after quiet hours. Shed out back stays dry if you're working late.\"",
                "Tom nods. \"Village regards you well — planks come easier when you gather before the yard fills with noise.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Tom allows a half-smile. \"Neighbors speak well of you — lumber's less picked-over at dawn. Means something, that timing.\"",
                "Tom murmurs, \"People trust you here — fence line by the garden needs checking after storms. Good work if you're passing.\"",
            ],
            (NpcEntityIds.Tom, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Tom hands you a plank. \"For someone Bloomtown respects — good wood shouldn't go to waste.\"",
            ],
            (NpcEntityIds.Tom, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Tom says quietly, \"Take a breath by the woodpile — trusted folk don't have to prove themselves every hour.\"",
            ],

            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Greta beams. \"The whole village talks about you — travelers ask after the regulars first. My parlor hears every name worth knowing.\"",
                "Greta fusses over your cup. \"Well-liked guests get the truth — kitchen's busiest before dusk, but the hearth's warm all day for folk Bloomtown trusts.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Greta smiles. \"Neighbors regard you fondly — sit near the hearth before the evening rush. Regulars get the quiet hour.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Greta says cheerfully, \"Word reaches the inn that folk trust you — meal hours are gentler just before the square empties.\"",
                "Greta waves you closer. \"Bloomtown's starting to count on you — guests swap village news at my table. Listen, and you'll hear what matters.\"",
            ],
            (NpcEntityIds.Greta, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Greta presses food into your hands. \"From the kitchen — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Greta, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Greta squeezes your arm. \"Sit by the hearth a while — my inn looks after people Bloomtown respects.\"",
            ],

            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Nora murmurs, \"Bloomtown regards you well — the herb rows yield best when tended before the square stirs.\"",
                "Nora says quietly, \"Neighbors trust you — the quiet patch by the garden stays peaceful before noon.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Nora nods. \"You're well-liked here — Elsie's beds and mine share the same morning breeze if you need calm.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Nora says softly, \"Word reaches the herbs that folk trust you — balance grows where patience is remembered.\"",
                "Nora adds, \"People greet you warmer — I save shade for folk the village respects.\"",
            ],
            (NpcEntityIds.Nora, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Nora presses an apple into your hands. \"From the garden — trusted regulars get small kindnesses.\"",
            ],
            (NpcEntityIds.Nora, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Nora gestures to the shade. \"Sit among the herbs a while — Bloomtown respects folk who rest when they need it.\"",
            ],

            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Elias says plainly, \"Bloomtown regards you well — timber stacks higher before dusk near the bridge site.\"",
                "Elias adds, \"Neighbors trust you — the forge stays useful before the square stirs.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Elias nods. \"You're well-liked here — Tom's woodpile and my forge both run smoother before noon.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Elias says plainly, \"Word reaches the forge that folk trust you — quality work needs steady materials.\"",
                "Elias adds, \"People greet you warmer — I save a bench for folk the village respects.\"",
            ],
            (NpcEntityIds.Elias, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Elias presses wood into your hands. \"From the forge — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Elias, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Elias gestures to the bench. \"Sit by the anvil a while — Bloomtown respects folk who rest when they need it.\"",
            ],

            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Marcus says plainly, \"Bloomtown regards you well — plank stock firms up before dusk near the bridge site.\"",
                "Marcus adds, \"Neighbors trust you — the workshop stays useful before the square stirs.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Marcus nods warmly. \"You're well-liked here — Tom's woodpile and my workshop both run smoother before noon.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Marcus says plainly, \"Word reaches the workshop that folk trust you — quality work needs steady materials.\"",
                "Marcus adds, \"People greet you warmer — I save a stool for folk the village respects.\"",
            ],
            (NpcEntityIds.Marcus, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Marcus presses planks into your hands. \"From the workshop — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Marcus, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Marcus gestures to the bench. \"Sit by the worktable a while — Bloomtown respects folk who rest when they need it.\"",
            ],

            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Ben says evenly, \"Bloomtown regards you well — the east lane gets loose after market hours.\"",
                "Ben adds, \"Neighbors trust you — patrol help lands best before the square stirs.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Ben nods. \"You're well-liked here — Harold's well-side and my route both run smoother before noon.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Ben says plainly, \"Word reaches the guard post that folk trust you — steady eyes keep a village honest.\"",
                "Ben adds, \"People greet you warmer — I save a bench for folk the village respects.\"",
            ],
            (NpcEntityIds.Ben, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Ben presses wood into your hands. \"From the post — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Ben, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Ben gestures to the bench. \"Sit by the guard post a while — Bloomtown respects folk who rest when they need it.\"",
            ],

            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Lila says brightly, \"Bloomtown regards you well — the square hums brightest before noon.\"",
                "Lila adds, \"Neighbors trust you — village help lands best before the market stirs.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Lila grins. \"You're well-liked here — Greta's porch and the garden path both run smoother before noon.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Lila says warmly, \"Word reaches the lanes that folk trust you — steady hands keep a village welcoming.\"",
                "Lila adds, \"People greet you warmer — I save a story for folk the village respects.\"",
            ],
            (NpcEntityIds.Lila, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Lila presses an apple into your hands. \"From the lanes — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Lila, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Lila gestures to the bench. \"Sit by the square a while — Bloomtown respects folk who rest when they need it.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, true) =>
            [
                "Rowan says quietly, \"Bloomtown regards you well — the bench is quietest before noon.\"",
                "Rowan adds, \"Neighbors trust you — story listening lands best before the market stirs.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked, SocialStandingFavorOutcomeKind.Info, false) =>
            [
                "Rowan nods. \"You're well-liked here — Greta's hearth and the story bench both run smoother before noon.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.Respected, SocialStandingFavorOutcomeKind.Info, _) =>
            [
                "Rowan says warmly, \"Word reaches the bench that folk trust you — patient ears keep a village remembering.\"",
                "Rowan adds, \"People greet you warmer — I save a tale for folk the village respects.\"",
            ],
            (NpcEntityIds.Rowan, _, SocialStandingFavorOutcomeKind.Item, _) =>
            [
                "Rowan presses wood into your hands. \"From the bench — trusted regulars get the little extras.\"",
            ],
            (NpcEntityIds.Rowan, _, SocialStandingFavorOutcomeKind.Recovery, _) =>
            [
                "Rowan gestures to the bench. \"Sit by the inn a while — Bloomtown respects folk who rest when they need it.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetDeclineLine(
        uint npcEntityId,
        VillageSocialStandingTier tier,
        uint variationSeed)
    {
        string[] lines = (npcEntityId, tier) switch
        {
            (NpcEntityIds.Elsie, VillageSocialStandingTier.WellLiked) =>
            [
                "Elsie smiles apologetically. \"I'd love to help more, love — the beds need me now. Come back when the garden settles.\"",
            ],
            (NpcEntityIds.Elsie, VillageSocialStandingTier.Respected) =>
            [
                "Elsie says gently, \"I hear you — and Bloomtown trusts you — but today's hands are full. Try me again later.\"",
            ],

            (NpcEntityIds.Harold, VillageSocialStandingTier.WellLiked) =>
            [
                "Harold tips his hat. \"You have my regard — just not much spare time at the well today. Soon, though.\"",
            ],
            (NpcEntityIds.Harold, VillageSocialStandingTier.Respected) =>
            [
                "Harold murmurs, \"Neighbors speak well of you, so I hate to say no — but the well's busy. Another day.\"",
            ],

            (NpcEntityIds.Mira, VillageSocialStandingTier.WellLiked) =>
            [
                "Mira grins. \"I'd bend the rules for you — but the market's chaos right now. Catch me when it calms.\"",
            ],
            (NpcEntityIds.Mira, VillageSocialStandingTier.Respected) =>
            [
                "Mira sighs cheerfully. \"I want to help — I do — but the square's eating me alive today. Later?\"",
            ],

            (NpcEntityIds.Tom, VillageSocialStandingTier.WellLiked) =>
            [
                "Tom shakes his head slowly. \"I trust you — village does too — but the yard needs me. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Tom, VillageSocialStandingTier.Respected) =>
            [
                "Tom says plainly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],

            (NpcEntityIds.Greta, VillageSocialStandingTier.WellLiked) =>
            [
                "Greta fusses anyway. \"Oh love, the parlor's bursting — but you're still family here. Come back when the rush eases.\"",
            ],
            (NpcEntityIds.Greta, VillageSocialStandingTier.Respected) =>
            [
                "Greta smiles warmly. \"I'd fuss over you properly, but the kitchen's shouting. Soon, dear — you're still welcome.\"",
            ],

            (NpcEntityIds.Nora, VillageSocialStandingTier.WellLiked) =>
            [
                "Nora smiles apologetically. \"I'd help more, but the drying beds need me now. Come back when the rows settle.\"",
            ],
            (NpcEntityIds.Nora, VillageSocialStandingTier.Respected) =>
            [
                "Nora says gently, \"I hear you — and Bloomtown trusts you — but today's hands are full. Try me again later.\"",
            ],

            (NpcEntityIds.Elias, VillageSocialStandingTier.WellLiked) =>
            [
                "Elias shakes his head slowly. \"I trust you — village does too — but the forge needs me. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Elias, VillageSocialStandingTier.Respected) =>
            [
                "Elias says plainly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],

            (NpcEntityIds.Marcus, VillageSocialStandingTier.WellLiked) =>
            [
                "Marcus shakes his head slowly. \"I trust you — village does too — but the workshop needs me. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Marcus, VillageSocialStandingTier.Respected) =>
            [
                "Marcus says plainly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],

            (NpcEntityIds.Ben, VillageSocialStandingTier.WellLiked) =>
            [
                "Ben shakes his head slowly. \"I trust you — village does too — but the route needs me. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Ben, VillageSocialStandingTier.Respected) =>
            [
                "Ben says plainly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],

            (NpcEntityIds.Lila, VillageSocialStandingTier.WellLiked) =>
            [
                "Lila smiles apologetically. \"I trust you — village does too — but errands need me. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Lila, VillageSocialStandingTier.Respected) =>
            [
                "Lila says warmly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.WellLiked) =>
            [
                "Rowan smiles apologetically. \"I trust you — village does too — but a tale needs finishing. Ask again when it's quiet.\"",
            ],
            (NpcEntityIds.Rowan, VillageSocialStandingTier.Respected) =>
            [
                "Rowan says warmly, \"Not today. Doesn't mean Bloomtown's wrong about you — just bad timing.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}