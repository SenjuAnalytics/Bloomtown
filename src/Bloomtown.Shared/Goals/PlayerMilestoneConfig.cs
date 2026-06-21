using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Village;
using System.Text;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Thresholds, progress hints, completion feedback, and status formatting for personal milestones.
/// </summary>
public static class PlayerMilestoneConfig
{
    public const int TotalMilestoneCount = 7;
    public const int HelpingHandRequired = 3;
    public const int SteadyRhythmRequiredDays = 3;
    public const int VillageRegularRequiredActivities = 4;
    public const int VillageRegularRequiredDays = 3;

    public const float MilestoneMoodReward = 2f;
    public const int MilestoneReputationReward = 1;

    public static readonly PlayerMilestoneKind[] AllMilestones =
    [
        PlayerMilestoneKind.FirstFurnishing,
        PlayerMilestoneKind.ComfortableNest,
        PlayerMilestoneKind.HelpingHand,
        PlayerMilestoneKind.CloseFriend,
        PlayerMilestoneKind.RespectedNeighbor,
        PlayerMilestoneKind.SteadyRhythm,
        PlayerMilestoneKind.VillageRegular,
    ];

    public static PlayerMilestoneProgress CreateDefault() => new();

    public static string GetDisplayName(PlayerMilestoneKind kind) =>
        kind switch
        {
            PlayerMilestoneKind.FirstFurnishing => "First Furnishing",
            PlayerMilestoneKind.ComfortableNest => "Comfortable Nest",
            PlayerMilestoneKind.HelpingHand => "Helping Hand",
            PlayerMilestoneKind.CloseFriend => "Close Friend",
            PlayerMilestoneKind.RespectedNeighbor => "Respected Neighbor",
            PlayerMilestoneKind.SteadyRhythm => "Steady Rhythm",
            PlayerMilestoneKind.VillageRegular => "Village Regular",
            _ => "Unknown",
        };

    public static bool IsMilestoneMet(
        PlayerMilestoneKind kind,
        PlayerMilestoneSnapshot snapshot,
        PlayerMilestoneProgress progress)
    {
        return kind switch
        {
            PlayerMilestoneKind.FirstFurnishing =>
                snapshot.PlacedFurnitureCount >= 1,

            PlayerMilestoneKind.ComfortableNest =>
                snapshot.ComfortScore >= FurnitureComfortConfig.MediumComfortThreshold,

            PlayerMilestoneKind.HelpingHand =>
                snapshot.TotalHelpCount >= HelpingHandRequired,

            PlayerMilestoneKind.CloseFriend =>
                snapshot.FocusCloseFriendCount >= 1,

            PlayerMilestoneKind.RespectedNeighbor =>
                snapshot.FocusCloseFriendCount >= VillageBondRecognitionConfig.MinCloseFriendsForCrossNpcRecognition,

            PlayerMilestoneKind.SteadyRhythm =>
                progress.RhythmAgencyDays.Count >= SteadyRhythmRequiredDays,

            PlayerMilestoneKind.VillageRegular =>
                progress.DailyActivityCount >= VillageRegularRequiredActivities
                && progress.DailyActivityDays.Count >= VillageRegularRequiredDays,

            _ => false,
        };
    }

    public static IReadOnlyList<PlayerMilestoneKind> EvaluateNewMilestones(
        PlayerMilestoneProgress progress,
        PlayerMilestoneSnapshot snapshot)
    {
        var completed = new List<PlayerMilestoneKind>();
        foreach (var kind in AllMilestones)
        {
            if (progress.IsCompleted(kind))
                continue;

            if (IsMilestoneMet(kind, snapshot, progress))
                completed.Add(kind);
        }

        return completed;
    }

    public static PlayerMilestoneKind? GetNextUncompleted(PlayerMilestoneProgress progress)
    {
        foreach (var kind in AllMilestones)
        {
            if (!progress.IsCompleted(kind))
                return kind;
        }

        return null;
    }

    public static string FormatProgressHint(
        PlayerMilestoneKind kind,
        PlayerMilestoneSnapshot snapshot,
        PlayerMilestoneProgress progress)
    {
        return kind switch
        {
            PlayerMilestoneKind.FirstFurnishing =>
                progress.IsCompleted(kind) ? "done" : $"{snapshot.PlacedFurnitureCount}/1 furniture placed",

            PlayerMilestoneKind.ComfortableNest =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"comfort {snapshot.ComfortScore}/{FurnitureComfortConfig.MediumComfortThreshold}",

            PlayerMilestoneKind.HelpingHand =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"{Math.Min(snapshot.TotalHelpCount, HelpingHandRequired)}/{HelpingHandRequired} community helps",

            PlayerMilestoneKind.CloseFriend =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"{snapshot.FocusCloseFriendCount}/1 focus close friend",

            PlayerMilestoneKind.RespectedNeighbor =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"{snapshot.FocusCloseFriendCount}/{VillageBondRecognitionConfig.MinCloseFriendsForCrossNpcRecognition} focus close friends",

            PlayerMilestoneKind.SteadyRhythm =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"{progress.RhythmAgencyDays.Count}/{SteadyRhythmRequiredDays} days with rhythm choices",

            PlayerMilestoneKind.VillageRegular =>
                progress.IsCompleted(kind)
                    ? "done"
                    : $"{progress.DailyActivityCount}/{VillageRegularRequiredActivities} activities across {progress.DailyActivityDays.Count}/{VillageRegularRequiredDays} days",

            _ => string.Empty,
        };
    }

