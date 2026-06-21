namespace Bloomtown.Shared.Items;

/// <summary>
/// Static item definitions and base shop prices.
/// </summary>
public static class ItemDatabase
{
    public static string GetDisplayName(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Wood => "Wood",
            ItemType.Stone => "Stone",
            ItemType.Apple => "Apple",
            ItemType.Tool => "Tool",
            ItemType.Plank => "Plank",
            _ => itemType.ToString(),
        };
    }

    /// <summary>Base price when an NPC sells this item to the player.</summary>
    public static int GetBaseBuyPrice(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Wood => 10,
            ItemType.Stone => 12,
            ItemType.Apple => 8,
            ItemType.Tool => 50,
            ItemType.Plank => 14,
            _ => 10,
        };
    }

    /// <summary>Base price when an NPC buys this item from the player.</summary>
    public static int GetBaseSellPrice(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Wood => 5,
            ItemType.Stone => 6,
            ItemType.Apple => 4,
            ItemType.Tool => 25,
            ItemType.Plank => 7,
            _ => 5,
        };
    }
}