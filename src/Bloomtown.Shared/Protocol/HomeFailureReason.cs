namespace Bloomtown.Shared.Protocol;

public enum HomeFailureReason : byte
{
    None = 0,
    UnknownItem = 1,
    NotAtHome = 2,
    NotEnoughItems = 3,
    InvalidQuantity = 4,
    UnknownRequest = 5,
    HomeUnavailable = 6,
    MaxTierReached = 7,
    InsufficientMaterials = 8,
    UnknownFurniture = 9,
    FurnitureAlreadyPlaced = 10,
    UnknownActivity = 11,
}