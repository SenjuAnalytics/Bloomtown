using Bloomtown.Shared.Housing;

namespace Bloomtown.Shared.Tests;

public sealed class FurnitureComfortConfigTests
{
    [Fact]
    public void CalculateComfortScore_SumsPlacedFurniture()
    {
        var placed = new Dictionary<FurnitureType, int>
        {
            [FurnitureType.SimpleBed] = 1,
            [FurnitureType.WoodenChair] = 1,
            [FurnitureType.SmallTable] = 1,
        };

        Assert.Equal(28, FurnitureComfortConfig.CalculateComfortScore(placed));
    }

    [Theory]
    [InlineData(0, 65f)]
    [InlineData(20, 65f)]
    [InlineData(21, 80f)]
    [InlineData(40, 80f)]
    [InlineData(41, 100f)]
    [InlineData(50, 100f)]
    public void GetSleepEnergyRecovery_UsesComfortTiers(int comfortScore, float expectedRecovery)
    {
        Assert.Equal(expectedRecovery, FurnitureComfortConfig.GetSleepEnergyRecovery(comfortScore));
    }

    [Fact]
    public void GetCombinedSleepRecovery_UsesBestOfTierAndComfort()
    {
        var fromComfort = FurnitureComfortConfig.GetCombinedSleepRecovery(HouseTier.Basic, 45);
        var fromTier = FurnitureComfortConfig.GetCombinedSleepRecovery(HouseTier.Comfortable, 0);

        Assert.Equal(100f, fromComfort);
        Assert.Equal(100f, fromTier);
    }

    [Fact]
    public void FurnitureCatalog_DefinesExpectedComfortValues()
    {
        Assert.True(FurnitureCatalog.TryGet(FurnitureType.SimpleBed, out var bed));
        Assert.Equal(15, bed.ComfortValue);
        Assert.Equal(5, FurnitureCatalog.GetComfortValue(FurnitureType.WoodenChair));
        Assert.Equal(8, FurnitureCatalog.GetComfortValue(FurnitureType.SmallTable));
        Assert.Equal(10, FurnitureCatalog.GetComfortValue(FurnitureType.Bookshelf));
    }
}