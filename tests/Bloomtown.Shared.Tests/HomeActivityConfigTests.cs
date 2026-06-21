using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Needs;

namespace Bloomtown.Shared.Tests;

public sealed class HomeActivityConfigTests
{
    [Fact]
    public void Relax_HasStrongerRecoveryThanOutdoorRest()
    {
        var effects = HomeActivityConfig.CalculateEffects(HomeActivityType.Relax, new Dictionary<FurnitureType, int>());

        Assert.True(effects.MoodGain > PlayerNeedsConfig.RestMoodGain);
        Assert.True(effects.FatigueReduction > PlayerNeedsConfig.RestFatigueReduction);
        Assert.Equal(15f, effects.MoodGain);
        Assert.Equal(40f, effects.FatigueReduction);
    }

    [Fact]
    public void ReadBook_AppliesBookshelfBonusWhenPlaced()
    {
        var without = HomeActivityConfig.CalculateEffects(HomeActivityType.ReadBook, new Dictionary<FurnitureType, int>());
        var withBookshelf = HomeActivityConfig.CalculateEffects(
            HomeActivityType.ReadBook,
            new Dictionary<FurnitureType, int> { [FurnitureType.Bookshelf] = 1 });

        Assert.False(without.BonusFurnitureApplied);
        Assert.Equal(12f, without.MoodGain);
        Assert.Equal(25f, without.FatigueReduction);

        Assert.True(withBookshelf.BonusFurnitureApplied);
        Assert.Equal(25f, withBookshelf.MoodGain);
        Assert.Equal(35f, withBookshelf.FatigueReduction);
    }

    [Fact]
    public void SitByTable_AppliesTableBonusWhenPlaced()
    {
        var withTable = HomeActivityConfig.CalculateEffects(
            HomeActivityType.SitByTable,
            new Dictionary<FurnitureType, int> { [FurnitureType.SmallTable] = 1 });

        Assert.True(withTable.BonusFurnitureApplied);
        Assert.Equal(18f, withTable.MoodGain);
        Assert.Equal(38f, withTable.FatigueReduction);
    }

    [Fact]
    public void EnjoyTea_AppliesChairBonusWhenPlaced()
    {
        var withChair = HomeActivityConfig.CalculateEffects(
            HomeActivityType.EnjoyTea,
            new Dictionary<FurnitureType, int> { [FurnitureType.WoodenChair] = 1 });

        Assert.True(withChair.BonusFurnitureApplied);
        Assert.Equal(16f, withChair.MoodGain);
        Assert.Equal(32f, withChair.FatigueReduction);
    }

    [Fact]
    public void PickFlavorText_UsesBonusLinesWhenFurniturePresent()
    {
        var baseText = HomeActivityConfig.PickFlavorText(HomeActivityType.ReadBook, bonusApplied: false, variationSeed: 0);
        var bonusText = HomeActivityConfig.PickFlavorText(HomeActivityType.ReadBook, bonusApplied: true, variationSeed: 0);

        Assert.Contains("book", baseText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bookshelf", bonusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Nap_HasStrongFatigueRecovery()
    {
        var effects = HomeActivityConfig.CalculateEffects(HomeActivityType.Nap, new Dictionary<FurnitureType, int>());

        Assert.Equal(8f, effects.MoodGain);
        Assert.Equal(48f, effects.FatigueReduction);
        Assert.True(effects.FatigueReduction > effects.MoodGain * 4f);
    }

    [Fact]
    public void Nap_AppliesBedBonusWhenPlaced()
    {
        var withBed = HomeActivityConfig.CalculateEffects(
            HomeActivityType.Nap,
            new Dictionary<FurnitureType, int> { [FurnitureType.SimpleBed] = 1 });

        Assert.True(withBed.BonusFurnitureApplied);
        Assert.Equal(13f, withBed.MoodGain);
        Assert.Equal(58f, withBed.FatigueReduction);
    }

    [Fact]
    public void FormatAvailableActivities_ListsAllCommands()
    {
        var text = HomeActivityConfig.FormatAvailableActivities(new Dictionary<FurnitureType, int>());

        Assert.Contains("relax", text);
        Assert.Contains("read", text);
        Assert.Contains("sit", text);
        Assert.Contains("tea", text);
        Assert.Contains("nap", text);
    }
}