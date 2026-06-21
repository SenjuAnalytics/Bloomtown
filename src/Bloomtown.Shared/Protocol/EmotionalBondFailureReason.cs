namespace Bloomtown.Shared.Protocol;

public enum EmotionalBondFailureReason : byte
{
    None = 0,
    UnknownRequest = 1,
    InvalidTarget = 2,
    InvalidAction = 3,
    NotInRange = 4,
    OnCooldown = 5,
    RelationshipTooLow = 6,
    Unavailable = 7,
    StandingTooLow = 8,
}