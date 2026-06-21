using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Activities;

/// <summary>
/// Light time-of-day tuning for cozy home and village leisure activities.
/// Bonuses stay small (+1~3) so no single hour dominates.
/// </summary>
public static class ActivityTimeOfDayConfig
{
    public readonly record struct TimedEffectAdjustment(
        float MoodBonus,
        float FatigueReductionBonus,
        float SocialReductionPenalty,
        bool IndoorActivity);

    public readonly record struct TimedFlavorSet(
        string[] Morning,
        string[] Afternoon,
        string[] Evening,
        string[] Night);

    private static readonly Dictionary<HomeActivityType, TimedEffectAdjustment[]> HomeAdjustments = new()
    {
        [HomeActivityType.Nap] =
        [
            new(2f, 0f, 0f, true),   // Morning — refreshing
            new(0f, 0f, 0f, true),   // Afternoon — normal midday rest
            new(0f, 2f, 0f, true),   // Evening — eases the day
            new(2f, 1f, 0f, true),   // Night — cozy indoor sleep
        ],
        [HomeActivityType.Relax] =
        [
            new(1f, 0f, 0f, true),
            new(-1f, 0f, 0f, true),
            new(0f, 1f, 0f, true),
            new(2f, 1f, 0f, true),
        ],
        [HomeActivityType.ReadBook] =
        [
            new(2f, 0f, 0f, true),
            new(1f, 0f, 0f, true),
            new(1f, 1f, 0f, true),
            new(1f, 1f, 0f, true),
        ],
        [HomeActivityType.EnjoyTea] =
        [
            new(1f, 0f, 0f, true),
            new(0f, 0f, 0f, true),
            new(2f, 1f, 0f, true),
            new(1f, 1f, 0f, true),
        ],
    };

    private static readonly Dictionary<DailyVillageActivityKind, TimedEffectAdjustment[]> VillageAdjustments = new()
    {
        [DailyVillageActivityKind.SitOnBench] =
        [
            new(2f, 0f, 0f, false),
            new(-1f, 0f, 0f, false),
            new(0f, 2f, 0f, false),
            new(0f, 1f, 1f, false),
        ],
        [DailyVillageActivityKind.WatchVillage] =
        [
            new(2f, 0f, 0f, false),
            new(0f, 0f, 0f, false),
            new(1f, 1f, 0f, false),
            new(1f, 0f, 1f, false),
        ],
        [DailyVillageActivityKind.ChatWithLocals] =
        [
            new(1f, 0f, 0f, false),
            new(2f, 0f, 0f, false),
            new(1f, 0f, 0f, false),
            new(0f, 0f, 1f, false),
        ],
        [DailyVillageActivityKind.TendPublicGarden] =
        [
            new(1f, 1f, 0f, false),
            new(0f, 0f, 0f, false),
            new(1f, 2f, 0f, false),
            new(0f, 1f, 0f, false),
        ],
        [DailyVillageActivityKind.PracticeWorkshop] =
        [
            new(2f, 0f, 0f, false),
            new(1f, 0f, 0f, false),
            new(0f, 1f, 0f, false),
            new(-1f, 0f, 0f, false),
        ],
    };

    private static readonly Dictionary<HomeActivityType, TimedFlavorSet> HomeTimedFlavors = new()
    {
        [HomeActivityType.Nap] = new(
            Morning: ["Morning light filters in as you nap — you wake feeling gently refreshed."],
            Afternoon: ["A midday nap steals a few quiet minutes while the village hums outside."],
            Evening: ["You rest as the day softens — the nap carries a calm evening weight."],
            Night: ["Night wraps the house in stillness; your nap feels deep and unhurried."]),
        [HomeActivityType.Relax] = new(
            Morning: ["You relax as morning air drifts through — a fresh, unhurried start."],
            Afternoon: ["You pause indoors while afternoon warmth lingers — rest comes slowly."],
            Evening: ["Evening quiet settles in as you relax; the day loosens its hold."],
            Night: ["You relax in the hush of night — the village feels far away and peaceful."]),
        [HomeActivityType.ReadBook] = new(
            Morning: ["You read in the clear morning — pages turn with a fresh mind."],
            Afternoon: ["Afternoon light pools on the page as you read at an easy pace."],
            Evening: ["You read while evening dims outside — the story feels especially cozy."],
            Night: ["A lamp and a book make the night feel intimate and calm."]),
        [HomeActivityType.EnjoyTea] = new(
            Morning: ["You sip morning tea — warmth and a gentle start go hand in hand."],
            Afternoon: ["Afternoon tea steadies you between the day's small errands."],
            Evening: ["Evening tea tastes richer as the village quiets down."],
            Night: ["A late cup of tea feels like a small night ritual just for you."]),
    };

