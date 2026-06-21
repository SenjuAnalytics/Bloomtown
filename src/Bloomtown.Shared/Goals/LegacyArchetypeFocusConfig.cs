using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Goals;

/// <summary>
/// Conscious legacy-focus actions: deliberate choices that strengthen one path,
/// apply light drift to others, and unlock clearer village recognition.
/// </summary>
public static class LegacyArchetypeFocusConfig
{
    public const int ConsciousInfluencePoints = 2;
    public const int FocusDriftPenalty = 1;
    public const int FocusActionCooldownGameMinutes = 8;
    public const int MinInfluenceForIdentityRecognition = 6;
    public const int IdentityRecognitionChancePercent = 40;
    public const int IdentityAmbientChancePercent = 22;
    public const int IdentityInteractionCooldownGameMinutes = 35;
    public const int IdentityAmbientCooldownGameMinutes = 30;

    public const float BuilderSiteRadiusMeters = VillageSiteConfig.AmbientRadiusMeters;
    public const float CaretakerSiteRadiusMeters = Community.CommunityActivityConfig.InteractionRadiusMeters;
    public const float ConnectorMarketRadiusMeters = Community.CommunityActivityConfig.InteractionRadiusMeters;
    public const float ConnectorNpcRadiusMeters = 24f;

    private const float GardenWorldX = 20f;
    private const float GardenWorldZ = 14f;
    private const float MarketWorldX = 18f;
    private const float MarketWorldZ = 6f;

    public static bool IsValidFocusPath(LegacyArchetype path) =>
        path is LegacyArchetype.Builder or LegacyArchetype.Caretaker or LegacyArchetype.Connector;

    /// <summary>Conscious focus: +2 on chosen path, -1 drift on other paths (floor 0).</summary>
    public static LegacyArchetypeInfluence ApplyConsciousInfluenceGain(
        LegacyArchetypeInfluence current,
        LegacyArchetype path)
    {
        var updated = LegacyArchetypeAgencyConfig.ApplyInfluenceGain(
            current,
            path,
            ConsciousInfluencePoints);

        updated = ApplyDrift(updated, LegacyArchetype.Builder, path);
        updated = ApplyDrift(updated, LegacyArchetype.Caretaker, path);
        updated = ApplyDrift(updated, LegacyArchetype.Connector, path);

        return updated;
    }

    public static bool IsNearBuilderSite(float playerX, float playerZ) =>
        IsWithinRadius(playerX, playerZ, VillageSiteConfig.WellWorldX, VillageSiteConfig.WellWorldZ, BuilderSiteRadiusMeters)
        || IsWithinRadius(playerX, playerZ, VillageSiteConfig.BridgeWorldX, VillageSiteConfig.BridgeWorldZ, BuilderSiteRadiusMeters)
        || IsWithinRadius(playerX, playerZ, VillageSiteConfig.WarehouseWorldX, VillageSiteConfig.WarehouseWorldZ, BuilderSiteRadiusMeters);

    public static bool IsNearCaretakerSite(float playerX, float playerZ) =>
        IsWithinRadius(playerX, playerZ, GardenWorldX, GardenWorldZ, CaretakerSiteRadiusMeters)
        || IsWithinRadius(playerX, playerZ, VillageSiteConfig.WellWorldX, VillageSiteConfig.WellWorldZ, CaretakerSiteRadiusMeters);

    public static bool IsNearConnectorSite(float playerX, float playerZ, bool hasNearbyNpc) =>
        IsWithinRadius(playerX, playerZ, MarketWorldX, MarketWorldZ, ConnectorMarketRadiusMeters)
        || hasNearbyNpc;

    public static bool MeetsLocationRequirement(LegacyArchetype path, float playerX, float playerZ, bool hasNearbyNpc) =>
        path switch
        {
            LegacyArchetype.Builder => IsNearBuilderSite(playerX, playerZ),
            LegacyArchetype.Caretaker => IsNearCaretakerSite(playerX, playerZ),
            LegacyArchetype.Connector => IsNearConnectorSite(playerX, playerZ, hasNearbyNpc),
            _ => false,
        };

    public static string? TryGetConsciousActionFeedback(LegacyArchetype path, uint variationSeed) =>
        LegacyArchetypeFocusDialogue.TryGetConsciousActionFeedback(path, variationSeed);

    public static string? TryGetFocusTradeoffHint(LegacyArchetype focusedPath, uint variationSeed) =>
        LegacyArchetypeFocusDialogue.TryGetFocusTradeoffHint(focusedPath, variationSeed);

    public static bool QualifiesForIdentityRecognition(
        LegacyArchetype detectedArchetype,
        LegacyArchetypeInfluence influence) =>
        detectedArchetype != LegacyArchetype.None
        && influence.GetPoints(detectedArchetype) >= MinInfluenceForIdentityRecognition;

    public static string? TryGetIdentityRecognitionLine(LegacyArchetype archetype, uint variationSeed) =>
        LegacyArchetypeFocusDialogue.TryGetIdentityRecognitionLine(archetype, variationSeed);

    public static string? TryGetIdentityAmbientLine(LegacyArchetype archetype, uint variationSeed) =>
        LegacyArchetypeFocusDialogue.TryGetIdentityAmbientLine(archetype, variationSeed);

    public static bool ShouldTriggerIdentityRecognition(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 113 + (uint)(totalGameMinutes % 811)) % 100;
        return roll < IdentityRecognitionChancePercent;
    }

    public static bool ShouldTriggerIdentityAmbient(uint playerEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 131 + (uint)(totalGameMinutes % 743)) % 100;
        return roll < IdentityAmbientChancePercent;
    }

    public static string? FormatActiveFocusStatusLine(LegacyArchetype activeFocus, LegacyArchetypeInfluence influence)
    {
        if (!IsValidFocusPath(activeFocus))
            return null;

        var name = LegacyArchetypeConfig.GetDisplayName(activeFocus);
        var points = influence.GetPoints(activeFocus);
        return $"Legacy focus: {name} ({points}/{LegacyArchetypeAgencyConfig.MaxInfluencePerPath}) — other paths grow more slowly while you lean in.";
    }

    public static string FormatLocationHint(LegacyArchetype path) =>
        LegacyArchetypeFocusDialogue.FormatLocationHint(path);

    public static string? FormatFocusGuidanceDetail(LegacyArchetype activeFocus)
    {
        if (!IsValidFocusPath(activeFocus))
        {
            return "Conscious legacy choices: focus build | focus tend | focus connect (at the right location).";
        }

        return $"Last conscious focus: {LegacyArchetypeConfig.GetDisplayName(activeFocus)}. "
               + "Use focus build | focus tend | focus connect to strengthen a path deliberately.";
    }

    private static LegacyArchetypeInfluence ApplyDrift(
        LegacyArchetypeInfluence current,
        LegacyArchetype path,
        LegacyArchetype focusedPath)
    {
        if (path == focusedPath)
            return current;

        var points = current.GetPoints(path);
        if (points <= 0)
            return current;

        var reduced = Math.Max(0, points - FocusDriftPenalty);
        return path switch
        {
            LegacyArchetype.Builder => current with { BuilderPoints = reduced },
            LegacyArchetype.Caretaker => current with { CaretakerPoints = reduced },
            LegacyArchetype.Connector => current with { ConnectorPoints = reduced },
            _ => current,
        };
    }

    private static bool IsWithinRadius(float playerX, float playerZ, float siteX, float siteZ, float radiusMeters)
    {
        var dx = playerX - siteX;
        var dz = playerZ - siteZ;
        return dx * dx + dz * dz <= radiusMeters * radiusMeters;
    }
}