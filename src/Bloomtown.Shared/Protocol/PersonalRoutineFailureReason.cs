namespace Bloomtown.Shared.Protocol;

public enum PersonalRoutineFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    UnknownRoutine = 2,
    EconomyUnavailable = 3,
    OnCooldown = 4,
}