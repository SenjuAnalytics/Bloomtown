namespace Bloomtown.Shared.Memory;

/// <summary>
/// Simple memory kinds NPCs can retain about a player in this spike.
/// </summary>
public enum NpcMemoryType : byte
{
    /// <summary>Player gave this NPC a preferred gift for the first time.</summary>
    FirstPreferredGiftReceived = 1,

    /// <summary>Player contributed to a village project (stored with village-wide npc id 0).</summary>
    HelpedVillageProject = 2,

    /// <summary>Player gifted this NPC several times in a short span.</summary>
    FrequentGifter = 3,

    /// <summary>Player has talked with Elsie often enough to feel like a familiar companion.</summary>
    FrequentElsieCompanion = 4,

    /// <summary>Player has helped at the Community Garden often — Elsie remembers.</summary>
    HelpedGardenOften = 5,

    /// <summary>Player has talked with Harold often enough to feel like a familiar companion.</summary>
    FrequentHaroldCompanion = 6,

    /// <summary>Player has helped at the Village Well often — Harold remembers.</summary>
    HelpedWellOften = 7,

    /// <summary>Player consciously checked on Elsie — she remembers the care.</summary>
    CheckedOnElsie = 8,

    /// <summary>Player consciously checked on Harold — he remembers the care.</summary>
    CheckedOnHarold = 9,

    /// <summary>Player shared a quiet personal moment with Elsie.</summary>
    SharedMomentWithElsie = 10,

    /// <summary>Player shared a quiet personal moment with Harold.</summary>
    SharedMomentWithHarold = 11,

    /// <summary>Player offered hands-on help to Elsie beyond routine chores.</summary>
    ConsciouslyHelpedElsie = 12,

    /// <summary>Player offered hands-on help to Harold beyond routine chores.</summary>
    ConsciouslyHelpedHarold = 13,

    /// <summary>Player has talked with Mira often enough to feel like a familiar face at the square.</summary>
    FrequentMiraCompanion = 14,

    /// <summary>Player has helped at Market Square often — Mira remembers.</summary>
    HelpedMarketOften = 15,

    /// <summary>Player consciously checked on Mira — she remembers the care.</summary>
    CheckedOnMira = 16,

    /// <summary>Player shared a quiet personal moment with Mira.</summary>
    SharedMomentWithMira = 17,

    /// <summary>Player offered hands-on help to Mira beyond routine market chores.</summary>
    ConsciouslyHelpedMira = 18,

    /// <summary>Player has talked with Tom often enough to feel like a familiar face at the lumber yard.</summary>
    FrequentTomCompanion = 19,

    /// <summary>Player has helped at the lumber yard often — Tom remembers.</summary>
    HelpedLumberOften = 20,

    /// <summary>Player consciously checked on Tom — he remembers the care.</summary>
    CheckedOnTom = 21,

    /// <summary>Player shared a quiet personal moment with Tom.</summary>
    SharedMomentWithTom = 22,

    /// <summary>Player offered hands-on help to Tom beyond routine chores.</summary>
    ConsciouslyHelpedTom = 23,

    /// <summary>Player gave Elsie a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToElsie = 24,

    /// <summary>Player gave Harold a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToHarold = 25,

    /// <summary>Player gave Mira a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToMira = 26,

    /// <summary>Player gave Tom a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToTom = 27,

    /// <summary>Player spent quiet, unhurried time with Elsie.</summary>
    SpentQuietTimeWithElsie = 28,

    /// <summary>Player spent quiet, unhurried time with Harold.</summary>
    SpentQuietTimeWithHarold = 29,

    /// <summary>Player spent quiet, unhurried time with Mira.</summary>
    SpentQuietTimeWithMira = 30,

    /// <summary>Player spent quiet, unhurried time with Tom.</summary>
    SpentQuietTimeWithTom = 31,

    /// <summary>Village has noticed the player's close bonds with multiple focus NPCs (village-wide).</summary>
    VillageNoticedYourBonds = 32,

    /// <summary>Player has talked with Greta often enough to feel like a familiar guest at the inn.</summary>
    FrequentGretaCompanion = 33,

    /// <summary>Player has helped at the village inn often — Greta remembers.</summary>
    HelpedInnOften = 34,

    /// <summary>Player consciously checked on Greta — she remembers the care.</summary>
    CheckedOnGreta = 35,

    /// <summary>Player shared a quiet personal moment with Greta.</summary>
    SharedMomentWithGreta = 36,

    /// <summary>Player offered hands-on help to Greta beyond routine inn chores.</summary>
    ConsciouslyHelpedGreta = 37,

    /// <summary>Player gave Greta a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToGreta = 38,

    /// <summary>Player spent quiet, unhurried time with Greta at the inn.</summary>
    SpentQuietTimeWithGreta = 39,

    /// <summary>Player has talked with Nora often enough to feel like a familiar presence at the herb garden.</summary>
    FrequentNoraCompanion = 40,

    /// <summary>Player has helped at the herb garden often — Nora remembers.</summary>
    HelpedHerbGardenOften = 41,

    /// <summary>Player consciously checked on Nora — she remembers the care.</summary>
    CheckedOnNora = 42,

    /// <summary>Player shared a quiet personal moment with Nora.</summary>
    SharedMomentWithNora = 43,

    /// <summary>Player offered hands-on help to Nora beyond routine herb chores.</summary>
    ConsciouslyHelpedNora = 44,

