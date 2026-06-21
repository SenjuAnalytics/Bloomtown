using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Cooldowns, chance rolls, and status text for village social fabric —
/// community moments, gossip weighting, and player community-help reactions.
/// </summary>
public static class VillageSocialFabricConfig
{
    public static readonly TimeSpan CommunityMomentCooldown = TimeSpan.FromMinutes(8);

    /// <summary>Prevents ambient spam when the player performs several help actions in a row.</summary>
    public static readonly TimeSpan CommunityHelpAmbientReactionCooldown = TimeSpan.FromMinutes(3);

    /// <summary>Very low chance — community moments should feel like rare glimpses of village life.</summary>
    public const int CommunityMomentChancePercent = 6;

    public static bool ShouldTriggerCommunityMoment(uint playerEntityId, long totalGameMinutes, uint attemptCounter)
    {
        var roll = (playerEntityId * 59 + (uint)(totalGameMinutes % 977) + attemptCounter * 11) % 100;
        return roll < CommunityMomentChancePercent;
    }

    /// <summary>
    /// Picks a rare community moment line shaped by development level and Elsie–Tom relationship tone.
    /// </summary>
    public static string? TryGetCommunityMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        return VillageSocialFabricDialogue.TryGetCommunityMoment(
            timeOfDay,
            developmentLevel,
            interpersonalRelationship,
            variationSeed,
            out momentKind);
    }

    /// <summary>
    /// Secondary ambient reaction when a player completes a community-help activity nearby.
    /// Tone follows Elsie–Tom relationship and which activity was performed.
    /// </summary>
    public static string? TryGetCommunityHelpAmbientReaction(
        CommunityActivityKind activity,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint npcEntityId,
        uint variationSeed)
    {
        return VillageSocialFabricDialogue.TryGetCommunityHelpAmbientReaction(
            activity,
            interpersonalRelationship,
            npcEntityId,
            variationSeed);
    }

    /// <summary>Short follow-up ambient line after community help — village-wide social echo.</summary>
    public static string? TryGetCommunityHelpFollowUp(
        CommunityActivityKind activity,
        NpcInterpersonalRelationship interpersonalRelationship,
        uint variationSeed) =>
        VillageSocialFabricDialogue.TryGetCommunityHelpFollowUp(
            activity,
            interpersonalRelationship,
            variationSeed);

    public static string GetCommunityMomentDisplayName(VillageCommunityMomentKind kind) =>
        kind switch
        {
            VillageCommunityMomentKind.WarmGathering => "warm gathering",
            VillageCommunityMomentKind.SmallPreparation => "small preparation",
            VillageCommunityMomentKind.DistantCluster => "distant cluster",
            VillageCommunityMomentKind.NeighborlyPause => "neighborly pause",
            VillageCommunityMomentKind.GroupGathering => "group gathering",
            _ => "community moment",
        };

    /// <summary>
    /// Status-line flavor so the player senses they belong in the village social fabric.
    /// </summary>
    public static string FormatSocialFabricStatus(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        PlayerLegacyContext? legacyContext = null)
    {
        var belonging = FormatBelongingHint(legacyContext);
        var rhythm = (timeOfDay, developmentLevel) switch
        {
            (GameTimeOfDay.Morning, VillageDevelopmentLevel.Bustling) =>
                "Morning social life: greetings ripple lane to lane — you are part of the bustle.",
            (GameTimeOfDay.Morning, VillageDevelopmentLevel.Lively) =>
                "Morning social life: neighbors trade small talk as chores begin — the village is waking together.",
            (GameTimeOfDay.Morning, _) =>
                "Morning social life: quiet hellos and shared routines — Bloomtown knows its regular faces.",

            (GameTimeOfDay.Afternoon, VillageDevelopmentLevel.Bustling) =>
                "Afternoon social life: errands overlap and stories get passed along — you fit right in.",
            (GameTimeOfDay.Afternoon, VillageDevelopmentLevel.Lively) =>
                "Afternoon social life: familiar faces cross paths often — the village feels stitched together.",
            (GameTimeOfDay.Afternoon, _) =>
                "Afternoon social life: unhurried check-ins between neighbors — a gentle, knowing pace.",

            (GameTimeOfDay.Evening, VillageDevelopmentLevel.Bustling) =>
                "Evening social life: porches and doorsteps hum softly — you are welcome in the fold.",
            (GameTimeOfDay.Evening, VillageDevelopmentLevel.Lively) =>
                "Evening social life: supper talk drifts between houses — Bloomtown shares its evenings.",
            (GameTimeOfDay.Evening, _) =>
                "Evening social life: softer voices and familiar nods — the village closes the day together.",

            (GameTimeOfDay.Night, VillageDevelopmentLevel.Bustling) =>
                "Night social life: lanterns and late goodnights — even after dark, you are not a stranger here.",
            (GameTimeOfDay.Night, VillageDevelopmentLevel.Lively) =>
                "Night social life: a restful hush, but neighbors still look out for one another.",
            (GameTimeOfDay.Night, _) =>
                "Night social life: deep calm — the village sleeps, but you still belong to it.",

            _ => "Village social life: Bloomtown carries on with the gentle rhythm of people who know each other.",
        };

        return string.IsNullOrWhiteSpace(belonging) ? rhythm : $"{rhythm} {belonging}";
    }

    private static string? FormatBelongingHint(PlayerLegacyContext? context)
    {
        if (context is null || !context.HasRecognition)
            return null;

        if (context.VillageTitle >= VillageTitle.RespectedVillager)
            return "Villagers greet you by name more often now.";

        if (context.VillageTitle >= VillageTitle.Helper
            || context.Markers.HasFlag(PlayerLegacyMarker.HelpedCommunityProject))
            return "People seem to remember when you lend a hand.";

        return null;
    }
}