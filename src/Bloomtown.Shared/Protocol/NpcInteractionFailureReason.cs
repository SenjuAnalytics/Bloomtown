namespace Bloomtown.Shared.Protocol;

public enum NpcInteractionFailureReason : byte
{
    None = 0,
    NoNpcNearby = 1,
    TooFar = 2,
    NotInAoi = 3,
    InvalidTarget = 4,
    UnknownInteraction = 5,
}