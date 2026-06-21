using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Personal appreciation, relevant village tips, and meaningful favors from emotionally close focus NPCs.
/// Lines reference specific memories so NPCs feel like they truly remember the player.
/// </summary>
internal static class NpcEmotionalBondImpactDialogue
{
    internal static string[] GetPersonalAppreciationLines(
        uint npcEntityId,
        NpcMemoryType memoryType,
        RelationshipTier tier)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetPersonalAppreciationLines(memoryType, tier);

        return (npcEntityId, memoryType, tier) switch
        {
            (NpcEntityIds.Elsie, _, RelationshipTier.CloseFriend) =>
            [
                "Elsie adds softly, \"I'm glad you came by — your presence steadies me more than you know. Bloomtown feels warmer when you're in it.\"",
                "Elsie meets your eyes. \"You've become someone I count on seeing — that's not a small thing, and I won't pretend it is.\"",
                "Elsie smiles. \"I remember the small ways you've shown up. They add up to something I trust.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, _) =>
            [
                "Elsie touches your arm. \"Seeing you here — it's like the garden and I both breathe easier. The beds remember your hands.\"",
                "Elsie says warmly, \"Every time you help at the garden, I feel less alone with the work. I haven't forgotten a single visit.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.SharedMomentWithElsie, _) =>
            [
                "Elsie glances at you fondly. \"Moments like the one we shared — they stay with me when the day runs long.\"",
                "Elsie murmurs, \"That quiet spell we shared still warms me. I hope you know it mattered.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.CheckedOnElsie, _) =>
            [
                "Elsie says gently, \"You checked on me before — I carry that kindness. Not everyone bothers to look back.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.ConsciouslyHelpedElsie, _) =>
            [
                "Elsie adds, \"When you offered hands-on help — that wasn't duty. I felt it, and I still do.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.FrequentElsieCompanion, _) =>
            [
                "Elsie says warmly, \"You stop by often enough that I notice when you don't — and I'm always glad when you do.\"",
            ],
            (NpcEntityIds.Elsie, _, _) =>
            [
                "Elsie says warmly, \"I always appreciate when you stop by. Don't underestimate what that means to me.\"",
                "Elsie adds, \"You make this village feel less lonely for me — thank you for being here.\"",
            ],

            (NpcEntityIds.Harold, _, RelationshipTier.CloseFriend) =>
            [
                "Harold says quietly, \"Good to see you. Familiar company keeps a village honest — and me steady.\"",
                "Harold nods. \"Your visits matter to me. I don't hand that out lightly, and you know I don't.\"",
                "Harold speaks plainly, \"You're one of the faces I trust in Bloomtown. Glad it's you, every time.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _) =>
            [
                "Harold murmurs, \"You showing up at the well-side — it's the kind of reliability I notice and remember.\"",
                "Harold says, \"The well-side's steadier because of folk like you. I don't forget who keeps showing up.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.SharedMomentWithHarold, _) =>
            [
                "Harold adds quietly, \"That quiet spell we shared — still a comfort when the day runs long.\"",
                "Harold meets your gaze. \"I remember that still moment between us. It meant more than I said.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.CheckedOnHarold, _) =>
            [
                "Harold says, \"You checked on me when you didn't have to. I remember that — and I respect it.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.ConsciouslyHelpedHarold, _) =>
            [
                "Harold nods slowly. \"When you lent a hand personally — that told me plenty about who you are.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.FrequentHaroldCompanion, _) =>
            [
                "Harold tips his hat. \"You come by often enough I count on it. Good company is scarce — yours is worth having.\"",
            ],
            (NpcEntityIds.Harold, _, _) =>
            [
                "Harold tips his hat. \"Glad you're here. Means something, coming from me.\"",
                "Harold says, \"Good company is scarce. Yours is worth having around.\"",
            ],

            _ => [],
        };
    }

    internal static string[] GetEmotionalInfoTips(
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetEmotionalInfoTips(timeOfDay, archetype);

        return (npcEntityId, timeOfDay, archetype) switch
        {
            (NpcEntityIds.Elsie, GameTimeOfDay.Morning, LegacyArchetype.Caretaker) =>
            [
                "Elsie mentions, \"Morning's best for help garden — beds are dew-soft and Tom checks the fence at dawn if you want company.\"",
                "Elsie shares quietly, \"The community board by the garden lists what the village needs — caretakers who read it early get first pick of good work.\"",
            ],
            (NpcEntityIds.Elsie, GameTimeOfDay.Morning, LegacyArchetype.Connector) =>
            [
                "Elsie shares, \"Mira sets up the market before noon — connectors who greet vendors early earn trust that lasts the week.\"",
                "Elsie adds, \"Harold's at the well at dawn most days. A friendly word there opens doors across the village.\"",
            ],
            (NpcEntityIds.Elsie, GameTimeOfDay.Morning, _) =>
            [
                "Elsie mentions quietly, \"Mira says the market's gentler before noon — good if you need supplies without the crowd.\"",
                "Elsie adds, \"Wood nodes east of the well refresh after dawn — gather before the square wakes if you're stocking up.\"",
            ],
            (NpcEntityIds.Elsie, GameTimeOfDay.Afternoon, LegacyArchetype.Connector) =>
            [
                "Elsie says, \"Afternoons bring half the village to the square — if you want to meet folk, that's your window, not mine.\"",
                "Elsie murmurs, \"Mira told me who's been lonely at the market lately — connectors notice, and it matters.\"",
            ],
            (NpcEntityIds.Elsie, GameTimeOfDay.Afternoon, _) =>
            [
                "Elsie says, \"Afternoons bring more folk to the square — chat locals there if your social need is climbing.\"",
                "Elsie murmurs, \"Harold told me the well-side gets busy around now. Help well earns quiet thanks from him.\"",
            ],
            (NpcEntityIds.Elsie, _, LegacyArchetype.Connector) =>
            [
                "Elsie shares, \"Mira's been lively at the market — connectors like you might find good company and useful gossip there.\"",
                "Elsie adds, \"The village project board lists who needs help — worth a glance if you want to meet builders and caretakers alike.\"",
            ],
            (NpcEntityIds.Elsie, _, LegacyArchetype.Caretaker) =>
            [
                "Elsie shares, \"Tom's been tending the fence near the garden — he wouldn't mind company if you're passing.\"",
                "Elsie murmurs, \"If the well project still needs timber, Harold could use steady hands — caretakers understand showing up.\"",
            ],
            (NpcEntityIds.Elsie, _, _) =>
            [
                "Elsie says, \"If energy's low, rest outdoors or sleep at home — I've watched newcomers forget that.\"",
                "Elsie adds softly, \"The bridge project east of the square still gathers planks sometimes — builders post updates on the community board.\"",
            ],

            (NpcEntityIds.Harold, GameTimeOfDay.Evening, LegacyArchetype.Builder) =>
            [
                "Harold mentions, \"Evening's when Tom finishes fence work — builders notice what's still loose by dusk near the garden.\"",
                "Harold says, \"Timber near the bridge site stacks higher before nightfall — worth a look if you're building-minded.\"",
            ],
            (NpcEntityIds.Harold, GameTimeOfDay.Evening, LegacyArchetype.Connector) =>
            [
                "Harold murmurs, \"Evenings at the square, Mira hears everything — connectors who linger learn who's struggling before anyone asks.\"",
                "Harold says, \"Tom wraps up fence work around dusk. A nod from a familiar face means more than folk admit.\"",
            ],
            (NpcEntityIds.Harold, GameTimeOfDay.Evening, _) =>
            [
                "Harold mentions, \"Evenings by the well are quiet — good for clearing your head if the day's been heavy.\"",
                "Harold says, \"Tom wraps up fence work around dusk. He appreciates a nod if you're nearby.\"",
            ],
            (NpcEntityIds.Harold, GameTimeOfDay.Morning, LegacyArchetype.Builder) =>
            [
                "Harold shares, \"Wood nodes east of the well refresh after dawn — gatherers who know that save themselves half a day's walk.\"",
                "Harold murmurs, \"Bridge site needs planks more than praise — morning's when folk still have energy to haul.\"",
            ],
            (NpcEntityIds.Harold, GameTimeOfDay.Morning, _) =>
            [
                "Harold shares, \"Wood nodes east of the well refresh after dawn — gatherers who know that save themselves a walk.\"",
                "Harold murmurs, \"Morning help at the well sets the village's rhythm. Steady hands matter early.\"",
            ],
            (NpcEntityIds.Harold, _, LegacyArchetype.Builder) =>
            [
                "Harold shares, \"There's timber stacked near the bridge site — builders like you might spot what's needed next.\"",
                "Harold adds, \"Warehouse project posts near the community board when it needs hands — check before you wander.\"",
            ],
            (NpcEntityIds.Harold, _, LegacyArchetype.Connector) =>
            [
                "Harold says quietly, \"Mira trades best when the square's lively — afternoons, usually. She remembers who greets her.\"",
                "Harold murmurs, \"Elsie's garden could use a spare hand this week. She won't ask loudly, but she'd welcome you.\"",
            ],
            (NpcEntityIds.Harold, _, LegacyArchetype.Caretaker) =>
            [
                "Harold says quietly, \"Help garden lifts Elsie's spirits more than folk realize — caretakers understand that.\"",
                "Harold murmurs, \"The well-side and the garden both need steady hearts. You're the sort who shows up.\"",
            ],
            (NpcEntityIds.Harold, _, _) =>
            [
                "Harold murmurs, \"Mira trades best when the square's lively — afternoons, usually.\"",
                "Harold says quietly, \"If loneliness creeps in, talk with folk you trust before it hardens. I learned that here.\"",
            ],

            _ => [],
        };
    }

    internal static string[] GetHelpfulFavorLines(
        uint npcEntityId,
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetHelpfulFavorLines(kind, memoryType, tier);

        return (npcEntityId, kind, memoryType, tier) switch
        {
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.HelpedGardenOften, RelationshipTier.CloseFriend) =>
            [
                "Elsie hands you fresh herbs and two apples from the garden. \"For the road — saved these knowing you'd use them well.\"",
                "Elsie says firmly, \"Rest in the garden shade before you push on. I'll cover your absence if anyone asks.\"",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.HelpedGardenOften, _) =>
            [
                "Elsie hands you a small bundle of herbs and an apple. \"For the road — the garden can spare them, and you earned it.\"",
                "Elsie says, \"If your energy dips, tend plants or help garden — honest work restores more than you'd think.\"",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.CheckedOnElsie, _) =>
            [
                "Elsie presses a warm cup into your hands. \"You looked after me — let me return the favor, even small.\"",
            ],
            (NpcEntityIds.Elsie, _, _, RelationshipTier.CloseFriend) =>
            [
                "Elsie presses a warm cup, bread, and a plank into your hands. \"Eat, drink, and take this wood — you've been giving more than you take.\"",
                "Elsie smiles. \"I'll tell Mira you're in good spirits when I see her — that kind of word helps at the market.\"",
            ],
            (NpcEntityIds.Elsie, _, _, _) =>
            [
                "Elsie smiles. \"Take your time today — I'll keep an eye out and send word if someone truly needs you.\"",
                "Elsie adds, \"If coins are tight, Mira's fair to friends of mine. Mention the garden when you browse.\"",
            ],

            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.HelpedWellOften, RelationshipTier.CloseFriend) =>
            [
                "Harold fills your pack with two pieces of wood from the well-side stores. \"Take extra — long days need more than pride admits.\"",
                "Harold says, \"Paths near the well are clear today. Start gathering there — your work'll go smoother.\"",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.HelpedWellOften, _) =>
            [
                "Harold hands you a piece of wood from the well-side stores. \"For your next project — small thing, but useful.\"",
                "Harold offers you a seat by the well. \"Catch your breath — steady work needs steady rest.\"",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.SharedMomentWithHarold, _) =>
            [
                "Harold shares a plank from his stores. \"For the road — quiet moments like ours deserve something practical too.\"",
            ],
            (NpcEntityIds.Harold, _, _, RelationshipTier.CloseFriend) =>
            [
                "Harold shares bread, dried fruit, and wood from his pack. \"Eat and take this — you've earned more than hard work today.\"",
                "Harold nods toward the well. \"Rest here as long as you need. I'll say nothing about it.\"",
            ],
            (NpcEntityIds.Harold, _, _, _) =>
            [
                "Harold nods. \"I'll mention your name if folk ask who's reliable — you deserve the credit.\"",
                "Harold says, \"If fatigue's climbing, help well or rest outdoors. Humble chores can reset a tired body.\"",
            ],

            _ => [],
        };
    }

    internal static string[] GetBondingActionFavorLines(
        uint npcEntityId,
        EmotionalBondActionKind action,
        RelationshipTier tier)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetBondingActionFavorLines(action, tier);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetBondingActionFavorLines(action, tier);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetBondingActionFavorLines(action, tier);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetBondingActionFavorLines(action, tier);

        return (npcEntityId, action, tier) switch
        {
            (NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Elsie insists you sit and shares her lunch with an apple from the garden. \"You look after everyone — let someone look after you properly.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn, _) =>
            [
                "Elsie insists you sit a moment and offers bread. \"You look after everyone — let someone look after you.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Elsie packs a posy, pressed herbs, and two apples for you. \"A keepsake from today — and something practical for the road.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Elsie packs a small posy and an apple for you. \"A keepsake from today — nothing grand, just remembered.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.HelpWith, _) =>
            [
                "Elsie thanks you and sends you off with a full waterskin and a plank. \"Your hands help — take this so you don't run dry or empty-handed.\"",
            ],

            (NpcEntityIds.Harold, EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend) =>
            [
                "Harold pours you water, shares his bench, and sets wood aside for you. \"Drink. Rest. Small kindnesses should flow both ways.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.CheckOn, _) =>
            [
                "Harold pours you water from the well and offers a piece of wood. \"Drink. Small kindnesses should flow both ways.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend) =>
            [
                "Harold shares his bench, points out a quiet gathering spot, and hands you two planks. \"Peace when you need it — and materials for when you're ready.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment, _) =>
            [
                "Harold shares his bench without a word and leaves a plank within reach — the quiet itself feels like a gift.",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.HelpWith, _) =>
            [
                "Harold says firmly, \"Don't forget to rest after helping — and take this bread and wood. I'll notice if you don't eat.\"",
            ],

            _ => [],
        };
    }
}