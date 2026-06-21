using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Tests;

public sealed class VillageProjectBenefitConfigTests
{
    [Theory]
    [InlineData(true, 25f)]
    [InlineData(false, 15f)]
    public void GetWellDrinkEnergyRestore_UsesProjectCompletion(bool projectComplete, float expected)
    {
        Assert.Equal(expected, VillageProjectBenefitConfig.GetWellDrinkEnergyRestore(projectComplete));
    }

    [Theory]
    [InlineData(true, 8)]
    [InlineData(false, 5)]
    public void GetWarehouseStipendCoins_UsesProjectCompletion(bool projectComplete, int expected)
    {
        Assert.Equal(expected, VillageProjectBenefitConfig.GetWarehouseStipendCoins(projectComplete));
    }

    [Theory]
    [InlineData(1, "Village Well", "drink well restores +25 Energy")]
    [InlineData(2, "Repaired Bridge", "less Fatigue near the bridge")]
    [InlineData(3, "Village Warehouse", "collect stipend gives +8 coins")]
    public void FormatHelpers_DescribeBuiltinProjects(byte projectId, string expectedName, string expectedBenefit)
    {
        Assert.Equal(expectedName, VillageProjectBenefitConfig.FormatProjectDisplayName(projectId));
        Assert.Equal(expectedBenefit, VillageProjectBenefitConfig.FormatStatusBenefit(projectId));
    }

    [Fact]
    public void GetCompletionBroadcast_ReturnsNonEmptyLineForBuiltinProjects()
    {
        foreach (var projectId in new byte[] { 1, 2, 3 })
        {
            var line = VillageProjectNpcDialogue.GetCompletionBroadcast(projectId);
            Assert.False(string.IsNullOrWhiteSpace(line));
        }
    }

    [Fact]
    public void TryGetSiteAmbientComment_ReturnsLineForBuiltinProjects()
    {
        foreach (var projectId in new byte[] { 1, 2, 3 })
        {
            var line = VillageProjectNpcDialogue.TryGetSiteAmbientComment(
                projectId,
                VillageDevelopmentLevel.Lively,
                (uint)(projectId + 7));
            Assert.False(string.IsNullOrWhiteSpace(line));
        }
    }
}