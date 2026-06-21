namespace Bloomtown.Shared.Community;

/// <summary>Tracked community-help frequency and derived social role for one player.</summary>
public readonly record struct CommunityReputationState(
    int HelpGardenCount,
    int HelpMarketCount,
    int HelpWellCount)
{
    public int TotalHelpCount => HelpGardenCount + HelpMarketCount + HelpWellCount;

    public CommunityActivityKind? DominantActivity
    {
        get
        {
            if (TotalHelpCount == 0)
                return null;

            var max = Math.Max(HelpGardenCount, Math.Max(HelpMarketCount, HelpWellCount));
            if (HelpGardenCount == max && HelpGardenCount > 0)
                return CommunityActivityKind.HelpGarden;

            if (HelpMarketCount == max && HelpMarketCount > 0)
                return CommunityActivityKind.HelpMarket;

            if (HelpWellCount == max && HelpWellCount > 0)
                return CommunityActivityKind.HelpWell;

            return null;
        }
    }
}