    /// <summary>Player gave Nora a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToNora = 45,

    /// <summary>Player spent quiet, unhurried time with Nora among the herbs.</summary>
    SpentQuietTimeWithNora = 46,

    /// <summary>Player has talked with Elias often enough to feel like a familiar presence at the smithy.</summary>
    FrequentEliasCompanion = 47,

    /// <summary>Player has helped at the smithy often — Elias remembers.</summary>
    HelpedSmithyOften = 48,

    /// <summary>Player consciously checked on Elias — he remembers the care.</summary>
    CheckedOnElias = 49,

    /// <summary>Player shared a quiet personal moment with Elias.</summary>
    SharedMomentWithElias = 50,

    /// <summary>Player offered hands-on help to Elias beyond routine forge chores.</summary>
    ConsciouslyHelpedElias = 51,

    /// <summary>Player gave Elias a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToElias = 52,

    /// <summary>Player spent quiet, unhurried time with Elias at the forge.</summary>
    SpentQuietTimeWithElias = 53,

    /// <summary>Player has talked with Ben often enough to feel like a familiar presence on patrol.</summary>
    FrequentBenCompanion = 54,

    /// <summary>Player has helped on village patrol often — Ben remembers.</summary>
    HelpedPatrolOften = 55,

    /// <summary>Player consciously checked on Ben — he remembers the care.</summary>
    CheckedOnBen = 56,

    /// <summary>Player shared a quiet personal moment with Ben.</summary>
    SharedMomentWithBen = 57,

    /// <summary>Player offered hands-on help to Ben beyond routine guard chores.</summary>
    ConsciouslyHelpedBen = 58,

    /// <summary>Player gave Ben a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToBen = 59,

    /// <summary>Player spent quiet, unhurried time with Ben at the guard post.</summary>
    SpentQuietTimeWithBen = 60,

    /// <summary>Player has talked with Lila often enough to feel like a familiar young face around the village.</summary>
    FrequentLilaCompanion = 61,

    /// <summary>Player has helped around the village often — Lila remembers.</summary>
    HelpedVillageOften = 62,

    /// <summary>Player consciously checked on Lila — she remembers the care.</summary>
    CheckedOnLila = 63,

    /// <summary>Player shared a quiet personal moment with Lila.</summary>
    SharedMomentWithLila = 64,

    /// <summary>Player offered hands-on help to Lila beyond routine chores.</summary>
    ConsciouslyHelpedLila = 65,

    /// <summary>Player gave Lila a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToLila = 66,

    /// <summary>Player spent quiet, unhurried time with Lila around the village.</summary>
    SpentQuietTimeWithLila = 67,

    /// <summary>Player has talked with Rowan often enough to feel like a familiar listener at the story bench.</summary>
    FrequentRowanCompanion = 68,

    /// <summary>Player has listened to village stories often — Rowan remembers.</summary>
    ListenedToStoriesOften = 69,

    /// <summary>Player consciously checked on Rowan — he remembers the care.</summary>
    CheckedOnRowan = 70,

    /// <summary>Player shared a quiet personal moment with Rowan.</summary>
    SharedMomentWithRowan = 71,

    /// <summary>Player offered hands-on help to Rowan beyond routine chores.</summary>
    ConsciouslyHelpedRowan = 72,

    /// <summary>Player gave Rowan a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToRowan = 73,

    /// <summary>Player spent quiet, unhurried time with Rowan at the story bench.</summary>
    SpentQuietTimeWithRowan = 74,

    /// <summary>Player has talked with Marcus often enough to feel like a familiar face at the workshop.</summary>
    FrequentMarcusCompanion = 75,

    /// <summary>Player has helped at the workshop often — Marcus remembers.</summary>
    HelpedWorkshopOften = 76,

    /// <summary>Player consciously checked on Marcus — he remembers the care.</summary>
    CheckedOnMarcus = 77,

    /// <summary>Player shared a quiet personal moment with Marcus.</summary>
    SharedMomentWithMarcus = 78,

    /// <summary>Player offered hands-on help to Marcus beyond routine chores.</summary>
    ConsciouslyHelpedMarcus = 79,

    /// <summary>Player gave Marcus a favorite gift — he remembers the personal gesture.</summary>
    GaveFavoriteGiftToMarcus = 80,

    /// <summary>Player spent quiet, unhurried time with Marcus at the workshop.</summary>
    SpentQuietTimeWithMarcus = 81,

    /// <summary>Player has talked with Eleanor often enough to feel like a familiar face on her porch.</summary>
    FrequentEleanorCompanion = 82,

    /// <summary>Player has listened to Eleanor's old village stories often — she remembers.</summary>
    ListenedToEleanorStories = 83,

    /// <summary>Player consciously checked on Eleanor — she remembers the care.</summary>
    CheckedOnEleanor = 84,

    /// <summary>Player shared a quiet personal moment with Eleanor.</summary>
    SharedMomentWithEleanor = 85,

    /// <summary>Player offered hands-on help to Eleanor beyond routine chores.</summary>
    ConsciouslyHelpedEleanor = 86,

    /// <summary>Player gave Eleanor a favorite gift — she remembers the personal gesture.</summary>
    GaveFavoriteGiftToEleanor = 87,

    /// <summary>Player spent quiet, unhurried time with Eleanor on her porch.</summary>
    SpentQuietTimeWithEleanor = 88,
}