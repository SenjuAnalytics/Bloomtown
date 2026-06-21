using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Unlock requirements, world positions, passive effects, and interaction definitions for village areas.
/// </summary>
public static class VillageAreaConfig
{
    public const float InteractionRadiusMeters = 10f;

    /// <summary>Radius for gentle passive Mood/Fatigue relief while lingering in an unlocked area.</summary>
    public const float PassiveRadiusMeters = 12f;

    public static readonly TimeSpan AreaAmbientCommentCooldown = TimeSpan.FromMinutes(4);

    private static readonly VillageAreaDefinition[] Areas =
    [
        new()
        {
            Area = VillageArea.MarketSquare,
            Name = "Market Square",
            Description = "A small trading corner where villagers gather to barter and chat.",
            RequiredLevel = VillageDevelopmentLevel.Lively,
            WorldX = 18f,
            WorldZ = 6f,
            // Passive: gentle mood lift from the lively bustle.
            PassiveMoodRecoveryPerGameMinute = 0.04f,
            UnlockAnnouncement =
                "The Market Square is now open! Try 'browse market' or 'chat locals' nearby.",
        },
        new()
        {
            Area = VillageArea.CommunityGarden,
            Name = "Community Garden",
            Description = "Shared garden beds and a quiet bench — a cozy place to unwind.",
            RequiredLevel = VillageDevelopmentLevel.Bustling,
            WorldX = 20f,
            WorldZ = 14f,
            // Passive: shared green space eases fatigue over time.
            PassiveFatigueRecoveryPerGameMinute = 0.04f,
            UnlockAnnouncement =
                "The Community Garden has opened! Try 'relax garden' or 'tend plants' nearby.",
        },
        new()
        {
            Area = VillageArea.RiversideWalk,
            Name = "Riverside Walk",
            Description = "A pleasant path along the water, opened as the village flourishes.",
            RequiredLevel = VillageDevelopmentLevel.Bustling,
            WorldX = 14f,
            WorldZ = 18f,
            // Passive: riverside air lifts mood and eases fatigue gently.
            PassiveMoodRecoveryPerGameMinute = 0.03f,
            PassiveFatigueRecoveryPerGameMinute = 0.03f,
            UnlockAnnouncement =
                "The Riverside Walk is open! Try 'stroll river' or 'reflect river' nearby.",
        },
    ];

    private static readonly VillageAreaInteractionDefinition[] Interactions =
    [
        new()
        {
            Kind = VillageAreaInteractionKind.BrowseMarket,
            Area = VillageArea.MarketSquare,
            CommandHint = "browse market",
            Cooldown = TimeSpan.FromMinutes(3),
            MoodGain = 3f,
            SocialReduction = 2f,
            FlavorLines =
            [
                "You wander between the stalls, taking in colors and friendly chatter.",
                "A vendor offers a sample — the square feels welcoming today.",
                "You pause to watch a trade unfold; the bustle is oddly soothing.",
            ],
        },
        new()
        {
            Kind = VillageAreaInteractionKind.ChatLocals,
            Area = VillageArea.MarketSquare,
            CommandHint = "chat locals",
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 2f,
            SocialReduction = 4f,
            FlavorLines =
            [
                "You swap small talk with a few regulars — everyone seems glad to see you.",
                "A local shares village gossip; you feel a little more connected.",
                "Laughter ripples through a cluster of shoppers as you join the conversation.",
            ],
        },
        new()
        {
            Kind = VillageAreaInteractionKind.RelaxGarden,
            Area = VillageArea.CommunityGarden,
            CommandHint = "relax garden",
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 5f,
            FatigueReduction = 6f,
            FlavorLines =
            [
                "You settle on the shared bench and let the garden quiet your thoughts.",
                "Herbs and flowers sway nearby — a peaceful break from the day's work.",
                "You breathe slowly, listening to bees among the beds; tension fades.",
            ],
        },
        new()
        {
            Kind = VillageAreaInteractionKind.TendPlants,
            Area = VillageArea.CommunityGarden,
            CommandHint = "tend plants",
            Cooldown = TimeSpan.FromMinutes(3),
            MoodGain = 3f,
            FatigueReduction = 3f,
            FlavorLines =
            [
                "You pull a few weeds and straighten a bed — small work, steady satisfaction.",
                "Watering the shared herbs feels grounding; the garden appreciates the care.",
                "You tidy a row of seedlings, enjoying the calm rhythm of tending.",
            ],
        },
        new()
        {
            Kind = VillageAreaInteractionKind.StrollRiver,
            Area = VillageArea.RiversideWalk,
            CommandHint = "stroll river",
            Cooldown = TimeSpan.FromMinutes(3),
            MoodGain = 4f,
            FatigueReduction = 4f,
            FlavorLines =
            [
                "You walk the riverside path at an easy pace, letting the water set the tempo.",
                "Cool air off the river makes each step feel lighter.",
                "You follow the bend in the path, soothed by the steady murmur of the stream.",
            ],
        },
        new()
        {
            Kind = VillageAreaInteractionKind.ReflectRiver,
            Area = VillageArea.RiversideWalk,
            CommandHint = "reflect river",
            Cooldown = TimeSpan.FromMinutes(4),
            MoodGain = 6f,
            FatigueReduction = 2f,
            FlavorLines =
            [
                "You find a quiet spot and watch the current — thoughts settle like silt downstream.",
                "The river's patience reminds you to breathe; you leave feeling clearer.",
                "You sit awhile, listening to the water, and carry a little more peace with you.",
            ],
        },
    ];

