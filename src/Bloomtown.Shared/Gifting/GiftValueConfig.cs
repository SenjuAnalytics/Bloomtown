using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;

namespace Bloomtown.Shared.Gifting;

/// <summary>
/// Base affinity granted per item when gifted to an NPC.
/// </summary>
public static class GiftValueConfig
{
    /// <summary>Multiplier applied when the item matches the NPC's preference.</summary>
    public const float PreferredItemMultiplier = 2f;

    public static int GetBaseAffinity(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Wood => 2,
            ItemType.Stone => 2,
            ItemType.Apple => 5,
            ItemType.Plank => 3,
            ItemType.Tool => 8,
            _ => 1,
        };
    }

    /// <summary>
    /// Total affinity for a gift batch: base value per item, doubled when preferred.
    /// </summary>
    public static int CalculateAffinityGain(uint npcEntityId, ItemType itemType, int quantity)
    {
        var basePerItem = GetBaseAffinity(itemType);
        var perItem = NpcGiftPreference.IsPreferred(npcEntityId, itemType)
            ? (int)MathF.Round(basePerItem * PreferredItemMultiplier)
            : basePerItem;

        var total = perItem * quantity;

        if (NpcGiftPreference.IsPreferred(npcEntityId, itemType)
            && NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
        {
            total += NpcEmotionalBondGiftConfig.FocusNpcFavoriteGiftBonusPerItem * quantity;
        }

        return total;
    }
}