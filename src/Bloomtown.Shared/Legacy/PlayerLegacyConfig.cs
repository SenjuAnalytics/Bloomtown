using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Legacy;

/// <summary>
/// Thresholds, marker resolution, and rare legacy recognition selection.
/// </summary>
public static class PlayerLegacyConfig
{
    /// <summary>NPC who speaks for the village on major contributions (Elsie).</summary>
    public const uint ElderVoiceNpcEntityId = NpcEntityIds.Elsie;

    /// <summary>Game minutes between elder recognition lines during interaction.</summary>
    public const int InteractionRecognitionCooldownGameMinutes = 90;

    /// <summary>Game minutes between legacy ambient recognition comments.</summary>
    public const int AmbientRecognitionCooldownGameMinutes = 75;

    /// <summary>Low chance during talk/greet — recognition should feel special.</summary>
    public const int InteractionRecognitionChancePercent = 10;

    /// <summary>Low chance during ambient checks — village-wide regard is rare.</summary>
    public const int AmbientRecognitionChancePercent = 8;

    /// <summary>
    /// Marker priority for legacy comment selection.
    /// Completed project contributions rank first so dialogue can reference specific builds
    /// (e.g. "the bridge you helped finish") before generic title-based regard.
    /// </summary>
    private static readonly PlayerLegacyMarker[] MarkerPriorityOrder =
    [
        PlayerLegacyMarker.ContributedToWell,
        PlayerLegacyMarker.ContributedToBridge,
        PlayerLegacyMarker.ContributedToWarehouse,
        PlayerLegacyMarker.ElderCandidateTitle,
        PlayerLegacyMarker.RespectedTitle,
        PlayerLegacyMarker.BuilderTitle,
        PlayerLegacyMarker.HelpedCommunityProject,
        PlayerLegacyMarker.HelperTitle,
    ];

    public static bool IsElderVoiceNpc(uint npcEntityId) =>
        npcEntityId == ElderVoiceNpcEntityId;

    public static PlayerLegacyContext BuildContext(
        VillageTitle villageTitle,
        int villageContributionScore,
        IReadOnlyCollection<NpcMemoryType> villageWideMemories,
        IReadOnlyList<byte> completedProjectContributions)
    {
        var markers = PlayerLegacyMarker.None;

        if (villageWideMemories.Contains(NpcMemoryType.HelpedVillageProject))
            markers |= PlayerLegacyMarker.HelpedCommunityProject;

        foreach (var projectId in completedProjectContributions)
        {
            markers |= projectId switch
            {
                VillageProjectBenefitConfig.WellProjectId => PlayerLegacyMarker.ContributedToWell,
                VillageProjectBenefitConfig.BridgeProjectId => PlayerLegacyMarker.ContributedToBridge,
                VillageProjectBenefitConfig.WarehouseProjectId => PlayerLegacyMarker.ContributedToWarehouse,
                _ => PlayerLegacyMarker.None,
            };
        }

        if (villageTitle >= VillageTitle.Helper)
            markers |= PlayerLegacyMarker.HelperTitle;
        if (villageTitle >= VillageTitle.Builder)
            markers |= PlayerLegacyMarker.BuilderTitle;
        if (villageTitle >= VillageTitle.RespectedVillager)
            markers |= PlayerLegacyMarker.RespectedTitle;
        if (villageTitle >= VillageTitle.ElderCandidate)
            markers |= PlayerLegacyMarker.ElderCandidateTitle;

        return new PlayerLegacyContext
        {
            Markers = markers,
            VillageTitle = villageTitle,
            VillageContributionScore = villageContributionScore,
            CompletedProjectContributions = completedProjectContributions,
        };
    }

    /// <summary>
    /// Picks the highest-priority legacy marker present on the player.
    /// Project markers integrate with VillageProjectStateService completion data
    /// passed in via <see cref="PlayerLegacyContext.CompletedProjectContributions"/>.
    /// </summary>
    public static PlayerLegacyMarker? TrySelectRecognitionMarker(PlayerLegacyContext context)
    {
        if (!context.HasRecognition)
            return null;

        foreach (var marker in MarkerPriorityOrder)
        {
            if (context.Markers.HasFlag(marker))
                return marker;
        }

        return null;
    }

    public static string? TryGetElderRecognitionLine(
        NpcInteractionKind kind,
        PlayerLegacyContext context,
        uint variationSeed)
    {
        var marker = TrySelectRecognitionMarker(context);
        if (marker is null)
            return null;

        var lines = PlayerLegacyDialogue.GetElderRecognitionLines(kind, marker.Value);
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetVillageWideAmbientLine(PlayerLegacyContext context, uint variationSeed)
    {
        var marker = TrySelectRecognitionMarker(context);
        if (marker is null)
            return null;

        var lines = PlayerLegacyDialogue.GetVillageWideAmbientLines(marker.Value);
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? TryGetElderAmbientLine(PlayerLegacyContext context, uint variationSeed)
    {
        var marker = TrySelectRecognitionMarker(context);
        if (marker is null)
            return null;

        var lines = PlayerLegacyDialogue.GetElderAmbientLines(marker.Value);
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static bool ShouldTriggerInteractionRecognition(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes)
    {
        var roll = (playerEntityId * 41 + npcEntityId * 23 + (uint)(totalGameMinutes % 997)) % 100;
        return roll < InteractionRecognitionChancePercent;
    }

    public static bool ShouldTriggerAmbientRecognition(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 53 + (uint)(totalGameMinutes % 983)) % 100;
        return roll < AmbientRecognitionChancePercent;
    }

    public static string? FormatCommunityRecognitionStatus(PlayerLegacyContext context)
    {
        if (!context.HasRecognition)
            return null;

        if (context.VillageTitle >= VillageTitle.ElderCandidate)
            return "Community regard: The village speaks of you with deep respect — your legacy here is unmistakable.";

        if (context.VillageTitle >= VillageTitle.RespectedVillager)
            return "Community regard: Villagers hold you in high esteem for what you've built here.";

        if (context.VillageTitle >= VillageTitle.Builder
            || context.Markers.HasFlag(PlayerLegacyMarker.ContributedToWell)
            || context.Markers.HasFlag(PlayerLegacyMarker.ContributedToBridge)
            || context.Markers.HasFlag(PlayerLegacyMarker.ContributedToWarehouse))
        {
            return "Community regard: Bloomtown remembers the projects you helped complete.";
        }

        if (context.VillageTitle >= VillageTitle.Helper
            || context.Markers.HasFlag(PlayerLegacyMarker.HelpedCommunityProject))
        {
            return "Community regard: The villagers seem to appreciate your help.";
        }

        return null;
    }
}