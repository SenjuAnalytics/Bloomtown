using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Tests;

public sealed class VillageAtmosphereConfigTests
{
    [Theory]
    [InlineData(0, VillageDevelopmentLevel.Quiet)]
    [InlineData(1, VillageDevelopmentLevel.Lively)]
    [InlineData(2, VillageDevelopmentLevel.Lively)]
    [InlineData(3, VillageDevelopmentLevel.Bustling)]
    [InlineData(5, VillageDevelopmentLevel.Bustling)]
    public void CalculateLevel_UsesCompletedProjectCount(int completedCount, VillageDevelopmentLevel expected)
    {
        Assert.Equal(expected, VillageAtmosphereConfig.CalculateLevel(completedCount));
    }

    [Theory]
    [InlineData(VillageDevelopmentLevel.Quiet, "Quiet")]
    [InlineData(VillageDevelopmentLevel.Lively, "Lively")]
    [InlineData(VillageDevelopmentLevel.Bustling, "Bustling")]
    public void GetDisplayName_MatchesLevel(VillageDevelopmentLevel level, string expected)
    {
        Assert.Equal(expected, VillageAtmosphereConfig.GetDisplayName(level));
    }

    [Fact]
    public void GetStatusHeadline_IncludesAtmosphereLabel()
    {
        Assert.Contains("Lively", VillageAtmosphereConfig.GetStatusHeadline(VillageDevelopmentLevel.Lively));
        Assert.Contains("Quiet", VillageAtmosphereConfig.GetStatusHeadline(VillageDevelopmentLevel.Quiet));
    }

    [Theory]
    [InlineData(VillageDevelopmentLevel.Quiet, 8.0)]
    [InlineData(VillageDevelopmentLevel.Lively, 6.0)]
    [InlineData(VillageDevelopmentLevel.Bustling, 5.0)]
    public void GetAmbientCheckIntervalSeconds_ShortensAsVillageGrows(
        VillageDevelopmentLevel level,
        double expected)
    {
        Assert.Equal(expected, VillageAtmosphereConfig.GetAmbientCheckIntervalSeconds(level));
    }

    [Fact]
    public void TryGetGeneralAmbientComment_ReturnsNullWhenQuiet()
    {
        var line = VillageAtmosphereConfig.TryGetGeneralAmbientComment(
            VillageDevelopmentLevel.Quiet,
            [VillageProjectBenefitConfig.WellProjectId],
            3);

        Assert.Null(line);
    }

    [Fact]
    public void TryGetGeneralAmbientComment_ReturnsProjectAwareLineWhenLively()
    {
        var line = VillageAtmosphereConfig.TryGetGeneralAmbientComment(
            VillageDevelopmentLevel.Lively,
            [VillageProjectBenefitConfig.WellProjectId],
            1);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("well", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetInteractionFlavor_ReturnsNullWhenQuiet()
    {
        var line = VillageAtmosphereConfig.TryGetInteractionFlavor(
            VillageDevelopmentLevel.Quiet,
            NpcInteractionFlavorKind.Talk,
            2);

        Assert.Null(line);
    }

    [Fact]
    public void TryGetInteractionFlavor_ReturnsLineWhenBustling()
    {
        var line = VillageAtmosphereConfig.TryGetInteractionFlavor(
            VillageDevelopmentLevel.Bustling,
            NpcInteractionFlavorKind.Greet,
            4);

        Assert.False(string.IsNullOrWhiteSpace(line));
    }

    [Theory]
    [InlineData(VillageDevelopmentLevel.Quiet)]
    [InlineData(VillageDevelopmentLevel.Lively)]
    [InlineData(VillageDevelopmentLevel.Bustling)]
    public void TryGetSiteAmbientComment_ReturnsLinesForEachLevel(VillageDevelopmentLevel level)
    {
        foreach (var projectId in new byte[] { 1, 2, 3 })
        {
            var line = VillageProjectNpcDialogue.TryGetSiteAmbientComment(projectId, level, projectId);
            Assert.False(string.IsNullOrWhiteSpace(line));
        }
    }

    [Fact]
    public void GetActiveBonuses_ReturnsDefaultsWhenQuiet()
    {
        var bonuses = VillageAtmosphereConfig.GetActiveBonuses(VillageDevelopmentLevel.Quiet);

        Assert.Equal(1f, bonuses.FatigueRiseMultiplier);
        Assert.Equal(0f, bonuses.PassiveMoodRecoveryPerGameMinute);
        Assert.False(bonuses.HasPassiveBonus);
    }

    [Fact]
    public void GetActiveBonuses_Lively_SlowsFatigueAndSupportsMood()
    {
        var bonuses = VillageAtmosphereConfig.GetActiveBonuses(VillageDevelopmentLevel.Lively);

        Assert.True(bonuses.FatigueRiseMultiplier < 1f);
        Assert.True(bonuses.PassiveMoodRecoveryPerGameMinute > 0f);
        Assert.True(bonuses.HasPassiveBonus);
    }

    [Fact]
    public void GetActiveBonuses_Bustling_IsStrongerThanLively()
    {
        var lively = VillageAtmosphereConfig.GetActiveBonuses(VillageDevelopmentLevel.Lively);
        var bustling = VillageAtmosphereConfig.GetActiveBonuses(VillageDevelopmentLevel.Bustling);

        Assert.True(bustling.FatigueRiseMultiplier < lively.FatigueRiseMultiplier);
        Assert.True(bustling.PassiveMoodRecoveryPerGameMinute > lively.PassiveMoodRecoveryPerGameMinute);
        Assert.True(bustling.MoodDecayUnderStressMultiplier < 1f);
    }

    [Theory]
    [InlineData(VillageDevelopmentLevel.Quiet, 0)]
    [InlineData(VillageDevelopmentLevel.Lively, 2)]
    [InlineData(VillageDevelopmentLevel.Bustling, 3)]
    public void FormatActiveBonusLines_MatchesDevelopmentLevel(
        VillageDevelopmentLevel level,
        int expectedCount)
    {
        Assert.Equal(expectedCount, VillageAtmosphereConfig.FormatActiveBonusLines(level).Count);
    }

    [Fact]
    public void TryGetLevelExclusiveAmbientComment_ReturnsNullWhenQuiet()
    {
        Assert.Null(VillageAtmosphereConfig.TryGetLevelExclusiveAmbientComment(
            VillageDevelopmentLevel.Quiet,
            1));
    }

    [Theory]
    [InlineData(VillageDevelopmentLevel.Lively)]
    [InlineData(VillageDevelopmentLevel.Bustling)]
    public void TryGetLevelExclusiveAmbientComment_ReturnsLineWhenDeveloped(VillageDevelopmentLevel level)
    {
        var line = VillageAtmosphereConfig.TryGetLevelExclusiveAmbientComment(level, 3);
        Assert.False(string.IsNullOrWhiteSpace(line));
    }
}