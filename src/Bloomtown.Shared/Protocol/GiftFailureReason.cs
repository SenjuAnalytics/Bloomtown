namespace Bloomtown.Shared.Protocol;

public enum GiftFailureReason : byte
{
    None = 0,
    UnknownNpc = 1,
    NotInRange = 2,
    NotInAoi = 3,
    NotEnoughItems = 4,
    UnknownItem = 5,
    InvalidQuantity = 6,
    PlayerUnavailable = 7,
}