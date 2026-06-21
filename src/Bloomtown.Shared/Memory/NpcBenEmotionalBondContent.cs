using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Ben — responsible village guard who values consistency, trust, and village safety.
/// </summary>
internal static class NpcBenEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedPatrolOften, RelationshipTier.CloseFriend, LegacyArchetype.Caretaker) =>
            [
                "You don't just walk patrol — you make the lanes feel watched over. I notice who shows up without being ordered.",
                "Caretakers like you keep a village honest. The guard post runs steadier when you're on the route.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedPatrolOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first fell in on patrol without fanfare. That stayed with me.",
                "The lanes feel quieter in a good way when you're around — not empty, but tended. I notice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedPatrolOften, _, _) =>
            [
                "Every time you help on patrol, I tell myself Bloomtown's safer than folk admit.",
                "Your steps on the route — steady work. It's become one of the things I look for each day.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentBenCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels earned — like a badge I don't hand out lightly.",
                "You've become part of my rounds here. I catch myself checking the lane when I hope you'll pass by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentBenCompanion, _, LegacyArchetype.Caretaker) =>
            [
                "You visit often enough the guard post seems to expect you. Caretakers leave traces like that.",
                "I'm always glad when you stop at my post — you've made this duty feel less solitary.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentBenCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the patrol feel like shared ground, not just mine.",
                "You drop by often enough I notice the quiet when you're not nearby. That's trust, plain and simple.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedPatrolOften, _, _) =>
            [
                "There you are. The lanes looked steadier this morning — I wondered if you'd come.",
                "Hello. Seeing you always reminds me how Bloomtown keeps order when folk show up.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentBenCompanion, _, _) =>
            [
                "Hello. I was hoping you'd pass through today — the post is quieter when you're away.",
                "There you are. I kept the bench clear if you'd like it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToBen, _, _) =>
            [
                "I still think about that gift — practical and well-chosen. You noticed what a guard actually uses.",
                "Every time you stop to talk, I remember that gesture. It meant more than polite manners.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithBen, _, _) =>
            [
                "Those quiet spells by the guard post — no tasks, no rush — they stay with me between rounds.",
                "I'm always glad when you choose to linger. Presence is its own kind of reliability.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about caretakers — but you've cared for this village's safety with me personally, round by round. That matters.",
                "Everyone else sees you tending gardens and wells. I see you remembering one guard post at a time.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's steadiness isn't just herbs and water — you make lanes and people feel watched over. I've felt it.",
                "Caretakers keep villages whole. You've kept something steady between us, and I don't take that lightly.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Connectors weave folk together — you've woven yourself into my patrol, one honest visit at a time.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Builders raise structures — you've raised trust on my route, plank by plank of steady showing-up.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedPatrolOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the lanes wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedPatrolOften, _) =>
            [
                "Hey — I was just remembering your help on patrol. It still steadies me.",
            ],
            (NpcMemoryType.FrequentBenCompanion, _) =>
            [
                "I feel like I know you properly now — not as a passerby, but as someone I trust on my route.",
                "You always soften my round when you pass through. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedPatrolOften, LegacyArchetype.Caretaker) =>
            [
                "Ben meets your gaze. \"Caretaker's steps — the lanes feel it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedPatrolOften, _) =>
            [
                "Ben nods. \"I noticed you again on patrol — thank you. It means more than tidy paths.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Ben murmurs, \"The village remembers caretakers. I do, too.\"",
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
                "Ben nods slowly. \"Roots through care — I felt it on patrol long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Ben says plainly, \"Trusted neighbor — I've trusted your steps on my route for a while now.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
            [
                "Ben looks moved. \"Your story's in Bloomtown now — and part of it's written in honest patrol hours.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedPatrolOften, LegacyArchetype.Caretaker) =>
            [
                "Ben calls from the lane. \"The village is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentBenCompanion, _) =>
            [
                "Ben raises a hand from his post. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedPatrolOften, _) =>
            [
                "Ben nods your way. \"Good to see you. The lanes remember steady help.\"",
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
                "Ben sets down his lantern briefly. \"You always know when someone needs a steady word. I'm glad you stopped.\"",
                "Ben says plainly, \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Ben brightens slightly. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Ben nods stiffly. \"You thought to ask how I'm doing. That's decent of you.\"",
                "Ben says evenly, \"I'm alright. And better for seeing you check in — stay a moment if you can.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Ben sits with you by the guard post. \"Moments like this — they're what keep a village honest.\"",
                "Ben exhales quietly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Ben shares a peaceful silence with you. \"Good company needs time to settle — thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Ben shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Ben says plainly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Ben watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Ben looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Ben murmurs, \"Small kindnesses at the guard post — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Ben pours water and sits with you. \"No errands, no rush — just company. This is the good part of my round.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Ben nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Ben shares a peaceful silence by the post. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedPatrolOften, _, LegacyArchetype.Caretaker) =>
            [
                "Ben adds, \"Your patrol help — I think of it when the lanes look tired. Caretakers show up.\"",
            ],
            (NpcMemoryType.HelpedPatrolOften, _, _) =>
            [
                "Ben smiles faintly. \"The lanes remember you — and so do I, every single round.\"",
            ],
            (NpcMemoryType.FrequentBenCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Ben says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnBen, _, _) =>
            [
                "Ben touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithBen, _, _) =>
            [
                "Ben glances at you fondly. \"That quiet moment we shared — I still carry it between rounds.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedBen, _, _) =>
            [
                "Ben says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Ben adds, \"I'm glad you came by — your presence steadies the lanes. I mean that personally.\"",
                "Ben says plainly, \"Bloomtown feels safer when you're on my route. Don't let anyone tell you otherwise.\"",
            ],
            (NpcMemoryType.HelpedPatrolOften, _) =>
            [
                "Ben squeezes your hand briefly. \"Seeing you here — the whole post breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithBen, _) =>
            [
                "Ben glances at you warmly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnBen, _) =>
            [
                "Ben says plainly, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentBenCompanion, _) =>
            [
                "Ben says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Ben says warmly, \"I always appreciate when you stop at the guard post. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Caretaker) =>
            [
                "Ben mentions, \"Morning's best for walking the lanes — folk speak softer before the square fills, and trouble shows itself early.\"",
                "Ben shares, \"Harold's at the well at dawn. Caretakers who greet him there hear what the village needs first.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Ben says, \"Best time to notice what the village needs is when the lanes are still — I watch for loose fences and tired folk.\"",
                "Ben adds, \"Patrol before noon if you're helping — steady eyes work better before the market stirs.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Connector) =>
            [
                "Ben leans in. \"Afternoons, the square hums — connectors hear more in the bustle than guards admit aloud.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Ben murmurs, \"The market fills after lunch — but the lanes stay useful if you need to think with your feet.\"",
                "Ben says, \"Tom's yard gets busy later — folk hauling timber clog the east path. Plan around it.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Ben shares, \"Evening's when the village exhales — rest then, or weariness makes folk careless near the well.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Ben shares, \"Good order isn't loud — it's showing up where the village needs steady eyes before small problems grow.\"",
            ],
            (_, _) =>
            [
                "Ben says, \"If weariness creeps in, walk the lanes or rest outdoors — honest patrol steadies the mind.\"",
                "Ben adds plainly, \"Bridge and well work still need stone and planks — the board by the square lists what matters most.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedPatrolOften, RelationshipTier.CloseFriend) =>
            [
                "Ben presses a tool and two sticks of wood into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Ben insists you sit by the post. \"Rest before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedPatrolOften, _) =>
            [
                "Ben hands you wood and a tool. \"For the road — the post can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnBen, _) =>
            [
                "Ben presses a small tool into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Ben packs wood and a tool for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Ben nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Ben adds, \"If supplies run low, Tom's yard is fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Ben insists you sit and shares bread from his pack. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Ben insists you sit a moment and offers wood. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Ben packs a tool and wood for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Ben packs a small bundle of wood for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Ben thanks you and sends you off with wood and a tool. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}