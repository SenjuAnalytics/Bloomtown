using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct CommunityProjectRequest(
    CommunityProjectRequestKind Kind,
    byte ProjectId,
    ItemType ItemType,
    byte Quantity);