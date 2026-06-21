namespace Bloomtown.Shared.Community;

/// <summary>
/// Passive benefit values granted when a communal project is completed.
/// Base interaction values live in VillageMilestoneConfig; these are the boosted amounts.
/// </summary>
public static class VillageProjectBenefitConfig
{
    public const byte WellProjectId = 1;
    public const byte BridgeProjectId = 2;
    public const byte WarehouseProjectId = 3;

    /// <summary>Energy restored by 'drink well' once the well project is complete (base: 15).</summary>
    public const float WellDrinkEnergyRestore = 25f;

    /// <summary>Coins from 'collect stipend' once the warehouse is complete (base: 5).</summary>
    public const int WarehouseStipendCoins = 8;

    /// <summary>Fatigue reduced when crossing the repaired bridge.</summary>
    public const float BridgeCrossFatigueReduction = 5f;

    /// <summary>Radius around the bridge where passive fatigue relief applies.</summary>
    public const float BridgePassiveRadiusMeters = 12f;

    /// <summary>Passive fatigue relief per real second while near the bridge.</summary>
    public const float BridgePassiveFatigueReliefPerSecond = 0.12f;

    /// <summary>Radius for NPC ambient comments near completed project sites.</summary>
    public const float ProjectSiteCommentRadiusMeters = 14f;

    public static readonly TimeSpan ProjectSiteCommentCooldown = TimeSpan.FromMinutes(3);

    public static float GetWellDrinkEnergyRestore(bool projectComplete) =>
        projectComplete ? WellDrinkEnergyRestore : 15f;

    public static int GetWarehouseStipendCoins(bool projectComplete) =>
        projectComplete ? WarehouseStipendCoins : 5;

    public static string FormatStatusBenefit(byte projectId)
    {
        return projectId switch
        {
            WellProjectId => "drink well restores +25 Energy",
            BridgeProjectId => "less Fatigue near the bridge",
            WarehouseProjectId => "collect stipend gives +8 coins",
            _ => "village benefit active",
        };
    }

    public static string FormatProjectDisplayName(byte projectId)
    {
        return projectId switch
        {
            WellProjectId => "Village Well",
            BridgeProjectId => "Repaired Bridge",
            WarehouseProjectId => "Village Warehouse",
            _ => $"Project {projectId}",
        };
    }
}