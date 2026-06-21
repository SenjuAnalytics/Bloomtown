using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Nora — calm herbalist who values patience, plants, and village balance.
/// </summary>
internal static class NpcNoraEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedHerbGardenOften, RelationshipTier.CloseFriend, LegacyArchetype.Caretaker) =>
            [
                "You tend the herbs with real care — not hurry. The garden feels steadier when you're among the beds.",
                "Caretakers understand slow work. You've helped Bloomtown breathe easier, one row at a time.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedHerbGardenOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first helped without being asked. The herbs remember too — they lean toward your hands.",
                "The herb garden hums differently when you're here. Quieter, but healthier. I notice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedHerbGardenOften, _, _) =>
            [
                "Every time you help among the herbs, I feel a little less alone with the tending.",
                "Folk say the village is lucky to have you near the garden. I agree, softly.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentNoraCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like sitting in shade after long sun — restful, and earned.",
                "You've become part of my days here. I save the quiet corner of the garden when I hope you'll visit.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentNoraCompanion, _, LegacyArchetype.Caretaker) =>
            [
                "You visit often enough that the beds seem to expect you. Caretakers leave traces like that.",
                "I'm always glad when you stop among the herbs — you've made this patch feel less solitary.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentNoraCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the herb garden feel like shared ground, not just mine.",
                "You drop by often enough I notice the hush when you're not nearby. That's a good kind of missing.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedHerbGardenOften, _, _) =>
            [
                "Oh — there you are. The herbs looked brighter this morning. I wondered if you'd come.",
                "Hello. Seeing you always reminds me how gently Bloomtown can grow when folk show up.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentNoraCompanion, _, _) =>
            [
                "Hello. I was hoping you'd wander through today — the garden's quieter when you're away.",
                "There you are. I saved a moment of shade if you'd like it.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToNora, _, _) =>
            [
                "I still think about that gift — you have a way of noticing what folk need before they say it.",
                "Every time you stop to talk, I remember that gesture. It meant more than polite manners.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithNora, _, _) =>
            [
                "Those quiet spells among the herbs — no tasks, no rush — they stay with me between harvests.",
                "I'm always glad when you choose to linger. Presence is its own medicine.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about steady care — but you've tended me personally, among the herbs. That matters.",
                "Caretakers heal more than beds and wells. You've healed something quiet in me, visit by visit.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's patience shows in how you show up here. The garden feels it, and so do I.",
            ],
            (LegacyArchetype.Connector, RelationshipTier.CloseFriend) =>
            [
                "Connectors weave folk together — you've woven yourself into this garden, one gentle word at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your warmth isn't loud — but folk feel it. I've felt it in our talks among the leaves.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Builders raise structures — you've raised something steady between us. I don't take that lightly.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedHerbGardenOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the herb rows wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedHerbGardenOften, _) =>
            [
                "Hey — I was just remembering your help among the herbs. It still steadies me.",
            ],
            (NpcMemoryType.FrequentNoraCompanion, _) =>
            [
                "I feel like I know you properly now — not as a passerby, but as someone I trust among the leaves.",
                "You always soften my day when you wander through. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedHerbGardenOften, LegacyArchetype.Caretaker) =>
            [
                "Nora meets your gaze quietly. \"A caretaker's touch — the herbs feel it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedHerbGardenOften, _) =>
            [
                "Nora nods. \"I noticed you again among the beds — thank you. It means more than tidy rows.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Nora murmurs, \"The garden remembers caretakers. I do, too.\"",
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
                "Nora smiles softly. \"Roots through care — I felt it in the herb beds long before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Nora says gently, \"Trusted neighbor — I've trusted your hands among the herbs for a while now.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
            [
                "Nora looks moved. \"Your story's in Bloomtown now — and part of it's written in quiet garden hours.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedHerbGardenOften, LegacyArchetype.Caretaker) =>
            [
                "Nora calls softly, \"The herbs are lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentNoraCompanion, _) =>
            [
                "Nora waves from the beds. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedHerbGardenOften, _) =>
            [
                "Nora nods your way. \"Good to see you. The garden remembers steady help.\"",
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
                "Nora takes your hands briefly. \"You always know when someone needs a steady word. I'm glad you stopped.\"",
                "Nora says quietly, \"Thank you for checking on me. It means more than I say aloud.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Nora brightens slightly. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it here.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Nora smiles. \"Oh — you thought to ask how I'm doing. That's lovely of you.\"",
                "Nora nods. \"I'm alright. And better for seeing you check in — stay a moment if you can.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Nora sits with you among the herbs. \"Moments like this — they're what keep a village balanced.\"",
                "Nora laughs softly. \"I won't forget this little pause we took together.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Caretaker) =>
            [
                "Nora shares a peaceful silence with you. \"Good company needs time to steep — like tea. Thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Nora shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit.\"",
                "Nora says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Nora watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Nora looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Nora murmurs, \"Small kindnesses among the herbs — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Nora pours tea and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Nora nods. \"You stayed without a task — trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Nora shares a peaceful silence among the herbs. \"Glad you lingered. Means something to me.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedHerbGardenOften, _, LegacyArchetype.Caretaker) =>
            [
                "Nora adds, \"Your herb help — I think of it when the rows look tired. Caretakers show up.\"",
            ],
            (NpcMemoryType.HelpedHerbGardenOften, _, _) =>
            [
                "Nora smiles. \"The garden remembers you — and so do I, every single day.\"",
            ],
            (NpcMemoryType.FrequentNoraCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Nora says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnNora, _, _) =>
            [
                "Nora touches your arm briefly. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithNora, _, _) =>
            [
                "Nora glances at you fondly. \"That quiet moment we shared — I still carry it between harvests.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedNora, _, _) =>
            [
                "Nora says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Nora adds, \"I'm glad you came by — your presence steadies the garden. I mean that personally.\"",
                "Nora says softly, \"Bloomtown's herbs feel safer when you're in them. Don't let anyone tell you otherwise.\"",
            ],
            (NpcMemoryType.HelpedHerbGardenOften, _) =>
            [
                "Nora squeezes your hand gently. \"Seeing you here — the whole patch breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithNora, _) =>
            [
                "Nora glances at you warmly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnNora, _) =>
            [
                "Nora says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentNoraCompanion, _) =>
            [
                "Nora says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Nora says warmly, \"I always appreciate when you stop among the herbs. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Caretaker) =>
            [
                "Nora mentions quietly, \"Morning dew holds the herbs' strength — gather or tend before the sun climbs too high.\"",
                "Nora shares, \"Elsie's beds and mine share the same breeze at dawn. Caretakers who walk both patches learn the village's rhythm.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Nora says, \"Best time to notice what the village needs is when the lanes are still — folk speak softer then.\"",
                "Nora adds, \"Wood east of the well refreshes after dawn if you're stocking up for garden work.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Caretaker) =>
            [
                "Nora leans in. \"Afternoons, Harold could use a steady hand at the well — caretakers know he won't ask loudly.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Nora murmurs, \"The market hums after lunch — but the herb rows stay peaceful if you need to think.\"",
                "Nora says, \"Tom's woodpile gets busy later — steady folk show up before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Nora shares, \"Evening's when the village exhales — rest then, or the body forgets why we tend things slowly.\"",
            ],
            (_, LegacyArchetype.Caretaker) =>
            [
                "Nora shares, \"Balance isn't loud — it's showing up for plants and people before either wilts.\"",
            ],
            (_, _) =>
            [
                "Nora says, \"If weariness creeps in, walk the herb rows before the square fills — green steadiness helps.\"",
                "Nora adds softly, \"Village projects need patient hands — the board by the garden lists what matters most.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedHerbGardenOften, RelationshipTier.CloseFriend) =>
            [
                "Nora presses two apples and a bundle of leaves into your hands. \"For the road — saved these knowing you'd forget to eat.\"",
                "Nora insists you sit in the shade. \"Rest among the herbs before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedHerbGardenOften, _) =>
            [
                "Nora hands you an apple and a small bundle of leaves. \"For the road — the garden can spare them, and you earned it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnNora, _) =>
            [
                "Nora presses a warm cup into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Nora packs bread, an apple, and a bit of wood for your hands. \"Take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Nora smiles. \"Take your time today — I'll send word if someone truly needs you.\"",
                "Nora adds, \"If coins are tight, Mira's fair to folk who tend the village gently.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Nora insists you sit and shares her lunch with an apple from the garden. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Nora insists you sit a moment and offers bread. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Nora packs pressed leaves and two apples for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Nora packs a small bundle and an apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Nora thanks you and sends you off with a warm cup and an apple. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}