using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Eleanor — warm, slightly chatty retired teacher who loves village history,
/// nostalgic and caring, slow to open her heart but fiercely loyal once trust is earned.
/// </summary>
internal static class NpcEleanorEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToEleanorStories, RelationshipTier.CloseFriend, LegacyArchetype.Connector) =>
            [
                "You don't just listen to my old stories — you carry them forward. Connectors like you keep a village's heart alive.",
                "Bloomtown's luckier than it knows — folk who sit and listen turn memory into belonging.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToEleanorStories, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first sat on my porch without hurry. I thought — oh, they mean to stay.",
                "Every story you hear with me — I carry it like good news. It adds up in ways the square never names.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToEleanorStories, _, _) =>
            [
                "Every time you listen to my old tales, I tell myself Bloomtown remembers more than its stones.",
                "Your patience on the porch — honest attention. I look for it now when the lanes grow quiet.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEleanorCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like warmth that stayed — I don't say that to everyone who passes the well.",
                "You've become part of my afternoons here. I catch myself saving a story when I hope you'll stop by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEleanorCompanion, _, LegacyArchetype.Caretaker) =>
            [
                "You visit often enough the porch seems to expect you. Caretakers leave that kind of quiet warmth.",
                "I'm always glad when you wander through — you've made Bloomtown feel less like a map and more like memory.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEleanorCompanion, _, _) =>
            [
                "There you are — good. You've made my little porch feel like shared ground, not just an old woman's seat.",
                "You drop by often enough I notice when you're not around. I hope that doesn't sound fussy — I mean it kindly.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.ListenedToEleanorStories, _, _) =>
            [
                "There you are, dear. I was just thinking about your visits — they still steady me.",
                "Hello. Seeing you always reminds me how Bloomtown keeps its heart when folk truly listen.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentEleanorCompanion, _, _) =>
            [
                "Hello — I was hoping I'd see you today. The porch feels emptier when you're away.",
                "Ah, perfect timing. I saved something small to tell you, if you've got a minute.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToEleanor, _, _) =>
            [
                "I still think about that gift — you noticed what makes an old teacher feel seen, not merely tolerated.",
                "Every time you stop to talk, I remember that gesture. It meant more than you probably know.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithEleanor, _, _) =>
            [
                "Those quiet spells we shared — no rush, no audience — they stay with me between stories.",
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
                "Everyone else sees you greeting folk at the square. I see you remembering one old neighbor at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your connector's warmth isn't just for the market — you make quiet folk like me feel included. I've felt it.",
                "Connectors weave people together. You've woven yourself into my afternoons here, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended my hope in this village, quietly and often.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's heart shows in how you show up — I've felt it in our talks on the porch.",
                "Bloomtown sees a caretaker in you. I see it in the small things you do for people like me.",
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
            (NpcMemoryType.ListenedToEleanorStories, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — Bloomtown wouldn't feel the same to me without you. I mean that personally.",
            ],
            (NpcMemoryType.ListenedToEleanorStories, _) =>
            [
                "Dear — I was just remembering your visits on the porch. They still brighten my afternoon.",
            ],
            (NpcMemoryType.FrequentEleanorCompanion, _) =>
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
            (NpcMemoryType.ListenedToEleanorStories, LegacyArchetype.Connector) =>
            [
                "Eleanor smiles warmly. \"Connector's ear — the village feels it, and so do I.\"",
            ],
            (NpcMemoryType.ListenedToEleanorStories, _) =>
            [
                "Eleanor nods slowly. \"I noticed you listening again — thank you, dear. It means more than a tidy porch.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Eleanor touches your sleeve gently. \"The village remembers caretakers. I do, too.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalMilestoneLines(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype) =>
        (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Caretaker) =>
            [
                "Eleanor smiles with quiet pride. \"Roots through kindness — I felt it in our talks long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Eleanor says warmly, \"Trusted neighbor — I've trusted your company for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Eleanor looks moved. \"Your story's in Bloomtown now — and part of it's written in the visits you kept making.\"",
            ],
            (PlayerLongTermGoalMilestone.BloomtownLegacy, _) =>
            [
                "Eleanor speaks slowly. \"A living legacy — what you built includes the trust between us, dear.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.ListenedToEleanorStories, LegacyArchetype.Connector) =>
            [
                "Eleanor calls over from the porch. \"The village is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentEleanorCompanion, _) =>
            [
                "Eleanor waves from her chair. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.ListenedToEleanorStories, _) =>
            [
                "Eleanor nods your way. \"Good to see you, dear. The porch remembers steady listeners.\"",
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
                "Eleanor's face softens. \"You always know when someone needs a kind word. I'm really glad you stopped, dear.\"",
                "Eleanor takes your hand briefly. \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Eleanor brightens a little. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Eleanor looks touched. \"You thought to ask how I'm doing. That's lovely of you.\"",
                "Eleanor nods warmly. \"I'm alright, truly. And better for seeing you check in — stay a moment if you can.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Eleanor sits with you on the porch. \"Moments like this — they're what make Bloomtown feel like home.\"",
                "Eleanor laughs softly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Eleanor shares a peaceful silence with you. \"Good company needs time to grow — thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Eleanor shares a quiet moment with you. \"Thank you for the company, dear. I needed that more than I'd admit.\"",
                "Eleanor says warmly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Eleanor watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Eleanor looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Eleanor murmurs, \"Small kindnesses like this — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Eleanor pours tea and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Eleanor nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Eleanor shares a peaceful silence on the porch. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.ListenedToEleanorStories, _, LegacyArchetype.Connector) =>
            [
                "Eleanor adds, \"Your visits on the porch — I think of them when the lanes grow quiet. Connectors listen.\"",
            ],
            (NpcMemoryType.ListenedToEleanorStories, _, _) =>
            [
                "Eleanor smiles. \"The village remembers you — and so do I, every single afternoon.\"",
            ],
            (NpcMemoryType.FrequentEleanorCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Eleanor says, \"You've become part of how I picture Bloomtown — I hope you know that, dear.\"",
            ],
            (NpcMemoryType.CheckedOnEleanor, _, _) =>
            [
                "Eleanor touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithEleanor, _, _) =>
            [
                "Eleanor glances at you fondly. \"That quiet moment we shared — I still carry it between stories.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedEleanor, _, _) =>
            [
                "Eleanor says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Eleanor adds, \"I'm glad you came by — your presence steadies me more than you know. Bloomtown feels warmer when you're in it.\"",
                "Eleanor meets your eyes. \"You've become someone I count on seeing — that's not a small thing, and I won't pretend it is.\"",
            ],
            (NpcMemoryType.ListenedToEleanorStories, _) =>
            [
                "Eleanor squeezes your hand briefly. \"Seeing you here — the whole village breathes easier. I haven't forgotten your listening.\"",
            ],
            (NpcMemoryType.SharedMomentWithEleanor, _) =>
            [
                "Eleanor glances at you fondly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnEleanor, _) =>
            [
                "Eleanor says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentEleanorCompanion, _) =>
            [
                "Eleanor says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Eleanor says warmly, \"I always appreciate when you stop to talk. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Eleanor mentions, \"Morning's when the old schoolhouse stood tallest — folk remember kindness at dawn before the square fills.\"",
                "Eleanor shares, \"Harold's porch catches the first light — connectors and caretakers both find good company there.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Eleanor says, \"Best time to hear village history is when the lanes are still — folk speak truer then, dear.\"",
                "Eleanor adds, \"Chat on the porch before noon — old stories land better before the market stirs.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Caretaker) =>
            [
                "Eleanor leans in. \"Afternoons, the square hums — but Elsie's garden holds tales the market never tells.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Eleanor murmurs, \"The bridge was raised after the first harvest — but the well's roots go deeper still.\"",
                "Eleanor says, \"Rowan's bench gets busy later — old classroom tales are best before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Eleanor shares, \"Evening's when Bloomtown exhales — rest then, or weariness makes folk forget what came before.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Eleanor shares, \"Good days in Bloomtown aren't loud — they're showing up where folk need a patient ear.\"",
            ],
            (_, _) =>
            [
                "Eleanor says, \"If loneliness creeps in, sit on the porch or walk the lanes — honest company steadies the mind.\"",
                "Eleanor adds plainly, \"The community board lists what matters most — but the village's memory lives in its people.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToEleanorStories, RelationshipTier.CloseFriend) =>
            [
                "Eleanor presses an apple and wood into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Eleanor insists you sit a moment. \"Rest before you push on, dear. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToEleanorStories, _) =>
            [
                "Eleanor hands you an apple and wood. \"For the road — the village can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnEleanor, _) =>
            [
                "Eleanor presses a warm apple into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Eleanor packs an apple and wood for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Eleanor nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Eleanor adds, \"If coins are tight, Mira's fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Eleanor insists you sit and shares bread and an apple. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Eleanor insists you sit a moment and offers an apple. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Eleanor packs wood and an apple for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Eleanor packs a small apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Eleanor thanks you and sends you off with wood and an apple. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}