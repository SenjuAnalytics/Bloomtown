using System.Text;
using Bloomtown.Shared.Activities;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Cozy village leisure spots — gentler than community help, available without area unlocks.
/// </summary>
public static class DailyVillageActivityConfig
{
    public const float InteractionRadiusMeters = 10f;

    private static readonly DailyVillageActivityDefinition[] Activities =
    [
        new()
        {
            Kind = DailyVillageActivityKind.SitOnBench,
            CommandHint = "sit bench",
            LocationName = "Village Green Bench",
            WorldX = 17f,
            WorldZ = 10f,
            Cooldown = TimeSpan.FromMinutes(3),
            MoodGain = 5f,
            FatigueReduction = 5f,
            SocialReduction = 3f,
            FlavorTexts =
            [
                "You settle on the village bench and let the lanes drift past in easy rhythm.",
                "A villager nods as they pass — the bench feels like a small shared pause.",
                "You rest your legs and listen to distant chatter; Bloomtown hums at a gentle pace.",
            ],
            StandingFlavorTexts =
            [
                "Neighbors smile as they pass — your bench seat feels quietly welcomed.",
                "Someone leaves a friendly wave; the village green feels like it knows you.",
            ],
        },
        new()
        {
            Kind = DailyVillageActivityKind.WatchVillage,
            CommandHint = "watch village",
            LocationName = "Village Outlook",
            WorldX = 19f,
            WorldZ = 12f,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 6f,
            FatigueReduction = 2f,
            SocialReduction = 4f,
            FlavorTexts =
            [
                "You linger at the outlook and watch rooftops, paths, and small errands weave together.",
                "From here the village feels whole — smoke, footsteps, and laughter in soft layers.",
                "You take in the day's texture: a cart rolling by, children calling, life unfolding unhurried.",
            ],
            StandingFlavorTexts =
            [
                "Familiar faces below glance up — watching Bloomtown feels like belonging to its story.",
                "A neighbor calls a warm greeting from the lane; the view feels personally alive today.",
            ],
        },
        new()
        {
            Kind = DailyVillageActivityKind.ChatWithLocals,
            CommandHint = "chat locals",
            LocationName = "Village Green & Market Lane",
            WorldX = 17f,
            WorldZ = 10f,
            SecondaryWorldX = 18f,
            SecondaryWorldZ = 6f,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 4f,
            FatigueReduction = 1f,
            SocialReduction = 8f,
            RhythmCategory = DailyRhythmActivityCategory.Social,
            FlavorTexts =
            [
                "You trade easy greetings with passersby — small talk that stitches you into the village fabric.",
                "A local shares a harmless bit of news; laughter nearby makes the square feel alive.",
                "You pause for conversation by the green — connection arrives in unhurried scraps.",
            ],
            StandingFlavorTexts =
            [
                "Neighbors greet you by name — chatting here feels like belonging, not just passing through.",
                "A familiar voice pulls you into the circle; Bloomtown's gossip feels warmly inclusive today.",
            ],
        },
        new()
        {
            Kind = DailyVillageActivityKind.TendPublicGarden,
            CommandHint = "tend public garden",
            LocationName = "Public Garden Beds",
            WorldX = 21f,
            WorldZ = 16f,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 4f,
            FatigueReduction = 2f,
            SocialReduction = 5f,
            RhythmCategory = DailyRhythmActivityCategory.Social,
            FlavorTexts =
            [
                "You tidy shared beds and pull a few weeds — public greenery that everyone enjoys.",
                "You water the ornamental rows; strangers nod thanks as they pass the garden path.",
                "You straighten markers and clear fallen petals — humble care for a space the village shares.",
            ],
            StandingFlavorTexts =
            [
                "A neighbor mentions how nice the beds look — your tending feels quietly appreciated.",
                "Someone leaves a warm word as they pass; the public garden feels like shared pride today.",
            ],
        },
        new()
        {
            Kind = DailyVillageActivityKind.PracticeWorkshop,
            CommandHint = "practice workshop",
            LocationName = "Village Workshop",
            WorldX = 10f,
            WorldZ = 16f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 5f,
            FatigueReduction = 1f,
            SocialReduction = 2f,
            RhythmCategory = DailyRhythmActivityCategory.Leisure,
            FlavorTexts =
            [
                "You practice simple joints at the bench — honest repetition that steadies the hands and mind.",
                "You plane a scrap plank and reset the tools; the workshop hums with quiet purpose.",
                "You rehearse a repair pattern Marcus left on the board — skill grows in small, patient passes.",
            ],
            StandingFlavorTexts =
            [
                "Marcus glances over and offers a approving nod — practicing here feels encouraged, not judged.",
                "A villager watches your steady hands and smiles; the workshop feels like a place you belong.",
            ],
        },
    ];

