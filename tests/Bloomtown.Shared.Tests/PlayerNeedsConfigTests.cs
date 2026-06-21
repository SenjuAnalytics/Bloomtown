using Bloomtown.Shared.Needs;

namespace Bloomtown.Shared.Tests;

public sealed class PlayerNeedsConfigTests
{
    [Theory]
    [InlineData(70f, "Good")]
    [InlineData(60f, "Good")]
    [InlineData(45f, "Okay")]
    [InlineData(29f, "Low")]
    public void FormatMoodLabel_UsesThresholds(float mood, string expected)
    {
        Assert.Equal(expected, PlayerNeedsConfig.FormatMoodLabel(mood));
    }

    [Theory]
    [InlineData(10f, "Rested")]
    [InlineData(29f, "Rested")]
    [InlineData(45f, "Tired")]
    [InlineData(70f, "Exhausted")]
    [InlineData(90f, "Exhausted")]
    public void FormatFatigueLabel_UsesThresholds(float fatigue, string expected)
    {
        Assert.Equal(expected, PlayerNeedsConfig.FormatFatigueLabel(fatigue));
    }

    [Theory]
    [InlineData(10f, "Connected")]
    [InlineData(29f, "Connected")]
    [InlineData(45f, "Okay")]
    [InlineData(60f, "Lonely")]
    [InlineData(80f, "Lonely")]
    public void FormatSocialLabel_UsesThresholds(float socialNeed, string expected)
    {
        Assert.Equal(expected, PlayerNeedsConfig.FormatSocialLabel(socialNeed));
    }

    [Fact]
    public void GetGatherDurationMultiplier_StacksFatigueAndMoodPenalties()
    {
        var restedGoodMood = PlayerNeedsConfig.GetGatherDurationMultiplier(70f, 20f);
        var exhausted = PlayerNeedsConfig.GetGatherDurationMultiplier(70f, 75f);
        var lowMood = PlayerNeedsConfig.GetGatherDurationMultiplier(20f, 20f);
        var both = PlayerNeedsConfig.GetGatherDurationMultiplier(20f, 75f);

        Assert.Equal(1.0, restedGoodMood);
        Assert.Equal(1.2, exhausted);
        Assert.Equal(1.1, lowMood);
        Assert.Equal(1.32, both, precision: 2);
    }

    [Theory]
    [InlineData(0, 3f)]
    [InlineData(20, 3f)]
    [InlineData(21, 5f)]
    [InlineData(40, 5f)]
    [InlineData(41, 8f)]
    public void GetSleepMoodGain_UsesComfortTiers(int comfortScore, float expectedGain)
    {
        Assert.Equal(expectedGain, PlayerNeedsConfig.GetSleepMoodGain(comfortScore));
    }

    [Theory]
    [InlineData(69f, 59f, 69f, false)]
    [InlineData(70f, 30f, 30f, true)]
    [InlineData(30f, 60f, 30f, true)]
    [InlineData(30f, 30f, 70f, true)]
    public void IsUnderStress_DetectsBadConditions(float fatigue, float socialNeed, float hunger, bool expected)
    {
        Assert.Equal(expected, PlayerNeedsConfig.IsUnderStress(fatigue, socialNeed, hunger));
    }

    [Fact]
    public void Clamp_KeepsValuesInRange()
    {
        Assert.Equal(0f, PlayerNeedsConfig.Clamp(-5f));
        Assert.Equal(100f, PlayerNeedsConfig.Clamp(150f));
        Assert.Equal(50f, PlayerNeedsConfig.Clamp(50f));
    }
}