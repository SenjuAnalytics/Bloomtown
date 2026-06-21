using Bloomtown.Shared.Community;

namespace Bloomtown.Shared.Protocol;

public readonly record struct CommunityActivityRequest(
    CommunityActivityRequestKind Kind,
    CommunityActivityKind Activity);