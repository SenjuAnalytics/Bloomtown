using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using System.Text;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Milestone thresholds, eligibility checks, and status formatting for the village legacy goal.
/// Milestones integrate community help, social roles, contribution titles, relationships, and legacy markers.
/// </summary>
public static class PlayerLongTermGoalConfig
{
    public const string GoalTitle = "Building Your Legacy in Bloomtown";

    /// <summary>Community helps before the first milestone unlocks.</summary>
    public const int MinHelpsForPuttingDownRoots = 3;

    /// <summary>Chance for milestone feedback during greet/talk — should feel special.</summary>
    public const int MilestoneInteractionChancePercent = 100;

    /// <summary>Chance for overheard milestone ambient — lower than interaction.</summary>
    public const int MilestoneAmbientChancePercent = 35;

    public const int MilestoneInteractionCooldownGameMinutes = 30;
    public const int MilestoneAmbientCooldownGameMinutes = 25;

    /// <summary>Chance for personal aligned-action narrative feedback after archetype-supporting actions.</summary>
    public const int PersonalAlignedActionFeedbackChancePercent = 34;

    /// <summary>Chance for NPC-specific archetype recognition during greet/talk.</summary>
    public const int NpcArchetypeRecognitionChancePercent = 42;

    public static PlayerLongTermGoalProgress CreateDefault() =>
        new(PlayerLongTermGoalKind.VillageLegacy, PlayerLongTermGoalMilestone.None, null);

    public static string GetMilestoneDisplayName(PlayerLongTermGoalMilestone milestone) =>
        milestone switch
        {
            PlayerLongTermGoalMilestone.PuttingDownRoots => "Putting Down Roots",
            PlayerLongTermGoalMilestone.TrustedNeighbor => "Trusted Neighbor",
            PlayerLongTermGoalMilestone.VillageStory => "Village Story",
            PlayerLongTermGoalMilestone.BloomtownLegacy => "Bloomtown Legacy",
            _ => "Not started",
        };

    /// <summary>Returns the next milestone the player can earn, or null when the goal is complete.</summary>
    public static PlayerLongTermGoalMilestone? GetNextMilestone(PlayerLongTermGoalProgress progress) =>
        progress.HighestCompletedMilestone switch
        {
            PlayerLongTermGoalMilestone.None => PlayerLongTermGoalMilestone.PuttingDownRoots,
            PlayerLongTermGoalMilestone.PuttingDownRoots => PlayerLongTermGoalMilestone.TrustedNeighbor,
            PlayerLongTermGoalMilestone.TrustedNeighbor => PlayerLongTermGoalMilestone.VillageStory,
            PlayerLongTermGoalMilestone.VillageStory => PlayerLongTermGoalMilestone.BloomtownLegacy,
            _ => null,
        };

    /// <summary>
    /// Checks whether a specific milestone's criteria are met.
    /// Social role ties to <see cref="CommunityReputationConfig"/>; titles and legacy tie to contribution systems.
    /// </summary>
    public static bool IsMilestoneMet(
        PlayerLongTermGoalMilestone milestone,
        PlayerLongTermGoalSnapshot snapshot)
    {
        return milestone switch
        {
            PlayerLongTermGoalMilestone.PuttingDownRoots =>
                snapshot.TotalHelpCount >= MinHelpsForPuttingDownRoots,

            PlayerLongTermGoalMilestone.TrustedNeighbor =>
                snapshot.SocialRole != CommunitySocialRole.None,

            PlayerLongTermGoalMilestone.VillageStory =>
                snapshot.VillageTitle >= VillageTitle.Helper
                || snapshot.FriendCount >= 1
                || snapshot.CompletedProjectContributions >= 1,

            PlayerLongTermGoalMilestone.BloomtownLegacy =>
                snapshot.VillageTitle >= VillageTitle.Builder
                || snapshot.HasLegacyRecognition
                || snapshot.CloseFriendCount >= 1,

            _ => false,
        };
    }

