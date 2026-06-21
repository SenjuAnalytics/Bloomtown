using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Detects the player's dominant legacy path from contribution, community help, and relationships.
/// Social roles from <see cref="CommunityReputationConfig"/> nudge Caretaker vs Connector scoring.
/// </summary>
public static class LegacyArchetypeConfig
{
    /// <summary>Minimum signal before a path is named — avoids labeling brand-new players.</summary>
    public const int MinHelpsForDetection = 2;
    public const int MinContributionScoreForDetection = 15;
    public const int MinSocialBondsForDetection = 2;

    /// <summary>Winning score must clear this bar to earn a named archetype.</summary>
    public const int MinDominantScore = 12;

    public static string GetDisplayName(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder => "Builder",
            LegacyArchetype.Caretaker => "Caretaker",
            LegacyArchetype.Connector => "Connector",
            _ => "Undecided",
        };

    /// <summary>
    /// Resolves the dominant legacy path from play-pattern scores.
    /// Builder favors project contribution; Caretaker favors community help and social roles;
    /// Connector favors NPC friendships and acquaintance breadth.
    /// </summary>
    public static LegacyArchetype ResolveDominantArchetype(
        PlayerLongTermGoalSnapshot snapshot,
        CommunityReputationState reputation,
        LegacyArchetypeInfluence influence = default)
    {
        if (!IsEligibleForDetection(snapshot, reputation))
            return LegacyArchetype.None;

        var builderScore = ScoreBuilder(snapshot)
            + influence.BuilderPoints * LegacyArchetypeAgencyConfig.InfluenceScoreBonusPerPoint;
        var caretakerScore = ScoreCaretaker(snapshot, reputation)
            + influence.CaretakerPoints * LegacyArchetypeAgencyConfig.InfluenceScoreBonusPerPoint;
        var connectorScore = ScoreConnector(snapshot)
            + influence.ConnectorPoints * LegacyArchetypeAgencyConfig.InfluenceScoreBonusPerPoint;

        var best = LegacyArchetype.None;
        var bestScore = 0;

        TryConsider(LegacyArchetype.Builder, builderScore, ref best, ref bestScore);
        TryConsider(LegacyArchetype.Caretaker, caretakerScore, ref best, ref bestScore);
        TryConsider(LegacyArchetype.Connector, connectorScore, ref best, ref bestScore);

        return bestScore >= MinDominantScore ? best : LegacyArchetype.None;
    }

    /// <summary>Personal one-liner for status — how the village is beginning to know the player.</summary>
    public static string? FormatVillageIdentityStatusLine(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder =>
                "Village identity: Bloomtown is starting to know you as someone who builds what lasts.",
            LegacyArchetype.Caretaker =>
                "Village identity: You're becoming known as someone the village can rely on when things get difficult.",
            LegacyArchetype.Connector =>
                "Village identity: People here are learning your name — and introducing you before you even ask.",
            _ => null,
        };

    /// <summary>Richer identity framing for the goal command detail view.</summary>
    public static string FormatVillageIdentityDetailLine(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder =>
                "In Bloomtown's eyes, you're a builder — hands that raise shared work, a name beside timber and stone.",
            LegacyArchetype.Caretaker =>
                "In Bloomtown's eyes, you're a caretaker — steady help on hard days, warmth in small chores.",
            LegacyArchetype.Connector =>
                "In Bloomtown's eyes, you're a connector — greetings that open doors, friendships that hold the village together.",
            _ => "Bloomtown hasn't named your place yet — your story here is still an open page.",
        };

    /// <summary>How the village currently sees the player — short narrative perspective.</summary>
    public static string? FormatVillagePerspectiveLine(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder =>
                "How the village sees you: a patient builder shaping Bloomtown's skyline.",
            LegacyArchetype.Caretaker =>
                "How the village sees you: quiet strength when routines fray and someone needs tending.",
            LegacyArchetype.Connector =>
                "How the village sees you: the friendly face in every crowd, weaving people together.",
            _ => null,
        };

    public static string GetIdentityEpithet(LegacyArchetype archetype) =>
        archetype switch
        {
            LegacyArchetype.Builder => "Bloomtown's builder",
            LegacyArchetype.Caretaker => "a steady caretaker",
            LegacyArchetype.Connector => "the village connector",
            _ => "a newcomer still finding their story",
        };

    public static string? FormatLegacyPathStatusLine(LegacyArchetype archetype) =>
        FormatVillageIdentityStatusLine(archetype);

    public static string FormatLegacyPathDetailLine(LegacyArchetype archetype) =>
        FormatVillageIdentityDetailLine(archetype);

    private static bool IsEligibleForDetection(
        PlayerLongTermGoalSnapshot snapshot,
        CommunityReputationState reputation)
    {
        var socialBonds = snapshot.FriendCount + snapshot.AcquaintanceCount + snapshot.CloseFriendCount;
        return reputation.TotalHelpCount >= MinHelpsForDetection
               || snapshot.VillageContributionScore >= MinContributionScoreForDetection
               || socialBonds >= MinSocialBondsForDetection
               || snapshot.CompletedProjectContributions >= 1;
    }

    private static int ScoreBuilder(PlayerLongTermGoalSnapshot snapshot)
    {
        var score = snapshot.VillageContributionScore;
        score += snapshot.CompletedProjectContributions * 30;

        if (snapshot.VillageTitle >= VillageTitle.Builder)
            score += 40;
        else if (snapshot.VillageTitle >= VillageTitle.Helper)
            score += 15;

        return score;
    }

    private static int ScoreCaretaker(
        PlayerLongTermGoalSnapshot snapshot,
        CommunityReputationState reputation)
    {
        var score = reputation.TotalHelpCount * 4;
        score += Math.Max(reputation.HelpGardenCount,
            Math.Max(reputation.HelpMarketCount, reputation.HelpWellCount)) * 3;

        score += snapshot.SocialRole switch
        {
            CommunitySocialRole.GardenHelper => 25,
            CommunitySocialRole.WellKeeper => 25,
            CommunitySocialRole.AllRoundHelper => 30,
            CommunitySocialRole.MarketHelper => 12,
            _ => 0,
        };

        return score;
    }

    private static int ScoreConnector(PlayerLongTermGoalSnapshot snapshot)
    {
        var score = snapshot.AcquaintanceCount * 4;
        score += snapshot.FriendCount * 10;
        score += snapshot.CloseFriendCount * 20;

        if (snapshot.SocialRole == CommunitySocialRole.MarketHelper)
            score += 18;

        return score;
    }

    private static void TryConsider(
        LegacyArchetype archetype,
        int score,
        ref LegacyArchetype best,
        ref int bestScore)
    {
        if (score <= bestScore)
            return;

        best = archetype;
        bestScore = score;
    }
}