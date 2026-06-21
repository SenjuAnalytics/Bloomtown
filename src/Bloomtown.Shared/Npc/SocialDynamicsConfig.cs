using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Npc;

/// <summary>
/// Thresholds, chance rolls, and bonuses for meaningful-but-light social dynamics.
/// Information tips stay rare; friend bonuses stay subtle.
/// </summary>
public static class SocialDynamicsConfig
{
    public static readonly TimeSpan LightInfoCooldown = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan PersonalHabitCooldown = TimeSpan.FromMinutes(20);
    public static readonly TimeSpan ContextualAmbientCooldown = TimeSpan.FromMinutes(11);

    public const int LightInfoChancePercent = 12;
    public const int PersonalHabitChancePercent = 14;
    public const int ContextualAmbientChancePercent = 9;

    public const float FriendInteractionMoodBonus = 1f;
    public const float FriendInteractionSocialBonus = 1f;
    public const float SocialRoleInteractionMoodBonus = 0.5f;
    public const float SocialRoleInteractionSocialBonus = 0.5f;

    public const int MinHelpsForHabitRecognition = 3;

    public static bool IsInfoSharingNpc(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Elsie or NpcEntityIds.Mira or NpcEntityIds.Harold or NpcEntityIds.Greta;

    /// <summary>Whether greet/talk deserves a small mood/social bump beyond the baseline.</summary>
    public static bool QualifiesForBetterTreatment(
        RelationshipTier tier,
        CommunitySocialRole socialRole) =>
        tier >= RelationshipTier.Friend || socialRole != CommunitySocialRole.None;

    /// <summary>Subtle interaction bonus — friends and recognized helpers feel slightly more uplifted.</summary>
    public static (float MoodBonus, float SocialBonus) GetInteractionBonus(
        RelationshipTier tier,
        CommunitySocialRole socialRole)
    {
        var mood = 0f;
        var social = 0f;

        if (tier >= RelationshipTier.Friend)
        {
            mood += FriendInteractionMoodBonus;
            social += FriendInteractionSocialBonus;
        }

        if (socialRole != CommunitySocialRole.None)
        {
            mood += SocialRoleInteractionMoodBonus;
            social += SocialRoleInteractionSocialBonus;
        }

        return (mood, social);
    }

    public static bool ShouldTriggerLightInfo(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 79 + (uint)(totalGameMinutes % 953)) % 100;
        return roll < LightInfoChancePercent;
    }

    public static bool ShouldTriggerPersonalHabit(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 83 + (uint)(totalGameMinutes % 947)) % 100;
        return roll < PersonalHabitChancePercent;
    }

    public static bool ShouldTriggerContextualAmbient(
        uint playerEntityId,
        long totalGameMinutes,
        uint attemptCounter)
    {
        var roll = (playerEntityId * 89 + (uint)(totalGameMinutes % 941) + attemptCounter * 5) % 100;
        return roll < ContextualAmbientChancePercent;
    }

    public static bool IsEligibleForPersonalHabit(
        RelationshipTier tier,
        CommunityReputationState reputation) =>
        tier >= RelationshipTier.Acquaintance
        && (tier >= RelationshipTier.Friend
            || reputation.TotalHelpCount >= MinHelpsForHabitRecognition
            || CommunityReputationConfig.GetDominantSocialRole(reputation) != CommunitySocialRole.None);

    /// <summary>Light summary of how the village socially regards the player.</summary>
    public static string FormatSocialStanding(
        int friendCount,
        int closeFriendCount,
        int acquaintanceCount,
        CommunityReputationState reputation)
    {
        var role = CommunityReputationConfig.GetDominantSocialRole(reputation);
        var roleHint = role != CommunitySocialRole.None
            ? $"recognized as a {CommunityReputationConfig.GetSocialRoleDisplayName(role)}"
            : reputation.TotalHelpCount >= MinHelpsForHabitRecognition
                ? "noticed for pitching in around the village"
                : string.Empty;

        if (closeFriendCount > 0)
        {
            var closeness = closeFriendCount == 1
                ? "one close friend in Bloomtown"
                : $"{closeFriendCount} close friends in Bloomtown";
            return string.IsNullOrWhiteSpace(roleHint)
                ? $"Social standing: well known — {closeness}."
                : $"Social standing: well known — {closeness}, {roleHint}.";
        }

        if (friendCount > 0)
        {
            var friends = friendCount == 1
                ? "one friendly bond"
                : $"{friendCount} friendly bonds";
            return string.IsNullOrWhiteSpace(roleHint)
                ? $"Social standing: settling in — {friends} with villagers."
                : $"Social standing: settling in — {friends}, {roleHint}.";
        }

        if (acquaintanceCount > 0)
        {
            return string.IsNullOrWhiteSpace(roleHint)
                ? "Social standing: newcomers still learning your face — a few acquaintances so far."
                : $"Social standing: newcomers still learning your face — {roleHint}.";
        }

        return string.IsNullOrWhiteSpace(roleHint)
            ? "Social standing: still a stranger to most of Bloomtown — greetings will warm with time."
            : $"Social standing: still new here, but {roleHint}.";
    }

    public static string? TryGetLightInfoTip(
        uint npcEntityId,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed) =>
        SocialDynamicsDialogue.TryGetLightInfoTip(
            npcEntityId,
            timeOfDay,
            developmentLevel,
            completedProjectIds,
            variationSeed);

    public static string? TryGetPersonalHabitLine(
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        uint variationSeed) =>
        SocialDynamicsDialogue.TryGetPersonalHabitLine(
            npcEntityId,
            tier,
            reputation,
            variationSeed);

    public static string? TryGetContextualAmbientComment(
        uint npcEntityId,
        RelationshipTier tier,
        CommunityReputationState reputation,
        NpcInterpersonalRelationship elsieTomRelationship,
        GameTimeOfDay timeOfDay,
        VillageDevelopmentLevel developmentLevel,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed) =>
        SocialDynamicsDialogue.TryGetContextualAmbientComment(
            npcEntityId,
            tier,
            reputation,
            elsieTomRelationship,
            timeOfDay,
            developmentLevel,
            completedProjectIds,
            variationSeed);
}