using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Defines which items each NPC shop sells to and buys from players.
/// </summary>
public static class NpcShopCatalog
{
    private static readonly Dictionary<uint, NpcShopDefinition> Shops = new()
    {
        [NpcEntityIds.Elsie] = new NpcShopDefinition(
            NpcEntityIds.Elsie,
            "Elsie",
            Sells: [ItemType.Wood, ItemType.Tool],
            Buys: [ItemType.Stone, ItemType.Apple]),

        [NpcEntityIds.Tom] = new NpcShopDefinition(
            NpcEntityIds.Tom,
            "Tom",
            Sells: [ItemType.Apple, ItemType.Stone],
            Buys: [ItemType.Wood]),

        [NpcEntityIds.Mira] = new NpcShopDefinition(
            NpcEntityIds.Mira,
            "Mira",
            Sells: [ItemType.Apple, ItemType.Stone, ItemType.Tool],
            Buys: [ItemType.Wood, ItemType.Plank, ItemType.Apple]),
    };

    public static bool TryGetShop(uint npcEntityId, out NpcShopDefinition shop)
    {
        return Shops.TryGetValue(npcEntityId, out shop!);
    }

    public static bool NpcSellsItem(uint npcEntityId, ItemType itemType)
    {
        return TryGetShop(npcEntityId, out var shop) && shop.Sells.Contains(itemType);
    }

    public static bool NpcBuysItem(uint npcEntityId, ItemType itemType)
    {
        return TryGetShop(npcEntityId, out var shop) && shop.Buys.Contains(itemType);
    }
}

public readonly record struct NpcShopDefinition(
    uint NpcEntityId,
    string NpcName,
    IReadOnlyList<ItemType> Sells,
    IReadOnlyList<ItemType> Buys);