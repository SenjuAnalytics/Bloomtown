using Bloomtown.Shared.Community;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Time-of-day bands, location resolution, ambient cooldowns, and village rhythm status text.
/// </summary>
public static class VillageLifeConfig
{
    public static readonly TimeSpan TimeLocationCommentCooldown = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan EmergentEventCooldown = TimeSpan.FromMinutes(5);
    /// <summary>Legacy alias — social cooldowns live in <see cref="VillageCommunityLifeConfig"/>.</summary>
    public static readonly TimeSpan NpcToNpcCommentCooldown = VillageCommunityLifeConfig.NpcToNpcCommentCooldown;

    /// <summary>Low chance per ambient check — emergent events stay rare.</summary>
    public const int EmergentEventChancePercent = 15;

    /// <summary>
    /// Maps game hour (0–23) to a broad time band for ambient flavor selection.
    /// Morning 05–11, Afternoon 12–16, Evening 17–20, Night 21–04.
    /// </summary>
    public static GameTimeOfDay GetTimeOfDay(int gameHour)
    {
        return gameHour switch
        {
            >= 5 and < 12 => GameTimeOfDay.Morning,
            >= 12 and < 17 => GameTimeOfDay.Afternoon,
            >= 17 and < 21 => GameTimeOfDay.Evening,
            _ => GameTimeOfDay.Night,
        };
    }

    public static string GetTimeOfDayDisplayName(GameTimeOfDay timeOfDay) =>
        timeOfDay switch
        {
            GameTimeOfDay.Morning => "Morning",
            GameTimeOfDay.Afternoon => "Afternoon",
            GameTimeOfDay.Evening => "Evening",
            _ => "Night",
        };

    public static string FormatGameTimeStatus(int gameDay, int gameHour, int minuteOfHour)
    {
        var band = GetTimeOfDay(gameHour);
        return $"Game time: Day {gameDay}, {gameHour:D2}:{minuteOfHour:D2} ({GetTimeOfDayDisplayName(band)})";
    }

    /// <summary>Short rhythm line for the status command — time + development level.</summary>
    public static string FormatVillageRhythm(GameTimeOfDay timeOfDay, VillageDevelopmentLevel developmentLevel)
    {
        return (timeOfDay, developmentLevel) switch
        {
            (GameTimeOfDay.Morning, VillageDevelopmentLevel.Bustling) =>
                "Village rhythm: Bloomtown wakes busy — morning chores and greetings everywhere.",
            (GameTimeOfDay.Morning, VillageDevelopmentLevel.Lively) =>
                "Village rhythm: A warm morning bustle is growing in the lanes.",
            (GameTimeOfDay.Morning, _) =>
                "Village rhythm: A calm morning — the village eases into the day.",

            (GameTimeOfDay.Afternoon, VillageDevelopmentLevel.Bustling) =>
                "Village rhythm: Afternoon energy fills the streets — Bloomtown feels fully alive.",
            (GameTimeOfDay.Afternoon, VillageDevelopmentLevel.Lively) =>
                "Village rhythm: Steady afternoon life — neighbors crossing paths often.",
            (GameTimeOfDay.Afternoon, _) =>
                "Village rhythm: A peaceful afternoon pace — unhurried and quiet.",

            (GameTimeOfDay.Evening, VillageDevelopmentLevel.Bustling) =>
                "Village rhythm: Evening settles warmly — lanterns and supper smells.",
            (GameTimeOfDay.Evening, VillageDevelopmentLevel.Lively) =>
                "Village rhythm: Dusk brings softer voices and a homely feel.",
            (GameTimeOfDay.Evening, _) =>
                "Village rhythm: Evening hush — the village winds down gently.",

            (GameTimeOfDay.Night, VillageDevelopmentLevel.Bustling) =>
                "Village rhythm: Night is quieter, but you still sense neighbors nearby.",
            (GameTimeOfDay.Night, VillageDevelopmentLevel.Lively) =>
                "Village rhythm: A restful night — the grown village sleeps together.",
            (GameTimeOfDay.Night, _) =>
                "Village rhythm: Deep night calm — Bloomtown rests.",

            _ => "Village rhythm: Bloomtown carries on at its own gentle pace.",
        };
    }

