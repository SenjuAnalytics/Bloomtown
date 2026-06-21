using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Cooldowns, chance rolls, and comment-pool selection for village social dynamics.
/// Integrates completed project ids from <see cref="VillageProjectStateService"/> and
/// <see cref="VillageDevelopmentLevel"/> when picking NPC-to-NPC lines and gossip.
/// </summary>
public static class VillageCommunityLifeConfig
{
    public static readonly TimeSpan NpcToNpcCommentCooldown = TimeSpan.FromMinutes(6);
    public static readonly TimeSpan VillageGossipCooldown = TimeSpan.FromMinutes(7);

    /// <summary>Low chance per ambient check — NPC social chatter stays rare but a touch more present.</summary>
    public const int NpcToNpcChancePercent = 16;

    /// <summary>Lower chance than NPC-to-NPC — gossip should feel like overheard rumor.</summary>
    public const int VillageGossipChancePercent = 10;

    /// <summary>
    /// NPC-to-NPC pool priority when completed projects exist:
    /// project-aware lines first, then development-level social lines, then general neighbor chatter.
    /// </summary>
    private static readonly byte[] ProjectSelectionOrder =
    [
        VillageProjectBenefitConfig.WellProjectId,
        VillageProjectBenefitConfig.BridgeProjectId,
        VillageProjectBenefitConfig.WarehouseProjectId,
    ];

