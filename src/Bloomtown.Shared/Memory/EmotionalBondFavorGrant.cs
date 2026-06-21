using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Memory;

/// <summary>Small item gift tied to a rare emotional-bond favor from a focus NPC.</summary>
public readonly record struct EmotionalBondFavorGrant(ItemType ItemType, int Quantity);