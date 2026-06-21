using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Needs;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerRecord
{
    public uint EntityId { get; init; }
    public float PositionX { get; init; }
    public float PositionY { get; init; }
    public float PositionZ { get; init; }
    public float RotationYaw { get; init; }
    public DateTime LastSeenUtc { get; init; }
    public int Coins { get; init; } = EconomyConfig.StartingCoins;
    public int VillageReputation { get; init; }
    public float Energy { get; init; } = 100f;
    public float Hunger { get; init; } = PlayerHungerConfig.DefaultHunger;
    public float Mood { get; init; } = PlayerNeedsConfig.DefaultMood;
    public float Fatigue { get; init; } = PlayerNeedsConfig.DefaultFatigue;
    public float SocialNeed { get; init; } = PlayerNeedsConfig.DefaultSocialNeed;
    public long NeedsLastGameMinute { get; init; }
    public int VillageContributionScore { get; init; }
    public VillageTitle VillageTitle { get; init; } = VillageTitle.Newcomer;
    public VillagePosition VillagePosition { get; init; } = VillagePosition.None;
    public DateTime? PositionAssignedAtUtc { get; init; }
}