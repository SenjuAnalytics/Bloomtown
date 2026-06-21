using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Housing;

/// <summary>
/// Comfort values, material costs, and placement limits for basic furniture.
/// </summary>
public static class FurnitureCatalog
{
    private static readonly Dictionary<FurnitureType, FurnitureDefinition> Definitions = new()
    {
        [FurnitureType.SimpleBed] = new FurnitureDefinition(
            FurnitureType.SimpleBed,
            ComfortValue: 15,
            MaxPerHome: 1,
            new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 5,
                [ItemType.Plank] = 3,
            }),
        [FurnitureType.WoodenChair] = new FurnitureDefinition(
            FurnitureType.WoodenChair,
            ComfortValue: 5,
            MaxPerHome: 1,
            new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 3,
                [ItemType.Plank] = 1,
            }),
        [FurnitureType.SmallTable] = new FurnitureDefinition(
            FurnitureType.SmallTable,
            ComfortValue: 8,
            MaxPerHome: 1,
            new Dictionary<ItemType, int>
            {
                [ItemType.Wood] = 4,
                [ItemType.Plank] = 2,
            }),
        [FurnitureType.Bookshelf] = new FurnitureDefinition(
            FurnitureType.Bookshelf,
            ComfortValue: 10,
            MaxPerHome: 1,
            new Dictionary<ItemType, int>
            {
                [ItemType.Plank] = 2,
                [ItemType.Tool] = 1,
            }),
    };

    public static IReadOnlyCollection<FurnitureType> AllTypes => Definitions.Keys;

    public static bool TryGet(FurnitureType furnitureType, out FurnitureDefinition definition)
    {
        return Definitions.TryGetValue(furnitureType, out definition!);
    }

    public static int GetComfortValue(FurnitureType furnitureType)
    {
        return TryGet(furnitureType, out var definition) ? definition.ComfortValue : 0;
    }

    public static string FormatMaterialRequirements(IReadOnlyDictionary<ItemType, int> requirements)
    {
        return string.Join(
            ", ",
            requirements
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Value} {ItemDatabase.GetDisplayName(pair.Key)}"));
    }
}

public sealed class FurnitureDefinition
{
    public FurnitureType Type { get; }
    public int ComfortValue { get; }
    public int MaxPerHome { get; }
    public IReadOnlyDictionary<ItemType, int> MaterialRequirements { get; }

    public FurnitureDefinition(
        FurnitureType type,
        int ComfortValue,
        int MaxPerHome,
        IReadOnlyDictionary<ItemType, int> materialRequirements)
    {
        Type = type;
        this.ComfortValue = ComfortValue;
        this.MaxPerHome = MaxPerHome;
        MaterialRequirements = materialRequirements;
    }
}