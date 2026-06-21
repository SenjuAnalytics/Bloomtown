using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Marcus — practical, friendly craftsman who values quality work
/// and steady helpers. Warmer and more open than Elias, but still honors consistency.
/// </summary>
internal static class NpcMarcusEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedWorkshopOften, RelationshipTier.CloseFriend, LegacyArchetype.Builder) =>
            [
                "You don't just lend a hand — you make the workshop feel like someone cares about the finish. I notice every time.",
                "Builders like you show up with patient hands. The bench runs better when you're in it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedWorkshopOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first pitched in without being asked. That stayed with me.",
                "The workshop hums steadier when you're around — not louder, but better tended. I notice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedWorkshopOften, _, _) =>
            [
                "Every time you help at the workshop, I tell myself Bloomtown's luckier than folk admit.",
                "Your hands at the bench — honest craft. It's become one of the things I look for each day.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMarcusCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels easy — like good joinery. I don't hand that out lightly.",
                "You've become part of my days here. I catch myself setting aside a stool when I hope you'll stop by.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMarcusCompanion, _, LegacyArchetype.Builder) =>
            [
                "You visit often enough the workshop seems to expect you. Builders leave traces like that.",
                "I'm always glad when you stop by — you've made this shop feel less solitary.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMarcusCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the workshop feel like shared ground, not just mine.",
                "You drop by often enough I notice the quiet when you're not nearby. That's a good kind of missing.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedWorkshopOften, _, _) =>
            [
                "There you are. The bench looked steadier this morning — I wondered if you'd come.",
                "Hello. Seeing you always reminds me how Bloomtown keeps its repairs honest when folk show up.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentMarcusCompanion, _, _) =>
            [
                "Hello. I was hoping you'd wander through today — the workshop's quieter when you're away.",
                "There you are. I set a stool aside if you'd like it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToMarcus, _, _) =>
            [
                "I still think about that gift — practical and well-chosen. You noticed what a craftsman actually uses.",
                "Every time you stop to talk, I remember that gesture. It meant more than polite manners.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithMarcus, _, _) =>
            [
                "Those quiet spells by the bench — no tasks, no rush — they stay with me between orders.",
                "I'm always glad when you choose to linger. Presence is its own kind of respect.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Builder, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about builders — but you've built trust with me personally, joint by joint. That matters.",
                "Everyone else sees you raising projects. I see you remembering one workshop at a time.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Your builder's steadiness isn't just timber and nails — you make tools and people feel solid. I've felt it.",
                "Builders raise structures. You've raised something steady between us, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended the workshop, and me along the way.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's patience shows in how you show up here. The bench feels it, and so do I.",
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
            (NpcMemoryType.HelpedWorkshopOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the workshop wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedWorkshopOften, _) =>
            [
                "Hey — I was just remembering your help at the bench. It still steadies me.",
            ],
            (NpcMemoryType.FrequentMarcusCompanion, _) =>
            [
                "I feel like I know you properly now — not as a passerby, but as someone I trust by the workbench.",
                "You always soften my day when you wander through. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedWorkshopOften, LegacyArchetype.Builder) =>
            [
                "Marcus meets your gaze. \"Builder's hands — the workshop feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedWorkshopOften, _) =>
            [
                "Marcus smiles. \"I noticed you again at the bench — thank you. It means more than tidy tools.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Marcus murmurs, \"The workshop remembers builders. I do, too.\"",
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
                "Marcus nods warmly. \"Roots through work — I felt it at the bench long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Marcus says plainly, \"Trusted neighbor — I've trusted your hands at the workshop for a while now.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
            [
                "Marcus looks moved. \"Your story's in Bloomtown now — and part of it's written in honest bench hours.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedWorkshopOften, LegacyArchetype.Builder) =>
            [
                "Marcus calls over the sawdust. \"The workshop is lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentMarcusCompanion, _) =>
            [
                "Marcus waves from the bench. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedWorkshopOften, _) =>
            [
                "Marcus nods your way. \"Good to see you. The workshop remembers steady help.\"",
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
                "Marcus sets down his plane briefly. \"You always know when someone needs a steady word. I'm glad you stopped.\"",
                "Marcus says warmly, \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Marcus brightens. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Marcus smiles. \"You thought to ask how I'm doing. That's decent of you.\"",
                "Marcus nods. \"I'm alright. And better for seeing you check in — stay a moment if you can.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Marcus sits with you by the bench. \"Moments like this — they're what keep a village honest.\"",
                "Marcus laughs quietly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Builder) =>
            [
                "Marcus shares a peaceful silence with you. \"Good company needs time to settle — like fine wood. Thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Marcus shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Marcus says warmly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Builder) =>
            [
                "Marcus watches you work. \"Builder's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Marcus looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Marcus murmurs, \"Small kindnesses at the bench — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Marcus pours water and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Marcus nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Marcus shares a peaceful silence by the bench. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedWorkshopOften, _, LegacyArchetype.Builder) =>
            [
                "Marcus adds, \"Your workshop help — I think of it when the tools look tired. Builders show up.\"",
            ],
            (NpcMemoryType.HelpedWorkshopOften, _, _) =>
            [
                "Marcus smiles. \"The workshop remembers you — and so do I, every single day.\"",
            ],
            (NpcMemoryType.FrequentMarcusCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Marcus says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnMarcus, _, _) =>
            [
                "Marcus touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithMarcus, _, _) =>
            [
                "Marcus glances at you fondly. \"That quiet moment we shared — I still carry it between orders.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedMarcus, _, _) =>
            [
                "Marcus says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Marcus adds, \"I'm glad you came by — your presence steadies the workshop. I mean that personally.\"",
                "Marcus says warmly, \"Bloomtown's repairs feel safer when you're in the shop. Don't let anyone tell you otherwise.\"",
            ],
            (NpcMemoryType.HelpedWorkshopOften, _) =>
            [
                "Marcus squeezes your hand briefly. \"Seeing you here — the whole bench breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithMarcus, _) =>
            [
                "Marcus glances at you warmly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnMarcus, _) =>
            [
                "Marcus says plainly, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentMarcusCompanion, _) =>
            [
                "Marcus says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Marcus says warmly, \"I always appreciate when you stop at the workshop. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Builder) =>
            [
                "Marcus mentions, \"Morning's best for stocking timber — wood nodes east of the well refresh after dawn if you're building.\"",
                "Marcus shares, \"Tom's woodpile gets busy later — builders who show up early save themselves half a day's hauling.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Marcus says, \"Best time to notice what the village needs is when the lanes are still — folk speak softer then.\"",
                "Marcus adds, \"Craft plank before noon if you're stocking up — steady hands work better before the square fills.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Builder) =>
            [
                "Marcus leans in. \"Afternoons, the bridge site still gathers planks — builders know Harold posts updates near the well.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Marcus murmurs, \"The market hums after lunch — but the workshop stays useful if you need to think with your hands.\"",
                "Marcus says, \"Tom's yard gets busy later — steady folk show up before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Marcus shares, \"Evening's when the village exhales — rest then, or the body forgets why we work steadily.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Marcus shares, \"Good repairs aren't loud — they're showing up with quality materials before a project wilts.\"",
            ],
            (_, _) =>
            [
                "Marcus says, \"If weariness creeps in, work the bench or rest outdoors — honest craft steadies the mind.\"",
                "Marcus adds warmly, \"Village projects need timber and planks — the board by the well lists what matters most.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedWorkshopOften, RelationshipTier.CloseFriend) =>
            [
                "Marcus presses a tool and two planks into your hands. \"For the road — saved these knowing you'd put them to use.\"",
                "Marcus insists you sit by the bench. \"Rest before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedWorkshopOften, _) =>
            [
                "Marcus hands you wood and a plank. \"For the road — the workshop can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnMarcus, _) =>
            [
                "Marcus presses a small tool into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Marcus packs wood, a plank, and a tool for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Marcus nods. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Marcus adds, \"If coins are tight, Mira's fair to folk who tend the village honestly.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Marcus insists you sit and shares bread from his bench. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Marcus insists you sit a moment and offers wood. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Marcus packs a plank and a tool for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Marcus packs a small bundle of wood for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Marcus thanks you and sends you off with wood and a plank. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}