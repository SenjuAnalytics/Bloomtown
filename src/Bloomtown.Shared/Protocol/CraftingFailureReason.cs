namespace Bloomtown.Shared.Protocol;

public enum CraftingFailureReason : byte
{
    None = 0,
    UnknownRecipe = 1,
    InvalidQuantity = 2,
    PlayerUnavailable = 3,
    InsufficientMaterials = 4,
}