using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Lightweight village event calendar: Market Day, Community Work Day, and Rainy Day.
/// Events last one full game day and recur on fixed intervals — rare, not daily.
/// </summary>
public static class VillageEventConfig
{
    public const int MarketDayIntervalDays = 4;
    public const int MarketDayPhaseOffset = 2;

    public const int CommunityWorkDayIntervalDays = 5;
    public const int CommunityWorkDayPhaseOffset = 4;

    public const int RainyDayIntervalDays = 7;
    public const int RainyDayPhaseOffset = 7;

    public const float MarketDayBuyDiscountPercent = 2f;
    public const float MarketDaySellBonusPercent = 2f;

    public const float CommunityWorkDayMoodBonus = 1f;
    public const float CommunityWorkDaySocialBonus = 1f;

    public const float RainyDayOutdoorMoodPenalty = 1f;
    public const float RainyDayOutdoorSocialPenalty = 1f;
    public const float RainyDayIndoorMoodBonus = 1f;
    public const float RainyDayIndoorSocialBonus = 1f;
    public const float RainyDayGatheringFatiguePenalty = 1f;

    public const int EventAmbientChancePercent = 28;
    public const int EventInteractionChancePercent = 45;

    public static bool IsMarketDay(int gameDay) =>
        gameDay >= MarketDayPhaseOffset
        && (gameDay - MarketDayPhaseOffset) % MarketDayIntervalDays == 0;

    public static bool IsCommunityWorkDay(int gameDay) =>
        gameDay >= CommunityWorkDayPhaseOffset
        && (gameDay - CommunityWorkDayPhaseOffset) % CommunityWorkDayIntervalDays == 0;

    public static bool IsRainyDay(int gameDay) =>
        gameDay >= RainyDayPhaseOffset
        && (gameDay - RainyDayPhaseOffset) % RainyDayIntervalDays == 0;

    public static bool HasActiveEvent(int gameDay) =>
        IsMarketDay(gameDay) || IsCommunityWorkDay(gameDay) || IsRainyDay(gameDay);

    public static IReadOnlyList<VillageEventKind> GetActiveEvents(int gameDay)
    {
        var events = new List<VillageEventKind>(3);
        if (IsMarketDay(gameDay))
            events.Add(VillageEventKind.MarketDay);
        if (IsCommunityWorkDay(gameDay))
            events.Add(VillageEventKind.CommunityWorkDay);
        if (IsRainyDay(gameDay))
            events.Add(VillageEventKind.RainyDay);
        return events;
    }

    public static string GetEventDisplayName(VillageEventKind kind) =>
        kind switch
        {
            VillageEventKind.MarketDay => "Market Day",
            VillageEventKind.CommunityWorkDay => "Community Work Day",
            VillageEventKind.RainyDay => "Rainy Day",
            _ => string.Empty,
        };

    public static string GetEventShortDescription(VillageEventKind kind) =>
        kind switch
        {
            VillageEventKind.MarketDay =>
                "the square hums with extra trade — Mira offers a small market-day discount on buys and sells",
            VillageEventKind.CommunityWorkDay =>
                "gotong royong spirit — help activities earn a little extra mood and social recovery",
            VillageEventKind.RainyDay =>
                "soft rain settles over Bloomtown — outdoor chores feel heavier, but porches, inns, and quiet company feel especially welcoming",
            _ => string.Empty,
        };

    public static string? FormatActiveEventsStatus(int gameDay)
    {
        var events = GetActiveEvents(gameDay);
        if (events.Count == 0)
            return null;

        if (events.Count == 1)
        {
            var kind = events[0];
            return $"── Village Event ──{Environment.NewLine}"
                + $"{GetEventDisplayName(kind)}: {GetEventShortDescription(kind)}.";
        }

        var lines = new List<string> { "── Village Events ──" };
        foreach (var kind in events)
            lines.Add($"  • {GetEventDisplayName(kind)}: {GetEventShortDescription(kind)}");

        return string.Join(Environment.NewLine, lines);
    }

    public static int ApplyMarketDayBuyPrice(int unitPrice) =>
        Math.Max(1, (int)MathF.Round(unitPrice * (1f - MarketDayBuyDiscountPercent / 100f)));

    public static int ApplyMarketDaySellPrice(int unitPrice) =>
        Math.Max(1, (int)MathF.Round(unitPrice * (1f + MarketDaySellBonusPercent / 100f)));

    public static string FormatMarketDayTradeFeedback(bool isBuy) =>
        isBuy
            ? $"[Village event — Market Day: Mira's buys are ~{MarketDayBuyDiscountPercent:F0}% friendlier today.]"
            : $"[Village event — Market Day: Mira's sells pay ~{MarketDaySellBonusPercent:F0}% better today.]";

    public static string FormatCommunityWorkDayHelpFeedback() =>
        $"[Village event — Community Work Day: help activities earn +{CommunityWorkDayMoodBonus:F0} mood / +{CommunityWorkDaySocialBonus:F0} social recovery.]";

    public static string FormatRainyDayOutdoorFeedback() =>
        $"[Village event — Rainy Day: outdoor work feels harder (-{RainyDayOutdoorMoodPenalty:F0} mood / -{RainyDayOutdoorSocialPenalty:F0} social recovery).]";

    public static string FormatRainyDayIndoorFeedback() =>
        $"[Village event — Rainy Day: calm indoor company feels restorative (+{RainyDayIndoorMoodBonus:F0} mood / +{RainyDayIndoorSocialBonus:F0} social recovery).]";

