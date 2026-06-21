using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Protocol;

public readonly record struct DailyVillageActivityRequest(
    DailyVillageActivityRequestKind Kind,
    DailyVillageActivityKind Activity);