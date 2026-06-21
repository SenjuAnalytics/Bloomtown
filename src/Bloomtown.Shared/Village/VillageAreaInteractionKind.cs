namespace Bloomtown.Shared.Village;

/// <summary>
/// Interactions available at unlocked village areas.
/// </summary>
public enum VillageAreaInteractionKind : byte
{
    None = 0,
    BrowseMarket = 1,
    ChatLocals = 2,
    RelaxGarden = 3,
    TendPlants = 4,
    StrollRiver = 5,
    ReflectRiver = 6,
}