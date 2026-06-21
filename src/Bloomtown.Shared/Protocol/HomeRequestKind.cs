namespace Bloomtown.Shared.Protocol;

public enum HomeRequestKind : byte
{
    View = 1,
    Deposit = 2,
    Withdraw = 3,
    Upgrade = 4,
    PlaceFurniture = 5,
    Activity = 6,
}