    /// <summary>
    /// Advances through all newly earned milestones in order.
    /// Returns milestones completed during this evaluation (may be more than one).
    /// </summary>
    public static IReadOnlyList<PlayerLongTermGoalMilestone> EvaluateNewMilestones(
        PlayerLongTermGoalProgress progress,
        PlayerLongTermGoalSnapshot snapshot)
    {
        var completed = new List<PlayerLongTermGoalMilestone>();
        var current = progress;

        while (true)
        {
            var next = GetNextMilestone(current);
            if (next is null || !IsMilestoneMet(next.Value, snapshot))
                break;

            completed.Add(next.Value);
            var completedAt = next.Value == PlayerLongTermGoalMilestone.BloomtownLegacy
                ? DateTime.UtcNow
                : current.GoalCompletedAtUtc;

            current = new PlayerLongTermGoalProgress(
                current.GoalKind,
                next.Value,
                completedAt,
                current.LegacyArchetype,
                current.Influence,
                current.ActiveFocus);
        }

        return completed;
    }

    public static string FormatGoalStatusLine(
        PlayerLongTermGoalProgress progress,
        PlayerLongTermGoalSnapshot snapshot)
    {
        var archetypeLine = LegacyArchetypeConfig.FormatVillageIdentityStatusLine(progress.LegacyArchetype);
        var perspectiveLine = LegacyArchetypeConfig.FormatVillagePerspectiveLine(progress.LegacyArchetype);

        if (progress.IsComplete)
        {
            return string.IsNullOrWhiteSpace(archetypeLine)
                ? "Long-term goal: Building Your Legacy in Bloomtown — complete. Bloomtown remembers you."
                : $"{archetypeLine} Long-term goal complete — Bloomtown remembers you.";
        }

        var next = GetNextMilestone(progress);
        if (next is null)
            return $"Long-term goal: {GoalTitle} — complete.";

        var nextName = GetMilestoneDisplayName(next.Value);
        var hint = FormatNextMilestoneHint(next.Value, snapshot, progress);
        var completedCount = (int)progress.HighestCompletedMilestone;

        var goalLine = completedCount == 0
            ? $"Long-term goal: {GoalTitle} — next: {nextName}. {hint}"
            : $"Long-term goal: {GoalTitle} — {completedCount}/4 milestones. Next: {nextName}. {hint}";

        var directionHint = LegacyArchetypeAgencyConfig.FormatLegacyDirectionHint(
            progress.LegacyArchetype,
            progress.Influence);
        var focusHint = LegacyArchetypeFocusConfig.FormatActiveFocusStatusLine(
            progress.ActiveFocus,
            progress.Influence);

        var combinedHint = CombineHints(directionHint, focusHint);

        var identityBlock = CombineHints(archetypeLine, perspectiveLine);

        if (string.IsNullOrWhiteSpace(identityBlock))
            return string.IsNullOrWhiteSpace(combinedHint) ? goalLine : $"{goalLine} {combinedHint}";

        return string.IsNullOrWhiteSpace(combinedHint)
            ? $"{identityBlock} {goalLine}"
            : $"{identityBlock} {combinedHint} {goalLine}";
    }

