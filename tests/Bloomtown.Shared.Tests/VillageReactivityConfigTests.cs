using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class VillageReactivityConfigTests
{
    [Theory]
    [InlineData(VillageDevelopmentLevel.Quiet, 0)]
    [InlineData(VillageDevelopmentLevel.Lively, 18)]
    [InlineData(VillageDevelopmentLevel.Bustling, 22)]
    public void GetGrowthReactionChancePercent_MatchesLevel(
        VillageDevelopmentLevel level,
        int expected)
    {
        Assert.Equal(expected, VillageReactivityConfig.GetGrowthReactionChancePercent(level));
    }

    [Fact]
    public void ShouldTriggerGrowthReaction_UsesLevelBasedThreshold()
    {
        const uint playerId = 5;
        const long totalMinutes = 100;

        var roll = (int)((playerId * 59 + (uint)VillageDevelopmentLevel.Lively * 17 + (uint)(totalMinutes % 977)) % 100);
        Assert.Equal(roll < VillageReactivityConfig.LivelyGrowthReactionChancePercent,
            VillageReactivityConfig.ShouldTriggerGrowthReaction(
                playerId,
                VillageDevelopmentLevel.Lively,
                totalMinutes));
    }

    [Fact]
    public void TryGetGrowthReactionComment_ReturnsNullWhenQuiet()
    {
        var line = VillageReactivityConfig.TryGetGrowthReactionComment(
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            variationSeed: 0);

        Assert.Null(line);
    }

    [Fact]
    public void TryGetGrowthReactionComment_BustlingDiffersFromLively()
    {
        var completed = new[]
        {
            VillageProjectBenefitConfig.WellProjectId,
            VillageProjectBenefitConfig.BridgeProjectId,
        };

        var lively = VillageReactivityConfig.TryGetGrowthReactionComment(
            VillageDevelopmentLevel.Lively,
            completed,
            variationSeed: 1);

        var bustling = VillageReactivityConfig.TryGetGrowthReactionComment(
            VillageDevelopmentLevel.Bustling,
            completed,
            variationSeed: 1);

        Assert.False(string.IsNullOrWhiteSpace(lively));
        Assert.False(string.IsNullOrWhiteSpace(bustling));
        Assert.NotEqual(lively, bustling);
    }

    [Fact]
    public void TryGetProjectCompletionReaction_ReturnsProjectSpecificLine()
    {
        var line = VillageReactivityConfig.TryGetProjectCompletionReaction(
            VillageProjectBenefitConfig.BridgeProjectId,
            VillageDevelopmentLevel.Lively,
            variationSeed: 2,
            out var speaker);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("bridge", line, StringComparison.OrdinalIgnoreCase);
        Assert.True(speaker is NpcEntityIds.Elsie or NpcEntityIds.Tom);
    }

    [Fact]
    public void TryGetProjectSiteGrowthComment_ReflectsDevelopmentLevel()
    {
        var lively = VillageReactivityConfig.TryGetProjectSiteGrowthComment(
            VillageProjectBenefitConfig.WellProjectId,
            VillageDevelopmentLevel.Lively,
            GameTimeOfDay.Morning,
            variationSeed: 0);

        var bustling = VillageReactivityConfig.TryGetProjectSiteGrowthComment(
            VillageProjectBenefitConfig.WellProjectId,
            VillageDevelopmentLevel.Bustling,
            GameTimeOfDay.Morning,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(lively));
        Assert.False(string.IsNullOrWhiteSpace(bustling));
        Assert.NotEqual(lively, bustling);
    }

    [Fact]
    public void FormatVisibleGrowthSummary_MentionsVisibleChangeWhenLively()
    {
        var summary = VillageReactivityConfig.FormatVisibleGrowthSummary(
            VillageDevelopmentLevel.Lively,
            [VillageProjectBenefitConfig.WellProjectId]);

        Assert.Contains("Visible growth", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("see", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsWithinCompletionReactionWindow_RespectsConfiguredDuration()
    {
        Assert.True(VillageReactivityConfig.IsWithinCompletionReactionWindow(100, 200));
        Assert.False(VillageReactivityConfig.IsWithinCompletionReactionWindow(
            100,
            100 + VillageReactivityConfig.ProjectCompletionReactionWindowGameMinutes + 1));
    }
}