    public static string FormatRainyDayGatheringFeedback() =>
        $"[Village event — Rainy Day: gathering in the wet adds +{RainyDayGatheringFatiguePenalty:F0} fatigue.]";

    public static bool IsHelpActivity(CommunityActivityKind activity) =>
        activity is CommunityActivityKind.HelpGarden
            or CommunityActivityKind.HelpMarket
            or CommunityActivityKind.HelpWell
            or CommunityActivityKind.HelpLumber
            or CommunityActivityKind.HelpInn
            or CommunityActivityKind.HelpHerbGarden
            or CommunityActivityKind.HelpSmithy
            or CommunityActivityKind.HelpPatrol
            or CommunityActivityKind.HelpVillage
            or CommunityActivityKind.HelpWorkshop
            or CommunityActivityKind.ListenToStories
            or CommunityActivityKind.ChatWithEleanor;

    public static bool IsOutdoorCommunityActivity(CommunityActivityKind activity) =>
        activity is CommunityActivityKind.HelpGarden
            or CommunityActivityKind.HelpMarket
            or CommunityActivityKind.HelpWell
            or CommunityActivityKind.HelpLumber
            or CommunityActivityKind.HelpHerbGarden
            or CommunityActivityKind.HelpSmithy
            or CommunityActivityKind.HelpPatrol
            or CommunityActivityKind.HelpVillage
            or CommunityActivityKind.HelpWorkshop;

    public static bool IsIndoorCalmCommunityActivity(CommunityActivityKind activity) =>
        activity is CommunityActivityKind.ListenToStories
            or CommunityActivityKind.ChatWithEleanor
            or CommunityActivityKind.HelpInn;

    public static bool IsIndoorCalmBondingAction(EmotionalBondActionKind action) =>
        action is EmotionalBondActionKind.CheckOn
            or EmotionalBondActionKind.ShareMoment
            or EmotionalBondActionKind.SpendTime;

    public static bool IsIndoorCalmNpcInteraction(NpcInteractionKind kind) =>
        kind is NpcInteractionKind.Talk or NpcInteractionKind.Greet;

    public static (float MoodAdjust, float SocialAdjust) GetRainyDayCommunityActivityAdjustments(
        CommunityActivityKind activity)
    {
        if (IsOutdoorCommunityActivity(activity))
            return (-RainyDayOutdoorMoodPenalty, -RainyDayOutdoorSocialPenalty);

        if (IsIndoorCalmCommunityActivity(activity))
            return (RainyDayIndoorMoodBonus, RainyDayIndoorSocialBonus);

        return (0f, 0f);
    }

    public static (float MoodAdjust, float SocialAdjust) GetRainyDayBondingActionAdjustments(
        EmotionalBondActionKind action)
    {
        if (IsIndoorCalmBondingAction(action))
            return (RainyDayIndoorMoodBonus, RainyDayIndoorSocialBonus);

        if (action == EmotionalBondActionKind.HelpWith)
            return (-RainyDayOutdoorMoodPenalty, -RainyDayOutdoorSocialPenalty);

        return (0f, 0f);
    }

    public static (float MoodAdjust, float SocialAdjust) GetRainyDayNpcInteractionAdjustments(
        NpcInteractionKind kind)
    {
        return IsIndoorCalmNpcInteraction(kind)
            ? (RainyDayIndoorMoodBonus, RainyDayIndoorSocialBonus)
            : (0f, 0f);
    }

    public static bool ShouldTriggerEventAmbientComment(
        uint playerEntityId,
        int gameDay,
        long totalGameMinutes,
        uint attemptCounter)
    {
        if (!HasActiveEvent(gameDay))
            return false;

        var roll = (playerEntityId * 163 + (uint)(totalGameMinutes % 983) + attemptCounter * 23 + (uint)gameDay * 7) % 100;
        return roll < EventAmbientChancePercent;
    }

    public static bool ShouldTriggerEventInteractionLine(
        uint playerEntityId,
        uint npcEntityId,
        int gameDay,
        uint variationSeed)
    {
        if (!HasActiveEvent(gameDay))
            return false;

        var roll = (playerEntityId * 167 + npcEntityId * 109 + variationSeed * 71 + (uint)gameDay * 13) % 100;
        return roll < EventInteractionChancePercent;
    }

    public static bool ShouldTriggerCommunityWorkHelpAcknowledgment(
        uint playerEntityId,
        CommunityActivityKind activity,
        int gameDay,
        uint variationSeed)
    {
        if (!IsCommunityWorkDay(gameDay) || !IsHelpActivity(activity))
            return false;

        var roll = (playerEntityId * 173 + (uint)activity * 97 + variationSeed * 59 + (uint)gameDay * 11) % 100;
        return roll < 55;
    }

    public static bool ShouldTriggerRainyDayActivityAcknowledgment(
        uint playerEntityId,
        CommunityActivityKind activity,
        int gameDay,
        uint variationSeed)
    {
        if (!IsRainyDay(gameDay))
            return false;

        if (!IsOutdoorCommunityActivity(activity) && !IsIndoorCalmCommunityActivity(activity))
            return false;

        var roll = (playerEntityId * 179 + (uint)activity * 101 + variationSeed * 61 + (uint)gameDay * 17) % 100;
        return roll < 50;
    }
}