    public static IReadOnlyList<DailyVillageActivityDefinition> All => Activities;

    public static bool TryGet(DailyVillageActivityKind kind, out DailyVillageActivityDefinition definition)
    {
        definition = Activities.FirstOrDefault(entry => entry.Kind == kind)!;
        return definition is not null && definition.Kind != DailyVillageActivityKind.None;
    }

    public static TimeSpan GetCooldown(DailyVillageActivityKind kind) =>
        TryGet(kind, out var definition) ? definition.Cooldown : TimeSpan.Zero;

    public static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public static bool IsInRange(DailyVillageActivityDefinition definition, float playerX, float playerZ) =>
        IsNearPoint(playerX, playerZ, definition.WorldX, definition.WorldZ)
        || definition.SecondaryWorldX is float secondaryX
            && definition.SecondaryWorldZ is float secondaryZ
            && IsNearPoint(playerX, playerZ, secondaryX, secondaryZ);

    public static bool IsAvailableAt(
        DailyVillageActivityDefinition definition,
        float playerX,
        float playerZ) =>
        IsInRange(definition, playerX, playerZ);

    private static bool IsNearPoint(float playerX, float playerZ, float worldX, float worldZ) =>
        GetDistance(playerX, playerZ, worldX, worldZ) <= InteractionRadiusMeters;

    public static string PickFlavorText(
        DailyVillageActivityDefinition definition,
        bool useStandingFlavor,
        uint variationSeed)
    {
        var texts = useStandingFlavor && definition.StandingFlavorTexts.Length > 0
            ? definition.StandingFlavorTexts
            : definition.FlavorTexts;

        if (texts.Length == 0)
            return "You spend a peaceful moment in the village.";

        return texts[(int)(variationSeed % (uint)texts.Length)];
    }

    public static string FormatActivityList()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Daily village activities:");
        builder.AppendLine("  Leisure, social, community, and light practice — gentler than community help.");
        builder.AppendLine();

        foreach (var activity in Activities)
        {
            var locationHint = activity.SecondaryWorldX is float secondaryX && activity.SecondaryWorldZ is float secondaryZ
                ? $"near ({activity.WorldX:F0},{activity.WorldZ:F0}) or ({secondaryX:F0},{secondaryZ:F0})"
                : $"near ({activity.WorldX:F0},{activity.WorldZ:F0})";

            builder.AppendLine(
                $"  {activity.CommandHint} — {activity.LocationName} — {locationHint} within {InteractionRadiusMeters:F0}m (cooldown {activity.Cooldown.TotalMinutes:F0}m)");
            builder.AppendLine(FormatActivityHint(activity));
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatNearbyStatus(float playerX, float playerZ)
    {
        var nearby = Activities
            .Where(activity => IsAvailableAt(activity, playerX, playerZ))
            .Select(activity => activity.CommandHint)
            .ToList();

        if (nearby.Count == 0)
        {
            return "Daily activities nearby: (none — try village green (17,10), public garden (21,16), or workshop (10,16))";
        }

        return $"Daily activities nearby: {string.Join(", ", nearby)}";
    }

    public static string FormatActivityHint(DailyVillageActivityDefinition definition)
    {
        var nuance = definition.RhythmCategory switch
        {
            DailyRhythmActivityCategory.Social => "social",
            DailyRhythmActivityCategory.Recovery => "cozy",
            _ => definition.Kind == DailyVillageActivityKind.PracticeWorkshop ? "productive" : "leisure",
        };

        return
            $"    ({nuance}) Mood +{definition.MoodGain:F0}, Fatigue -{definition.FatigueReduction:F0}, Social -{definition.SocialReduction:F0}";
    }
}

/// <summary>One repeatable village leisure activity with mixed need effects.</summary>
public sealed class DailyVillageActivityDefinition
{
    public required DailyVillageActivityKind Kind { get; init; }
    public required string CommandHint { get; init; }
    public required string LocationName { get; init; }
    public required float WorldX { get; init; }
    public required float WorldZ { get; init; }
    public required TimeSpan Cooldown { get; init; }
    public required float MoodGain { get; init; }
    public required float FatigueReduction { get; init; }
    public required float SocialReduction { get; init; }
    public required string[] FlavorTexts { get; init; }
    public required string[] StandingFlavorTexts { get; init; }
    public DailyRhythmActivityCategory RhythmCategory { get; init; } = DailyRhythmActivityCategory.Leisure;
    public float? SecondaryWorldX { get; init; }
    public float? SecondaryWorldZ { get; init; }
}