    private static readonly Dictionary<DailyVillageActivityKind, TimedFlavorSet> VillageTimedFlavors = new()
    {
        [DailyVillageActivityKind.SitOnBench] = new(
            Morning: ["The morning bench is cool and bright — villagers greet the day around you."],
            Afternoon: ["The afternoon sun feels warm as you sit on the bench, watching life drift by."],
            Evening: ["Evening light gilds the bench; the lanes slow to a gentler pace."],
            Night: ["You sit alone in the night air — peaceful, though the square is mostly quiet."]),
        [DailyVillageActivityKind.WatchVillage] = new(
            Morning: ["Morning bustle below makes the outlook feel alive and hopeful."],
            Afternoon: ["The afternoon sun warms the rooftops as you watch errands weave together."],
            Evening: ["Evening smoke and lantern glow make the village look softly lived-in."],
            Night: ["Night folds over Bloomtown — a few windows still glow as you watch in calm."]),
        [DailyVillageActivityKind.ChatWithLocals] = new(
            Morning: ["Morning greetings flow easily — the village feels awake and approachable."],
            Afternoon: ["Afternoon chatter clusters in the shade; joining in feels natural."],
            Evening: ["Evening small talk softens as lanterns appear — connection feels unhurried."],
            Night: ["Night conversation is sparse but warm; a few locals still trade quiet words."]),
        [DailyVillageActivityKind.TendPublicGarden] = new(
            Morning: ["Morning light on the shared beds makes tending feel fresh and purposeful."],
            Afternoon: ["Afternoon warmth slows your pace among the public rows — steady, grounding work."],
            Evening: ["Evening petals and cool air make the shared garden especially peaceful to tend."],
            Night: ["You tend by lamplight — quiet care for a garden the whole village walks past."]),
        [DailyVillageActivityKind.PracticeWorkshop] = new(
            Morning: ["Morning practice at the bench feels clear-headed and deliberate."],
            Afternoon: ["Afternoon repetition at the worktable builds quiet confidence in your hands."],
            Evening: ["Evening practice winds down the day with honest, tactile focus."],
            Night: ["A late session by lantern — the workshop feels intimate and patiently productive."]),
    };

    public static bool SupportsHomeActivity(HomeActivityType type) => HomeAdjustments.ContainsKey(type);

    public static bool SupportsVillageActivity(DailyVillageActivityKind kind) =>
        VillageAdjustments.ContainsKey(kind);

    public static TimedEffectAdjustment GetHomeAdjustment(HomeActivityType type, GameTimeOfDay phase) =>
        HomeAdjustments.TryGetValue(type, out var adjustments)
            ? adjustments[(int)phase]
            : default;

    public static TimedEffectAdjustment GetVillageAdjustment(DailyVillageActivityKind kind, GameTimeOfDay phase) =>
        VillageAdjustments.TryGetValue(kind, out var adjustments)
            ? adjustments[(int)phase]
            : default;

