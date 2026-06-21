namespace Bloomtown.Shared.Protocol;

public readonly record struct PersonalRoutineResponse(
    bool Success,
    PersonalRoutineRequestKind Kind,
    PersonalRoutineFailureReason FailureReason,
    string Message);