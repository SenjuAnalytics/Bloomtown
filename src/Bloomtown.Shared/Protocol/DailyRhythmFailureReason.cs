namespace Bloomtown.Shared.Protocol;

public enum DailyRhythmFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    EconomyUnavailable = 2,
    WrongPhase = 3,
    AlreadyUsed = 4,
}