    public static string FormatGoalDetail(
        PlayerLongTermGoalProgress progress,
        PlayerLongTermGoalSnapshot snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine(GoalTitle);
        builder.AppendLine(LegacyArchetypeConfig.FormatVillageIdentityDetailLine(progress.LegacyArchetype));
        var perspective = LegacyArchetypeConfig.FormatVillagePerspectiveLine(progress.LegacyArchetype);
        if (!string.IsNullOrWhiteSpace(perspective))
            builder.AppendLine(perspective);
        builder.AppendLine(LegacyArchetypeAgencyConfig.FormatAgencyGuidanceDetail(
            progress.LegacyArchetype,
            progress.Influence));
        var focusGuidance = LegacyArchetypeFocusConfig.FormatFocusGuidanceDetail(progress.ActiveFocus);
        if (!string.IsNullOrWhiteSpace(focusGuidance))
            builder.AppendLine(focusGuidance);
        builder.AppendLine("A personal journey to become part of Bloomtown's story.");

        AppendMilestoneLine(builder, PlayerLongTermGoalMilestone.PuttingDownRoots, progress, snapshot);
        AppendMilestoneLine(builder, PlayerLongTermGoalMilestone.TrustedNeighbor, progress, snapshot);
        AppendMilestoneLine(builder, PlayerLongTermGoalMilestone.VillageStory, progress, snapshot);
        AppendMilestoneLine(builder, PlayerLongTermGoalMilestone.BloomtownLegacy, progress, snapshot);

        if (progress.IsComplete)
        {
            builder.AppendLine();
            var epithet = progress.LegacyArchetype != LegacyArchetype.None
                ? $" as {LegacyArchetypeConfig.GetIdentityEpithet(progress.LegacyArchetype)}"
                : string.Empty;
            builder.Append($"Status: Complete — your legacy lives in Bloomtown{epithet}.");
        }
        else
        {
            var next = GetNextMilestone(progress);
            if (next is not null)
            {
                builder.AppendLine();
                builder.Append($"Working toward: {GetMilestoneDisplayName(next.Value)} — {FormatNextMilestoneHint(next.Value, snapshot, progress)}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    public static string? TryGetMilestoneFeedbackLine(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype,
        uint variationSeed) =>
        PlayerLongTermGoalDialogue.TryGetMilestoneFeedbackLine(milestone, archetype, variationSeed);

    public static string? TryGetMilestoneAmbientLine(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype,
        uint variationSeed) =>
        PlayerLongTermGoalDialogue.TryGetMilestoneAmbientLine(milestone, archetype, variationSeed);

    public static string? TryGetNpcArchetypeRecognitionLine(
        uint npcEntityId,
        LegacyArchetype archetype,
        uint variationSeed) =>
        PlayerLongTermGoalDialogue.TryGetNpcArchetypeRecognitionLine(npcEntityId, archetype, variationSeed);

    public static string? TryGetPersonalAlignedActionFeedback(
        LegacyArchetype archetype,
        LegacyAlignedActionKind action,
        uint variationSeed) =>
        PlayerLongTermGoalDialogue.TryGetPersonalAlignedActionFeedback(archetype, action, variationSeed);

    public static string? TryGetNarrativeDirectionHint(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        uint variationSeed)
    {
        var leading = LegacyArchetypeAgencyConfig.GetLeadingInfluencePath(influence);
        return PlayerLongTermGoalDialogue.TryGetNarrativeDirectionHint(
            detectedArchetype,
            leading,
            influence,
            variationSeed);
    }

    public static bool ShouldShowPersonalAlignedActionFeedback(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 89 + (uint)(totalGameMinutes % 877)) % 100;
        return roll < PersonalAlignedActionFeedbackChancePercent;
    }

    public static bool ShouldTriggerNpcArchetypeRecognition(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 103 + (uint)(totalGameMinutes % 919)) % 100;
        return roll < NpcArchetypeRecognitionChancePercent;
    }

    public static bool ShouldTriggerMilestoneInteraction(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 67 + (uint)(totalGameMinutes % 967)) % 100;
        return roll < MilestoneInteractionChancePercent;
    }

