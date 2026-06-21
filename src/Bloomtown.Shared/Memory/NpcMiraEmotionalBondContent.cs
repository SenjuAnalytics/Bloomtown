using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Emotional bond dialogue and light benefits content for Mira — market-square warmth,
/// Connector archetype nuance, and memory-aware personal responses.
/// </summary>
internal static class NpcMiraEmotionalBondContent
{
    internal static string[] GetEmotionalInteractionLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (kind, memoryType, tier, archetype) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedMarketOften, RelationshipTier.CloseFriend, LegacyArchetype.Connector) =>
            [
                "You didn't just help at the market — you made the square feel like a place people belong. I notice.",
                "Every time you pitch in here, I think: that's what a connector looks like in practice.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedMarketOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first steadied the market stalls without fanfare. That stayed with me.",
                "The square runs smoother when you're around — and honestly, so do I.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedMarketOften, _, _) =>
            [
                "Your help at the market — it's not just chores. It tells me who shows up for Bloomtown.",
                "I notice when you lend a hand at the square. It's become one of my favorite habits to watch for.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMiraCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like catching up with someone I've known for years — not weeks.",
                "You've become part of how I picture the square. I don't say that to everyone.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMiraCompanion, _, LegacyArchetype.Connector) =>
            [
                "You visit often enough that vendors ask after you. Connectors leave traces like that.",
                "I'm always glad when you stop to talk — you've made the market feel less like trade and more like community.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.FrequentMiraCompanion, _, _) =>
            [
                "Good to see a familiar face — you've made the square feel warmer than it used to.",
                "You stop by often enough that I start wondering when you'll wander past. That's a good feeling.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.HelpedMarketOften, _, _) =>
            [
                "Oh, hello — seeing you always reminds me the market's lucky to have you.",
                "There you are! I was just thinking about your help at the square.",
            ],
            (NpcInteractionKind.Greet, NpcMemoryType.FrequentMiraCompanion, _, _) =>
            [
                "Hello, you — I was hoping I'd catch you at the square today.",
                "Perfect timing. The market feels brighter when you're nearby.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToMira, _, _) =>
            [
                "I still smile when I think of that gift — you know how to make someone feel seen.",
                "That present you gave me told me plenty about who you are.",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithMira, _, _) =>
            [
                "Those quiet pauses at the square — I treasure them more than busy days.",
                "I'm glad you choose to linger. Company without errands is a rare gift.",
            ],
            _ => [],
        };

    internal static string[] GetArchetypeEmotionalBondLines(LegacyArchetype archetype, RelationshipTier tier) =>
        (archetype, tier) switch
        {
            (LegacyArchetype.Connector, RelationshipTier.CloseFriend) =>
            [
                "You're the connector Bloomtown talks about — but you've connected with me personally, too. That matters.",
                "Everyone else sees you weaving the village together. I see you remembering one trader at a time.",
            ],
            (LegacyArchetype.Connector, _) =>
            [
                "Your connector's warmth isn't just gossip and introductions — you make people feel seen. I've felt it.",
                "Connectors build bridges. You've built one to me, and I don't take that lightly.",
            ],
            (LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "Caretakers tend gardens and wells — you've tended the square, and me along the way.",
            ],
            (LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's heart shows in how you show up here. The market feels it.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Builders raise projects — you've raised trust at the square, one steady day at a time.",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalPersonalMomentLines(NpcMemoryType memoryType, RelationshipTier tier) =>
        (memoryType, tier) switch
        {
            (NpcMemoryType.HelpedMarketOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the square wouldn't feel the same without you. I mean that personally.",
            ],
            (NpcMemoryType.HelpedMarketOften, _) =>
            [
                "Hey — I was just remembering your help at the market. It still warms me.",
            ],
            (NpcMemoryType.FrequentMiraCompanion, _) =>
            [
                "I feel like I know you properly now — not as a customer, but as someone I care about.",
                "You always brighten my day when you wander past the stalls. I notice when you're near.",
            ],
            _ => [],
        };

    internal static string[] GetCommunityHelpEmotionalLines(
        NpcMemoryType memoryType,
        LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedMarketOften, LegacyArchetype.Connector) =>
            [
                "Mira catches your eye. \"A connector's touch — the square feels it, and so do I.\"",
            ],
            (NpcMemoryType.HelpedMarketOften, _) =>
            [
                "Mira smiles warmly. \"I noticed you again at the market — thank you. It means more than upkeep.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Mira leans in. \"The market remembers connectors. I do, too.\"",
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
                "Mira squeezes your arm. \"Roots through connection — I felt it at the square before the village named it.\"",
            ],
            (PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Mira says brightly, \"Trusted neighbor — I've trusted you at the market for a while now. Glad the village caught up.\"",
            ],
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
            [
                "Mira looks moved. \"Your story's in Bloomtown now — and part of it's written in our square-side talks.\"",
            ],
            _ => [],
        };

    internal static string[] GetEmotionalAmbientLines(NpcMemoryType memoryType, LegacyArchetype archetype) =>
        (memoryType, archetype) switch
        {
            (NpcMemoryType.HelpedMarketOften, LegacyArchetype.Connector) =>
            [
                "Mira calls softly, \"The square's lucky — and so am I, having you nearby.\"",
            ],
            (NpcMemoryType.FrequentMiraCompanion, _) =>
            [
                "Mira waves. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcMemoryType.HelpedMarketOften, _) =>
            [
                "Mira nods your way. \"Good to see you. The market remembers steady help.\"",
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
                "Mira smiles. \"You always know when someone needs a kind word. I'm glad you stopped.\"",
                "Mira takes your hand briefly. \"Thank you for checking on me. It means more than you know.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, LegacyArchetype.Connector) =>
            [
                "Mira brightens. \"Checking on folk — that's the connector in you. I'm glad it reached me.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Mira laughs softly. \"Oh — you thought to ask how I'm doing. That's lovely of you.\"",
                "Mira nods warmly. \"I'm alright, truly. And better for seeing you check in.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Mira sits with you by the stalls. \"Moments like this — they're what make the square feel like home.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, LegacyArchetype.Connector) =>
            [
                "Mira shares a peaceful silence with you. \"Connectors need quiet too — thank you for the company.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Mira says softly, \"It's nice — just being here together, no haggling, no rush.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Connector) =>
            [
                "Mira watches you work. \"You help like someone who cares about every face in the square. I see it.\"",
            ],
            (EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Mira looks touched. \"You didn't have to help me with this — but I'm glad you did.\"",
                "Mira murmurs, \"Small kindnesses at the market — they stay with a person.\"",
            ],
            (EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Mira settles beside you at the stall. \"No trading, no rush — just company. I needed this.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Mira smiles. \"You chose to linger — connectors know that's a gift in itself.\"",
            ],
            (EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Mira sits with you quietly. \"Thank you for staying — the square's gentler with familiar company.\"",
            ],
            _ => [],
        };

    internal static string[] GetNpcRemembranceLines(
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype) =>
        (memoryType, tier, archetype) switch
        {
            (NpcMemoryType.HelpedMarketOften, _, LegacyArchetype.Connector) =>
            [
                "Mira adds, \"Your market help — I think of it when the stalls look tired. Connectors show up.\"",
            ],
            (NpcMemoryType.HelpedMarketOften, _, _) =>
            [
                "Mira smiles. \"The square remembers you — and so do I.\"",
            ],
            (NpcMemoryType.FrequentMiraCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Mira says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcMemoryType.CheckedOnMira, _, _) =>
            [
                "Mira touches your arm. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcMemoryType.SharedMomentWithMira, _, _) =>
            [
                "Mira glances at you. \"That quiet moment we shared — I still carry it.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedMira, _, LegacyArchetype.Connector) =>
            [
                "Mira murmurs, \"Connectors show up in small ways. You showed up for me.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedMira, _, _) =>
            [
                "Mira says warmly, \"When you helped me last time — that stayed with me.\"",
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
                "Mira adds softly, \"I'm glad you came by — your presence makes the square feel less lonely, and I mean that.\"",
                "Mira grins. \"Bloomtown's market feels warmer when you're in it. You've become someone I count on seeing.\"",
                "Mira meets your eyes. \"I remember the small ways you've shown up here. They add up to trust.\"",
            ],
            (NpcMemoryType.HelpedMarketOften, _) =>
            [
                "Mira touches your sleeve. \"Seeing you here — it's like the market and I both breathe easier. I haven't forgotten your help.\"",
                "Mira says warmly, \"Every time you help at the square, I feel less alone with the bustle. I remember each visit.\"",
            ],
            (NpcMemoryType.SharedMomentWithMira, _) =>
            [
                "Mira glances at you fondly. \"Moments like the one we shared — they stay with me when the stalls go quiet.\"",
            ],
            (NpcMemoryType.CheckedOnMira, _) =>
            [
                "Mira says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcMemoryType.ConsciouslyHelpedMira, _) =>
            [
                "Mira adds, \"When you offered hands-on help — that wasn't duty. I felt it, and I still do.\"",
            ],
            (NpcMemoryType.FrequentMiraCompanion, _) =>
            [
                "Mira says warmly, \"You stop by often enough that I notice when you don't — and I'm always glad when you do.\"",
            ],
            (_, _) =>
            [
                "Mira says warmly, \"I always appreciate when you stop by. Don't underestimate what that means to me.\"",
            ],
        };

    internal static string[] GetEmotionalInfoTips(GameTimeOfDay timeOfDay, LegacyArchetype archetype) =>
        (timeOfDay, archetype) switch
        {
            (GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Mira mentions, \"Morning's when vendors set up — connectors who greet folk early earn trust that lasts the week.\"",
                "Mira shares quietly, \"Harold's at the well at dawn most days. A friendly word there opens doors across the village.\"",
                "Mira leans in. \"Elsie's garden is peaceful before noon — good if you need calm, or if you want to meet her without the crowd.\"",
            ],
            (GameTimeOfDay.Morning, _) =>
            [
                "Mira says, \"Best bargains often appear early — before the crowd, if you're browsing or stocking up.\"",
                "Mira adds, \"Wood nodes east of the well refresh after dawn — gather before the square wakes if you're short on timber.\"",
            ],
            (GameTimeOfDay.Afternoon, LegacyArchetype.Connector) =>
            [
                "Mira leans in. \"Afternoons are lively — chat locals here if you want to meet half the village at once.\"",
                "Mira adds, \"I hear who's been lonely at the stalls lately — connectors notice, and it matters more than folk admit.\"",
                "Mira murmurs, \"Harold's well-side gets busy later — help well earns quiet thanks if you're passing.\"",
            ],
            (GameTimeOfDay.Afternoon, _) =>
            [
                "Mira murmurs, \"Tom trades near the square some days — he appreciates a friendly nod from regulars.\"",
                "Mira says, \"Bridge project east of the square still gathers planks sometimes — builders post updates on the community board.\"",
            ],
            (GameTimeOfDay.Evening, LegacyArchetype.Connector) =>
            [
                "Mira shares, \"Evenings here, I hear everything — connectors who linger learn who's struggling before anyone asks.\"",
                "Mira adds softly, \"Elsie checks the garden at dusk. She'd welcome company if your social need is climbing.\"",
            ],
            (_, LegacyArchetype.Connector) =>
            [
                "Mira shares, \"Elsie told me who helps at the garden — connectors like you should know she'd welcome you.\"",
                "Mira says, \"The community board lists who needs help — good gossip for someone who knows everyone.\"",
                "Mira murmurs, \"Warehouse project posts near the square when it needs hands — check before you wander off.\"",
            ],
            (_, _) =>
            [
                "Mira says, \"If energy's low, browse market then rest — I've watched newcomers forget to pause.\"",
                "Mira adds softly, \"Village projects post near the square — steady helpers get noticed here first.\"",
            ],
        };

    internal static string[] GetHelpfulFavorLines(
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier) =>
        (kind, memoryType, tier) switch
        {
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedMarketOften, RelationshipTier.CloseFriend) =>
            [
                "Mira slips you two apples from her stall. \"Vendor samples — saved these knowing you'd put them to good use.\"",
                "Mira says, \"If coins are tight today, mention my name to the herb seller. I'll vouch for you.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.HelpedMarketOften, _) =>
            [
                "Mira hands you an apple from her stall. \"For the road — nothing grand, just remembered.\"",
            ],
            (NpcInteractionKind.Talk, NpcMemoryType.CheckedOnMira, _) =>
            [
                "Mira presses a warm pastry into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (_, _, RelationshipTier.CloseFriend) =>
            [
                "Mira presses a warm pastry and a plank into your hands. \"Eat before you rush off — and take this wood for your next errand.\"",
            ],
            (_, _, _) =>
            [
                "Mira smiles. \"Take your time today — I'll send word if anyone at the square truly needs you.\"",
                "Mira adds, \"If loneliness creeps in, chat locals here — the square's gentler to familiar faces.\"",
            ],
        };

    internal static string[] GetBondingActionFavorLines(
        EmotionalBondActionKind action,
        RelationshipTier tier) =>
        (action, tier) switch
        {
            (EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Mira insists you sit, shares her tea, and sets two apples aside for you. \"You look after everyone — let someone look after you properly.\"",
            ],
            (EmotionalBondActionKind.CheckOn, _) =>
            [
                "Mira pours you tea from her stall and offers an apple. \"Drink. Small kindnesses should flow both ways.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Mira packs market treats and two apples for you. \"A keepsake from today — and something sweet and practical for the road.\"",
            ],
            (EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Mira shares a quiet corner of the square with you and leaves an apple within reach — the pause itself feels like a gift.",
            ],
            (EmotionalBondActionKind.HelpWith, _) =>
            [
                "Mira thanks you, hands you a plank, and points out who's struggling at the stalls today. \"If you want to help more — start there.\"",
            ],
            _ => [],
        };
}