namespace Bloomtown.Shared.Village;

/// <summary>
/// Ambient flavor lines heard while spending time in unlocked village areas.
/// </summary>
public static class VillageAreaNpcDialogue
{
    private static readonly string[] MarketSquareLines =
    [
        "Someone nearby laughs about yesterday's prices — the square feels alive.",
        "You catch bits of friendly bargaining from the market stalls.",
        "A villager waves as they pass with a basket of fresh goods.",
        "The hum of conversation here always lifts the mood a little.",
    ];

    private static readonly string[] CommunityGardenLines =
    [
        "Bees drift between the herbs — the garden smells wonderful today.",
        "A neighbor is quietly tending the shared beds nearby.",
        "The garden bench looks inviting; even standing here feels restful.",
        "Green leaves rustle softly — a peaceful corner of Bloomtown.",
    ];

    private static readonly string[] RiversideWalkLines =
    [
        "The river murmurs along the path — easy to lose track of time here.",
        "Cool air off the water makes the walk feel lighter.",
        "Someone left fresh footprints on the riverside path ahead.",
        "You spot a heron downstream; the village feels connected to the wild.",
    ];

    public static string? TryGetAreaAmbientComment(VillageArea area, uint variationSeed)
    {
        var lines = area switch
        {
            VillageArea.MarketSquare => MarketSquareLines,
            VillageArea.CommunityGarden => CommunityGardenLines,
            VillageArea.RiversideWalk => RiversideWalkLines,
            _ => null,
        };

        if (lines is null || lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    internal static string[] GetMarketSquareLines() => MarketSquareLines;

    internal static string[] GetCommunityGardenLines() => CommunityGardenLines;

    internal static string[] GetRiversideWalkLines() => RiversideWalkLines;
}