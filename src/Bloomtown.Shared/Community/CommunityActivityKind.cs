namespace Bloomtown.Shared.Community;

/// <summary>
/// Small community-help activities the player can perform near village gathering spots.
/// </summary>
public enum CommunityActivityKind : byte
{
    None = 0,
    HelpGarden = 1,
    HelpMarket = 2,
    HelpWell = 3,
    HelpLumber = 4,
    HelpInn = 5,
    HelpHerbGarden = 6,
    HelpSmithy = 7,
    HelpPatrol = 8,
    HelpVillage = 9,
    ListenToStories = 10,
    HelpWorkshop = 11,
    ChatWithEleanor = 12,
}