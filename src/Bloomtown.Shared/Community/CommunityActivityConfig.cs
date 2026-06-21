using System.Text;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Community;

/// <summary>
/// Community-help activity definitions, location hints, and formatting helpers.
/// Effects favor Social Need relief over personal leisure activities.
/// </summary>
public static class CommunityActivityConfig
{
    public const float InteractionRadiusMeters = 10f;

    private static readonly CommunityActivityDefinition[] Activities =
    [
        new()
        {
            Kind = CommunityActivityKind.HelpGarden,
            CommandHint = "help garden",
            LocationName = "Community Garden",
            RequiredArea = VillageArea.CommunityGarden,
            RequiresWellCompleted = false,
            WorldX = 20f,
            WorldZ = 14f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 4f,
            SocialReduction = 7f,
            FlavorTexts =
            [
                "You weed shared beds and straighten markers — small work that keeps the garden welcoming.",
                "You carry water for a neighbor's seedlings; the garden hums with quiet cooperation.",
                "You tidy tools by the bench so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "Your hands-on help makes the Community Garden feel cared for by everyone.",
                "Neighbors notice the extra effort — you're part of how this garden thrives.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpMarket,
            CommandHint = "help market",
            LocationName = "Market Square",
            RequiredArea = VillageArea.MarketSquare,
            RequiresWellCompleted = false,
            WorldX = 18f,
            WorldZ = 6f,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 3f,
            SocialReduction = 8f,
            FlavorTexts =
            [
                "You help set out crates and sweep the square — the bustle feels friendlier with you in it.",
                "You lend a hand stacking goods; vendors chat warmly as the market takes shape.",
                "You guide a newcomer to a stall — the square works better when everyone pitches in.",
            ],
            ContributionTexts =
            [
                "The Market Square runs smoother thanks to your help — villagers feel it.",
                "Your contribution keeps trade day feeling like a shared village ritual.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpWell,
            CommandHint = "help well",
            LocationName = "Village Well",
            RequiredArea = null,
            RequiresWellCompleted = true,
            WorldX = VillageSiteConfig.WellWorldX,
            WorldZ = VillageSiteConfig.WellWorldZ,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 3f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You scrub the well rim and clear stray buckets — a humble chore everyone relies on.",
                "You tidy the gathering spot so the next visitor finds it welcoming.",
                "You check the path to the well; small upkeep keeps the heart of the village bright.",
            ],
            ContributionTexts =
            [
                "Looking after the well is looking after the village — your help matters here.",
                "This shared gathering spot stays welcoming because people like you show up.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpLumber,
            CommandHint = "help lumber",
            LocationName = "Lumber Yard",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 110f,
            WorldZ = 110f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 3f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You stack cut timber and clear sawdust — honest work that keeps the yard usable.",
                "You mend a fence rail by the woodpile; Tom would notice the extra hands.",
                "You sort tools by the lumber stacks so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "The lumber yard stays workable because folk like you show up with steady hands.",
                "Tom notices when the woodpile's tended — Bloomtown's building depends on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpInn,
            CommandHint = "help inn",
            LocationName = "Village Inn",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 22f,
            WorldZ = 16f,
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 4f,
            SocialReduction = 7f,
            FlavorTexts =
            [
                "You wipe tables and straighten chairs — small chores that keep the parlor welcoming.",
                "You carry fresh linens to the rooms; Greta hums approvingly from the kitchen.",
                "You refill the hearth wood and sweep the porch — travelers notice a cared-for inn.",
            ],
            ContributionTexts =
            [
                "The inn feels homier thanks to your help — Greta and her guests both feel it.",
                "Your hands at the hearth make Bloomtown's gathering place steadier for everyone.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpHerbGarden,
            CommandHint = "help herb garden",
            LocationName = "Herb Garden",
            RequiredArea = VillageArea.CommunityGarden,
            RequiresWellCompleted = false,
            WorldX = 16f,
            WorldZ = 18f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 4f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You trim herb rows and clear wilted leaves — quiet work that keeps the patch healthy.",
                "You carry water to drying beds; Nora nods approvingly from the shade.",
                "You tidy markers and baskets so the next helper can tend without fuss.",
            ],
            ContributionTexts =
            [
                "The herb garden feels steadier thanks to your help — Nora and the village both feel it.",
                "Your patient hands among the herbs make Bloomtown's balance a little easier to keep.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpSmithy,
            CommandHint = "help smithy",
            LocationName = "Village Smithy",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 12f,
            WorldZ = 14f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 3f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You stoke the forge and straighten tools — honest work that keeps the smithy usable.",
                "You carry timber to the anvil; Elias grunts approvingly from the bench.",
                "You tidy the workbench so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "The smithy stays workable because folk like you show up with steady hands.",
                "Elias notices when the forge is tended — Bloomtown's building depends on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpPatrol,
            CommandHint = "help patrol",
            LocationName = "Village Guard Post",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 15f,
            WorldZ = 11f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 3f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You walk the lanes with Ben and clear loose debris — steady eyes keep the village honest.",
                "You help check fences and paths; Ben nods approvingly from the guard post.",
                "You tidy lanterns by the post so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "The patrol route stays workable because folk like you show up with steady steps.",
                "Ben notices when the lanes are tended — Bloomtown's safety depends on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpVillage,
            CommandHint = "help village",
            LocationName = "Village Lanes",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 19f,
            WorldZ = 10f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 4f,
            SocialReduction = 7f,
            FlavorTexts =
            [
                "You tidy benches and sweep the lanes with Lila — small work that keeps Bloomtown welcoming.",
                "You help carry supplies between the square and garden; Lila grins approvingly from the path.",
                "You straighten markers by the community board so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "The village lanes stay welcoming because folk like you show up with steady hands.",
                "Lila notices when the village is tended — Bloomtown's heart depends on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.HelpWorkshop,
            CommandHint = "help workshop",
            LocationName = "Village Workshop",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 10f,
            WorldZ = 16f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 3f,
            SocialReduction = 6f,
            FlavorTexts =
            [
                "You straighten tools and sort offcuts — honest work that keeps the workshop usable.",
                "You carry planks to the bench; Marcus smiles approvingly from the worktable.",
                "You tidy the workbench so the next helper can jump right in.",
            ],
            ContributionTexts =
            [
                "The workshop stays workable because folk like you show up with steady hands.",
                "Marcus notices when the bench is tended — Bloomtown's repairs depend on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.ListenToStories,
            CommandHint = "listen to stories",
            LocationName = "Storyteller's Bench",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 11f,
            WorldZ = 9f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 4f,
            SocialReduction = 7f,
            FlavorTexts =
            [
                "You sit at Rowan's bench and listen to village history — small patience that keeps Bloomtown's memory alive.",
                "You lend an ear while Rowan recounts old tales; the story corner hums with quiet belonging.",
                "You settle by the inn's story bench so the next listener can find good company waiting.",
            ],
            ContributionTexts =
            [
                "The story bench stays welcoming because folk like you show up with patient ears.",
                "Rowan notices when someone truly listens — Bloomtown's memory depends on it.",
            ],
        },
        new()
        {
            Kind = CommunityActivityKind.ChatWithEleanor,
            CommandHint = "chat with eleanor",
            LocationName = "Eleanor's Porch",
            RequiredArea = null,
            RequiresWellCompleted = false,
            WorldX = 13f,
            WorldZ = 12f,
            Cooldown = TimeSpan.FromMinutes(5),
            MoodGain = 4f,
            SocialReduction = 7f,
            FlavorTexts =
            [
                "You sit on Eleanor's porch and listen to old village stories — small patience that keeps Bloomtown's memory alive.",
                "You lend an ear while Eleanor recounts classroom days and harvest tales; the porch hums with quiet belonging.",
                "You settle by the cottage porch so the next listener can find good company waiting.",
            ],
            ContributionTexts =
            [
                "The porch stays welcoming because folk like you show up with patient ears.",
                "Eleanor notices when someone truly listens — Bloomtown's legacy depends on it.",
            ],
        },
    ];

    public static IReadOnlyList<CommunityActivityDefinition> All => Activities;

    public static bool TryGet(CommunityActivityKind kind, out CommunityActivityDefinition definition)
    {
        definition = Activities.FirstOrDefault(entry => entry.Kind == kind)!;
        return definition is not null && definition.Kind != CommunityActivityKind.None;
    }

    public static TimeSpan GetCooldown(CommunityActivityKind kind) =>
        TryGet(kind, out var definition) ? definition.Cooldown : TimeSpan.Zero;

    public static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }

    public static bool IsInRange(CommunityActivityDefinition definition, float playerX, float playerZ) =>
        GetDistance(playerX, playerZ, definition.WorldX, definition.WorldZ) <= InteractionRadiusMeters;

    /// <summary>
    /// Whether prerequisites are met and the player is close enough to perform the activity.
    /// </summary>
    public static bool IsAvailableAt(
        CommunityActivityDefinition definition,
        float playerX,
        float playerZ,
        ISet<VillageArea> unlockedAreas,
        ISet<byte> completedProjectIds)
    {
        if (!MeetsPrerequisites(definition, unlockedAreas, completedProjectIds))
            return false;

        return IsInRange(definition, playerX, playerZ);
    }

    public static bool MeetsPrerequisites(
        CommunityActivityDefinition definition,
        ISet<VillageArea> unlockedAreas,
        ISet<byte> completedProjectIds)
    {
        if (definition.RequiredArea is VillageArea requiredArea && !unlockedAreas.Contains(requiredArea))
            return false;

        if (definition.RequiresWellCompleted && !completedProjectIds.Contains(VillageSiteIds.Well))
            return false;

        return true;
    }

    public static string PickFlavorText(CommunityActivityDefinition definition, uint variationSeed)
    {
        if (definition.FlavorTexts.Length == 0)
            return "You spend a moment helping the village.";

        return definition.FlavorTexts[(int)(variationSeed % (uint)definition.FlavorTexts.Length)];
    }

    public static string PickContributionText(CommunityActivityDefinition definition, uint variationSeed)
    {
        if (definition.ContributionTexts.Length == 0)
            return "You're contributing to village life.";

        return definition.ContributionTexts[(int)(variationSeed % (uint)definition.ContributionTexts.Length)];
    }

    public static string FormatActivityList(
        ISet<VillageArea> unlockedAreas,
        ISet<byte> completedProjectIds)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Community help activities:");
        builder.AppendLine("  Pitch in near village gathering spots — stronger Social relief than personal leisure.");
        builder.AppendLine();

        foreach (var activity in Activities)
        {
            var status = MeetsPrerequisites(activity, unlockedAreas, completedProjectIds)
                ? $"near ({activity.WorldX:F0},{activity.WorldZ:F0}) within {InteractionRadiusMeters:F0}m"
                : FormatPrerequisite(activity);
            builder.AppendLine($"  {activity.CommandHint} — {activity.LocationName} — {status} (cooldown {activity.Cooldown.TotalMinutes:F0}m)");
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatNearbyStatus(
        float playerX,
        float playerZ,
        ISet<VillageArea> unlockedAreas,
        ISet<byte> completedProjectIds)
    {
        var nearby = Activities
            .Where(activity => IsAvailableAt(activity, playerX, playerZ, unlockedAreas, completedProjectIds))
            .Select(activity => activity.CommandHint)
            .ToList();

        if (nearby.Count == 0)
            return "Community help nearby: (none — move to an unlocked garden, market, inn, lumber yard, smithy, guard post, village lanes, storyteller's bench, or completed well)";

        return $"Community help nearby: {string.Join(", ", nearby)}";
    }

    private static string FormatPrerequisite(CommunityActivityDefinition definition)
    {
        if (definition.RequiredArea is VillageArea area
            && VillageAreaConfig.TryGet(area, out var areaDefinition))
        {
            return $"requires {areaDefinition.Name} unlocked ({VillageAreaConfig.FormatUnlockRequirement(areaDefinition.RequiredLevel)})";
        }

        if (definition.RequiresWellCompleted)
            return "requires Village Well project completed";

        return "requirements not met";
    }
}

/// <summary>One community-help activity with social-focused effects and location metadata.</summary>
public sealed class CommunityActivityDefinition
{
    public required CommunityActivityKind Kind { get; init; }
    public required string CommandHint { get; init; }
    public required string LocationName { get; init; }
    public VillageArea? RequiredArea { get; init; }
    public bool RequiresWellCompleted { get; init; }
    public required float WorldX { get; init; }
    public required float WorldZ { get; init; }
    public required TimeSpan Cooldown { get; init; }
    public required float MoodGain { get; init; }
    public required float SocialReduction { get; init; }
    public required string[] FlavorTexts { get; init; }
    public required string[] ContributionTexts { get; init; }
}