using Bloomtown.Shared.Needs;

namespace Bloomtown.Shared.Console;

/// <summary>
/// Consistent one-line need-change summaries for activity feedback.
/// </summary>
public static class ActivityFeedbackFormat
{
    public static string FormatNeedChanges(
        float moodBefore,
        float moodAfter,
        float fatigueBefore,
        float fatigueAfter,
        float socialBefore = 0f,
        float socialAfter = 0f,
        bool includeSocial = false)
    {
        var parts = new List<string>();
        var moodDelta = moodAfter - moodBefore;
        var fatigueDelta = fatigueBefore - fatigueAfter;

        if (MathF.Abs(moodDelta) >= 0.5f)
            parts.Add($"Mood {(moodDelta > 0 ? "+" : "")}{moodDelta:F0} → {moodAfter:F0}/{PlayerNeedsConfig.MaxValue:F0}");

        if (fatigueDelta >= 0.5f)
            parts.Add($"Fatigue -{fatigueDelta:F0} → {fatigueAfter:F0}/{PlayerNeedsConfig.MaxValue:F0}");
        else if (fatigueBefore - fatigueAfter <= -0.5f)
        {
            var gain = fatigueAfter - fatigueBefore;
            parts.Add($"Fatigue +{gain:F0} → {fatigueAfter:F0}/{PlayerNeedsConfig.MaxValue:F0}");
        }

        if (includeSocial)
        {
            var socialDelta = socialBefore - socialAfter;
            if (MathF.Abs(socialDelta) >= 0.5f)
            {
                parts.Add(socialDelta > 0
                    ? $"Social -{socialDelta:F0} → {socialAfter:F0}/{PlayerNeedsConfig.MaxValue:F0}"
                    : $"Social +{-socialDelta:F0} → {socialAfter:F0}/{PlayerNeedsConfig.MaxValue:F0}");
            }
        }

        return parts.Count == 0 ? string.Empty : $"▸ {string.Join(" · ", parts)}";
    }
}