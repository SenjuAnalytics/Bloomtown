namespace Bloomtown.Shared.Items;

public readonly record struct ItemStack(ItemType ItemType, int Quantity)
{
    public static ItemStack Of(ItemType itemType, int quantity = 1)
    {
        return new ItemStack(itemType, quantity);
    }
}