using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Gifting;

/// <summary>
/// Simple per-NPC gift preferences that boost affinity when matched.
/// </summary>
public static class NpcGiftPreference
{
    private static readonly Dictionary<uint, HashSet<ItemType>> Preferences = new()
    {
        // Elsie enjoys farm produce and crafted goods.
        [NpcEntityIds.Elsie] = [ItemType.Apple, ItemType.Plank],
        // Harold appreciates practical village staples.
        [NpcEntityIds.Harold] = [ItemType.Apple, ItemType.Plank],
        // Mira enjoys market-friendly goods.
        [NpcEntityIds.Mira] = [ItemType.Apple, ItemType.Plank],
        // Tom enjoys raw wood and tools.
        [NpcEntityIds.Tom] = [ItemType.Wood, ItemType.Tool],
        // Greta enjoys hearty produce and practical inn supplies.
        [NpcEntityIds.Greta] = [ItemType.Apple, ItemType.Plank],
        // Nora enjoys gentle produce and practical garden staples.
        [NpcEntityIds.Nora] = [ItemType.Apple, ItemType.Wood],
        // Elias enjoys raw materials and quality tools.
        [NpcEntityIds.Elias] = [ItemType.Wood, ItemType.Tool, ItemType.Plank],
        // Ben appreciates practical guard supplies.
        [NpcEntityIds.Ben] = [ItemType.Wood, ItemType.Tool],
        // Lila enjoys hearty produce and small practical gifts.
        [NpcEntityIds.Lila] = [ItemType.Apple, ItemType.Wood],
        // Rowan enjoys story journals and simple keepsakes.
        [NpcEntityIds.Rowan] = [ItemType.Apple, ItemType.Wood],
        // Marcus enjoys quality materials and practical tools.
        [NpcEntityIds.Marcus] = [ItemType.Wood, ItemType.Plank, ItemType.Tool],
        // Eleanor enjoys simple keepsakes and harvest gifts.
        [NpcEntityIds.Eleanor] = [ItemType.Apple, ItemType.Wood],
    };

    public static bool IsPreferred(uint npcEntityId, ItemType itemType)
    {
        return Preferences.TryGetValue(npcEntityId, out var preferred) && preferred.Contains(itemType);
    }

    public static IReadOnlyCollection<ItemType> GetPreferredItems(uint npcEntityId)
    {
        return Preferences.TryGetValue(npcEntityId, out var preferred)
            ? preferred
            : Array.Empty<ItemType>();
    }
}