using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Needs;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Runtime wallet and inventory for one connected player.
/// </summary>
public sealed class PlayerEconomyState
{
    public required uint PlayerEntityId { get; init; }
    public Inventory Inventory { get; } = new();
    public int Coins { get; set; } = EconomyConfig.StartingCoins;
    public int VillageReputation { get; set; }
    public int VillageContributionScore { get; set; }
    public VillageTitle VillageTitle { get; set; } = VillageTitle.Newcomer;
    public VillagePosition VillagePosition { get; set; } = VillagePosition.None;
    public DateTime? PositionAssignedAtUtc { get; set; }
    public float Energy { get; set; } = VillageMilestoneConfig.DefaultPlayerEnergy;
    public float Hunger { get; set; } = PlayerHungerConfig.DefaultHunger;
    public float Mood { get; set; } = PlayerNeedsConfig.DefaultMood;
    public float Fatigue { get; set; } = PlayerNeedsConfig.DefaultFatigue;
    public float SocialNeed { get; set; } = PlayerNeedsConfig.DefaultSocialNeed;
    public long LastNeedsUpdateTotalGameMinute { get; set; }
}