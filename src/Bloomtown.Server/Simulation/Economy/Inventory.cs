using Bloomtown.Shared.Items;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Simple unlimited player item storage.
/// </summary>
public sealed class Inventory
{
    private readonly Dictionary<ItemType, int> _items = new();

    public IReadOnlyDictionary<ItemType, int> Items => _items;

    public void Load(IEnumerable<ItemStack> stacks)
    {
        _items.Clear();
        foreach (var stack in stacks)
            AddItem(stack.ItemType, stack.Quantity);
    }

    public IEnumerable<ItemStack> ToStacks()
    {
        foreach (var (itemType, quantity) in _items.OrderBy(pair => pair.Key))
            yield return new ItemStack(itemType, quantity);
    }

    public int GetItemCount(ItemType itemType)
    {
        return _items.GetValueOrDefault(itemType, 0);
    }

    public bool HasItem(ItemType itemType, int quantity = 1)
    {
        return GetItemCount(itemType) >= quantity;
    }

    public void AddItem(ItemType itemType, int quantity)
    {
        if (quantity <= 0)
            return;

        _items[itemType] = GetItemCount(itemType) + quantity;
    }

    public bool RemoveItem(ItemType itemType, int quantity)
    {
        if (quantity <= 0 || !HasItem(itemType, quantity))
            return false;

        var remaining = GetItemCount(itemType) - quantity;
        if (remaining <= 0)
            _items.Remove(itemType);
        else
            _items[itemType] = remaining;

        return true;
    }
}