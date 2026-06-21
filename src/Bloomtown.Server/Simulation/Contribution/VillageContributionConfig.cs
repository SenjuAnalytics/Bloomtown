namespace Bloomtown.Server.Simulation.Contribution;

/// <summary>
/// Contribution score thresholds and title-based shop bonuses.
/// </summary>
public static class VillageContributionConfig
{
    public const int HelperThreshold = 50;
    public const int BuilderThreshold = 150;
    public const int RespectedVillagerThreshold = 300;
    public const int ElderCandidateThreshold = 500;

    /// <summary>Buy price multiplier for Respected Villager (5% discount).</summary>
    public const float RespectedVillagerBuyMultiplier = 0.95f;

    /// <summary>Buy price multiplier for Elder Candidate (10% discount).</summary>
    public const float ElderCandidateBuyMultiplier = 0.90f;

    /// <summary>Extra village reputation for Respected Villager+ when contributing to communal projects.</summary>
    public const int PoliticalContributionReputationBonus = 1;
}