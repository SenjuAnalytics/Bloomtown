using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Leadership;

namespace Bloomtown.Server.Networking;

public readonly record struct PlayerPersistenceSnapshot(
    uint EntityId,
    float PositionX,
    float PositionY,
    float PositionZ,
    float RotationYaw,
    int Coins,
    int VillageReputation,
    float Energy,
    float Hunger,
    float Mood,
    float Fatigue,
    float SocialNeed,
    long NeedsLastGameMinute,
    int VillageContributionScore,
    VillageTitle VillageTitle,
    VillagePosition VillagePosition,
    DateTime? PositionAssignedAtUtc,
    IReadOnlyList<ItemStack> InventoryStacks);