namespace Bloomtown.Shared.Housing;

/// <summary>
/// Comfort score totals and sleep energy bonuses from placed furniture.
/// </summary>
public static class FurnitureComfortConfig
{
    public const int HighComfortThreshold = 41;
    public const int MediumComfortThreshold = 21;

    /// <summary>Energy restored when comfort score is 0–20.</summary>
    public const float LowComfortSleepRecovery = 65f;

    /// <summary>Energy restored when comfort score is 21–40.</summary>
    public const float MediumComfortSleepRecovery = 80f;

    /// <summary>Energy restored when comfort score is 41+.</summary>
    public const float HighComfortSleepRecovery = 100f;

    /// <summary>
    /// Sums comfort value from each placed furniture piece (quantity × comfort per type).
    /// </summary>
    public static int CalculateComfortScore(IReadOnlyDictionary<FurnitureType, int> placedFurniture)
    {
        var total = 0;
        foreach (var (furnitureType, quantity) in placedFurniture)
            total += FurnitureCatalog.GetComfortValue(furnitureType) * quantity;

        return total;
    }

    /// <summary>
    /// Sleep energy bonus tier based on total comfort score.
    /// </summary>
    public static float GetSleepEnergyRecovery(int comfortScore)
    {
        if (comfortScore >= HighComfortThreshold)
            return HighComfortSleepRecovery;

        if (comfortScore >= MediumComfortThreshold)
            return MediumComfortSleepRecovery;

        return LowComfortSleepRecovery;
    }

    /// <summary>
    /// Uses the better of house-tier sleep recovery and furniture comfort recovery.
    /// </summary>
    public static float GetCombinedSleepRecovery(HouseTier houseTier, int comfortScore)
    {
        var tierRecovery = HouseUpgradeConfig.GetSleepEnergyRecovery(houseTier);
        var comfortRecovery = GetSleepEnergyRecovery(comfortScore);
        return Math.Max(tierRecovery, comfortRecovery);
    }

    public static string FormatComfortTierLabel(int comfortScore)
    {
        if (comfortScore >= HighComfortThreshold)
            return $"cozy (+{HighComfortSleepRecovery:F0} sleep)";

        if (comfortScore >= MediumComfortThreshold)
            return $"comfortable (+{MediumComfortSleepRecovery:F0} sleep)";

        return $"basic (+{LowComfortSleepRecovery:F0} sleep)";
    }
}