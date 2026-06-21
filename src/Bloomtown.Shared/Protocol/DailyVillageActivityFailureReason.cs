namespace Bloomtown.Shared.Protocol;

public enum DailyVillageActivityFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    UnknownActivity = 2,
    EconomyUnavailable = 3,
    OnCooldown = 4,
    NotInRange = 5,
}