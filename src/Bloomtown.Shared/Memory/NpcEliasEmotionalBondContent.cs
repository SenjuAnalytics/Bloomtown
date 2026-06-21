using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Elias — rough but honest blacksmith who values hard work and quality craft.
/// </summary>
internal static class NpcEliasEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedSmithyOften, RelationshipTier.CloseFriend, LegacyArchetype.Builder) =>
            [
                "You don't just swing a hammer — you make the forge feel like someone gives a damn. I notice.",
                "Builders like you show up with steady hands. The smithy runs better when you're in it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedSmithyOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first pitched in without being asked. That stayed with me.",
                "The forge hums steadier when you're around — not louder, but better tended. I notice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedSmithyOften, _, _) =>
            [
                "Every time you help at the smithy, I tell myself Bloomtown's luckier than folk admit.",
                "Your hands at the forge — honest work. It's become one of the things I look for each day.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEliasCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels earned — like good steel. I don't hand that out lightly.",
                "You've become part of my days here. I catch myself setting aside a stool when I hope you'll stop by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEliasCompanion, _, LegacyArchetype.Builder) =>
            [
                "You visit often enough the anvil seems to expect you. Builders leave traces like that.",
                "I'm always glad when you stop at the forge — you've made this shop feel less solitary.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentEliasCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the smithy feel like shared ground, not just mine.",
                "You drop by often enough I notice the quiet when you're not nearby. That's a good kind of missing.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedSmithyOften, _, _) =>
            [
                "There you are. The forge looked steadier this morning — I wondered if you'd come.",
                "Hello. Seeing you always reminds me how Bloomtown keeps its tools honest when folk show up.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentEliasCompanion, _, _) =>
            [
                "Hello. I was hoping you'd wander through today — the forge's quieter when you're away.",
                "There you are. I set a stool aside if you'd like it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToElias, _, _) =>
            [
                "I still think about that gift — practical and well-chosen. You noticed what a smith actually uses.",
                "Every time you stop to talk, I remember that gesture. It meant more than polite manners.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithElias, _, _) =>
            [
                "Those quiet spells by the forge — no tasks, no rush — they stay with me between orders.",
                "I'm always glad when you choose to linger. Presence is its own kind of respect.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Builder, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about builders — but you've built trust with me personally, hammer stroke by hammer stroke. That matters.",
                "Everyone else sees you raising projects. I see you remembering one forge at a time.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Your builder's steadiness isn't just timber and nails — you make tools and people feel solid. I've felt it.",
                "Builders raise structures. You've raised something steady between us, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended the forge, and me along the way.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's patience shows in how you show up here. The smithy feels it, and so do I.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Connectors weave folk together — you've woven yourself into my days here, one honest visit at a time.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedSmithyOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the forge wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedSmithyOften, _) =>
            [
                "Hey — I was just remembering your help at the smithy. It still steadies me.",
            ],
            (NpcMemoryType.FrequentEliasCompanion, _) =>
            [
                "I feel like I know you properly now — not as a passerby, but as someone I trust by the anvil.",
                "You always soften my day when you wander through. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedSmithyOften, LegacyArchetype.Builder) =>
            [
                "Elias meets your gaze. \"Builder's hands — the forge feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedSmithyOften, _) =>
            [
                "Elias nods. \"I noticed you again at the forge — thank you. It means more than tidy tools.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Elias murmurs, \"The smithy remembers builders. I do, too.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalMilestoneLines(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype) =>
        (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Builder) =>
            [
                "Elias nods slowly. \"Roots through work — I felt it at the forge long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Elias says plainly, \"Trusted neighbor — I've trusted your hands at the smithy for a while now.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
            [
                "Elias looks moved. \"Your story's in Bloomtown now — and part of it's written in honest forge hours.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedSmithyOften, LegacyArchetype.Builder) =>
            [
                "Elias calls over the clang. \"The forge is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentEliasCompanion, _) =>
            [
                "Elias waves from the anvil. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedSmithyOften, _) =>
            [
                "Elias nods your way. \"Good to see you. The forge remembers steady help.\"",
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
                "Elias sets down his hammer briefly. \"You always know when someone needs a steady word. I'm glad you stopped.\"",
                "Elias says plainly, \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Elias brightens slightly. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Elias grunts softly. \"You thought to ask how I'm doing. That's decent of you.\"",
                "Elias nods. \"I'm alright. And better for seeing you check in — stay a moment if you can.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Elias sits with you by the forge. \"Moments like this — they're what keep a village honest.\"",
                "Elias laughs quietly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Builder) =>
            [
                "Elias shares a peaceful silence with you. \"Good company needs time to temper — like steel. Thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Elias shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Elias says plainly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Builder) =>
            [
                "Elias watches you work. \"Builder's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Elias looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Elias murmurs, \"Small kindnesses at the forge — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Elias pours water and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Elias nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Elias shares a peaceful silence by the anvil. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedSmithyOften, _, LegacyArchetype.Builder) =>
            [
                "Elias adds, \"Your smithy help — I think of it when the tools look tired. Builders show up.\"",
            ],
            (NpcMemoryType.HelpedSmithyOften, _, _) =>
            [
                "Elias smiles. \"The forge remembers you — and so do I, every single day.\"",
            ],
            (NpcMemoryType.FrequentEliasCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Elias says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnElias, _, _) =>
            [
                "Elias touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithElias, _, _) =>
            [
                "Elias glances at you fondly. \"That quiet moment we shared — I still carry it between orders.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedElias, _, _) =>
            [
                "Elias says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Elias adds, \"I'm glad you came by — your presence steadies the forge. I mean that personally.\"",
                "Elias says plainly, \"Bloomtown's tools feel safer when you're in the shop. Don't let anyone tell you otherwise.\"",
            ],
            (NpcMemoryType.HelpedSmithyOften, _) =>
            [
                "Elias squeezes your hand briefly. \"Seeing you here — the whole forge breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithElias, _) =>
            [
                "Elias glances at you warmly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnElias, _) =>
            [
                "Elias says plainly, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentEliasCompanion, _) =>
            [
                "Elias says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Elias says warmly, \"I always appreciate when you stop at the forge. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Builder) =>
            [
                "Elias mentions, \"Morning's best for stocking timber — wood nodes east of the well refresh after dawn if you're building.\"",
                "Elias shares, \"Tom's woodpile gets busy later — builders who show up early save themselves half a day's hauling.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Elias says, \"Best time to notice what the village needs is when the lanes are still — folk speak softer then.\"",
                "Elias adds, \"Craft plank before noon if you're stocking up — steady hands work better before the square fills.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Builder) =>
            [
                "Elias leans in. \"Afternoons, the bridge site still gathers planks — builders know Harold posts updates near the well.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Elias murmurs, \"The market hums after lunch — but the forge stays useful if you need to think with your hands.\"",
                "Elias says, \"Tom's yard gets busy later — steady folk show up before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Elias shares, \"Evening's when the village exhales — rest then, or the body forgets why we work steadily.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Elias shares, \"Good tools aren't loud — they're showing up with quality materials before a project wilts.\"",
            ],
            (_, _) =>
            [
                "Elias says, \"If weariness creeps in, work the forge or rest outdoors — honest labor steadies the mind.\"",
                "Elias adds plainly, \"Village projects need timber and planks — the board by the well lists what matters most.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedSmithyOften, RelationshipTier.CloseFriend) =>
            [
                "Elias presses a tool and two planks into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Elias insists you sit by the forge. \"Rest before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedSmithyOften, _) =>
            [
                "Elias hands you wood and a plank. \"For the road — the forge can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnElias, _) =>
            [
                "Elias presses a small tool into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Elias packs wood, a plank, and a tool for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Elias nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Elias adds, \"If coins are tight, Mira's fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Elias insists you sit and shares bread from his bench. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Elias insists you sit a moment and offers wood. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Elias packs a plank and a tool for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Elias packs a small bundle of wood for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Elias thanks you and sends you off with wood and a plank. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}