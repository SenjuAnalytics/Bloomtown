using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Favorite-gift bonding for focus NPCs — memory mapping, extra affinity, and warm acceptance dialogue.
/// </summary>
public static class NpcEmotionalBondGiftConfig
{
    /// <summary>Extra affinity per item when a focus NPC receives a favorite gift.</summary>
    public const int FocusNpcFavoriteGiftBonusPerItem = 3;

    public static NpcMemoryType? GetFavoriteGiftMemoryForNpc(uint npcEntityId) =>
        npcEntityId switch
        {
            NpcEntityIds.Elsie => NpcMemoryType.GaveFavoriteGiftToElsie,
            NpcEntityIds.Harold => NpcMemoryType.GaveFavoriteGiftToHarold,
            NpcEntityIds.Mira => NpcMemoryType.GaveFavoriteGiftToMira,
            NpcEntityIds.Tom => NpcMemoryType.GaveFavoriteGiftToTom,
            NpcEntityIds.Greta => NpcMemoryType.GaveFavoriteGiftToGreta,
            NpcEntityIds.Nora => NpcMemoryType.GaveFavoriteGiftToNora,
            NpcEntityIds.Elias => NpcMemoryType.GaveFavoriteGiftToElias,
            NpcEntityIds.Ben => NpcMemoryType.GaveFavoriteGiftToBen,
            NpcEntityIds.Lila => NpcMemoryType.GaveFavoriteGiftToLila,
            NpcEntityIds.Rowan => NpcMemoryType.GaveFavoriteGiftToRowan,
            NpcEntityIds.Marcus => NpcMemoryType.GaveFavoriteGiftToMarcus,
            NpcEntityIds.Eleanor => NpcMemoryType.GaveFavoriteGiftToEleanor,
            _ => null,
        };

    public static bool IsFavoriteGiftBondingMemory(NpcMemoryType memoryType) =>
        memoryType is NpcMemoryType.GaveFavoriteGiftToElsie
            or NpcMemoryType.GaveFavoriteGiftToHarold
            or NpcMemoryType.GaveFavoriteGiftToMira
            or NpcMemoryType.GaveFavoriteGiftToTom
            or NpcMemoryType.GaveFavoriteGiftToGreta
            or NpcMemoryType.GaveFavoriteGiftToNora
            or NpcMemoryType.GaveFavoriteGiftToElias
            or NpcMemoryType.GaveFavoriteGiftToBen
            or NpcMemoryType.GaveFavoriteGiftToLila
            or NpcMemoryType.GaveFavoriteGiftToRowan
            or NpcMemoryType.GaveFavoriteGiftToMarcus
            or NpcMemoryType.GaveFavoriteGiftToEleanor;

