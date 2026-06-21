namespace Bloomtown.Shared.Village;

/// <summary>How Bloomtown socially regards the player based on close bonds with focus NPCs.</summary>
public enum VillageSocialStandingTier : byte
{
    Stranger = 0,
    Known = 1,
    Respected = 2,
    WellLiked = 3,
}