namespace Bloomtown.Shared.Housing;

/// <summary>
/// Base and furniture-bonus recovery for cozy home activities.
/// Outdoor <c>rest</c> is weaker (see <see cref="Needs.PlayerNeedsConfig.RestMoodGain"/> / RestFatigueReduction).
/// </summary>
public static class HomeActivityConfig
{
    public readonly record struct ActivityEffects(
        float MoodGain,
        float FatigueReduction,
        bool BonusFurnitureApplied,
        FurnitureType? BonusFurniture);

    private static readonly IReadOnlyDictionary<HomeActivityType, HomeActivityDefinition> Definitions =
        new Dictionary<HomeActivityType, HomeActivityDefinition>
        {
            [HomeActivityType.Relax] = new(
                HomeActivityType.Relax,
                "relax",
                BaseMoodGain: 15f,
                BaseFatigueReduction: 40f,
                BonusFurniture: null,
                BonusMoodGain: 0f,
                BonusFatigueReduction: 0f,
                BaseFlavorTexts:
                [
                    "You take a moment to breathe and relax in your home.",
                    "You sink into a comfortable pause — the village noise feels far away.",
                    "A quiet minute at home helps your shoulders drop.",
                ],
                BonusFlavorTexts: []),

            [HomeActivityType.ReadBook] = new(
                HomeActivityType.ReadBook,
                "read",
                BaseMoodGain: 12f,
                BaseFatigueReduction: 25f,
                BonusFurniture: FurnitureType.Bookshelf,
                BonusMoodGain: 13f,
                BonusFatigueReduction: 10f,
                BaseFlavorTexts:
                [
                    "You spend some time reading a worn storybook from your shelf.",
                    "You lose yourself in a few chapters — a gentle escape.",
                ],
                BonusFlavorTexts:
                [
                    "You spend some time reading. The bookshelf makes it much more comfortable.",
                    "You curl up with a book beside your bookshelf — pages turn peacefully.",
                ]),

            [HomeActivityType.SitByTable] = new(
                HomeActivityType.SitByTable,
                "sit",
                BaseMoodGain: 10f,
                BaseFatigueReduction: 30f,
                BonusFurniture: FurnitureType.SmallTable,
                BonusMoodGain: 8f,
                BonusFatigueReduction: 8f,
                BaseFlavorTexts:
                [
                    "You sit quietly and let the day settle around you.",
                    "You rest at your table, listening to the soft sounds of home.",
                ],
                BonusFlavorTexts:
                [
                    "You sit by your table — a small ritual that steadies your mood.",
                    "Tea cup aside, you enjoy a calm moment at the table you placed.",
                ]),

            [HomeActivityType.EnjoyTea] = new(
                HomeActivityType.EnjoyTea,
                "tea",
                BaseMoodGain: 12f,
                BaseFatigueReduction: 28f,
                BonusFurniture: FurnitureType.WoodenChair,
                BonusMoodGain: 4f,
                BonusFatigueReduction: 4f,
                BaseFlavorTexts:
                [
                    "You brew a warm cup and sip slowly in your cozy corner.",
                    "The tea is simple, but being home makes it taste better.",
                ],
                BonusFlavorTexts:
                [
                    "You enjoy tea seated in your wooden chair — warmth spreads through you.",
                    "A quiet cup of tea in your chair feels like a small celebration.",
                ]),

            [HomeActivityType.Nap] = new(
                HomeActivityType.Nap,
                "nap",
                BaseMoodGain: 8f,
                BaseFatigueReduction: 48f,
                BonusFurniture: FurnitureType.SimpleBed,
                BonusMoodGain: 5f,
                BonusFatigueReduction: 10f,
                BaseFlavorTexts:
                [
                    "You curl up for a short nap — the village noise fades to a distant murmur.",
                    "A brief rest at home lets your body unclench; you wake a little lighter.",
                    "You close your eyes for just a while; even a small nap steadies the day.",
                ],
                BonusFlavorTexts:
                [
                    "You nap in your own bed — the mattress welcomes you like an old friend.",
                    "A proper nap in your bed leaves you surprisingly refreshed.",
                ]),
        };

    public static IReadOnlyList<HomeActivityType> AllActivities { get; } =
        Definitions.Keys.OrderBy(type => (int)type).ToList();

    public static bool IsKnownActivity(HomeActivityType type) => Definitions.ContainsKey(type);

    public static string GetCommandName(HomeActivityType type) =>
        Definitions.TryGetValue(type, out var definition) ? definition.CommandName : "cozy activity";

    /// <summary>
    /// Sums base recovery plus optional furniture bonus when the piece is placed.
    /// </summary>
    public static ActivityEffects CalculateEffects(
        HomeActivityType type,
        IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        if (!Definitions.TryGetValue(type, out var definition))
            return new ActivityEffects(0f, 0f, false, null);

        var moodGain = definition.BaseMoodGain;
        var fatigueReduction = definition.BaseFatigueReduction;
        var bonusApplied = false;

        if (definition.BonusFurniture is FurnitureType bonusFurniture
            && placedFurniture.GetValueOrDefault(bonusFurniture, 0) > 0)
        {
            moodGain += definition.BonusMoodGain;
            fatigueReduction += definition.BonusFatigueReduction;
            bonusApplied = true;
        }

        return new ActivityEffects(moodGain, fatigueReduction, bonusApplied, definition.BonusFurniture);
    }

    public static string PickFlavorText(HomeActivityType type, bool bonusApplied, uint variationSeed)
    {
        if (!Definitions.TryGetValue(type, out var definition))
            return "You spend a peaceful moment at home.";

        var texts = bonusApplied && definition.BonusFlavorTexts.Length > 0
            ? definition.BonusFlavorTexts
            : definition.BaseFlavorTexts;

        if (texts.Length == 0)
            return "You spend a peaceful moment at home.";

        var index = (int)(variationSeed % (uint)texts.Length);
        return texts[index];
    }

    public static string FormatActivityHint(HomeActivityType type, IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        if (!Definitions.TryGetValue(type, out var definition))
            return string.Empty;

        var effects = CalculateEffects(type, placedFurniture);
        var command = definition.CommandName;
        var bonusNote = definition.BonusFurniture is FurnitureType furniture
            ? effects.BonusFurnitureApplied
                ? $" (bonus: {FurnitureTypeDisplay.GetName(furniture)})"
                : $" (place {FurnitureTypeDisplay.GetName(furniture).ToLowerInvariant()} for bonus)"
            : string.Empty;

        return $"  {command} — Mood +{effects.MoodGain:F0}, Fatigue -{effects.FatigueReduction:F0}{bonusNote}";
    }

    public static string FormatAvailableActivities(IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        var lines = new List<string> { "Cozy activities (at home):" };
        foreach (var type in AllActivities)
            lines.Add(FormatActivityHint(type, placedFurniture));

        return string.Join(Environment.NewLine, lines);
    }

    private sealed record HomeActivityDefinition(
        HomeActivityType Type,
        string CommandName,
        float BaseMoodGain,
        float BaseFatigueReduction,
        FurnitureType? BonusFurniture,
        float BonusMoodGain,
        float BonusFatigueReduction,
        string[] BaseFlavorTexts,
        string[] BonusFlavorTexts);
}