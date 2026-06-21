namespace Bloomtown.Shared.Protocol;

public enum CommunityProjectFailureReason : byte
{
    None = 0,
    UnknownProject = 1,
    ProjectCompleted = 2,
    UnknownItem = 3,
    ItemNotNeeded = 4,
    NotEnoughItems = 5,
    InvalidQuantity = 6,
    UnknownRequest = 7,
    EconomyUnavailable = 8,
}