    public static bool ShouldTriggerMilestoneAmbient(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 71 + (uint)(totalGameMinutes % 961)) % 100;
        return roll < MilestoneAmbientChancePercent;
    }

    private static void AppendMilestoneLine(
        StringBuilder builder,
        PlayerLongTermGoalMilestone milestone,
        PlayerLongTermGoalProgress progress,
        PlayerLongTermGoalSnapshot snapshot)
    {
        var name = GetMilestoneDisplayName(milestone);
        string status;

        if (progress.HighestCompletedMilestone >= milestone)
        {
            status = "[done]";
        }
        else if (IsMilestoneMet(milestone, snapshot)
                 && GetNextMilestone(progress) == milestone)
        {
            status = "[ready]";
        }
        else
        {
            status = "[—]";
        }

        // Personal milestone flavor for every step once the village has named the player's archetype.
        var flavor = progress.LegacyArchetype != LegacyArchetype.None
            ? PlayerLongTermGoalDialogue.TryGetMilestonePersonalFlavor(milestone, progress.LegacyArchetype) ?? name
            : name;

        builder.AppendLine($"  {status} {flavor} — {FormatMilestoneRequirement(milestone)}");
    }

    private static string FormatMilestoneRequirement(PlayerLongTermGoalMilestone milestone) =>
        milestone switch
        {
            PlayerLongTermGoalMilestone.PuttingDownRoots =>
                $"help the village {MinHelpsForPuttingDownRoots}+ times",
            PlayerLongTermGoalMilestone.TrustedNeighbor =>
                "earn a community social role (garden/market/well helper)",
            PlayerLongTermGoalMilestone.VillageStory =>
                "reach Helper title, befriend a villager, or contribute to a finished project",
            PlayerLongTermGoalMilestone.BloomtownLegacy =>
                "reach Builder title, earn village legacy recognition, or gain a close friend",
            _ => string.Empty,
        };

    private static string FormatNextMilestoneHint(
        PlayerLongTermGoalMilestone next,
        PlayerLongTermGoalSnapshot snapshot,
        PlayerLongTermGoalProgress progress)
    {
        return next switch
        {
            PlayerLongTermGoalMilestone.PuttingDownRoots =>
                $"({snapshot.TotalHelpCount}/{MinHelpsForPuttingDownRoots} helps)",

            PlayerLongTermGoalMilestone.TrustedNeighbor =>
                snapshot.SocialRole != CommunitySocialRole.None
                    ? "social role earned — milestone ready"
                    : $"keep helping to earn a social role ({snapshot.TotalHelpCount} helps so far)",

            PlayerLongTermGoalMilestone.VillageStory =>
                FormatVillageStoryHint(snapshot),

            PlayerLongTermGoalMilestone.BloomtownLegacy =>
                FormatLegacyHint(snapshot),

            _ => string.Empty,
        };
    }

    private static string FormatVillageStoryHint(PlayerLongTermGoalSnapshot snapshot)
    {
        if (snapshot.VillageTitle >= VillageTitle.Helper)
            return $"Helper title earned ({VillageTitleDisplay.GetName(snapshot.VillageTitle)})";
        if (snapshot.FriendCount > 0)
            return $"{snapshot.FriendCount} friend bond(s) formed";
        if (snapshot.CompletedProjectContributions > 0)
            return $"contributed to {snapshot.CompletedProjectContributions} finished project(s)";
        return "reach Helper title, befriend someone, or help finish a village project";
    }

    private static string FormatLegacyHint(PlayerLongTermGoalSnapshot snapshot)
    {
        if (snapshot.VillageTitle >= VillageTitle.Builder)
            return $"Builder title earned ({VillageTitleDisplay.GetName(snapshot.VillageTitle)})";
        if (snapshot.HasLegacyRecognition)
            return "village legacy recognition earned";
        if (snapshot.CloseFriendCount > 0)
            return $"{snapshot.CloseFriendCount} close friend(s)";
        return "reach Builder title, earn legacy recognition, or deepen a close friendship";
    }

    private static string? CombineHints(string? directionHint, string? focusHint)
    {
        if (string.IsNullOrWhiteSpace(directionHint))
            return focusHint;

        if (string.IsNullOrWhiteSpace(focusHint))
            return directionHint;

        return $"{directionHint} {focusHint}";
    }
}