    public static string FormatNearbyFeel(VillageAmbientLocation location)
    {
        if (location == VillageAmbientLocation.General)
            return string.Empty;

        var name = GetLocationDisplayName(location);
        var feel = location switch
        {
            VillageAmbientLocation.MarketSquare => "lively trade and chatter",
            VillageAmbientLocation.CommunityGarden => "restful green calm",
            VillageAmbientLocation.RiversideWalk => "easy river air",
            VillageAmbientLocation.VillageWell => "neighborly gathering",
            VillageAmbientLocation.RepairedBridge => "safe crossing and river breeze",
            VillageAmbientLocation.VillageWarehouse => "shared purpose and order",
            _ => "village life",
        };

        return $"Nearby feel: {name} — {feel}.";
    }

    public static string GetLocationDisplayName(VillageAmbientLocation location) =>
        location switch
        {
            VillageAmbientLocation.MarketSquare => "Market Square",
            VillageAmbientLocation.CommunityGarden => "Community Garden",
            VillageAmbientLocation.RiversideWalk => "Riverside Walk",
            VillageAmbientLocation.VillageWell => "Village Well",
            VillageAmbientLocation.RepairedBridge => "Repaired Bridge",
            VillageAmbientLocation.VillageWarehouse => "Village Warehouse",
            _ => "Bloomtown",
        };

    /// <summary>
    /// Resolves the nearest recognized village spot within ambient radius.
    /// Unlocked areas and completed project sites only.
    /// </summary>
    public static bool TryResolveAmbientLocation(
        float playerX,
        float playerZ,
        ISet<VillageArea> unlockedAreas,
        IReadOnlyCollection<byte> completedProjectIds,
        out VillageAmbientLocation location,
        out float nearestDistanceMeters)
    {
        location = VillageAmbientLocation.General;
        nearestDistanceMeters = float.MaxValue;
        VillageAmbientLocation? nearest = null;

        foreach (var areaDefinition in VillageAreaConfig.All)
        {
            if (!unlockedAreas.Contains(areaDefinition.Area))
                continue;

            var distance = GetDistance(playerX, playerZ, areaDefinition.WorldX, areaDefinition.WorldZ);
            if (distance > VillageAreaConfig.PassiveRadiusMeters || distance >= nearestDistanceMeters)
                continue;

            nearestDistanceMeters = distance;
            nearest = MapAreaToLocation(areaDefinition.Area);
        }

        if (completedProjectIds.Contains(VillageSiteIds.Well))
            TryConsiderSite(playerX, playerZ, VillageSiteConfig.WellWorldX, VillageSiteConfig.WellWorldZ, VillageAmbientLocation.VillageWell, ref nearest, ref nearestDistanceMeters);

        if (completedProjectIds.Contains(VillageSiteIds.Bridge))
            TryConsiderSite(playerX, playerZ, VillageSiteConfig.BridgeWorldX, VillageSiteConfig.BridgeWorldZ, VillageAmbientLocation.RepairedBridge, ref nearest, ref nearestDistanceMeters);

        if (completedProjectIds.Contains(VillageSiteIds.Warehouse))
            TryConsiderSite(playerX, playerZ, VillageSiteConfig.WarehouseWorldX, VillageSiteConfig.WarehouseWorldZ, VillageAmbientLocation.VillageWarehouse, ref nearest, ref nearestDistanceMeters);

        if (nearest is null)
            return true;

        location = nearest.Value;
        return true;
    }

    /// <summary>Deterministic low-probability roll for emergent flavor events.</summary>
    public static bool ShouldTriggerEmergentEvent(uint playerEntityId, long totalGameMinutes, uint attemptCounter)
    {
        var roll = (playerEntityId * 31 + (uint)(totalGameMinutes % 997) + attemptCounter * 17) % 100;
        return roll < EmergentEventChancePercent;
    }

    private static void TryConsiderSite(
        float playerX,
        float playerZ,
        float siteX,
        float siteZ,
        VillageAmbientLocation siteLocation,
        ref VillageAmbientLocation? nearest,
        ref float nearestDistanceMeters)
    {
        var distance = GetDistance(playerX, playerZ, siteX, siteZ);
        if (distance > VillageSiteConfig.AmbientRadiusMeters || distance >= nearestDistanceMeters)
            return;

        nearestDistanceMeters = distance;
        nearest = siteLocation;
    }

    private static VillageAmbientLocation MapAreaToLocation(VillageArea area) =>
        area switch
        {
            VillageArea.MarketSquare => VillageAmbientLocation.MarketSquare,
            VillageArea.CommunityGarden => VillageAmbientLocation.CommunityGarden,
            VillageArea.RiversideWalk => VillageAmbientLocation.RiversideWalk,
            _ => VillageAmbientLocation.General,
        };

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}