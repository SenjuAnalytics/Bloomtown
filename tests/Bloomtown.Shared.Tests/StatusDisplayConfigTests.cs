using Bloomtown.Shared.Console;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Tests;

public sealed class StatusDisplayConfigTests
{
    [Fact]
    public void FormatNewcomerHint_ShowsTipsForEarlyDaysOnly()
    {
        Assert.Contains("greet", StatusDisplayConfig.FormatNewcomerHint(1, 0)!, StringComparison.OrdinalIgnoreCase);
        Assert.Null(StatusDisplayConfig.FormatNewcomerHint(6, 0));
    }

    [Fact]
    public void FormatCompactInventory_TruncatesLargeLists()
    {
        var stacks = Enumerable.Range(0, 8)
            .Select(i => ItemStack.Of(ItemType.Wood, i + 1))
            .ToList();

        var text = StatusDisplayConfig.FormatCompactInventory(stacks);

        Assert.Contains("inventory", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("+3 more", text);
    }

    [Fact]
    public void FormatCompactFurniture_SummarizesOnOneLine()
    {
        var furniture = new Dictionary<FurnitureType, int>
        {
            [FurnitureType.SimpleBed] = 1,
            [FurnitureType.WoodenChair] = 2,
        };

        var text = StatusDisplayConfig.FormatCompactFurniture(furniture);

        Assert.Contains("Simple Bed x1", text);
        Assert.Contains("Wooden Chair x2", text);
        Assert.Contains("comfort", text, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    public void ShouldShowSocialFabric_UnlocksAfterFirstDay(int gameDay, bool expected)
    {
        Assert.Equal(expected, StatusDisplayConfig.ShouldShowSocialFabric(gameDay));
    }
}