    public static bool ShouldTriggerNpcToNpcComment(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 43 + (uint)(totalGameMinutes % 991)) % 100;
        return roll < NpcToNpcChancePercent;
    }

    public static bool ShouldTriggerVillageGossip(uint playerEntityId, long totalGameMinutes, uint attemptCounter)
    {
        var roll = (playerEntityId * 47 + (uint)(totalGameMinutes % 983) + attemptCounter * 13) % 100;
        return roll < VillageGossipChancePercent;
    }

    /// <summary>
    /// Picks an NPC-to-NPC social line.
    /// Elsie–Tom <paramref name="interpersonalRelationship"/> selects tone;
    /// completed project ids from village state narrow the pool when available.
    /// </summary>
    public static string? TryGetNpcToNpcComment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId,
        out VillageSocialPair? socialPair)
    {
        preferredSpeakerNpcEntityId = 0;
        socialPair = null;

        if (completedProjectIds.Count > 0 && variationSeed % 3 != 2)
        {
            var projectLine = VillageCommunityLifeDialogue.TryGetProjectNpcToNpcLine(
                timeOfDay,
                developmentLevel,
                completedProjectIds,
                interpersonalRelationship,
                variationSeed,
                out preferredSpeakerNpcEntityId);

            if (!string.IsNullOrWhiteSpace(projectLine))
                return projectLine;
        }

        if (developmentLevel >= VillageDevelopmentLevel.Lively && variationSeed % 3 == 0)
        {
            var developmentLine = VillageCommunityLifeDialogue.TryGetDevelopmentSocialLine(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship,
                variationSeed,
                out preferredSpeakerNpcEntityId);

            if (!string.IsNullOrWhiteSpace(developmentLine))
                return developmentLine;
        }

        // Bond lines surface only when Elsie–Tom rapport is Friendly — warmer, more familiar tone.
        if (interpersonalRelationship == NpcInterpersonalRelationship.Friendly && variationSeed % 4 == 0)
        {
            var bondLine = VillageCommunityLifeDialogue.TryGetInterpersonalBondLine(
                timeOfDay,
                developmentLevel,
                variationSeed,
                out preferredSpeakerNpcEntityId);

            if (!string.IsNullOrWhiteSpace(bondLine))
                return bondLine;
        }

        // Expanded social circle — Elsie–Mira and Tom–Harold static pair chatter.
        if (variationSeed % 5 == 1)
        {
            var circleLine = VillageSocialCircleConfig.TryGetExpandedNpcToNpcComment(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship,
                variationSeed,
                out preferredSpeakerNpcEntityId,
                out socialPair);

            if (!string.IsNullOrWhiteSpace(circleLine))
                return circleLine;
        }

        return VillageCommunityLifeDialogue.TryGetGeneralNpcToNpcLine(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            variationSeed,
            out preferredSpeakerNpcEntityId);
    }

    /// <summary>
    /// Picks village gossip flavor. Completed projects and development level shape which rumors surface.
    /// </summary>
    public static string? TryGetVillageGossip(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind,
        out VillageSocialPair? socialPair)
    {
        gossipKind = VillageGossipKind.NeighborlyRumor;
        socialPair = null;

        if (completedProjectIds.Count > 0 && variationSeed % 3 != 1)
        {
            var projectGossip = VillageCommunityLifeDialogue.TryGetProjectGossip(
                timeOfDay,
                completedProjectIds,
                interpersonalRelationship,
                variationSeed,
                out gossipKind);

            if (!string.IsNullOrWhiteSpace(projectGossip))
                return projectGossip;
        }

        if (completedProjectIds.Count > 0 && variationSeed % 4 == 0)
        {
            var interpersonalProjectGossip = VillageCommunityLifeDialogue.TryGetInterpersonalProjectGossip(
                completedProjectIds,
                interpersonalRelationship,
                variationSeed,
                out gossipKind);

            if (!string.IsNullOrWhiteSpace(interpersonalProjectGossip))
                return interpersonalProjectGossip;
        }

        if (developmentLevel >= VillageDevelopmentLevel.Lively && variationSeed % 5 == 0)
        {
            var moodGossip = VillageCommunityLifeDialogue.TryGetCommunityMoodGossip(
                developmentLevel,
                timeOfDay,
                variationSeed,
                out gossipKind);

            if (!string.IsNullOrWhiteSpace(moodGossip))
                return moodGossip;
        }

        // Personal social-connection gossip — overheard bonds between villagers.
        if (variationSeed % 4 == 0)
        {
            var socialGossip = VillageCommunityLifeDialogue.TryGetSocialConnectionGossip(
                interpersonalRelationship,
                variationSeed,
                out gossipKind);

            if (!string.IsNullOrWhiteSpace(socialGossip))
                return socialGossip;
        }

        // Wider social circle gossip — Mira, Harold, and mixed village dynamics.
        if (variationSeed % 5 == 2)
        {
            var circleGossip = VillageSocialCircleConfig.TryGetSocialCircleGossip(
                timeOfDay,
                developmentLevel,
                interpersonalRelationship,
                variationSeed,
                out gossipKind,
                out socialPair);

            if (!string.IsNullOrWhiteSpace(circleGossip))
                return circleGossip;
        }

        return VillageCommunityLifeDialogue.TryGetNeighborlyGossip(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            variationSeed,
            out gossipKind);
    }

    /// <summary>
    /// Rare emergent social moment shaped by Elsie–Tom relationship tone.
    /// Neutral moments stay practical; Friendly moments feel warmer and more personal.
    /// </summary>
    public static string? TryGetEmergentSocialMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        return VillageCommunityLifeDialogue.TryGetEmergentSocialMoment(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            variationSeed,
            out momentKind);
    }

    /// <summary>Returns the first completed project id in stable priority order for logging.</summary>
    public static byte? TryGetPrimaryCompletedProject(IReadOnlyCollection<byte> completedProjectIds)
    {
        foreach (var projectId in ProjectSelectionOrder)
        {
            if (completedProjectIds.Contains(projectId))
                return projectId;
        }

        return null;
    }

    public static string GetGossipKindDisplayName(VillageGossipKind kind) =>
        kind switch
        {
            VillageGossipKind.ProjectPride => "project pride",
            VillageGossipKind.CommunityMood => "community mood",
            VillageGossipKind.SocialConnection => "social connection",
            VillageGossipKind.WiderSocialCircle => "wider social circle",
            _ => "neighborly rumor",
        };

    public static bool IsKnownVillager(uint npcEntityId) =>
        VillageSocialCircleConfig.IsSocialCircleVillager(npcEntityId);
}