using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Chance rolls, cooldowns, and status text for village growth reactivity.
/// Reacts to <see cref="VillageDevelopmentLevel"/> and completed project ids from village state.
/// </summary>
public static class VillageReactivityConfig
{
    public static readonly TimeSpan GrowthReactionCooldown = TimeSpan.FromMinutes(4);
    public static readonly TimeSpan ProjectCompletionReactionCooldown = TimeSpan.FromMinutes(3);

    /// <summary>Game minutes after completion when boosted project reactions stay meaningful.</summary>
    public const int ProjectCompletionReactionWindowGameMinutes = 180;

    public const int LivelyGrowthReactionChancePercent = 18;
    public const int BustlingGrowthReactionChancePercent = 22;
    public const int ProjectCompletionReactionChancePercent = 28;

    public static int GetGrowthReactionChancePercent(VillageDevelopmentLevel developmentLevel) =>
        developmentLevel switch
        {
            VillageDevelopmentLevel.Bustling => BustlingGrowthReactionChancePercent,
            VillageDevelopmentLevel.Lively => LivelyGrowthReactionChancePercent,
            _ => 0,
        };

    public static bool ShouldTriggerGrowthReaction(
        uint playerEntityId,
        VillageDevelopmentLevel developmentLevel,
        long totalGameMinutes)
    {
        var chance = GetGrowthReactionChancePercent(developmentLevel);
        if (chance <= 0)
            return false;

        var roll = (playerEntityId * 59 + (uint)developmentLevel * 17 + (uint)(totalGameMinutes % 977)) % 100;
        return roll < chance;
    }

    public static bool ShouldTriggerProjectCompletionReaction(
        uint playerEntityId,
        byte projectId,
        long totalGameMinutes)
    {
        var roll = (playerEntityId * 61 + projectId * 29 + (uint)(totalGameMinutes % 971)) % 100;
        return roll < ProjectCompletionReactionChancePercent;
    }

    public static bool IsWithinCompletionReactionWindow(long completedAtGameMinute, long currentGameMinute) =>
        currentGameMinute - completedAtGameMinute <= ProjectCompletionReactionWindowGameMinutes;

    /// <summary>
    /// Picks a growth reaction line. Level selects the tone pool; completed projects add specificity.
    /// </summary>
    public static string? TryGetGrowthReactionComment(
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        if (developmentLevel == VillageDevelopmentLevel.Quiet)
            return null;

        if (developmentLevel == VillageDevelopmentLevel.Bustling)
        {
            var bustlingLine = VillageReactivityDialogue.TryGetBustlingGrowthLine(
                completedProjectIds,
                variationSeed);

            if (!string.IsNullOrWhiteSpace(bustlingLine))
                return bustlingLine;
        }

        return VillageReactivityDialogue.TryGetLivelyGrowthLine(
            completedProjectIds,
            variationSeed);
    }

    /// <summary>
    /// Picks a boosted reaction to a recently finished project (Well, Bridge, Warehouse).
    /// </summary>
    public static string? TryGetProjectCompletionReaction(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        return VillageReactivityDialogue.TryGetProjectCompletionReaction(
            projectId,
            developmentLevel,
            variationSeed,
            out preferredSpeakerNpcEntityId);
    }

    /// <summary>
    /// Location feedback when the player stands near a finished project site.
    /// </summary>
    public static string? TryGetProjectSiteGrowthComment(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay,
        uint variationSeed)
    {
        return VillageReactivityDialogue.TryGetProjectSiteGrowthComment(
            projectId,
            developmentLevel,
            timeOfDay,
            variationSeed);
    }

    /// <summary>Alternates Elsie and Tom so project-site reactions are not always from the nearest NPC.</summary>
    public static uint SelectProjectReactionSpeaker(byte projectId, uint variationSeed) =>
        (projectId + variationSeed) % 2 == 0
            ? NpcEntityIds.Elsie
            : NpcEntityIds.Tom;

    public static string FormatVisibleGrowthSummary(
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds)
    {
        return developmentLevel switch
        {
            VillageDevelopmentLevel.Bustling =>
                completedProjectIds.Count >= VillageAtmosphereConfig.BustlingProjectThreshold
                    ? "Visible growth: New improvements are visible everywhere — the village feels lively and busy."
                    : "Visible growth: The village feels lively and busy — change is easy to spot as you walk around.",

            VillageDevelopmentLevel.Lively when completedProjectIds.Count > 0 =>
                "Visible growth: You can see and hear how recent projects changed the village.",

            VillageDevelopmentLevel.Lively =>
                "Visible growth: The village feels warmer and a little busier than before.",

            _ => "Visible growth: Bloomtown is still finding its shape — finished projects will leave a mark.",
        };
    }

    public static string FormatNearbyProjectFeel(byte projectId, VillageDevelopmentLevel developmentLevel)
    {
        var projectName = VillageProjectBenefitConfig.FormatProjectDisplayName(projectId);
        return developmentLevel switch
        {
            VillageDevelopmentLevel.Bustling =>
                $"Nearby project feel: {projectName} — a busy hub where the village shows how far it has come.",
            VillageDevelopmentLevel.Lively =>
                $"Nearby project feel: {projectName} — you can tell this place changed after the community finished it.",
            _ =>
                $"Nearby project feel: {projectName} — quiet proof that Bloomtown is growing.",
        };
    }
}