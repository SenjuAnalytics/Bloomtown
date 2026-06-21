namespace Bloomtown.Shared.Protocol;

public readonly record struct DailyVillageActivityResponse(
    bool Success,
    DailyVillageActivityRequestKind Kind,
    DailyVillageActivityFailureReason FailureReason,
    string Message);