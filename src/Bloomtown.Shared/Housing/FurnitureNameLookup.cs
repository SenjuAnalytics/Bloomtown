namespace Bloomtown.Shared.Housing;

/// <summary>
/// Case-insensitive furniture name parsing for console commands.
/// </summary>
public static class FurnitureNameLookup
{
    private static readonly Dictionary<string, FurnitureType> NamesByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bed"] = FurnitureType.SimpleBed,
        ["simplebed"] = FurnitureType.SimpleBed,
        ["simple bed"] = FurnitureType.SimpleBed,
        ["chair"] = FurnitureType.WoodenChair,
        ["woodenchair"] = FurnitureType.WoodenChair,
        ["wooden chair"] = FurnitureType.WoodenChair,
        ["table"] = FurnitureType.SmallTable,
        ["smalltable"] = FurnitureType.SmallTable,
        ["small table"] = FurnitureType.SmallTable,
        ["bookshelf"] = FurnitureType.Bookshelf,
        ["shelf"] = FurnitureType.Bookshelf,
    };

    public static string KnownFurnitureList => string.Join(", ", NamesByKey.Keys.OrderBy(name => name));

    public static bool TryResolve(string name, out FurnitureType furnitureType)
    {
        return NamesByKey.TryGetValue(name.Trim(), out furnitureType);
    }
}