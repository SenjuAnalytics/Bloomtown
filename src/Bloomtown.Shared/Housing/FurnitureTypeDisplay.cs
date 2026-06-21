namespace Bloomtown.Shared.Housing;

public static class FurnitureTypeDisplay
{
    public static string GetName(FurnitureType furnitureType)
    {
        return furnitureType switch
        {
            FurnitureType.SimpleBed => "Simple Bed",
            FurnitureType.WoodenChair => "Wooden Chair",
            FurnitureType.SmallTable => "Small Table",
            FurnitureType.Bookshelf => "Bookshelf",
            _ => furnitureType.ToString(),
        };
    }
}