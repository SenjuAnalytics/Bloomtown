namespace Bloomtown.Shared.Protocol;

public enum EconomyFailureReason : byte
{
    None = 0,
    UnknownItem = 1,
    UnknownNpc = 2,
    NotInRange = 3,
    NotInAoi = 4,
    ItemNotSoldByNpc = 5,
    ItemNotBoughtByNpc = 6,
    NotEnoughCoins = 7,
    NotEnoughItems = 8,
    InvalidQuantity = 9,
    UnknownRequest = 10,
}