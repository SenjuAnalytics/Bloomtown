using Bloomtown.Shared.Console;

namespace Bloomtown.Shared.Tests;

public sealed class ActivityFeedbackFormatTests
{
    [Fact]
    public void FormatNeedChanges_UsesConsistentArrowPrefix()
    {
        var line = ActivityFeedbackFormat.FormatNeedChanges(
            moodBefore: 50f,
            moodAfter: 55f,
            fatigueBefore: 40f,
            fatigueAfter: 35f,
            socialBefore: 60f,
            socialAfter: 50f,
            includeSocial: true);

        Assert.StartsWith("▸", line);
        Assert.Contains("Mood +5", line);
        Assert.Contains("Fatigue -5", line);
        Assert.Contains("Social -10", line);
    }

    [Fact]
    public void FormatNeedChanges_MoodOnlyOmitsFatigueAndSocial()
    {
        var line = ActivityFeedbackFormat.FormatNeedChanges(
            moodBefore: 50f,
            moodAfter: 53f,
            fatigueBefore: 40f,
            fatigueAfter: 40f,
            includeSocial: false);

        Assert.Contains("Mood +3", line);
        Assert.DoesNotContain("Fatigue", line);
        Assert.DoesNotContain("Social", line);
    }
}