using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Routines;

/// <summary>
/// Personal routine definitions, time-of-day bonuses, and flavor text.
/// </summary>
public static class PersonalRoutineConfig
{
    public const float OffPhaseMultiplier = 0.8f;

    private static readonly PersonalRoutineDefinition[] Routines =
    [
        new()
        {
            Kind = PersonalRoutineKind.MorningStretch,
            CommandHint = "morning stretch",
            Cooldown = TimeSpan.FromMinutes(4),
            IdealPhases = [GameTimeOfDay.Morning],
            BaseMoodGain = 4f,
            BaseFatigueReduction = 5f,
            IdealMoodBonus = 2f,
            IdealFatigueBonus = 2f,
            FlavorTexts =
            [
                "You roll your shoulders and stretch — a small wake-up ritual.",
                "You reach skyward and breathe deeply, easing into the day.",
                "A few gentle stretches help your body catch up with the morning.",
            ],
            IdealPhaseFlavorTexts =
            [
                "The morning air makes your stretch feel especially refreshing.",
                "You greet the day with an unhurried stretch — just right for this hour.",
            ],
        },
        new()
        {
            Kind = PersonalRoutineKind.EveningWindDown,
            CommandHint = "evening wind down",
            Cooldown = TimeSpan.FromMinutes(4),
            IdealPhases = [GameTimeOfDay.Evening, GameTimeOfDay.Night],
            BaseMoodGain = 5f,
            BaseFatigueReduction = 6f,
            IdealMoodBonus = 2f,
            IdealFatigueBonus = 2f,
            FlavorTexts =
            [
                "You slow your pace and let the day loosen its grip.",
                "You find a quiet moment to exhale and unwind.",
                "A gentle pause helps you transition toward rest.",
            ],
            IdealPhaseFlavorTexts =
            [
                "Dusk settles in — your wind-down feels perfectly timed.",
                "The evening hush makes it easy to release the day's tension.",
            ],
        },
        new()
        {
            Kind = PersonalRoutineKind.SitAndReflect,
            CommandHint = "sit and reflect",
            Cooldown = TimeSpan.FromMinutes(3),
            IdealPhases = [GameTimeOfDay.Afternoon],
            BaseMoodGain = 6f,
            BaseFatigueReduction = 2f,
            IdealMoodBonus = 2f,
            IdealFatigueBonus = 0f,
            FlavorTexts =
            [
                "You sit for a while and let your thoughts wander peacefully.",
                "You pause to reflect — no rush, just presence.",
                "A quiet sit helps you notice small joys around the village.",
            ],
            IdealPhaseFlavorTexts =
            [
                "The unhurried afternoon makes reflection especially rewarding.",
                "Warm afternoon light gives your thoughts a cozy place to land.",
            ],
        },
    ];

    public static IReadOnlyList<PersonalRoutineDefinition> All => Routines;

    public static bool TryGet(PersonalRoutineKind kind, out PersonalRoutineDefinition definition)
    {
        definition = Routines.FirstOrDefault(entry => entry.Kind == kind)!;
        return definition is not null && definition.Kind != PersonalRoutineKind.None;
    }

    /// <summary>
    /// Computes Mood/Fatigue recovery with a bonus during ideal phases, slightly reduced off-phase.
    /// </summary>
    public static PersonalRoutineEffects CalculateEffects(
        PersonalRoutineDefinition definition,
        GameTimeOfDay currentPhase)
    {
        var ideal = IsIdealPhase(definition, currentPhase);
        if (ideal)
        {
            return new PersonalRoutineEffects(
                definition.BaseMoodGain + definition.IdealMoodBonus,
                definition.BaseFatigueReduction + definition.IdealFatigueBonus,
                ideal);
        }

        return new PersonalRoutineEffects(
            definition.BaseMoodGain * OffPhaseMultiplier,
            definition.BaseFatigueReduction * OffPhaseMultiplier,
            ideal);
    }

    public static bool IsIdealPhase(PersonalRoutineDefinition definition, GameTimeOfDay currentPhase) =>
        definition.IdealPhases.Contains(currentPhase);

    public static string PickFlavorText(
        PersonalRoutineDefinition definition,
        bool idealPhase,
        uint variationSeed)
    {
        if (idealPhase && definition.IdealPhaseFlavorTexts.Length > 0)
        {
            var combined = definition.IdealPhaseFlavorTexts.Length + definition.FlavorTexts.Length;
            var index = (int)(variationSeed % (uint)combined);
            if (index < definition.IdealPhaseFlavorTexts.Length)
                return definition.IdealPhaseFlavorTexts[index];
        }

        return definition.FlavorTexts[(int)(variationSeed % (uint)definition.FlavorTexts.Length)];
    }

    public static string FormatIdealPhaseLabel(PersonalRoutineDefinition definition) =>
        string.Join(" / ", definition.IdealPhases.Select(GameTimeHelper.GetDisplayName));

    public static string FormatRoutineList(GameTimeOfDay currentPhase)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Personal routines:");
        builder.AppendLine($"  Current phase: {GameTimeHelper.GetDisplayName(currentPhase)}");
        builder.AppendLine();

        foreach (var routine in Routines)
        {
            var ideal = IsIdealPhase(routine, currentPhase);
            var timing = ideal ? "ideal now" : $"best during {FormatIdealPhaseLabel(routine)}";
            builder.AppendLine($"  {routine.CommandHint} — {timing} (cooldown {routine.Cooldown.TotalMinutes:F0}m)");
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatPersonalRhythmStatus(GameTimeOfDay currentPhase)
    {
        var suggestion = currentPhase switch
        {
            GameTimeOfDay.Morning => "A gentle start — morning stretch fits this hour.",
            GameTimeOfDay.Afternoon => "Unhurried daylight — good for sitting and reflecting.",
            GameTimeOfDay.Evening => "The day softens — evening wind down may feel especially cozy.",
            GameTimeOfDay.Night => "Night quiet — wind down or reflect at your own pace.",
            _ => "Take a small personal moment when it feels right.",
        };

        return $"Personal rhythm ({GameTimeHelper.GetDisplayName(currentPhase)}): {suggestion}";
    }

    public static TimeSpan GetCooldown(PersonalRoutineKind kind) =>
        TryGet(kind, out var definition) ? definition.Cooldown : TimeSpan.Zero;
}

/// <summary>One player personal routine with command, cooldown, and time-of-day tuning.</summary>
public sealed class PersonalRoutineDefinition
{
    public required PersonalRoutineKind Kind { get; init; }
    public required string CommandHint { get; init; }
    public required TimeSpan Cooldown { get; init; }
    public required GameTimeOfDay[] IdealPhases { get; init; }
    public required float BaseMoodGain { get; init; }
    public required float BaseFatigueReduction { get; init; }
    public required float IdealMoodBonus { get; init; }
    public required float IdealFatigueBonus { get; init; }
    public required string[] FlavorTexts { get; init; }
    public required string[] IdealPhaseFlavorTexts { get; init; }
}

public readonly record struct PersonalRoutineEffects(
    float MoodGain,
    float FatigueReduction,
    bool IdealPhase);