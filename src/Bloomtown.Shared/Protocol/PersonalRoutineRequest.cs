using Bloomtown.Shared.Routines;

namespace Bloomtown.Shared.Protocol;

public readonly record struct PersonalRoutineRequest(
    PersonalRoutineRequestKind Kind,
    PersonalRoutineKind Routine);