    public static IReadOnlyList<VillageAreaDefinition> All => Areas;

    public static IReadOnlyList<VillageAreaInteractionDefinition> AllInteractions => Interactions;

    public static bool TryGet(VillageArea area, out VillageAreaDefinition definition)
    {
        definition = Areas.FirstOrDefault(entry => entry.Area == area)!;
        return definition is not null;
    }

    public static bool TryGetInteraction(
        VillageAreaInteractionKind kind,
        out VillageAreaInteractionDefinition definition)
    {
        definition = Interactions.FirstOrDefault(entry => entry.Kind == kind)!;
        return definition is not null;
    }

    public static IReadOnlyList<VillageAreaInteractionDefinition> GetInteractionsForArea(VillageArea area) =>
        Interactions.Where(entry => entry.Area == area).ToList();

    public static bool IsUnlockedByLevel(VillageArea area, VillageDevelopmentLevel level)
    {
        return TryGet(area, out var definition) && level >= definition.RequiredLevel;
    }

    public static string FormatUnlockRequirement(VillageDevelopmentLevel level) =>
        VillageAtmosphereConfig.GetDisplayName(level);

    public static string FormatPassiveSummary(VillageAreaDefinition definition)
    {
        var parts = new List<string>();
        if (definition.PassiveMoodRecoveryPerGameMinute > 0f)
            parts.Add("gentle Mood while nearby");

        if (definition.PassiveFatigueRecoveryPerGameMinute > 0f)
            parts.Add("eases Fatigue while nearby");

        return parts.Count == 0 ? string.Empty : $" — passive: {string.Join(", ", parts)}";
    }

    public static string FormatStatusLine(VillageAreaDefinition definition, bool unlocked)
    {
        if (!unlocked)
        {
            return $"{definition.Name} — Locked (requires {FormatUnlockRequirement(definition.RequiredLevel)})";
        }

        var commands = string.Join(", ", GetInteractionsForArea(definition.Area).Select(i => i.CommandHint));
        return
            $"{definition.Name} — Unlocked — {commands} at ({definition.WorldX:F0},{definition.WorldZ:F0}){FormatPassiveSummary(definition)}";
    }

    public static string GetUnlockFlavor(VillageArea area)
    {
        if (!TryGet(area, out var definition))
            return "A new part of the village is now open to explore.";

        return definition.UnlockAnnouncement;
    }

    public static TimeSpan GetInteractionCooldown(VillageAreaInteractionKind interaction) =>
        TryGetInteraction(interaction, out var definition) ? definition.Cooldown : TimeSpan.Zero;

    public static string PickInteractionFlavor(VillageAreaInteractionDefinition interaction, uint variationSeed)
    {
        if (interaction.FlavorLines.Length == 0)
            return "You spend a pleasant moment here.";

        return interaction.FlavorLines[(int)(variationSeed % (uint)interaction.FlavorLines.Length)];
    }
}

/// <summary>
/// Static definition for one unlockable village area (metadata and passive effects).
/// </summary>
public sealed class VillageAreaDefinition
{
    public required VillageArea Area { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required VillageDevelopmentLevel RequiredLevel { get; init; }
    public required float WorldX { get; init; }
    public required float WorldZ { get; init; }
    public required string UnlockAnnouncement { get; init; }

    /// <summary>Passive mood recovery per game minute while inside <see cref="VillageAreaConfig.PassiveRadiusMeters"/>.</summary>
    public float PassiveMoodRecoveryPerGameMinute { get; init; }

    /// <summary>Passive fatigue relief per game minute while inside <see cref="VillageAreaConfig.PassiveRadiusMeters"/>.</summary>
    public float PassiveFatigueRecoveryPerGameMinute { get; init; }
}

/// <summary>
/// One player command available at an unlocked village area.
/// </summary>
public sealed class VillageAreaInteractionDefinition
{
    public required VillageAreaInteractionKind Kind { get; init; }
    public required VillageArea Area { get; init; }
    public required string CommandHint { get; init; }
    public required string[] FlavorLines { get; init; }
    public required TimeSpan Cooldown { get; init; }
    public float MoodGain { get; init; }
    public float FatigueReduction { get; init; }
    public float SocialReduction { get; init; }
}