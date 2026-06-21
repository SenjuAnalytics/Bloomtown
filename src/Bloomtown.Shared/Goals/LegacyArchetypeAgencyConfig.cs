using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Light player agency over legacy archetype: influence points from consistent actions,
/// archetype-specific action bonuses, and clear direction hints for status/goal commands.
/// </summary>
public static class LegacyArchetypeAgencyConfig
{
    public const int InfluencePointsPerAction = 1;
    public const int MaxInfluencePerPath = 24;

    /// <summary>Each influence point nudges archetype scoring — subtle but accumulates with habit.</summary>
    public const int InfluenceScoreBonusPerPoint = 6;

    /// <summary>Special archetype action flavor/bonus unlocks after a few aligned choices.</summary>
    public const int MinInfluenceForSpecialAction = 4;

    public const float CaretakerHelpMoodBonus = 1f;
    public const int InfluenceGainFeedbackChancePercent = 28;

    /// <summary>Maps community-help location to the legacy path it reinforces.</summary>
    public static LegacyArchetype? GetInfluenceForCommunityActivity(CommunityActivityKind activity) =>
        activity switch
        {
            CommunityActivityKind.HelpGarden => LegacyArchetype.Caretaker,
            CommunityActivityKind.HelpWell => LegacyArchetype.Caretaker,
            CommunityActivityKind.HelpMarket => LegacyArchetype.Connector,
            CommunityActivityKind.HelpLumber => LegacyArchetype.Builder,
            CommunityActivityKind.HelpInn => LegacyArchetype.Connector,
            CommunityActivityKind.HelpHerbGarden => LegacyArchetype.Caretaker,
            CommunityActivityKind.HelpSmithy => LegacyArchetype.Builder,
            CommunityActivityKind.HelpPatrol => LegacyArchetype.Caretaker,
            CommunityActivityKind.HelpVillage => LegacyArchetype.Connector,
            CommunityActivityKind.ListenToStories => LegacyArchetype.Connector,
            CommunityActivityKind.HelpWorkshop => LegacyArchetype.Builder,
            CommunityActivityKind.ChatWithEleanor => LegacyArchetype.Connector,
            _ => null,
        };

    public static LegacyArchetype GetInfluenceForProjectContribution() => LegacyArchetype.Builder;

    public static LegacyArchetype GetInfluenceForNpcInteraction() => LegacyArchetype.Connector;

    public static LegacyArchetypeInfluence ApplyInfluenceGain(
        LegacyArchetypeInfluence current,
        LegacyArchetype path,
        int points = InfluencePointsPerAction) =>
        path switch
        {
            LegacyArchetype.Builder => current with
            {
                BuilderPoints = Math.Min(MaxInfluencePerPath, current.BuilderPoints + points),
            },
            LegacyArchetype.Caretaker => current with
            {
                CaretakerPoints = Math.Min(MaxInfluencePerPath, current.CaretakerPoints + points),
            },
            LegacyArchetype.Connector => current with
            {
                ConnectorPoints = Math.Min(MaxInfluencePerPath, current.ConnectorPoints + points),
            },
            _ => current,
        };

    /// <summary>Which path the player is currently pushing via influence — may differ from detected archetype.</summary>
    public static LegacyArchetype? GetLeadingInfluencePath(LegacyArchetypeInfluence influence)
    {
        var best = LegacyArchetype.None;
        var bestScore = 0;

        TryLead(LegacyArchetype.Builder, influence.BuilderPoints, ref best, ref bestScore);
        TryLead(LegacyArchetype.Caretaker, influence.CaretakerPoints, ref best, ref bestScore);
        TryLead(LegacyArchetype.Connector, influence.ConnectorPoints, ref best, ref bestScore);

        return bestScore >= MinInfluenceForSpecialAction ? best : null;
    }

    public static bool QualifiesForSpecialAction(
        LegacyArchetype path,
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence) =>
        detectedArchetype == path
        || influence.GetPoints(path) >= MinInfluenceForSpecialAction;

