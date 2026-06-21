namespace Bloomtown.Shared.Protocol;

public readonly record struct DailyRhythmResponse(
    bool Success,
    DailyRhythmRequestKind Kind,
    DailyRhythmFailureReason FailureReason,
    string Message);