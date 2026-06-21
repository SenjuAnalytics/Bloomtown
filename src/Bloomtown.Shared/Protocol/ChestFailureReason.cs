namespace Bloomtown.Shared.Protocol;

public enum ChestFailureReason : byte
{
    None = 0,
    UnknownItem = 1,
    NotInRange = 2,
    NotEnoughItems = 3,
    InvalidQuantity = 4,
    UnknownRequest = 5,
    ChestUnavailable = 6,
}