    /// <summary>Warm NPC response when a focus NPC receives a favorite gift.</summary>
    public static string? TryGetFavoriteGiftAcceptanceLine(
        uint npcEntityId,
        ItemType itemType,
        RelationshipTier tier,
        bool justRecordedBondMemory,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        var itemLabel = ItemDatabase.GetDisplayName(itemType);
        string[] lines = (npcEntityId, tier, justRecordedBondMemory) switch
        {
            (NpcEntityIds.Elsie, RelationshipTier.CloseFriend, true) =>
            [
                $"Elsie's eyes soften as she accepts the {itemLabel}. \"You remembered — that's the sort of kindness I carry with me.\"",
                $"Elsie holds the {itemLabel} close. \"This isn't just a gift. It's you seeing me. Thank you.\"",
            ],
            (NpcEntityIds.Elsie, _, true) =>
            [
                $"Elsie's eyes soften as she accepts the {itemLabel}. \"You really know how to make someone feel welcome.\"",
                $"Elsie smiles warmly at the {itemLabel}. \"I'll remember this — you thought of me.\"",
            ],
            (NpcEntityIds.Elsie, RelationshipTier.CloseFriend, false) =>
            [
                $"Elsie accepts the {itemLabel} with a grateful hug. \"You always know what I need.\"",
            ],
            (NpcEntityIds.Elsie, _, false) =>
            [
                $"Elsie accepts the {itemLabel} warmly. \"Another perfect choice — thank you.\"",
            ],

            (NpcEntityIds.Harold, RelationshipTier.CloseFriend, true) =>
            [
                $"Harold takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Harold nods at the {itemLabel}. \"Not everyone bothers to notice what folk like. You did.\"",
            ],
            (NpcEntityIds.Harold, _, true) =>
            [
                $"Harold accepts the {itemLabel} with a quiet nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Harold, RelationshipTier.CloseFriend, false) =>
            [
                $"Harold accepts the {itemLabel}. \"Good choice. You know me better than most.\"",
            ],
            (NpcEntityIds.Harold, _, false) =>
            [
                $"Harold tips his hat at the {itemLabel}. \"Appreciated — means more than you might think.\"",
            ],

            (NpcEntityIds.Mira, RelationshipTier.CloseFriend, true) =>
            [
                $"Mira's face lights up at the {itemLabel}. \"You remembered what I like — that's rare, and I won't forget it.\"",
                $"Mira accepts the {itemLabel} warmly. \"Gifts like this say 'I see you.' Thank you.\"",
            ],
            (NpcEntityIds.Mira, _, true) =>
            [
                $"Mira grins at the {itemLabel}. \"You thought of me — that stays with a person.\"",
            ],
            (NpcEntityIds.Mira, RelationshipTier.CloseFriend, false) =>
            [
                $"Mira accepts the {itemLabel} with a squeeze of your hand. \"You always know how to brighten my day.\"",
            ],
            (NpcEntityIds.Mira, _, false) =>
            [
                $"Mira accepts the {itemLabel} cheerfully. \"Another favorite — you spoil me.\"",
            ],

            (NpcEntityIds.Tom, RelationshipTier.CloseFriend, true) =>
            [
                $"Tom accepts the {itemLabel} and almost smiles. \"You remembered. That's worth more than the wood itself.\"",
                $"Tom nods at the {itemLabel}. \"Not flashy — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Tom, _, true) =>
            [
                $"Tom accepts the {itemLabel} quietly. \"Good choice. I appreciate you remembering.\"",
            ],
            (NpcEntityIds.Tom, RelationshipTier.CloseFriend, false) =>
            [
                $"Tom accepts the {itemLabel}. \"You know what I need — thank you.\"",
            ],
            (NpcEntityIds.Tom, _, false) =>
            [
                $"Tom accepts the {itemLabel} with a grateful nod. \"Handy — and thoughtful.\"",
            ],

            (NpcEntityIds.Greta, RelationshipTier.CloseFriend, true) =>
            [
                $"Greta's face lights up at the {itemLabel}. \"You remembered — that's the sort of kindness I carry with me.\"",
                $"Greta fusses over the {itemLabel} fondly. \"This isn't just a gift. It's you seeing me. Thank you, dear.\"",
            ],
            (NpcEntityIds.Greta, _, true) =>
            [
                $"Greta beams at the {itemLabel}. \"You really know how to make an innkeeper feel welcome.\"",
                $"Greta smiles warmly at the {itemLabel}. \"I'll remember this — you thought of me.\"",
            ],
            (NpcEntityIds.Greta, RelationshipTier.CloseFriend, false) =>
            [
                $"Greta accepts the {itemLabel} with a squeeze of your hand. \"You always know what I need.\"",
            ],
            (NpcEntityIds.Greta, _, false) =>
            [
                $"Greta accepts the {itemLabel} cheerfully. \"Another perfect choice — you spoil your innkeeper.\"",
            ],

            (NpcEntityIds.Nora, RelationshipTier.CloseFriend, true) =>
            [
                $"Nora's eyes soften as she accepts the {itemLabel}. \"You remembered — that's the sort of kindness I carry with me.\"",
                $"Nora holds the {itemLabel} close. \"This isn't just a gift. It's you seeing me. Thank you.\"",
            ],
            (NpcEntityIds.Nora, _, true) =>
            [
                $"Nora smiles at the {itemLabel}. \"You really know how to make someone feel welcome among the herbs.\"",
                $"Nora nods warmly at the {itemLabel}. \"I'll remember this — you thought of me.\"",
            ],
            (NpcEntityIds.Nora, RelationshipTier.CloseFriend, false) =>
            [
                $"Nora accepts the {itemLabel} with a grateful touch. \"You always know what I need.\"",
            ],
            (NpcEntityIds.Nora, _, false) =>
            [
                $"Nora accepts the {itemLabel} quietly. \"Another thoughtful choice — thank you.\"",
            ],

            (NpcEntityIds.Elias, RelationshipTier.CloseFriend, true) =>
            [
                $"Elias takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Elias nods at the {itemLabel}. \"Not flashy — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Elias, _, true) =>
            [
                $"Elias accepts the {itemLabel} with a quiet nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Elias, RelationshipTier.CloseFriend, false) =>
            [
                $"Elias accepts the {itemLabel}. \"Good choice. You know what a smith actually uses.\"",
            ],
            (NpcEntityIds.Elias, _, false) =>
            [
                $"Elias accepts the {itemLabel} with a grateful nod. \"Handy — and thoughtful.\"",
            ],

            (NpcEntityIds.Ben, RelationshipTier.CloseFriend, true) =>
            [
                $"Ben takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Ben nods at the {itemLabel}. \"Practical — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Ben, _, true) =>
            [
                $"Ben accepts the {itemLabel} with a quiet nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Ben, RelationshipTier.CloseFriend, false) =>
            [
                $"Ben accepts the {itemLabel}. \"Good choice. You know what a guard actually uses.\"",
            ],
            (NpcEntityIds.Ben, _, false) =>
            [
                $"Ben accepts the {itemLabel} with a grateful nod. \"Handy — and thoughtful.\"",
            ],

            (NpcEntityIds.Lila, RelationshipTier.CloseFriend, true) =>
            [
                $"Lila takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Lila beams at the {itemLabel}. \"Sweet and thoughtful — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Lila, _, true) =>
            [
                $"Lila accepts the {itemLabel} with a warm nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Lila, RelationshipTier.CloseFriend, false) =>
            [
                $"Lila accepts the {itemLabel}. \"Good choice. You know what makes a young villager feel seen.\"",
            ],
            (NpcEntityIds.Lila, _, false) =>
            [
                $"Lila accepts the {itemLabel} with a grateful smile. \"Lovely — and thoughtful.\"",
            ],

            (NpcEntityIds.Rowan, RelationshipTier.CloseFriend, true) =>
            [
                $"Rowan takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Rowan smiles faintly at the {itemLabel}. \"Thoughtful — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Rowan, _, true) =>
            [
                $"Rowan accepts the {itemLabel} with a warm nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Rowan, RelationshipTier.CloseFriend, false) =>
            [
                $"Rowan accepts the {itemLabel}. \"Good choice. You know what makes an old storyteller feel seen.\"",
            ],
            (NpcEntityIds.Rowan, _, false) =>
            [
                $"Rowan accepts the {itemLabel} with a grateful smile. \"Lovely — and thoughtful.\"",
            ],

            (NpcEntityIds.Marcus, RelationshipTier.CloseFriend, true) =>
            [
                $"Marcus takes the {itemLabel} and meets your eyes. \"You remembered. That tells me plenty about who you are.\"",
                $"Marcus smiles at the {itemLabel}. \"Thoughtful — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Marcus, _, true) =>
            [
                $"Marcus accepts the {itemLabel} with a warm nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Marcus, RelationshipTier.CloseFriend, false) =>
            [
                $"Marcus accepts the {itemLabel}. \"Good choice. You know what a craftsman actually uses.\"",
            ],
            (NpcEntityIds.Marcus, _, false) =>
            [
                $"Marcus accepts the {itemLabel} with a grateful smile. \"Handy — and thoughtful.\"",
            ],

            (NpcEntityIds.Eleanor, RelationshipTier.CloseFriend, true) =>
            [
                $"Eleanor takes the {itemLabel} and meets your eyes. \"You remembered, dear. That tells me plenty about who you are.\"",
                $"Eleanor smiles warmly at the {itemLabel}. \"Thoughtful — just right. I'll remember you thought of me.\"",
            ],
            (NpcEntityIds.Eleanor, _, true) =>
            [
                $"Eleanor accepts the {itemLabel} with a warm nod. \"Thoughtful — I'll remember that.\"",
            ],
            (NpcEntityIds.Eleanor, RelationshipTier.CloseFriend, false) =>
            [
                $"Eleanor accepts the {itemLabel}. \"Good choice. You know what makes an old teacher feel seen.\"",
            ],
            (NpcEntityIds.Eleanor, _, false) =>
            [
                $"Eleanor accepts the {itemLabel} with a grateful smile. \"Lovely — and thoughtful.\"",
            ],

            _ => [],
        };

        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    /// <summary>Player-facing feedback when a favorite gift deepens an emotional bond.</summary>
    public static string FormatFavoriteGiftBondFeedback(string npcDisplayName, bool recordedNewMemory)
    {
        return recordedNewMemory
            ? $"[{npcDisplayName} will remember this gift as something personal — not just polite.] "
            : $"[Your bond with {npcDisplayName} feels warmer after this gift.] ";
    }
}