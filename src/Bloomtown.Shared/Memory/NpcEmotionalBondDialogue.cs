using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Warm, memory-aware emotional dialogue for focus NPCs — personal bonds, not just functional praise.
/// </summary>
internal static class NpcEmotionalBondDialogue
{
    internal static string[] GetEmotionalInteractionLines(
        uint npcEntityId,
        NpcInteractionKind kind,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetEmotionalInteractionLines(kind, memoryType, tier, archetype);

        return (npcEntityId, kind, memoryType, tier, archetype) switch
        {
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.HelpedGardenOften, RelationshipTier.CloseFriend, _) =>
            [
                "I still remember when you first showed up at the garden without anyone asking. That stays with me.",
                "The garden feels different when you're there — not just tended, but cared for. I hope you know that.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.HelpedGardenOften, _, _) =>
            [
                "Every time you help at the garden, I feel a little less alone in keeping Bloomtown warm.",
                "I notice when you're at the garden — it's become one of the ways I know you're really here.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.FrequentElsieCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Talking with you feels like talking with family now — I didn't expect that when you first arrived.",
                "You've become part of my days here. I don't say that lightly.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.FrequentElsieCompanion, _, _) =>
            [
                "I'm always glad when you stop to talk — you've made Bloomtown feel less like a place and more like home.",
                "You visit often enough that I start wondering when you're coming by. That's a good feeling.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Greet, NpcMemoryType.HelpedGardenOften, _, _) =>
            [
                "Oh, hello — seeing you always reminds me how lucky the garden is to have you.",
                "There you are! I was just thinking about your help at the garden.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Greet, NpcMemoryType.FrequentElsieCompanion, _, _) =>
            [
                "Hello, you — I was hoping I'd see you today.",
                "Oh, perfect timing. I always breathe easier when you're nearby.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToElsie, _, _) =>
            [
                "I still think about that gift you brought me — you have a way of making people feel seen.",
                "Every time you stop to talk, I think of that present. It meant more than you know.",
            ],
            (NpcEntityIds.Elsie, NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithElsie, _, _) =>
            [
                "Those quiet moments we share — no chores, no rush — they stay with me.",
                "I'm always glad when you choose to linger. Presence is its own kindness.",
            ],

            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.HelpedWellOften, RelationshipTier.CloseFriend, _) =>
            [
                "You've kept the well-side steady more times than I can count. I don't forget who does that.",
                "When the well needs tending, I think of you first now. That's trust, plain and simple.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.HelpedWellOften, _, _) =>
            [
                "Your help at the well — humble work, but it means the village keeps its rhythm. Thank you.",
                "I notice when you tend the well. Small chores, but they tell me who I can rely on.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.FrequentHaroldCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "You've become someone I look forward to seeing. Bloomtown needs more of that.",
                "I trust you more than most newcomers earn in a season. You've earned it.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.FrequentHaroldCompanion, _, _) =>
            [
                "Good to talk with you again — you've got a steady way about you that I appreciate.",
                "You stop by often enough that I consider you part of how this village works.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Greet, NpcMemoryType.HelpedWellOften, _, _) =>
            [
                "Hello — good to see the person who keeps our well-side honest.",
                "Ah, there you are. The well's always a little brighter when you've been by.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Greet, NpcMemoryType.FrequentHaroldCompanion, _, _) =>
            [
                "Hello, friend — I was just thinking I hadn't seen you yet today.",
                "Good to see you. Familiar faces keep a village honest.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.GaveFavoriteGiftToHarold, _, _) =>
            [
                "That gift you gave me — I still carry the kindness of it.",
                "You remembered what I like. I don't forget gestures like that.",
            ],
            (NpcEntityIds.Harold, NpcInteractionKind.Talk, NpcMemoryType.SpentQuietTimeWithHarold, _, _) =>
            [
                "Quiet company by the well — I remember when you choose to stay.",
                "Those unhurried moments mean more than folk admit. I'm glad you linger.",
            ],

            _ => [],
        };
    }

    internal static string[] GetArchetypeEmotionalBondLines(
        uint npcEntityId,
        LegacyArchetype archetype,
        RelationshipTier tier)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetArchetypeEmotionalBondLines(archetype, tier);

        return (npcEntityId, archetype, tier) switch
        {
            (NpcEntityIds.Elsie, LegacyArchetype.Caretaker, RelationshipTier.CloseFriend) =>
            [
                "You're a caretaker to all of Bloomtown — but you've been one to me, too. That matters.",
                "When folk call you steady help, I smile. I've felt it myself.",
            ],
            (NpcEntityIds.Elsie, LegacyArchetype.Caretaker, _) =>
            [
                "Your caretaker's heart shows in how you show up — I've felt it in our talks.",
                "Bloomtown sees a caretaker in you. I see it in the small things you do for people like me.",
            ],
            (NpcEntityIds.Elsie, LegacyArchetype.Connector, _) =>
            [
                "You connect everyone else — but you make sure I feel included, too. That's rare.",
                "Your connector's warmth isn't just for the square. I'm glad it reaches me.",
            ],
            (NpcEntityIds.Elsie, LegacyArchetype.Builder, _) =>
            [
                "You build things for the village — and somehow you've built trust with me along the way.",
                "A builder's hands on shared work — I've watched you, and I trust what you're raising here.",
            ],

            (NpcEntityIds.Harold, LegacyArchetype.Builder, RelationshipTier.CloseFriend) =>
            [
                "Every project you touch — I read it as you building faith in this place. Including with me.",
                "You're a builder to Bloomtown, but you've built something steady between us, too.",
            ],
            (NpcEntityIds.Harold, LegacyArchetype.Builder, _) =>
            [
                "Builders leave timber and stone — you've left me believing this village has a future.",
                "I see the builder in you at every site. I see the same care when you stop to talk.",
            ],
            (NpcEntityIds.Harold, LegacyArchetype.Caretaker, _) =>
            [
                "Caretakers keep the village's daily rhythm — you've kept mine steadier than you know.",
                "Your caretaker's touch at the well — I connect it to how you treat people. Same steady heart.",
            ],
            (NpcEntityIds.Harold, LegacyArchetype.Connector, _) =>
            [
                "You know everyone's name — but you remember mine like it matters. It does.",
                "Connectors weave the village together. You've woven me in, and I'm grateful.",
            ],

            _ => [],
        };
    }

    internal static string[] GetEmotionalPersonalMomentLines(
        uint npcEntityId,
        NpcMemoryType memoryType,
        RelationshipTier tier)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetEmotionalPersonalMomentLines(memoryType, tier);

        return (npcEntityId, memoryType, tier) switch
        {
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, RelationshipTier.CloseFriend) =>
            [
                "I was just thinking — the garden wouldn't feel the same without you. I mean that personally.",
                "Funny thing: I told myself I'd thank you properly today. So... thank you. Truly.",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, _) =>
            [
                "Hey — I was just remembering your help at the garden. It still warms me.",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.FrequentElsieCompanion, _) =>
            [
                "I feel like I know you properly now — not as a visitor, but as someone I care about.",
                "You always brighten my day when you wander past. I notice when you're near.",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, RelationshipTier.CloseFriend) =>
            [
                "I don't say this often — but I'm glad you're in Bloomtown. The well work is proof, but so are you.",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _) =>
            [
                "Thanks again for the well-side help. I don't say it enough, and I should.",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.FrequentHaroldCompanion, _) =>
            [
                "Good to see you nearby — familiar footsteps are a comfort around here.",
            ],
            _ => [],
        };
    }

    internal static string[] GetCommunityHelpEmotionalLines(
        uint npcEntityId,
        NpcMemoryType memoryType,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetCommunityHelpEmotionalLines(memoryType, archetype);

        return (npcEntityId, memoryType, archetype) switch
        {
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, LegacyArchetype.Caretaker) =>
            [
                "Elsie catches your eye. \"A caretaker's touch — the garden feels it, and so do I.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, _) =>
            [
                "Elsie smiles warmly. \"I noticed you again — thank you. It means more than upkeep.\"",
            ],
            (NpcEntityIds.Elsie, _, LegacyArchetype.Caretaker) =>
            [
                "Elsie touches your arm gently. \"The garden remembers caretakers. I do, too.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, LegacyArchetype.Caretaker) =>
            [
                "Harold nods quietly. \"Caretaker's work at the well — humble, and I don't forget it.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _) =>
            [
                "Harold murmurs, \"Good hands at the well today. I appreciate you.\"",
            ],
            (NpcEntityIds.Harold, _, LegacyArchetype.Builder) =>
            [
                "Harold tips his hat. \"Steady help here, too — builders show up everywhere, don't they.\"",
            ],
            _ => [],
        };
    }

    internal static string[] GetEmotionalMilestoneLines(
        uint npcEntityId,
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetEmotionalMilestoneLines(milestone, archetype);

        return (npcEntityId, milestone, archetype) switch
        {
            (NpcEntityIds.Elsie, PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Caretaker) =>
            [
                "Elsie squeezes your hand briefly. \"Roots through care — I felt it before the village named it.\"",
            ],
            (NpcEntityIds.Elsie, PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Elsie says softly, \"Trusted neighbor — I've trusted you for a while now. Glad the village caught up.\"",
            ],
            (NpcEntityIds.Elsie, PlayerLongTermGoalMilestone.VillageStory, _) =>
            [
                "Elsie looks moved. \"Your story's in Bloomtown now — and part of it's written in our talks.\"",
            ],
            (NpcEntityIds.Harold, PlayerLongTermGoalMilestone.PuttingDownRoots, LegacyArchetype.Builder) =>
            [
                "Harold says quietly, \"First roots — I saw them in how you showed up for shared work.\"",
            ],
            (NpcEntityIds.Harold, PlayerLongTermGoalMilestone.TrustedNeighbor, _) =>
            [
                "Harold tips his hat. \"Trusted neighbor. I considered you one already.\"",
            ],
            (NpcEntityIds.Harold, PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Builder) =>
            [
                "Harold speaks slowly. \"A living legacy — what you built includes the trust between us.\"",
            ],
            _ => [],
        };
    }

    internal static string[] GetEmotionalAmbientLines(
        uint npcEntityId,
        NpcMemoryType memoryType,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetEmotionalAmbientLines(memoryType, archetype);

        return (npcEntityId, memoryType, archetype) switch
        {
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, LegacyArchetype.Caretaker) =>
            [
                "Elsie says quietly, \"The garden's lucky — and so am I, having you nearby.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.FrequentElsieCompanion, _) =>
            [
                "Elsie waves. \"There you are — I was hoping I'd see you today.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _) =>
            [
                "Harold nods your way. \"Good to see you. The well remembers steady hands.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.FrequentHaroldCompanion, LegacyArchetype.Builder) =>
            [
                "Harold murmurs, \"Familiar face — builders like you make a village feel solid.\"",
            ],
            _ => [],
        };
    }
}