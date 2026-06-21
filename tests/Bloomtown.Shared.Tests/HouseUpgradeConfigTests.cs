using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Tests;

public sealed class HouseUpgradeConfigTests
{
    [Fact]
    public void BasicTier_HasImprovedUpgradeRequirements()
    {
        Assert.True(HouseUpgradeConfig.TryGetUpgradeRequirements(HouseTier.Basic, out var nextTier, out var requirements));

        Assert.Equal(HouseTier.Improved, nextTier);
        Assert.Equal(20, requirements[ItemType.Wood]);
        Assert.Equal(10, requirements[ItemType.Plank]);
    }

    [Fact]
    public void ImprovedTier_HasComfortableUpgradeRequirements()
    {
        Assert.True(HouseUpgradeConfig.TryGetUpgradeRequirements(HouseTier.Improved, out var nextTier, out var requirements));

        Assert.Equal(HouseTier.Comfortable, nextTier);
        Assert.Equal(30, requirements[ItemType.Plank]);
        Assert.Equal(15, requirements[ItemType.Stone]);
        Assert.Equal(2, requirements[ItemType.Tool]);
    }

    [Fact]
    public void ComfortableTier_IsMaxTier()
    {
        Assert.True(HouseUpgradeConfig.IsMaxTier(HouseTier.Comfortable));
        Assert.False(HouseUpgradeConfig.TryGetUpgradeRequirements(HouseTier.Comfortable, out _, out _));
    }

    [Theory]
    [InlineData(HouseTier.Basic, 65f)]
    [InlineData(HouseTier.Improved, 80f)]
    [InlineData(HouseTier.Comfortable, 100f)]
    public void SleepEnergyRecovery_IncreasesWithTier(HouseTier tier, float expectedRecovery)
    {
        Assert.Equal(expectedRecovery, HouseUpgradeConfig.GetSleepEnergyRecovery(tier));
    }
}