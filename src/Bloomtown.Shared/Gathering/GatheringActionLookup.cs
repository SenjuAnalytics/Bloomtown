using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Gathering;

/// <summary>
/// Maps console gathering verbs to item types.
/// </summary>
public static class GatheringActionLookup
{
    private static readonly Dictionary<string, ItemType> ActionsByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["wood"] = ItemType.Wood,
        ["tree"] = ItemType.Wood,
        ["stone"] = ItemType.Stone,
        ["rock"] = ItemType.Stone,
    };

    public static string KnownActionsList => "gather wood, chop tree, mine stone";

    public static bool TryResolve(string action, out ItemType resourceType)
    {
        return ActionsByKey.TryGetValue(action.Trim(), out resourceType);
    }
}