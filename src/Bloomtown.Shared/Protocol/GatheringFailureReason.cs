namespace Bloomtown.Shared.Protocol;

public enum GatheringFailureReason : byte
{
    None = 0,
    UnknownResource = 1,
    NoNodeNearby = 2,
    NotInRange = 3,
    NotInAoi = 4,
    OnCooldown = 5,
    AlreadyGathering = 6,
    EconomyUnavailable = 7,
}