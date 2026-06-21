using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue for Greta — warm, chatty innkeeper who fusses fondly over regular guests.
/// </summary>
internal static class NpcGretaEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedInnOften, RelationshipTier.CloseFriend, LegacyArchetype.Caretaker) =>
            [
                "You didn't just tidy the inn — you made it feel like a home people want to return to. I notice every time.",
                "Caretakers like you keep a village breathing. My parlor's warmer because you keep showing up.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedInnOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first pitched in without being asked. Regular guests like you are rare treasure.",
                "The inn hums differently when you're here — not just busier, but cared for. Thank you for that.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedInnOften, _, _) =>
            [
                "Every time you help at the inn, I tell someone in the kitchen — in the best way, I mean.",
                "Folk say Bloomtown's lucky to have you around the parlor. I started that rumor, if I'm honest.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGretaCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like catching up with family — and I don't hand that word out lightly.",
                "You've become part of my days here. I catch myself setting out your usual corner before you arrive.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGretaCompanion, _, LegacyArchetype.Connector) =>
            [
                "You visit often enough that travelers ask after you by name. Connectors leave traces like that.",
                "I'm always glad when you stop to talk — you've made the inn feel like the heart of the village.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentGretaCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the inn feel less like a stopover and more like yours.",
                "You drop by often enough I start wondering when you'll wander through the door. That's a good feeling.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedInnOften, _, _) =>
            [
                "Oh, there you are! I was just telling the kitchen how lucky we are when you help out.",
                "Hello, dear — seeing you always reminds me the inn's in good hands.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentGretaCompanion, _, _) =>
            [
                "Hello, you — I was hoping you'd come by today. Sit if you've a moment; I'll fuss over you properly.",
                "Oh, perfect timing. The parlor feels brighter when you're nearby.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToGreta, _, _) =>
            [
                "I still think about that gift you brought — you have a way of making a busy innkeeper feel seen.",
                "Every time you stop to talk, I remember that present. It meant more than polite manners.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithGreta, _, _) =>
            [
                "Those quiet moments by the hearth — no rush, no chores — they stay with me between supper rushes.",
                "I'm always glad when you choose to linger. Presence is its own kindness at an inn.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Connector, RelationshipTier.CloseFriend) =>
            [
                "Bloomtown talks about who holds the village together — but you've held a place at my hearth personally. That matters.",
                "Connectors weave folk together. You've woven yourself into my inn, one warm visit at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your connector's warmth isn't just gossip and introductions — you make rooms feel inhabited. I've felt it.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended my guests' spirits, and mine along the way.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's heart shows in how you show up here. The inn feels it, and so do I.",
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
            (NpcMemoryType.HelpedInnOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the parlor wouldn't feel the same without you. I mean that personally, not just professionally.",
            ],
            (NpcMemoryType.HelpedInnOften, _) =>
            [
                "Hey — I was just remembering your help at the inn. It still warms me more than a good fire.",
            ],
            (NpcMemoryType.FrequentGretaCompanion, _) =>
            [
                "I feel like I know you properly now — not as a traveler passing through, but as someone I care about.",
                "You always brighten my day when you wander in. I notice when you're near, believe me.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedInnOften, LegacyArchetype.Caretaker) =>
            [
                "Greta catches your eye. \"A caretaker's touch — the inn feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedInnOften, _) =>
            [
                "Greta beams. \"I noticed you again at the inn — thank you. It means more than tidy tables.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Greta leans on the counter. \"The parlor remembers who makes folk feel welcome. That's you.\"",
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
                "Greta squeezes your hand. \"Roots through belonging — I felt it at my hearth before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Greta says warmly, \"Trusted neighbor — I've trusted you at my table for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Greta looks moved. \"Your story's in Bloomtown now — and part of it's written in our parlor-side talks.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedInnOften, LegacyArchetype.Caretaker) =>
            [
                "Greta calls from the doorway, \"The inn's lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentGretaCompanion, _) =>
            [
                "Greta waves. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedInnOften, _) =>
            [
                "Greta nods your way. \"Good to see you. The parlor remembers steady help.\"",
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
                "Greta takes your hands briefly. \"You always know when someone needs a kind word. I'm glad you stopped — now sit, you look tired.\"",
                "Greta fusses over you fondly. \"Thank you for checking on me. It means more than I let on between supper rushes.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Caretaker) =>
            [
                "Greta brightens. \"Checking on folk — that's a caretaker's habit. I'm glad you brought it to my door.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Greta smiles. \"Oh — you thought to ask how I'm doing. That's lovely of you, truly.\"",
                "Greta says warmly, \"I'm alright, dear. And better for seeing you check in — don't rush off yet.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Greta sits with you by the hearth. \"Moments like this — they're what make an inn feel like home.\"",
                "Greta laughs softly. \"I won't forget this little pause we took together. I'll probably mention it twice.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Greta shares a peaceful silence with you. \"Good company needs time to steep — like tea. Thank you for staying.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Greta shares a quiet moment with you. \"Thank you for the company. I needed that more than I'd admit aloud.\"",
                "Greta says softly, \"It's nice — just being here together, no chores, no rush. More of this, please.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Greta watches you work. \"Caretaker's hands — I see why folk trust you around Bloomtown.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Greta looks touched. \"You didn't have to help me with this — but I'm glad you did. I'll remember.\"",
                "Greta murmurs, \"Small kindnesses at the inn — they stay with a person. You've left plenty.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Greta pours tea and sits with you. \"No errands, no rush — just company. This is the good part of my day.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Greta nods. \"You stayed without a task — connectors know trust grows in quiet, too.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Greta shares a peaceful silence by the hearth. \"Glad you lingered. Means something to an innkeeper.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedInnOften, _, LegacyArchetype.Caretaker) =>
            [
                "Greta adds, \"Your inn help — I think of it when the parlor looks tired. Caretakers show up.\"",
            ],
            (NpcMemoryType.HelpedInnOften, _, _) =>
            [
                "Greta smiles. \"The hearth remembers you — and so do I, every single day.\"",
            ],
            (NpcMemoryType.FrequentGretaCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Greta says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnGreta, _, _) =>
            [
                "Greta touches your arm briefly. \"You checked on me before — I haven't forgotten. Not likely to, either.\"",
            ],
            (NpcMemoryType.SharedMomentWithGreta, _, _) =>
            [
                "Greta glances at you fondly. \"That quiet moment we shared — I still carry it between rushes.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedGreta, _, _) =>
            [
                "Greta says warmly, \"When you helped me last time — that wasn't duty. I felt it, and I still do.\"",
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
                "Greta adds, \"I'm glad you came by — your presence makes the inn feel less empty. I mean that personally.\"",
                "Greta fusses fondly. \"Bloomtown's parlor feels steadier when you're in it. Don't let anyone tell you otherwise.\"",
            ],
            (NpcMemoryType.HelpedInnOften, _) =>
            [
                "Greta squeezes your hand. \"Seeing you here — the whole inn breathes easier. I haven't forgotten your help.\"",
            ],
            (NpcMemoryType.SharedMomentWithGreta, _) =>
            [
                "Greta glances at you warmly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
            ],
            (NpcMemoryType.CheckedOnGreta, _) =>
            [
                "Greta says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.FrequentGretaCompanion, _) =>
            [
                "Greta says warmly, \"You stop by often enough I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Greta says warmly, \"I always appreciate when you stop by. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Greta mentions, \"Morning travelers gossip over porridge — Mira's square gets lively by mid-afternoon if you want introductions.\"",
                "Greta shares, \"Elsie's garden crew passes through early. A friendly hello there goes further than folk admit.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Greta says, \"Best time to hear village news is over morning tea — folk talk when their hands are warm.\"",
                "Greta adds, \"Harold tends the well-side after dawn. Regular faces get the useful rumors first.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Caretaker) =>
            [
                "Greta leans in. \"Afternoons, Elsie could use a spare hand at the garden — caretakers know she won't ask loudly.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Greta murmurs, \"Market square hums after lunch — Mira remembers who greets her by name.\"",
                "Greta says, \"Tom's woodpile gets busy later — but he trusts folk who show up before the rush.\"",
            ],
            (GameTimeOfDay.Evening, _) =>
            [
                "Greta shares, \"Evening's when travelers swap stories — listen near the hearth if you want to know who's new in town.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Greta shares, \"Folk linger longer when someone makes them feel seen — you've got that gift, use it at the square.\"",
            ],
            (_, _) =>
            [
                "Greta says, \"If loneliness creeps in, talk with folk you trust before it hardens. The inn's good for that.\"",
                "Greta adds softly, \"Village projects post near the square — steady helpers find purpose there first.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedInnOften, RelationshipTier.CloseFriend) =>
            [
                "Greta presses two apples and bread into your hands. \"For the road — saved these knowing you'd forget to eat.\"",
                "Greta insists you sit. \"Rest in the parlor before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedInnOften, _) =>
            [
                "Greta hands you an apple and a warm cup. \"For the road — you earned it, and you look like you need it.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnGreta, _) =>
            [
                "Greta presses a warm cup into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Greta packs bread, an apple, and a plank into your hands. \"Eat, drink, and take this — you've been giving more than you take.\"",
            ],
            (_, _, _) =>
            [
                "Greta smiles. \"Take your time today — I'll keep an eye out and send word if someone truly needs you.\"",
                "Greta adds, \"If coins are tight, mention my name at the market. Mira's fair to friends of mine.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Greta insists you sit and shares her lunch with an apple from the kitchen. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Greta insists you sit a moment and offers bread. \"You look after everyone — let someone look after you.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Greta packs pressed herbs and two apples for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Greta packs a small bundle and an apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Greta thanks you and sends you off with a warm cup and an apple. \"Your hands help — take this so you don't run on empty.\"",
            ],
            _ => [],
        };
}