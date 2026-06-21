namespace Bloomtown.Shared.Protocol;

public enum VillageAreaFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    UnknownInteraction = 2,
    AreaLocked = 3,
    NotInRange = 4,
    OnCooldown = 5,
    EconomyUnavailable = 6,
}