    public static float GetCaretakerHelpMoodBonus(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        CommunityActivityKind activity)
    {
        if (activity is not (CommunityActivityKind.HelpGarden or CommunityActivityKind.HelpWell))
            return 0f;

        return QualifiesForSpecialAction(LegacyArchetype.Caretaker, detectedArchetype, influence)
            ? CaretakerHelpMoodBonus
            : 0f;
    }

    public static string? TryGetCommunityHelpAgencyFeedback(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        CommunityActivityKind activity,
        uint variationSeed)
    {
        var path = GetInfluenceForCommunityActivity(activity);
        if (path is null)
            return null;

        if (!QualifiesForSpecialAction(path.Value, detectedArchetype, influence))
            return null;

        return LegacyArchetypeAgencyDialogue.TryGetCommunityHelpFeedback(
            path.Value,
            activity,
            variationSeed);
    }

    public static string? TryGetProjectContributionAgencyFeedback(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        uint variationSeed)
    {
        if (!QualifiesForSpecialAction(LegacyArchetype.Builder, detectedArchetype, influence))
            return null;

        return LegacyArchetypeAgencyDialogue.TryGetProjectContributionFeedback(
            LegacyArchetype.Builder,
            variationSeed);
    }

    public static string? TryGetConnectorSocialInsight(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        uint variationSeed)
    {
        if (!QualifiesForSpecialAction(LegacyArchetype.Connector, detectedArchetype, influence))
            return null;

        return LegacyArchetypeAgencyDialogue.TryGetConnectorSocialInsight(variationSeed);
    }

    public static bool ShouldShowInfluenceGainFeedback(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 97 + (uint)(totalGameMinutes % 937)) % 100;
        return roll < InfluenceGainFeedbackChancePercent;
    }

    public static string? TryGetInfluenceGainFeedback(
        LegacyArchetype path,
        uint playerEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!ShouldShowInfluenceGainFeedback(playerEntityId, totalGameMinutes))
            return null;

        return LegacyArchetypeAgencyDialogue.TryGetInfluenceGainFeedback(path, variationSeed);
    }

    /// <summary>Short narrative hint for status — where the player's story is heading.</summary>
    public static string? FormatLegacyDirectionHint(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence,
        uint variationSeed = 0) =>
        PlayerLongTermGoalConfig.TryGetNarrativeDirectionHint(detectedArchetype, influence, variationSeed);

    public static string FormatAgencyGuidanceDetail(LegacyArchetype detectedArchetype, LegacyArchetypeInfluence influence)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Shape your legacy through action:");
        builder.AppendLine($"  Builder path ({influence.BuilderPoints}/{MaxInfluencePerPath}): contribute to communal projects.");
        builder.AppendLine($"  Caretaker path ({influence.CaretakerPoints}/{MaxInfluencePerPath}): help garden or well.");
        builder.AppendLine($"  Connector path ({influence.ConnectorPoints}/{MaxInfluencePerPath}): greet villagers, help market.");

        var hint = FormatLegacyDirectionHint(detectedArchetype, influence);
        if (!string.IsNullOrWhiteSpace(hint))
        {
            builder.AppendLine();
            builder.Append(hint);
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatArchetypeMilestoneFlavor(
        PlayerLongTermGoalMilestone milestone,
        LegacyArchetype archetype) =>
        (milestone, archetype) switch
        {
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Builder) =>
                "Village Story — your name beside finished work.",
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Caretaker) =>
                "Village Story — your name when someone needs steady help.",
            (PlayerLongTermGoalMilestone.VillageStory, LegacyArchetype.Connector) =>
                "Village Story — your name in every warm introduction.",
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Builder) =>
                "Bloomtown Legacy — what you raised endures.",
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Caretaker) =>
                "Bloomtown Legacy — who you cared for remembers.",
            (PlayerLongTermGoalMilestone.BloomtownLegacy, LegacyArchetype.Connector) =>
                "Bloomtown Legacy — the bonds you built endure.",
            _ => PlayerLongTermGoalConfig.GetMilestoneDisplayName(milestone),
        };

    private static void TryLead(
        LegacyArchetype archetype,
        int points,
        ref LegacyArchetype best,
        ref int bestScore)
    {
        if (points <= bestScore)
            return;

        best = archetype;
        bestScore = points;
    }
}