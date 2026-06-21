namespace Bloomtown.Shared.Village;

/// <summary>Outcome when a player successfully requests a social-standing favor from a focus NPC.</summary>
public enum SocialStandingFavorOutcomeKind : byte
{
    Info = 0,
    Item = 1,
    Recovery = 2,
}