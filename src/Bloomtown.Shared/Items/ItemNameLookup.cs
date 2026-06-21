namespace Bloomtown.Shared.Items;

/// <summary>
/// Case-insensitive item name parsing for console commands.
/// </summary>
public static class ItemNameLookup
{
    private static readonly Dictionary<string, ItemType> NamesByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["wood"] = ItemType.Wood,
        ["stone"] = ItemType.Stone,
        ["apple"] = ItemType.Apple,
        ["tool"] = ItemType.Tool,
        ["plank"] = ItemType.Plank,
        ["planks"] = ItemType.Plank,
    };

    public static string KnownItemsList => string.Join(", ", NamesByKey.Keys.OrderBy(name => name));

    public static bool TryResolve(string name, out ItemType itemType)
    {
        return NamesByKey.TryGetValue(name.Trim(), out itemType);
    }
}