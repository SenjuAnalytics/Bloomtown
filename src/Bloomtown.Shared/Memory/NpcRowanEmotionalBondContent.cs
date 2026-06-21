using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Rowan — wise, slightly melancholic storyteller who loves village history,
/// warm to listeners, slow to open his heart but loyal once trust is earned.
/// </summary>
internal static class NpcRowanEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToStoriesOften, RelationshipTier.CloseFriend, LegacyArchetype.Connector) =>
            [
                "You don't just listen to my stories — you carry them forward. Connectors like you keep a village's memory alive.",
                "Bloomtown's luckier than it knows — folk who stay and listen turn history into belonging.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToStoriesOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first sat at the bench without hurry. I thought — oh, they mean to stay.",
                "Every story you hear with me — I carry it like good news. It adds up in ways the square never names.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToStoriesOften, _, _) =>
            [
                "Every time you listen at the story bench, I tell myself Bloomtown remembers more than its stones.",
                "Your patience at the bench — honest attention. I look for it now when the lanes grow quiet.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentRowanCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like warmth that stayed — I don't say that to everyone who passes the inn.",
                "You've become part of my evenings here. I catch myself saving a tale when I hope you'll stop by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentRowanCompanion, _, LegacyArchetype.Connector) =>
            [
                "You visit often enough the bench seems to expect you. Connectors leave that kind of quiet warmth.",
                "I'm always glad when you wander through — you've made Bloomtown feel less like a map and more like memory.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentRowanCompanion, _, _) =>
            [
                "There you are — good. You've made the story corner feel like shared ground, not just background noise.",
                "You drop by often enough I notice when you're not around. I hope that doesn't sound strange — I mean it kindly.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.ListenedToStoriesOften, _, _) =>
            [
                "There you are. I was just thinking about your visits at the bench — they still steady me.",
                "Hello. Seeing you always reminds me how Bloomtown keeps its heart when folk truly listen.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentRowanCompanion, _, _) =>
            [
                "Hello — I was hoping I'd see you today. The bench feels emptier when you're away.",
                "Ah, perfect timing. I saved something small to tell you, if you've got a minute.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToRowan, _, _) =>
            [
                "I still think about that gift — you noticed what makes an old storyteller feel seen, not merely tolerated.",
                "Every time you stop to talk, I remember that gesture. It meant more than you probably know.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithRowan, _, _) =>
            [
                "Those quiet spells we shared — no rush, no audience — they stay with me between tales.",
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
                "Everyone else sees you greeting folk at the square. I see you remembering one listener at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your connector's warmth isn't just for the market — you make quiet folk like me feel included. I've felt it.",
                "Connectors weave people together. You've woven yourself into my evenings here, and I don't take that lightly.",
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
            (NpcMemoryType.ListenedToStoriesOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — Bloomtown wouldn't feel the same to me without you. I mean that personally.",
            ],
            (NpcMemoryType.ListenedToStoriesOften, _) =>
            [
                "Hey — I was just remembering your visits at the bench. They still brighten my evening.",
            ],
            (NpcMemoryType.FrequentRowanCompanion, _) =>
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
            (NpcMemoryType.ListenedToStoriesOften, LegacyArchetype.Connector) =>
            [
                "Rowan smiles faintly. \"Connector's ear — the village feels it, and so do I.\"",
            ],
            (NpcMemoryType.ListenedToStoriesOften, _) =>
            [
                "Rowan nods slowly. \"I noticed you listening again — thank you. It means more than a tidy bench.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Rowan touches your sleeve gently. \"The village remembers caretakers. I do, too.\"",
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
                "Rowan smiles with quiet pride. \"Roots through kindness — I felt it in our talks long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Rowan says warmly, \"Trusted neighbor — I've trusted your company for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Rowan looks moved. \"Your story's in Bloomtown now — and part of it's written in the visits you kept making.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.ListenedToStoriesOften, LegacyArchetype.Connector) =>
            [
                "Rowan calls over softly. \"The village is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentRowanCompanion, _) =>
            [
                "Rowan waves from the bench. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.ListenedToStoriesOften, _) =>
            [
                "Rowan nods your way. \"Good to see you. The bench remembers steady listeners.\"",
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
                "Rowan's face softens. \"You always know when someone needs a kind word. I'm really glad you stopped.\"",
                "Rowan takes your hand briefly. \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Rowan brightens a little. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Rowan looks touched. \"You thought to ask how I'm doing. That's lovely of you.\"",
                "Rowan nods warmly. \"I'm alright, truly. And better for seeing you check in.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Rowan sits with you quietly. \"Moments like this — they're what make Bloomtown feel like home.\"",
                "Rowan laughs softly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Rowan shares a peaceful silence with you. \"Good company needs time to grow — thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Rowan shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Rowan says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Rowan watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Rowan looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Rowan murmurs, \"Small kindnesses like this — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Rowan pours water and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Rowan nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Rowan shares a peaceful silence with you. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.ListenedToStoriesOften, _, LegacyArchetype.Connector) =>
            [
                "Rowan adds, \"Your visits at the bench — I think of them when the lanes grow quiet. Connectors listen.\"",
            ],
            (NpcMemoryType.ListenedToStoriesOften, _, _) =>
            [
                "Rowan smiles. \"The village remembers you — and so do I, every single evening.\"",
            ],
            (NpcMemoryType.FrequentRowanCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Rowan says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnRowan, _, _) =>
            [
                "Rowan touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithRowan, _, _) =>
            [
                "Rowan glances at you fondly. \"That quiet moment we shared — I still carry it between tales.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedRowan, _, _) =>
            [
                "Rowan says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Rowan adds, \"I'm glad you came by — your presence steadies me more than you know. Bloomtown feels warmer when you're in it.\"",
                "Rowan meets your eyes. \"You've become someone I count on seeing — that's not a small thing, and I won't pretend it is.\"",
            ],
            (NpcMemoryType.ListenedToStoriesOften, _) =>
            [
                "Rowan squeezes your hand briefly. \"Seeing you here — the whole village breathes easier. I haven't forgotten your listening.\"",
            ],
            (NpcMemoryType.SharedMomentWithRowan, _) =>
            [
                "Rowan glances at you fondly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnRowan, _) =>
            [
                "Rowan says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentRowanCompanion, _) =>
            [
                "Rowan says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Rowan says warmly, \"I always appreciate when you stop to talk. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Rowan mentions, \"Morning's when the old well was dug — folk remember kindness at dawn before the square fills.\"",
                "Rowan shares, \"Harold's porch catches the first light — connectors and caretakers both find good company there.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Rowan says, \"Best time to hear village history is when the lanes are still — folk speak truer then.\"",
                "Rowan adds, \"Listen at the bench before noon — the inn's stories land better before the market stirs.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Connector) =>
            [
                "Rowan leans in. \"Afternoons, the square hums — but Greta's hearth holds tales the market never tells.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Rowan murmurs, \"The bridge was raised after the first harvest — but the garden's roots go deeper still.\"",
                "Rowan says, \"Tom's yard gets busy later — old timber tales are best before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Rowan shares, \"Evening's when Bloomtown exhales — rest then, or weariness makes folk forget what came before.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Rowan shares, \"Good days in Bloomtown aren't loud — they're showing up where folk need a patient ear.\"",
            ],
            (_, _) =>
            [
                "Rowan says, \"If loneliness creeps in, sit at the bench or walk the lanes — honest company steadies the mind.\"",
                "Rowan adds plainly, \"The community board lists what matters most — but the village's memory lives in its stories.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToStoriesOften, RelationshipTier.CloseFriend) =>
            [
                "Rowan presses wood and an apple into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Rowan insists you sit a moment. \"Rest before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.ListenedToStoriesOften, _) =>
            [
                "Rowan hands you wood and an apple. \"For the road — the village can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnRowan, _) =>
            [
                "Rowan presses a warm apple into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Rowan packs wood and an apple for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Rowan nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Rowan adds, \"If coins are tight, Mira's fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Rowan insists you sit and shares bread and an apple. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Rowan insists you sit a moment and offers an apple. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Rowan packs wood and an apple for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Rowan packs a small apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Rowan thanks you and sends you off with wood and an apple. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}