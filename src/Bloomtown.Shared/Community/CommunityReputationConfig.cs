using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Community;

/// <summary>Where a village reaction based on the player's social role may surface.</summary>
public enum VillageReactionSurface : byte
{
    RecurringHelpAcknowledgment = 0,
    InteractionRecognition = 1,
    AmbientRoleComment = 2,
}

/// <summary>
/// Thresholds, chance rolls, and formatting for simple community participation reputation.
/// Tracks how often a player helps and when recurring NPC acknowledgment should surface.
/// </summary>
public static class CommunityReputationConfig
{
    /// <summary>Helps in one activity before recurring acknowledgment can appear.</summary>
    public const int MinHelpsForRecurringAcknowledgment = 3;

    /// <summary>Helps in dominant activity before a named social role emerges.</summary>
    public const int MinHelpsForSocialRole = 5;

    /// <summary>Total helps across all activities for an all-round helper role.</summary>
    public const int MinTotalHelpsForAllRoundRole = 8;

    /// <summary>Chance when eligible after a community-help action.</summary>
    public const int RecurringHelpAcknowledgmentChancePercent = 20;

    /// <summary>Chance during talk/greet when player has a social role.</summary>
    public const int InteractionRecognitionChancePercent = 18;

    /// <summary>Chance during ambient checks when player has a social role.</summary>
    public const int AmbientRoleCommentChancePercent = 10;

    public static readonly TimeSpan RecurringHelpAcknowledgmentCooldown = TimeSpan.FromMinutes(12);
    public static readonly TimeSpan AmbientRoleCommentCooldown = TimeSpan.FromMinutes(10);
    public const int InteractionRecognitionCooldownGameMinutes = 45;

    /// <summary>Subtle mood bonus when a named helper works in their usual area.</summary>
    public const float ConsistentHelperMoodBonus = 1f;

    /// <summary>Subtle social bonus when a named helper works in their usual area.</summary>
    public const float ConsistentHelperSocialBonus = 1f;

    public static CommunityReputationState CreateEmpty() =>
        new(0, 0, 0);

    public static CommunityReputationState Increment(
        CommunityReputationState state,
        CommunityActivityKind activity) =>
        activity switch
        {
            CommunityActivityKind.HelpGarden => state with { HelpGardenCount = state.HelpGardenCount + 1 },
            CommunityActivityKind.HelpMarket => state with { HelpMarketCount = state.HelpMarketCount + 1 },
            CommunityActivityKind.HelpWell => state with { HelpWellCount = state.HelpWellCount + 1 },
            _ => state,
        };

    public static int GetHelpCount(CommunityReputationState state, CommunityActivityKind activity) =>
        activity switch
        {
            CommunityActivityKind.HelpGarden => state.HelpGardenCount,
            CommunityActivityKind.HelpMarket => state.HelpMarketCount,
            CommunityActivityKind.HelpWell => state.HelpWellCount,
            _ => 0,
        };

    /// <summary>Derives the player's light social role from help-frequency counters.</summary>
    public static CommunitySocialRole ResolveSocialRole(CommunityReputationState state)
    {
        if (state.TotalHelpCount < MinHelpsForRecurringAcknowledgment)
            return CommunitySocialRole.None;

        if (state.TotalHelpCount >= MinTotalHelpsForAllRoundRole)
        {
            var dominant = state.DominantActivity;
            if (dominant is null)
                return CommunitySocialRole.AllRoundHelper;

            var dominantCount = GetHelpCount(state, dominant.Value);
            var secondHighest = state.TotalHelpCount - dominantCount;
            if (dominantCount - secondHighest <= 1)
                return CommunitySocialRole.AllRoundHelper;
        }

        var garden = state.HelpGardenCount;
        var market = state.HelpMarketCount;
        var well = state.HelpWellCount;

        if (garden >= MinHelpsForSocialRole && garden > market && garden > well)
            return CommunitySocialRole.GardenHelper;

        if (market >= MinHelpsForSocialRole && market > garden && market > well)
            return CommunitySocialRole.MarketHelper;

        if (well >= MinHelpsForSocialRole && well > garden && well > market)
            return CommunitySocialRole.WellKeeper;

        return CommunitySocialRole.None;
    }

    /// <summary>Returns the player's current dominant social role derived from help frequency.</summary>
    public static CommunitySocialRole GetDominantSocialRole(CommunityReputationState state) =>
        ResolveSocialRole(state);

    /// <summary>Whether a community activity matches the player's earned social role.</summary>
    public static bool IsActivityAlignedWithSocialRole(CommunitySocialRole role, CommunityActivityKind activity) =>
        role switch
        {
            CommunitySocialRole.GardenHelper => activity == CommunityActivityKind.HelpGarden,
            CommunitySocialRole.MarketHelper => activity == CommunityActivityKind.HelpMarket,
            CommunitySocialRole.WellKeeper => activity == CommunityActivityKind.HelpWell,
            CommunitySocialRole.AllRoundHelper => true,
            _ => false,
        };

    /// <summary>Small mood/social bonus for players who already have a matching social role.</summary>
    public static (float MoodBonus, float SocialBonus) GetConsistentHelperEffectBonus(
        CommunityReputationState state,
        CommunityActivityKind activity)
    {
        var role = ResolveSocialRole(state);
        if (role == CommunitySocialRole.None || !IsActivityAlignedWithSocialRole(role, activity))
            return (0f, 0f);

        return (ConsistentHelperMoodBonus, ConsistentHelperSocialBonus);
    }

    /// <summary>Whether the village has seen enough of the player to react at all.</summary>
    public static bool IsEligibleForVillageReaction(CommunityReputationState state)
    {
        if (ResolveSocialRole(state) != CommunitySocialRole.None)
            return true;

        return state.TotalHelpCount >= MinHelpsForRecurringAcknowledgment;
    }