    public static string? PickHomeTimedFlavor(HomeActivityType type, GameTimeOfDay phase, uint variationSeed)
    {
        if (!HomeTimedFlavors.TryGetValue(type, out var flavors))
            return null;

        var lines = GetFlavorLines(flavors, phase);
        return lines.Length == 0 ? null : lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string? PickVillageTimedFlavor(
        DailyVillageActivityKind kind,
        GameTimeOfDay phase,
        uint variationSeed)
    {
        if (!VillageTimedFlavors.TryGetValue(kind, out var flavors))
            return null;

        var lines = GetFlavorLines(flavors, phase);
        return lines.Length == 0 ? null : lines[(int)(variationSeed % (uint)lines.Length)];
    }

    public static string FormatTimingNote(HomeActivityType type, GameTimeOfDay phase)
    {
        if (!SupportsHomeActivity(type))
            return string.Empty;

        var adjustment = GetHomeAdjustment(type, phase);
        return FormatTimingNote(phase, adjustment);
    }

    public static string FormatTimingNote(DailyVillageActivityKind kind, GameTimeOfDay phase)
    {
        if (!SupportsVillageActivity(kind))
            return string.Empty;

        var adjustment = GetVillageAdjustment(kind, phase);
        return FormatTimingNote(phase, adjustment);
    }

    public static string GetRhythmMomentLabel(string activityLabel, GameTimeOfDay phaseAtActivity) =>
        (activityLabel.ToLowerInvariant(), phaseAtActivity) switch
        {
            ("nap", GameTimeOfDay.Morning) => "Morning nap felt refreshing.",
            ("nap", GameTimeOfDay.Afternoon) => "Midday nap steadied the afternoon.",
            ("nap", GameTimeOfDay.Evening) => "Evening nap eased the day's weight.",
            ("nap", GameTimeOfDay.Night) => "Night nap felt deep and cozy.",
            ("relax", GameTimeOfDay.Morning) => "Morning relax set a gentle tone.",
            ("relax", GameTimeOfDay.Evening) => "Evening relax felt especially calming.",
            ("relax", GameTimeOfDay.Night) => "Night relax wrapped the day in quiet.",
            ("read", GameTimeOfDay.Morning) => "Morning reading felt clear and fresh.",
            ("read", GameTimeOfDay.Afternoon) => "Afternoon reading was peacefully unhurried.",
            ("read", GameTimeOfDay.Evening) => "Evening reading felt cozy by lamplight.",
            ("tea", GameTimeOfDay.Evening) => "Evening tea tasted especially comforting.",
            ("tea", GameTimeOfDay.Morning) => "Morning tea was a warm start.",
            ("sit bench", GameTimeOfDay.Morning) => "Morning bench sit felt bright and easy.",
            ("sit bench", GameTimeOfDay.Afternoon) => "Afternoon bench sit was warm and watchful.",
            ("sit bench", GameTimeOfDay.Evening) => "Evening bench sit slowed the pace nicely.",
            ("watch village", GameTimeOfDay.Morning) => "Morning outlook felt hopeful and alive.",
            ("watch village", GameTimeOfDay.Evening) => "Evening watch felt softly homely.",
            ("watch village", GameTimeOfDay.Night) => "Night watch was calm, if a little lonely.",
            ("chat locals", GameTimeOfDay.Morning) => "Morning chat with locals felt easy and bright.",
            ("chat locals", GameTimeOfDay.Afternoon) => "Afternoon village chatter warmed you up.",
            ("tend public garden", GameTimeOfDay.Morning) => "Morning tending of public beds felt grounding.",
            ("tend public garden", GameTimeOfDay.Evening) => "Evening garden care felt peacefully shared.",
            ("practice workshop", GameTimeOfDay.Morning) => "Morning workshop practice felt focused.",
            ("practice workshop", GameTimeOfDay.Afternoon) => "Afternoon bench work built quiet skill.",
            _ => $"{GameTimeHelper.GetDisplayName(phaseAtActivity)} {activityLabel} felt natural.",
        };

    public static string FormatDailyRhythmStatus(
        GameTimeOfDay currentPhase,
        string? lastActivityLabel,
        GameTimeOfDay? lastActivityPhase,
        string? lastRhythmMoment,
        TimeSpan? elapsedSinceLast)
    {
        var phaseName = GameTimeHelper.GetDisplayName(currentPhase);
        var suggestion = GetLightSuggestion(currentPhase);

        if (string.IsNullOrWhiteSpace(lastActivityLabel) || lastActivityPhase is null)
        {
            return $"Daily rhythm ({phaseName}): {suggestion}";
        }

        var agoLabel = elapsedSinceLast is null
            ? "recently"
            : elapsedSinceLast.Value.TotalMinutes < 1
                ? "just now"
                : elapsedSinceLast.Value.TotalMinutes < 60
                    ? $"{elapsedSinceLast.Value.TotalMinutes:F0}m ago"
                    : $"{elapsedSinceLast.Value.TotalHours:F0}h ago";

        var lastPhaseName = GameTimeHelper.GetDisplayName(lastActivityPhase.Value);
        var moment = string.IsNullOrWhiteSpace(lastRhythmMoment)
            ? $"{lastPhaseName} {lastActivityLabel}"
            : lastRhythmMoment;

        return
            $"Daily rhythm ({phaseName}): last — {moment} ({agoLabel}). {suggestion}";
    }

    public static string GetLightSuggestion(GameTimeOfDay currentPhase) =>
        currentPhase switch
        {
            GameTimeOfDay.Morning => "Gentle start: nap, chat locals, or morning bench sit may feel especially fresh.",
            GameTimeOfDay.Afternoon => "Midday pace: chat locals, practice workshop, or tea can steady the afternoon.",
            GameTimeOfDay.Evening => "Dusk calm: tend public garden, watch village, or evening tea may feel especially cozy.",
            GameTimeOfDay.Night => "Night hush: home cozy rituals feel best; outdoor leisure is quieter.",
            _ => "A small cozy moment fits any hour.",
        };

    private static string FormatTimingNote(GameTimeOfDay phase, TimedEffectAdjustment adjustment)
    {
        var phaseName = GameTimeHelper.GetDisplayName(phase);
        if (adjustment.MoodBonus >= 2f)
            return $" ({phaseName} — especially refreshing!)";
        if (adjustment.MoodBonus == 1f)
            return $" ({phaseName} — a gentle lift this hour.)";
        if (adjustment.MoodBonus < 0f)
            return $" ({phaseName} — slower midday pace.)";
        if (adjustment.FatigueReductionBonus >= 2f)
            return $" ({phaseName} — restful hour.)";
        if (adjustment.FatigueReductionBonus == 1f)
            return $" ({phaseName} — eases fatigue gently.)";
        if (adjustment.SocialReductionPenalty > 0f)
            return $" ({phaseName} — peaceful, though the square is quieter.)";
        return $" ({phaseName})";
    }

    private static string[] GetFlavorLines(TimedFlavorSet flavors, GameTimeOfDay phase) =>
        phase switch
        {
            GameTimeOfDay.Morning => flavors.Morning,
            GameTimeOfDay.Afternoon => flavors.Afternoon,
            GameTimeOfDay.Evening => flavors.Evening,
            GameTimeOfDay.Night => flavors.Night,
            _ => [],
        };
}