using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue and light benefits for Tom — practical lumber-yard warmth,
/// Builder archetype nuance, and memory-aware personal responses.
/// </summary>
internal static class NpcTomEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedLumberOften, RelationshipTier.CloseFriend, LegacyArchetype.Builder) =>
            [
                "You didn't just stack wood — you made the yard feel like someone gives a damn. I notice.",
                "Every time you pitch in here, I think: that's what a builder's heart looks like in practice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedLumberOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first steadied the lumber piles without fanfare. That stayed with me.",
                "The yard runs smoother when you're around — and honestly, so do I.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedLumberOften, _, _) =>
            [
                "Your help at the lumber yard — it's not just chores. It tells me who shows up for Bloomtown.",
                "I notice when you lend a hand here. It's become one of the things I look for each day.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentTomCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like catching up with someone I've known for years — not weeks.",
                "You've become part of how I picture the yard. I don't say that to everyone.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentTomCompanion, _, LegacyArchetype.Builder) =>
            [
                "You visit often enough that folk ask after you. Builders leave traces like that.",
                "I'm always glad when you stop to talk — you've made the woodpile feel less like work and more like company.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentTomCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the lumber yard feel warmer than it used to.",
                "You stop by often enough I start wondering when you'll wander past. That's a good feeling.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedLumberOften, _, _) =>
            [
                "Oh — there you are. Seeing you always reminds me the yard's lucky to have you.",
                "There you are. I was just thinking about your help at the woodpile.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentTomCompanion, _, _) =>
            [
                "Hello — I was hoping I'd catch you near the yard today.",
                "Good timing. The woodpile feels steadier when you're nearby.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToTom, _, _) =>
            [
                "That gift you gave me — practical and thoughtful. I remember.",
                "You knew what I'd use. That says more than most words do.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithTom, _, _) =>
            [
                "Quiet time by the woodpile — I don't forget when someone stays.",
                "No task, no rush — just company. I appreciate that more than I say.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Builder, RelationshipTier.CloseFriend) =>
            [
                "You're the builder Bloomtown talks about — but you've built trust with me personally, too. That matters.",
                "Everyone else sees you raising projects. I see you remembering one woodpile at a time.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Your builder's steadiness isn't just timber and nails — you make people feel solid. I've felt it.",
                "Builders raise structures. You've raised something between us, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended the yard, and me along the way.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's heart shows in how you show up here. The yard feels it.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Connectors weave the village — you've woven yourself into my days here, one quiet visit at a time.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedLumberOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the yard wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedLumberOften, _) =>
            [
                "Hey — I was just remembering your help at the lumber yard. It still warms me.",
            ],
            (NpcMemoryType.FrequentTomCompanion, _) =>
            [
                "I feel like I know you properly now — not as a passerby, but as someone I care about.",
                "You always brighten my day when you wander past the woodpile. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedLumberOften, LegacyArchetype.Builder) =>
            [
                "Tom catches your eye. \"A builder's touch — the yard feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedLumberOften, _) =>
            [
                "Tom nods warmly. \"I noticed you again at the lumber yard — thank you. It means more than upkeep.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Tom leans on a stack. \"The yard remembers builders. I do, too.\"",
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
                "Tom grips your shoulder briefly. \"Roots through honest work — I felt it at the yard before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Tom says plainly, \"Trusted neighbor — I've trusted you at the woodpile for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
            [
                "Tom looks moved. \"Your story's in Bloomtown now — and part of it's written in our yard-side talks.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedLumberOften, LegacyArchetype.Builder) =>
            [
                "Tom calls softly, \"The yard's lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentTomCompanion, _) =>
            [
                "Tom waves. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedLumberOften, _) =>
            [
                "Tom nods your way. \"Good to see you. The woodpile remembers steady help.\"",
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
                "Tom meets your eyes. \"You always know when someone needs a steady word. I'm glad you stopped.\"",
                "Tom grips your shoulder briefly. \"Thank you for checking on me. It means more than I say.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Tom brightens slightly. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Tom nods. \"Oh — you thought to ask how I'm doing. That's good of you.\"",
                "Tom says quietly, \"I'm alright, truly. And better for seeing you check in.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Tom sits with you by the woodpile. \"Moments like this — they're what make hard work worth it.\"",
                "Tom laughs quietly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Builder) =>
            [
                "Tom shares a peaceful silence with you. \"Good timber needs time to season — so do good friendships.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Tom shares a quiet moment with you. \"Thank you for the company. I needed that.\"",
                "Tom says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Builder) =>
            [
                "Tom watches you work. \"Builder's hands — steady and sure. I notice.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Tom looks touched. \"You didn't have to help me with this — but I'm glad you did.\"",
                "Tom murmurs, \"Small kindnesses at the yard — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Tom sits with you by the woodpile. \"No work, no rush — just company. Good.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Builder) =>
            [
                "Tom nods. \"You stayed without a task — builders know trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Tom shares a peaceful silence. \"Glad you lingered. Means something.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedLumberOften, _, LegacyArchetype.Builder) =>
            [
                "Tom adds, \"Your yard help — I think of it when the stacks look tired. Builders show up.\"",
            ],
            (NpcMemoryType.HelpedLumberOften, _, _) =>
            [
                "Tom smiles faintly. \"The woodpile remembers you — and so do I.\"",
            ],
            (NpcMemoryType.FrequentTomCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Tom says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnTom, _, _) =>
            [
                "Tom touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithTom, _, _) =>
            [
                "Tom glances at you. \"That quiet moment we shared — I still carry it.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedTom, _, LegacyArchetype.Builder) =>
            [
                "Tom murmurs, \"Builders show up in small ways. You showed up for me.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedTom, _, _) =>
            [
                "Tom says warmly, \"When you helped me last time — that stayed with me.\"",
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
                "Tom adds quietly, \"I'm glad you came by — your presence makes the yard feel less lonely.\"",
                "Tom nods. \"Bloomtown's woodpile feels steadier when you're in it. I mean that personally.\"",
                "Tom meets your eyes. \"I remember the small ways you've shown up. They add up to trust.\"",
            ],
            (NpcMemoryType.HelpedLumberOften, _) =>
            [
                "Tom touches your sleeve. \"Seeing you here — it's like the yard and I both breathe easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithTom, _) =>
            [
                "Tom glances at you fondly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnTom, _) =>
            [
                "Tom says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedTom, _) =>
            [
                "Tom adds, \"When you offered hands-on help — that wasn't duty. I felt it, and I still do.\"",
            ],
            (NpcMemoryType.FrequentTomCompanion, _) =>
            [
                "Tom says warmly, \"You stop by often enough that I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Tom says warmly, \"I always appreciate when you stop by. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Builder) =>
            [
                "Tom mentions, \"Wood nodes east of the well refresh after dawn — gather early if you're stocking up for a project.\"",
                "Tom shares quietly, \"Morning's best for hauling timber — cool air, steady hands, fewer folk in the way.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Tom says, \"Best cutting's done early — before the heat makes honest work harder.\"",
                "Tom adds, \"Bridge project east of the square still gathers planks sometimes — check the board before you wander.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Builder) =>
            [
                "Tom leans on a stack. \"Afternoons, timber near the bridge site stacks higher — worth a look if you're building.\"",
                "Tom murmurs, \"Harold's well-side gets busy later — wood nodes there refresh after dawn, not now.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Tom says, \"Warehouse project posts near the community board when it needs planks — steady helpers get noticed.\"",
            ],
            (GameTimeOfDay.Evening, LegacyArchetype.Builder) =>
            [
                "Tom shares, \"Evening's when I finish fence work near the garden — builders notice what's still loose by dusk.\"",
            ],
            (_, LegacyArchetype.Builder) =>
            [
                "Tom shares, \"There's timber stacked near the bridge site — builders like you might spot what's needed next.\"",
                "Tom says, \"Elsie's fence could use a spare hand this week. She won't ask loudly, but she'd welcome you.\"",
            ],
            (_, _) =>
            [
                "Tom says, \"If fatigue's climbing, honest yard work or rest outdoors can reset a tired body.\"",
                "Tom adds softly, \"Village projects post near the square — steady hands find purpose there first.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedLumberOften, RelationshipTier.CloseFriend) =>
            [
                "Tom hands you two pieces of wood from his stores. \"For your next project — saved these knowing you'd use them well.\"",
                "Tom says, \"Paths near the yard are clear today. Start gathering from the east stacks — it'll go smoother.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedLumberOften, _) =>
            [
                "Tom hands you a piece of wood. \"For the road — nothing grand, just remembered.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnTom, _) =>
            [
                "Tom sets a plank within reach. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Tom presses wood and a plank into your hands. \"Take these before you rush off — you forget to stock up.\"",
            ],
            (_, _, _) =>
            [
                "Tom nods. \"Take your time today — I'll send word if anyone at the yard truly needs you.\"",
                "Tom adds, \"If coins are tight, mention my name when you trade lumber — I'll vouch for you.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Tom insists you sit and shares bread from his pack. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Tom offers you a seat and a piece of wood for your pack. \"Rest. Small kindnesses should flow both ways.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Tom packs two planks and wood into your hands. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Tom shares a quiet corner of the yard with you and leaves a plank within reach — the pause itself feels like a gift.",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Tom thanks you, hands you wood, and points toward the bridge site. \"If you want to help more — start there.\"",
            ],
            _ => [],
        };
}