    public static string FormatStatusLine(
        PlayerMilestoneProgress progress,
        PlayerMilestoneSnapshot snapshot)
    {
        if (progress.CompletedCount >= TotalMilestoneCount)
            return "Personal milestones: 7/7 — you've found your own rhythm in Bloomtown.";

        var next = GetNextUncompleted(progress);
        if (next is null)
            return $"Personal milestones: {progress.CompletedCount}/{TotalMilestoneCount}.";

        var nextName = GetDisplayName(next.Value);
        var hint = FormatProgressHint(next.Value, snapshot, progress);

        var builder = new StringBuilder();
        builder.Append($"Personal milestones: {progress.CompletedCount}/{TotalMilestoneCount} · Next: {nextName} ({hint})");

        var approaching = GetApproachingMilestone(progress, snapshot);
        if (approaching is not null && approaching != next)
        {
            builder.Append(
                $" · Also nearing: {GetDisplayName(approaching.Value)} ({FormatProgressHint(approaching.Value, snapshot, progress)})");
        }

        return builder.ToString();
    }

    public static string GetCompletionFeedback(PlayerMilestoneKind kind) =>
        kind switch
        {
            PlayerMilestoneKind.FirstFurnishing =>
                "★ Personal milestone: First Furnishing — your home finally feels like yours. Mood +2.",

            PlayerMilestoneKind.ComfortableNest =>
                "★ Personal milestone: Comfortable Nest — rest comes easier now. Mood +2.",

            PlayerMilestoneKind.HelpingHand =>
                "★ Personal milestone: Helping Hand — Bloomtown notices when you show up. Mood +2 · Reputation +1.",

            PlayerMilestoneKind.CloseFriend =>
                "★ Personal milestone: Close Friend — someone here truly knows you. Mood +2.",

            PlayerMilestoneKind.RespectedNeighbor =>
                "★ Personal milestone: Respected Neighbor — your bonds are shaping village life. Mood +2 · Reputation +1.",

            PlayerMilestoneKind.SteadyRhythm =>
                "★ Personal milestone: Steady Rhythm — your days have a gentle shape now. Mood +2.",

            PlayerMilestoneKind.VillageRegular =>
                "★ Personal milestone: Village Regular — Bloomtown feels familiar in the best way. Mood +2.",

            _ => string.Empty,
        };

    private static PlayerMilestoneKind? GetApproachingMilestone(
        PlayerMilestoneProgress progress,
        PlayerMilestoneSnapshot snapshot)
    {
        PlayerMilestoneKind? closest = null;
        var closestRemaining = int.MaxValue;

        foreach (var kind in AllMilestones)
        {
            if (progress.IsCompleted(kind))
                continue;

            var remaining = GetRemainingProgress(kind, snapshot, progress);
            if (remaining <= 0 || remaining >= closestRemaining)
                continue;

            closestRemaining = remaining;
            closest = kind;
        }

        return closestRemaining <= 2 ? closest : null;
    }

    private static int GetRemainingProgress(
        PlayerMilestoneKind kind,
        PlayerMilestoneSnapshot snapshot,
        PlayerMilestoneProgress progress)
    {
        return kind switch
        {
            PlayerMilestoneKind.FirstFurnishing =>
                Math.Max(0, 1 - snapshot.PlacedFurnitureCount),

            PlayerMilestoneKind.ComfortableNest =>
                Math.Max(0, FurnitureComfortConfig.MediumComfortThreshold - snapshot.ComfortScore),

            PlayerMilestoneKind.HelpingHand =>
                Math.Max(0, HelpingHandRequired - snapshot.TotalHelpCount),

            PlayerMilestoneKind.CloseFriend =>
                Math.Max(0, 1 - snapshot.FocusCloseFriendCount),

            PlayerMilestoneKind.RespectedNeighbor =>
                Math.Max(0, VillageBondRecognitionConfig.MinCloseFriendsForCrossNpcRecognition - snapshot.FocusCloseFriendCount),

            PlayerMilestoneKind.SteadyRhythm =>
                Math.Max(0, SteadyRhythmRequiredDays - progress.RhythmAgencyDays.Count),

            PlayerMilestoneKind.VillageRegular =>
                Math.Max(
                    Math.Max(0, VillageRegularRequiredActivities - progress.DailyActivityCount),
                    Math.Max(0, VillageRegularRequiredDays - progress.DailyActivityDays.Count)),

            _ => int.MaxValue,
        };
    }
}