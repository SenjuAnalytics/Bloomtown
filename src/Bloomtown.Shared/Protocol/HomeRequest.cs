using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;

namespace Bloomtown.Shared.Protocol;

public readonly record struct HomeRequest(
    HomeRequestKind Kind,
    ItemType ItemType,
    byte Quantity,
    FurnitureType FurnitureType = 0,
    HomeActivityType ActivityType = 0);