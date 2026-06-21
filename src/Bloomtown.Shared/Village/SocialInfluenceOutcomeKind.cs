namespace Bloomtown.Shared.Village;

/// <summary>Outcome when a Well-liked player actively calls on a focus NPC for social influence.</summary>
public enum SocialInfluenceOutcomeKind : byte
{
    Info = 0,

    /// <summary>Harold grants backing — next communal project contribution counts +1 extra progress.</summary>
    ProjectBacking = 1,

    /// <summary>Greta offers stronger social recovery from the inn.</summary>
    Recovery = 2,

    /// <summary>Small gift from the NPC (Greta, Mira, or Elsie).</summary>
    Item = 3,

    /// <summary>Mira grants a one-time extra trade discount or sell bonus.</summary>
    TradePrivilege = 4,

    /// <summary>Elsie grants backing — next food/garden project contribution counts +1 extra progress.</summary>
    GardenBacking = 5,

    /// <summary>Tom grants backing — next wood project contribution counts +1 extra progress.</summary>
    LumberBacking = 6,

    /// <summary>Nora grants backing — next herb/plant project contribution counts +1 extra progress.</summary>
    HerbalBacking = 7,

    /// <summary>Elias grants backing — next smithing-related contribution or smithy help gets extra benefit.</summary>
    SmithingBacking = 8,

    /// <summary>Ben grants backing — next security-related contribution or patrol help gets extra benefit.</summary>
    GuardBacking = 9,

    /// <summary>Lila grants backing — next warehouse/apple contribution or village help gets extra benefit.</summary>
    YouthBacking = 10,

    /// <summary>Rowan grants backing — next warehouse/apple contribution or story listening gets extra benefit.</summary>
    StoryBacking = 11,

    /// <summary>Marcus grants backing — next crafting-related contribution or workshop help gets extra benefit.</summary>
    CraftingBacking = 12,

    /// <summary>Eleanor grants backing — next warehouse/apple contribution or porch chat gets extra benefit.</summary>
    LegacyBacking = 13,
}