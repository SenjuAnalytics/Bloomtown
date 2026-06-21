namespace Bloomtown.Shared.Protocol;

public enum MilestoneFailureReason : byte
{
    None = 0,
    MilestoneLocked = 1,
    NotInRange = 2,
    OnCooldown = 3,
    AlreadyFull = 4,
    UnknownInteraction = 5,
    UnknownRequest = 6,
    EconomyUnavailable = 7,
}