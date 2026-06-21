namespace Bloomtown.Shared.Protocol;

public enum LegacyFocusFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    InvalidPath = 2,
    NotInRange = 3,
    OnCooldown = 4,
    Unavailable = 5,
}