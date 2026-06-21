using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct GatheringRequest(ItemType ResourceType);