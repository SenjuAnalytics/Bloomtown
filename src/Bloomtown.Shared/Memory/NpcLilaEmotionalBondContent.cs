using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Lila — energetic young villager who is warm, idealistic, and sensitive when overlooked.
/// </summary>
internal static class NpcLilaEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageOften, RelationshipTier.CloseFriend, LegacyArchetype.Connector) =>
            [
                "You don't just help around Bloomtown — you make it feel like a place worth showing up for. I notice every time.",
                "Connectors like you turn chores into company. The village hums brighter when you're lending a hand.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first pitched in without anyone asking. I thought — oh, they're really staying.",
                "Every little help you give around the village — I carry it like good news. It adds up.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageOften, _, _) =>
            [
                "Every time you help around the village, I tell myself Bloomtown's luckier than it looks on paper.",
                "Your hands everywhere — garden, square, lanes — honest work. I look for it now.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentLilaCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like sunshine that stayed — I don't say that to everyone.",
                "You've become part of my days here. I catch myself saving a story when I hope you'll stop by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentLilaCompanion, _, LegacyArchetype.Connector) =>
            [
                "You visit often enough the square seems to expect you. Connectors leave warmth like that.",
                "I'm always glad when you wander through — you've made Bloomtown feel less like a map and more like home.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentLilaCompanion, _, _) =>
            [
                "Oh — you again! Good. You've made the village feel like shared ground, not just background.",
                "You drop by often enough I notice when you're not around. I hope that doesn't sound strange — I mean it kindly.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedVillageOften, _, _) =>
            [
                "There you are! I was just thinking about your help around the village — it still makes me smile.",
                "Hello! Seeing you always reminds me how Bloomtown keeps its heart when folk show up.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentLilaCompanion, _, _) =>
            [
                "Hello — I was hoping I'd see you today. The lanes feel quieter when you're away.",
                "Oh, perfect timing! I saved something small to tell you, if you've got a minute.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToLila, _, _) =>
            [
                "I still think about that gift — you noticed what makes a young villager feel seen, not just tolerated.",
                "Every time you stop to talk, I remember that gesture. It meant more than you probably know.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithLila, _, _) =>
            [
                "Those quiet spells we shared — no rush, no audience — they stay with me between errands.",
                "I'm always glad when you choose to linger. Presence is its own kind of kindness.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Connector, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about connectors — but you've connected with me personally, visit by visit. That matters.",
                "Everyone else sees you greeting folk at the square. I see you remembering one young villager at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your connector's warmth isn't just for the market — you make shy folk like me feel included. I've felt it.",
                "Connectors weave people together. You've woven yourself into my days here, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Caretakers tend gardens and wells — you've tended my hope in this village, quietly and often.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Builders raise structures — you've raised something steady between us, one honest visit at a time.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedVillageOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — Bloomtown wouldn't feel the same to me without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedVillageOften, _) =>
            [
                "Hey — I was just remembering your help around the village. It still brightens my day.",
            ],
            (NpcMemoryType.FrequentLilaCompanion, _) =>
            [
                "I feel like I know you properly now — not as a newcomer passing through, but as someone I trust.",
                "You always brighten my step when you wander past. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedVillageOften, LegacyArchetype.Connector) =>
            [
                "Lila grins. \"Connector's hands — the village feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedVillageOften, _) =>
            [
                "Lila beams. \"I noticed you helping again — thank you. It means more than tidy lanes.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Lila touches your sleeve gently. \"The village remembers caretakers. I do, too.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalMilestoneLines(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype) =>
        (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Connector) =>
            [
                "Lila smiles widely. \"Roots through kindness — I felt it in our talks long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Lila says warmly, \"Trusted neighbor — I've trusted your company for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Lila looks moved. \"Your story's in Bloomtown now — and part of it's written in the visits you kept making.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedVillageOften, LegacyArchetype.Connector) =>
            [
                "Lila calls over cheerfully. \"The village is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentLilaCompanion, _) =>
            [
                "Lila waves eagerly. \"There you are — I was hoping I'd see you today!\"",
            ],
            (NpcMemoryType.HelpedVillageOften, _) =>
            [
                "Lila nods your way. \"Good to see you. The lanes remember steady help.\"",
            ],
            _ => [],
        };

    internal static string[] GetBondingActionLines(
        EmotionalBondActionKind action,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (action, tier, archetype) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend, _) =>
            [
                "Lila's face softens. \"You always know when someone needs a kind word. I'm really glad you stopped.\"",
                "Lila takes your hand briefly. \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Lila brightens. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Lila lights up. \"Oh — you thought to ask how I'm doing. That's lovely of you.\"",
                "Lila nods warmly. \"I'm alright, truly. And better for seeing you check in.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Lila sits with you quietly. \"Moments like this — they're what make Bloomtown feel like home.\"",
                "Lila laughs softly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Lila shares a peaceful silence with you. \"Good company needs time to grow — thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Lila shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Lila says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Lila watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Lila looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Lila murmurs, \"Small kindnesses like this — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Lila pours water and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Lila nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Lila shares a peaceful silence with you. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedVillageOften, _, LegacyArchetype.Connector) =>
            [
                "Lila adds, \"Your help around the village — I think of it when the lanes look tired. Connectors show up.\"",
            ],
            (NpcMemoryType.HelpedVillageOften, _, _) =>
            [
                "Lila smiles. \"The village remembers you — and so do I, every single day.\"",
            ],
            (NpcMemoryType.FrequentLilaCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Lila says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnLila, _, _) =>
            [
                "Lila touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithLila, _, _) =>
            [
                "Lila glances at you fondly. \"That quiet moment we shared — I still carry it between errands.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedLila, _, _) =>
            [
                "Lila says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
            ],
            _ => [],
        };

    internal static string[] GetPersonalAppreciationLines(
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (_, RelationshipTier.CloseFriend) =>
            [
                "Lila adds, \"I'm glad you came by — your presence steadies me more than you know. Bloomtown feels warmer when you're in it.\"",
                "Lila meets your eyes. \"You've become someone I count on seeing — that's not a small thing, and I won't pretend it is.\"",
            ],
            (NpcMemoryType.HelpedVillageOften, _) =>
            [
                "Lila squeezes your hand briefly. \"Seeing you here — the whole village breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithLila, _) =>
            [
                "Lila glances at you fondly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnLila, _) =>
            [
                "Lila says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentLilaCompanion, _) =>
            [
                "Lila says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Lila says warmly, \"I always appreciate when you stop to talk. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Lila mentions, \"Morning's best for greeting folk — vendors remember kindness before the square fills.\"",
                "Lila shares, \"Elsie's garden is dew-soft at dawn — caretakers and connectors both find good company there.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Lila says, \"Best time to notice what the village needs is when the lanes are still — folk smile easier then.\"",
                "Lila adds, \"Help around the village before noon — steady hands work better before the market stirs.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Connector) =>
            [
                "Lila leans in. \"Afternoons, the square hums — but Greta's porch is quieter if you need a real conversation.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Lila murmurs, \"The market fills after lunch — but the garden stays useful if you need to think with your hands.\"",
                "Lila says, \"Tom's yard gets busy later — young folk like us show up before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Lila shares, \"Evening's when the village exhales — rest then, or weariness makes folk careless near the well.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Lila shares, \"Good days in Bloomtown aren't loud — they're showing up where folk need a friendly face.\"",
            ],
            (_, _) =>
            [
                "Lila says, \"If loneliness creeps in, walk the lanes or help somewhere — honest company steadies the mind.\"",
                "Lila adds plainly, \"The community board lists what matters most — young or old, we all read it eventually.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageOften, RelationshipTier.CloseFriend) =>
            [
                "Lila presses an apple and a stick of wood into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Lila insists you sit a moment. \"Rest before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedVillageOften, _) =>
            [
                "Lila hands you an apple and wood. \"For the road — the village can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnLila, _) =>
            [
                "Lila presses a warm apple into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Lila packs apples and wood for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Lila nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Lila adds, \"If coins are tight, Mira's fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Lila insists you sit and shares bread and an apple. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Lila insists you sit a moment and offers an apple. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Lila packs two apples and wood for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Lila packs a small apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Lila thanks you and sends you off with an apple and wood. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}