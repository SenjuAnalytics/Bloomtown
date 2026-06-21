using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>Static social pairs beyond Elsie–Tom — flavor only, no evolution in this spike.</summary>
public enum VillageSocialPair : byte
{
    ElsieMira = 0,
    TomHarold = 1,
}

/// <summary>
/// Expanded village social circle: static NPC pair relationships, selection, and status helpers.
/// Elsie–Mira and Tom–Harold are fixed-tone bonds; Elsie–Tom evolution stays in
/// <see cref="NpcInterpersonalRelationshipService"/>.
/// </summary>
public static class VillageSocialCircleConfig
{
    public static readonly TimeSpan GroupSocialMomentCooldown = TimeSpan.FromMinutes(9);

    /// <summary>Very low chance — group moments should feel like rare glimpses of village life.</summary>
    public const int GroupSocialMomentChancePercent = 7;

    /// <summary>Static tone for Elsie–Mira — merchant and gardener as friendly neighbors.</summary>
    public const NpcInterpersonalRelationship ElsieMiraRelationship = NpcInterpersonalRelationship.Friendly;

    /// <summary>Static tone for Tom–Harold — woodsman and elder as practical neighbors.</summary>
    public const NpcInterpersonalRelationship TomHaroldRelationship = NpcInterpersonalRelationship.Neutral;

    public static bool ShouldTriggerGroupSocialMoment(uint playerEntityId, long totalGameMinutes, uint attemptCounter)
    {
        var roll = (playerEntityId * 53 + (uint)(totalGameMinutes % 979) + attemptCounter * 7) % 100;
        return roll < GroupSocialMomentChancePercent;
    }

    /// <summary>Returns the fixed relationship tone for a static social pair.</summary>
    public static NpcInterpersonalRelationship GetPairRelationship(VillageSocialPair pair) =>
        pair switch
        {
            VillageSocialPair.ElsieMira => ElsieMiraRelationship,
            VillageSocialPair.TomHarold => TomHaroldRelationship,
            _ => NpcInterpersonalRelationship.Neutral,
        };

    /// <summary>
    /// NPC-to-NPC line from the expanded social circle.
    /// Prioritizes Elsie–Mira and Tom–Harold pairs before falling back to Elsie–Tom pools.
    /// </summary>
    public static string? TryGetExpandedNpcToNpcComment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId,
        out VillageSocialPair? socialPair)
    {
        return VillageSocialCircleDialogue.TryGetExpandedNpcToNpcLine(
            timeOfDay,
            developmentLevel,
            elsieTomRelationship,
            variationSeed,
            out preferredSpeakerNpcEntityId,
            out socialPair);
    }

    /// <summary>Gossip that widens the village social circle beyond Elsie and Tom.</summary>
    public static string? TryGetSocialCircleGossip(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out VillageGossipKind gossipKind,
        out VillageSocialPair? socialPair)
    {
        return VillageSocialCircleDialogue.TryGetSocialCircleGossip(
            timeOfDay,
            developmentLevel,
            elsieTomRelationship,
            variationSeed,
            out gossipKind,
            out socialPair);
    }

    /// <summary>Small group social moment involving three or more villagers.</summary>
    public static string? TryGetGroupSocialMoment(
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        NpcInterpersonalRelationship elsieTomRelationship,
        uint variationSeed,
        out VillageCommunityMomentKind momentKind)
    {
        return VillageSocialCircleDialogue.TryGetGroupSocialMoment(
            timeOfDay,
            developmentLevel,
            elsieTomRelationship,
            variationSeed,
            out momentKind);
    }

    public static string GetSocialPairDisplayName(VillageSocialPair pair) =>
        pair switch
        {
            VillageSocialPair.ElsieMira => "Elsie–Mira",
            VillageSocialPair.TomHarold => "Tom–Harold",
            _ => "village pair",
        };

    public static string FormatSocialCircleStatus()
    {
        var elsieMira = GetPairRelationship(VillageSocialPair.ElsieMira);
        var tomHarold = GetPairRelationship(VillageSocialPair.TomHarold);

        var miraLine = elsieMira == NpcInterpersonalRelationship.Friendly
            ? "Elsie and Mira trade market gossip like old friends"
            : "Elsie and Mira coordinate around the square";

        var haroldLine = tomHarold == NpcInterpersonalRelationship.Friendly
            ? "Tom and Harold share easy porch talk"
            : "Tom and Harold compare notes when village work needs doing";

        return $"Village social circle: {miraLine}; {haroldLine}.";
    }

    public static bool IsSocialCircleVillager(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Elsie
            or NpcEntityIds.Tom
            or NpcEntityIds.Mira
            or NpcEntityIds.Harold;
}