namespace Bloomtown.Shared.Protocol;

public enum DailyRhythmRequestKind : byte
{
    List = 0,
    StartCalm = 1,
    StartActive = 2,
    WindDown = 3,
    FocusedBreak = 4,
    RestEarly = 5,
    PushThrough = 6,
}