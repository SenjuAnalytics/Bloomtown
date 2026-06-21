namespace Bloomtown.Shared.Protocol;

/// <summary>Player-initiated emotional bonding actions with focus NPCs (Elsie, Harold).</summary>
public enum EmotionalBondActionKind : byte
{
    None = 0,

    /// <summary>Consciously check on how the NPC is doing.</summary>
    CheckOn = 1,

    /// <summary>Share a quiet personal moment together.</summary>
    ShareMoment = 2,

    /// <summary>Offer small, hands-on help as a caring gesture.</summary>
    HelpWith = 3,

    /// <summary>Spend quiet, unhurried time together — personal presence, not productive work.</summary>
    SpendTime = 4,
}