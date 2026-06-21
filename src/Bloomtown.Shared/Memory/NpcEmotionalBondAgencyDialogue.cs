using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Dialogue for player-initiated emotional bonding actions and NPC remembrance responses.
/// </summary>
internal static class NpcEmotionalBondAgencyDialogue
{
    internal static string[] GetBondingActionLines(
        uint npcEntityId,
        EmotionalBondActionKind action,
        RelationshipTier tier,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetBondingActionLines(action, tier, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetBondingActionLines(action, tier, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetBondingActionLines(action, tier, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetBondingActionLines(action, tier, archetype);

        return (npcEntityId, action, tier, archetype) switch
        {
            (NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend, _) =>
            [
                "Elsie smiles softly. \"You always know when someone needs a kind word. I'm glad you stopped.\"",
                "Elsie takes your hand briefly. \"Thank you for checking on me. It means more than you know.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Elsie brightens. \"Oh — you thought to ask how I'm doing. That's lovely of you.\"",
                "Elsie nods warmly. \"I'm alright, truly. And better for seeing you check in.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Elsie sits with you quietly. \"Moments like this — they're what make Bloomtown feel like home.\"",
                "Elsie laughs gently. \"I won't forget this little pause we took together.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Elsie shares a peaceful silence with you. \"Thank you for the company. I needed that.\"",
                "Elsie says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Caretaker) =>
            [
                "Elsie watches you work. \"A caretaker's hands — I see why folk trust you.\"",
                "Elsie touches your shoulder. \"You help without being asked. That's rare.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Elsie looks touched. \"You didn't have to help me with this — but I'm glad you did.\"",
                "Elsie murmurs, \"Small kindnesses like this — they stay with a person.\"",
            ],

            (NpcEntityIds.Harold, EmotionalBondActionKind.CheckOn, RelationshipTier.CloseFriend, _) =>
            [
                "Harold meets your eyes. \"You check on folk like it's second nature. I appreciate it.\"",
                "Harold says quietly, \"Good to know someone's looking out for me. Thank you.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.CheckOn, _, _) =>
            [
                "Harold nods. \"Checking on an old hand like me — that's thoughtful. I'm well enough.\"",
                "Harold tips his hat. \"You didn't need to ask, but I'm glad you did.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment, RelationshipTier.CloseFriend, _) =>
            [
                "Harold sits beside you. \"Quiet company — best kind. Glad it's you.\"",
                "Harold speaks slowly. \"I'll remember this. Simple moments, but they matter.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment, _, _) =>
            [
                "Harold shares a comfortable silence. \"Good to pause with someone who understands.\"",
                "Harold murmurs, \"Not every day needs words. This one's better for the quiet.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.HelpWith, _, LegacyArchetype.Builder) =>
            [
                "Harold watches you work. \"Builder's hands — steady and sure. I notice.\"",
                "Harold nods approvingly. \"You pitch in like someone who cares about the whole village.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.HelpWith, _, _) =>
            [
                "Harold accepts your help with a grateful nod. \"Didn't expect it — but I'm glad you offered.\"",
                "Harold says, \"Hands-on help like this — it tells me who I can count on.\"",
            ],

            (NpcEntityIds.Elsie, EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Elsie settles beside you without hurry. \"No chores, no rush — just company. I needed this.\"",
                "Elsie shares a peaceful silence. \"Quiet moments like this are how trust grows.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.SpendTime, _, LegacyArchetype.Connector) =>
            [
                "Elsie smiles. \"You chose presence over errands — connectors understand that kind of gift.\"",
            ],
            (NpcEntityIds.Elsie, EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Elsie sits with you quietly. \"Thank you for staying — not every kindness needs words.\"",
                "Elsie says softly, \"It's nice — just being here together, no chores, no rush.\"",
            ],

            (NpcEntityIds.Harold, EmotionalBondActionKind.SpendTime, RelationshipTier.CloseFriend, _) =>
            [
                "Harold shares his bench without speaking. \"Good company doesn't always need chatter.\"",
                "Harold nods slowly. \"Quiet time like this — I'll remember it.\"",
            ],
            (NpcEntityIds.Harold, EmotionalBondActionKind.SpendTime, _, _) =>
            [
                "Harold sits with you by the well. \"Not every day needs words. This one's better for the quiet.\"",
                "Harold murmurs, \"Glad you stayed a while. Means something.\"",
            ],

            _ => [],
        };
    }

    internal static string[] GetNpcRemembranceLines(
        uint npcEntityId,
        NpcMemoryType memoryType,
        RelationshipTier tier,
        LegacyArchetype archetype)
    {
        if (npcEntityId == NpcEntityIds.Mira)
            return NpcMiraEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Tom)
            return NpcTomEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Greta)
            return NpcGretaEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Nora)
            return NpcNoraEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Elias)
            return NpcEliasEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Ben)
            return NpcBenEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Lila)
            return NpcLilaEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);
        if (npcEntityId == NpcEntityIds.Rowan)
            return NpcRowanEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);

        if (npcEntityId == NpcEntityIds.Marcus)
            return NpcMarcusEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);

        if (npcEntityId == NpcEntityIds.Eleanor)
            return NpcEleanorEmotionalBondContent.GetNpcRemembranceLines(memoryType, tier, archetype);

        return (npcEntityId, memoryType, tier, archetype) switch
        {
            (NpcEntityIds.Elsie, NpcMemoryType.HelpedGardenOften, _, _) =>
            [
                "Elsie adds quietly, \"I still think about your garden help when the beds look tired.\"",
                "Elsie smiles. \"The garden remembers you — and so do I.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.FrequentElsieCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Elsie says, \"You've become part of how I picture Bloomtown — I hope you know that.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.CheckedOnElsie, _, _) =>
            [
                "Elsie touches your arm. \"You checked on me before — I haven't forgotten.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.SharedMomentWithElsie, _, _) =>
            [
                "Elsie glances at you. \"That quiet moment we shared — I still carry it.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.ConsciouslyHelpedElsie, _, LegacyArchetype.Caretaker) =>
            [
                "Elsie murmurs, \"Caretakers show up in small ways. You showed up for me.\"",
            ],
            (NpcEntityIds.Elsie, NpcMemoryType.ConsciouslyHelpedElsie, _, _) =>
            [
                "Elsie says warmly, \"When you helped me last time — that stayed with me.\"",
            ],

            (NpcEntityIds.Harold, NpcMemoryType.HelpedWellOften, _, _) =>
            [
                "Harold adds, \"Your well-side help — I think of it when the buckets feel heavy.\"",
                "Harold nods. \"Steady hands at the well. I remember.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.FrequentHaroldCompanion, RelationshipTier.CloseFriend, _) =>
            [
                "Harold says quietly, \"You've become someone I look for in a crowd. That's trust.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.CheckedOnHarold, _, _) =>
            [
                "Harold tips his hat. \"You checked on me once — I appreciated it then and now.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.SharedMomentWithHarold, _, _) =>
            [
                "Harold murmurs, \"That quiet spell we shared — still a comfort to recall.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.ConsciouslyHelpedHarold, _, LegacyArchetype.Builder) =>
            [
                "Harold says, \"Builders leave marks on timber — you left one on me, too.\"",
            ],
            (NpcEntityIds.Harold, NpcMemoryType.ConsciouslyHelpedHarold, _, _) =>
            [
                "Harold nods. \"When you lent a hand before — I didn't forget who showed up.\"",
            ],

            _ => [],
        };
    }

    internal static string[] GetArchetypeBondingHintLines(
        LegacyArchetype archetype,
        EmotionalBondActionKind action)
    {
        return (archetype, action) switch
        {
            (LegacyArchetype.Caretaker, EmotionalBondActionKind.CheckOn) =>
            [
                "Your caretaker's instinct — noticing when someone needs you — feels right here.",
            ],
            (LegacyArchetype.Caretaker, EmotionalBondActionKind.HelpWith) =>
            [
                "Helping with your own hands fits the caretaker path you're walking.",
            ],
            (LegacyArchetype.Caretaker, EmotionalBondActionKind.ShareMoment) =>
            [
                "Quiet care isn't just chores — moments like this are caretaking, too.",
            ],
            (LegacyArchetype.Connector, EmotionalBondActionKind.ShareMoment) =>
            [
                "Sharing a moment — that's the connector in you, weaving bonds one person at a time.",
            ],
            (LegacyArchetype.Connector, EmotionalBondActionKind.CheckOn) =>
            [
                "Checking on someone personally — your connector's warmth reaches further than the square.",
            ],
            (LegacyArchetype.Builder, EmotionalBondActionKind.HelpWith) =>
            [
                "Hands-on help — builders show care through work, and this counts.",
            ],
            (LegacyArchetype.Builder, EmotionalBondActionKind.SpendTime) =>
            [
                "Steady presence without a task — builders know trust is built in the quiet, too.",
            ],
            (LegacyArchetype.Builder, _) =>
            [
                "Steady presence like this is part of how builders earn trust in Bloomtown.",
            ],
            (LegacyArchetype.Connector, EmotionalBondActionKind.SpendTime) =>
            [
                "Choosing to linger — that's the connector in you, weaving bonds one quiet moment at a time.",
            ],
            (LegacyArchetype.Caretaker, EmotionalBondActionKind.SpendTime) =>
            [
                "Quiet care isn't just chores — sitting with someone counts, too.",
            ],
            _ => [],
        };
    }
}