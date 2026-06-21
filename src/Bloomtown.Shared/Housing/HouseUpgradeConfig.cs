using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Housing;

/// <summary>
/// Material requirements per upgrade step and sleep energy bonuses by house tier.
/// </summary>
public static class HouseUpgradeConfig
{
    /// <summary>Energy restored when sleeping at a Basic-tier home.</summary>
    public const float BasicSleepEnergyRecovery = 65f;

    /// <summary>Energy restored when sleeping at an Improved-tier home.</summary>
    public const float ImprovedSleepEnergyRecovery = 80f;

    /// <summary>Energy restored when sleeping at a Comfortable-tier home.</summary>
    public const float ComfortableSleepEnergyRecovery = 100f;

    private static readonly Dictionary<HouseTier, IReadOnlyDictionary<ItemType, int>> UpgradeRequirements = new()
    {
        [HouseTier.Basic] = new Dictionary<ItemType, int>
        {
            [ItemType.Wood] = 20,
            [ItemType.Plank] = 10,
        },
        [HouseTier.Improved] = new Dictionary<ItemType, int>
        {
            [ItemType.Plank] = 30,
            [ItemType.Stone] = 15,
            [ItemType.Tool] = 2,
        },
    };

    public static HouseTier MaxTier => HouseTier.Comfortable;

    public static bool IsMaxTier(HouseTier tier) => tier >= MaxTier;

    /// <summary>
    /// Returns the next tier and material costs when upgrading from the current tier.
    /// </summary>
    public static bool TryGetUpgradeRequirements(
        HouseTier currentTier,
        out HouseTier nextTier,
        out IReadOnlyDictionary<ItemType, int> requirements)
    {
        if (IsMaxTier(currentTier))
        {
            nextTier = currentTier;
            requirements = new Dictionary<ItemType, int>();
            return false;
        }

        nextTier = currentTier + 1;
        requirements = UpgradeRequirements[currentTier];
        return true;
    }

    public static float GetSleepEnergyRecovery(HouseTier tier)
    {
        return tier switch
        {
            HouseTier.Comfortable => ComfortableSleepEnergyRecovery,
            HouseTier.Improved => ImprovedSleepEnergyRecovery,
            _ => BasicSleepEnergyRecovery,
        };
    }

    public static string FormatRequirements(IReadOnlyDictionary<ItemType, int> requirements)
    {
        return string.Join(
            ", ",
            requirements
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Value} {ItemDatabase.GetDisplayName(pair.Key)}"));
    }
}