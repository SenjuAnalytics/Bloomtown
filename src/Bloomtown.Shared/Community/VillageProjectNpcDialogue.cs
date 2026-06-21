namespace Bloomtown.Shared.Community;

/// <summary>
/// Short NPC-flavored lines for completed village projects.
/// </summary>
public static class VillageProjectNpcDialogue
{
    private static readonly string[] WellCompletionLines =
    [
        "Fresh water at last! The well will keep everyone going.",
        "I can already taste the cool water — thank you all for building this.",
        "No more long walks for water. The village well is a blessing.",
    ];

    private static readonly string[] BridgeCompletionLines =
    [
        "The bridge is sturdy again — we can cross the river safely!",
        "I watched the repairs all week. What a relief to walk across freely.",
        "Travelers won't have to take the long way around anymore.",
    ];

    private static readonly string[] WarehouseCompletionLines =
    [
        "Extra storage means we can share supplies more fairly.",
        "The warehouse is open — our harvest finally has a proper home.",
        "I heard there's a small stipend for helpers. The village remembers contributors.",
    ];

    private static readonly string[] WellSiteLines =
    [
        "Cool water, right when you need it.",
        "The well has been such a help this season.",
        "I fill my bucket here every morning now.",
    ];

    private static readonly string[] BridgeSiteLines =
    [
        "Crossing here feels so much easier since the repairs.",
        "The breeze over the bridge always wakes me up.",
        "I don't worry about the planks anymore — solid work.",
    ];

    private static readonly string[] WarehouseSiteLines =
    [
        "Plenty of room for everyone's tools and harvest now.",
        "The stipend drawer is stocked — don't forget to collect yours.",
        "I like knowing our supplies are safe in the warehouse.",
    ];

    /// <summary>Broadcast line when a project completes (appended to milestone notification).</summary>
    public static string GetCompletionBroadcast(byte projectId)
    {
        var lines = projectId switch
        {
            VillageProjectBenefitConfig.WellProjectId => WellCompletionLines,
            VillageProjectBenefitConfig.BridgeProjectId => BridgeCompletionLines,
            VillageProjectBenefitConfig.WarehouseProjectId => WarehouseCompletionLines,
            _ => ["The village feels a little brighter today."],
        };

        return Pick(lines, projectId);
    }

    /// <summary>Ambient comment when a player is near a completed project site.</summary>
    public static string? TryGetSiteAmbientComment(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed)
    {
        var lines = GetSiteLines(projectId, developmentLevel);
        if (lines is null || lines.Length == 0)
            return null;

        return Pick(lines, variationSeed);
    }

    private static string[]? GetSiteLines(byte projectId, VillageDevelopmentLevel developmentLevel)
    {
        return projectId switch
        {
            VillageProjectBenefitConfig.WellProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "The well's always busy now — everyone stops by for water and gossip.",
                    "I love how the village gathers here since the well went in.",
                    ..WellSiteLines,
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "The well changed how mornings feel around here.",
                    ..WellSiteLines,
                ],
                _ => WellSiteLines,
            },
            VillageProjectBenefitConfig.BridgeProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "Foot traffic over the bridge never seems to stop these days.",
                    "I waved to three neighbors crossing just this hour.",
                    ..BridgeSiteLines,
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "More folks take the bridge now that it's safe again.",
                    ..BridgeSiteLines,
                ],
                _ => BridgeSiteLines,
            },
            VillageProjectBenefitConfig.WarehouseProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "The warehouse keeps the whole village supplied — what a relief.",
                    "Helpers are in and out all day since we finished this place.",
                    ..WarehouseSiteLines,
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "The warehouse already feels like the center of trade here.",
                    ..WarehouseSiteLines,
                ],
                _ => WarehouseSiteLines,
            },
            _ => null,
        };
    }

    private static string Pick(string[] lines, uint seed)
    {
        var index = (int)(seed % (uint)lines.Length);
        return lines[index];
    }
}