    /// <summary>
    /// Gates village reactions so they only surface when participation warrants acknowledgment.
    /// Chance rolls and cooldowns are enforced separately by the caller.
    /// </summary>
    public static bool IsRightMomentForVillageReaction(
        CommunityReputationState state,
        VillageReactionSurface surface)
    {
        if (!IsEligibleForVillageReaction(state))
            return false;

        return surface switch
        {
            VillageReactionSurface.RecurringHelpAcknowledgment => true,
            VillageReactionSurface.InteractionRecognition => ResolveSocialRole(state) != CommunitySocialRole.None,
            VillageReactionSurface.AmbientRoleComment => ResolveSocialRole(state) != CommunitySocialRole.None,
            _ => false,
        };
    }

    public static bool ShouldTriggerRecurringHelpAcknowledgment(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 61 + (uint)(totalGameMinutes % 971)) % 100;
        return roll < RecurringHelpAcknowledgmentChancePercent;
    }

    public static bool ShouldTriggerInteractionRecognition(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 67 + (uint)(totalGameMinutes % 967)) % 100;
        return roll < InteractionRecognitionChancePercent;
    }

    public static bool ShouldTriggerAmbientRoleComment(uint playerEntityId, long totalGameMinutes, uint attemptCounter)
    {
        var roll = (playerEntityId * 71 + (uint)(totalGameMinutes % 963) + attemptCounter * 9) % 100;
        return roll < AmbientRoleCommentChancePercent;
    }

    public static string? TryGetRecurringHelpAcknowledgment(
        CommunityActivityKind activity,
        CommunityReputationState state,
        uint npcEntityId,
        uint variationSeed)
    {
        if (GetHelpCount(state, activity) < MinHelpsForRecurringAcknowledgment)
            return null;

        var role = ResolveSocialRole(state);

        // Named helpers get dependence-flavored lines; frequent helpers without a role get familiar-presence lines.
        if (role != CommunitySocialRole.None)
        {
            return CommunityReputationDialogue.TryGetRoleDependenceAcknowledgment(
                activity,
                role,
                npcEntityId,
                variationSeed)
                ?? CommunityReputationDialogue.TryGetRecurringHelpAcknowledgment(
                    activity,
                    role,
                    npcEntityId,
                    variationSeed);
        }

        return CommunityReputationDialogue.TryGetFamiliarPresenceAcknowledgment(
            activity,
            npcEntityId,
            variationSeed)
            ?? CommunityReputationDialogue.TryGetRecurringHelpAcknowledgment(
                activity,
                role,
                npcEntityId,
                variationSeed);
    }

    public static string? TryGetInteractionRecognition(
        CommunityReputationState state,
        uint npcEntityId,
        uint variationSeed)
    {
        var role = ResolveSocialRole(state);
        if (role == CommunitySocialRole.None)
            return null;

        return CommunityReputationDialogue.TryGetInteractionRecognition(
            role,
            state.DominantActivity,
            npcEntityId,
            variationSeed);
    }

    public static string? TryGetAmbientRoleComment(
        CommunityReputationState state,
        uint variationSeed)
    {
        var role = ResolveSocialRole(state);
        if (role == CommunitySocialRole.None)
            return null;

        return CommunityReputationDialogue.TryGetAmbientRoleComment(
            role,
            state.DominantActivity,
            variationSeed);
    }

    public static string GetSocialRoleDisplayName(CommunitySocialRole role) =>
        role switch
        {
            CommunitySocialRole.GardenHelper => "regular garden helper",
            CommunitySocialRole.MarketHelper => "familiar market helper",
            CommunitySocialRole.WellKeeper => "well keeper",
            CommunitySocialRole.AllRoundHelper => "all-round village helper",
            _ => string.Empty,
        };

    /// <summary>How the village currently regards the player's earned social role.</summary>
    public static string GetVillageViewOfRole(CommunitySocialRole role) =>
        role switch
        {
            CommunitySocialRole.GardenHelper =>
                "villagers count on you around the garden; Elsie mentions your name often",
            CommunitySocialRole.MarketHelper =>
                "the square feels steadier when you're there; neighbors expect your help on trade days",
            CommunitySocialRole.WellKeeper =>
                "folks treat the well as yours to tend; your consistency hasn't gone unnoticed",
            CommunitySocialRole.AllRoundHelper =>
                "Bloomtown leans on you across shared chores; you're becoming part of the routine",
            _ => string.Empty,
        };

    public static string? FormatParticipationStatus(CommunityReputationState state)
    {
        var role = ResolveSocialRole(state);
        if (role != CommunitySocialRole.None)
        {
            return
                $"Community participation: {GetSocialRoleDisplayName(role)} — {GetVillageViewOfRole(role)}.";
        }

        if (state.TotalHelpCount >= MinHelpsForRecurringAcknowledgment)
        {
            var dominant = state.DominantActivity;
            var areaHint = dominant switch
            {
                CommunityActivityKind.HelpGarden => "the garden crew is getting used to seeing you",
                CommunityActivityKind.HelpMarket => "market regulars are starting to expect you",
                CommunityActivityKind.HelpWell => "neighbors notice when you tend the well",
                _ => "neighbors are beginning to notice you pitch in",
            };

            return
                $"Community participation: {state.TotalHelpCount} help session(s) — {areaHint}.";
        }

        if (state.TotalHelpCount > 0)
        {
            return
                $"Community participation: {state.TotalHelpCount} help session(s) so far — keep showing up to become known in the village.";
        }

        return null;
    }
}