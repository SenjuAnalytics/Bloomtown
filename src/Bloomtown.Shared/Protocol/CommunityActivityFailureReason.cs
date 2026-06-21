namespace Bloomtown.Shared.Protocol;

public enum CommunityActivityFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    UnknownActivity = 2,
    EconomyUnavailable = 3,
    OnCooldown = 4,
    NotUnlocked = 5,
    NotInRange = 6,
}