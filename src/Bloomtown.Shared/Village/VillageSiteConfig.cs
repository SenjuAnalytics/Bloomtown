namespace Bloomtown.Shared.Village;

/// <summary>
/// World positions for communal project sites (mirrors milestone registry coords).
/// </summary>
public static class VillageSiteConfig
{
    public const float AmbientRadiusMeters = 14f;

    public const float WellWorldX = 5f;
    public const float WellWorldZ = 5f;

    public const float BridgeWorldX = 12f;
    public const float BridgeWorldZ = 8f;

    public const float WarehouseWorldX = 6f;
    public const float WarehouseWorldZ = 12f;

    public static bool IsProjectSite(byte projectId) =>
        projectId is VillageSiteIds.Well or VillageSiteIds.Bridge or VillageSiteIds.Warehouse;
}

/// <summary>Built-in communal project ids used for site location checks.</summary>
public static class VillageSiteIds
{
    public const byte Well = 1;
    public const byte Bridge = 2;
    public const byte Warehouse = 3;
}