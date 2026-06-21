using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Protocol;

public readonly record struct VillageAreaRequest(
    VillageAreaRequestKind Kind,
    VillageAreaInteractionKind Interaction);