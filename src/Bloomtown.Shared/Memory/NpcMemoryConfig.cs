using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Thresholds, dialogue snippets, personal moments, and ambient-comment rules for NPC memories.
/// </summary>
public static class NpcMemoryConfig
{
    /// <summary>Village-wide memories use npc entity id 0.</summary>
    public const uint VillageWideNpcEntityId = 0;

    /// <summary>Gifts on the same game day before this memory is recorded.</summary>
    public const int FrequentGifterGiftThreshold = 3;

    /// <summary>Minimum relationship tier for NPC-initiated ambient comments.</summary>
    public const RelationshipTier MinAmbientCommentTier = RelationshipTier.Friend;

    /// <summary>Game minutes between ambient comments from the same NPC to the same player.</summary>
    public const int AmbientCommentCooldownGameMinutes = 45;

    /// <summary>Game minutes between rare personal moments from the same NPC.</summary>
    public const int PersonalMomentCooldownGameMinutes = 60;

    /// <summary>Low chance per check — personal moments should feel special.</summary>
    public const int PersonalMomentChancePercent = 12;

    /// <summary>How often the ambient comment system evaluates nearby players (real seconds).</summary>
    public const double AmbientCommentCheckIntervalSeconds = 8.0;

    private static readonly NpcMemoryType[] MemoryPriorityOrder =
    [
        NpcMemoryType.FirstPreferredGiftReceived,
        NpcMemoryType.FrequentGifter,
        NpcMemoryType.HelpedVillageProject,
    ];

    public static bool IsVillageWideMemory(NpcMemoryType type) =>
        type is NpcMemoryType.HelpedVillageProject or NpcMemoryType.VillageNoticedYourBonds;

    public static uint GetStorageNpcEntityId(uint npcEntityId, NpcMemoryType type) =>
        IsVillageWideMemory(type) ? VillageWideNpcEntityId : npcEntityId;

    /// <summary>
    /// Returns a full personalized talk/greet line when the bond is warm enough.
    /// Picks the highest-priority memory the NPC holds about this player.
    /// </summary>
    public static string? TryGetPersonalizedResponse(
        NpcInteractionKind kind,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (tier < RelationshipTier.Friend || memories.Count == 0)
            return null;

        if (kind is not (NpcInteractionKind.Talk or NpcInteractionKind.Greet))
            return null;

        foreach (var memoryType in MemoryPriorityOrder)
        {
            if (!memories.Contains(memoryType))
                continue;

            var lines = NpcPersonalBondDialogue.GetPersonalizedLines(kind, memoryType, tier);
            if (lines.Length == 0)
                continue;

            return lines[(int)(variationSeed % (uint)lines.Length)];
        }

        return null;
    }

    /// <summary>Memory-based additions for talk dialogue (lighter touch for Acquaintance tier).</summary>
    public static string? TryGetTalkMemoryLine(IReadOnlyCollection<NpcMemoryType> memories, uint variationSeed = 0)
    {
        return TryGetAcquaintanceMemoryLine(NpcInteractionKind.Talk, memories, variationSeed);
    }

    /// <summary>Memory-based additions for greet dialogue (lighter touch for Acquaintance tier).</summary>
    public static string? TryGetGreetMemoryLine(IReadOnlyCollection<NpcMemoryType> memories, uint variationSeed = 0)
    {
        return TryGetAcquaintanceMemoryLine(NpcInteractionKind.Greet, memories, variationSeed);
    }

    /// <summary>Extra warmth when receiving a gift if a related memory already exists.</summary>
    public static string? TryGetGiftMemoryLine(
        IReadOnlyCollection<NpcMemoryType> memories,
        bool isPreferred,
        RelationshipTier tier = RelationshipTier.Acquaintance,
        uint variationSeed = 0)
    {
        foreach (var memoryType in MemoryPriorityOrder)
        {
            if (!memories.Contains(memoryType))
                continue;

            var lines = NpcPersonalBondDialogue.GetGiftMemoryLines(memoryType, isPreferred, tier);
            if (lines.Length == 0)
                continue;

            return lines[(int)(variationSeed % (uint)lines.Length)];
        }

        return null;
    }

    /// <summary>Dialogue when a preferred gift memory is recorded for the first time.</summary>
    public static string GetFirstPreferredGiftRecordedLine()
    {
        return "I'll remember this — you really know how to make someone feel welcome.";
    }

    /// <summary>Picks an ambient comment based on memories and relationship tier.</summary>
    public static string? TryGetAmbientComment(
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        uint variationSeed)
    {
        if (tier < MinAmbientCommentTier)
            return null;

        string[]? candidates = null;

        if (memories.Contains(NpcMemoryType.FirstPreferredGiftReceived))
        {
            candidates =
            [
                "Hey — I was just thinking about that gift you gave me.",
                "Still smiling about that present you brought by.",
            ];
        }
        else if (memories.Contains(NpcMemoryType.FrequentGifter))
        {
            candidates =
            [
                "You always brighten my day when you wander past.",
                "It's nice seeing a familiar friendly face nearby.",
            ];
        }
        else if (memories.Contains(NpcMemoryType.HelpedVillageProject))
        {
            candidates =
            [
                "Thanks again for helping with the village project.",
                "Bloomtown feels a little stronger because of folks like you.",
            ];
        }
        else if (tier >= RelationshipTier.CloseFriend)
        {
            candidates =
            [
                "Good to see you out and about — stay a while if you can.",
                "The town feels warmer when you're around.",
            ];
        }
        else if (tier >= RelationshipTier.Friend)
        {
            candidates =
            [
                "Oh, hello there — lovely day for a stroll, isn't it?",
                "Nice to see you nearby. Take your time around town.",
            ];
        }

        if (candidates is null || candidates.Length == 0)
            return null;

        return candidates[(int)(variationSeed % (uint)candidates.Length)];
    }

    /// <summary>Rare personal moment — NPC recalls something specific about the player.</summary>
    public static string? TryGetPersonalMoment(
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        string npcName,
        uint variationSeed)
    {
        if (tier < RelationshipTier.Friend || memories.Count == 0)
            return null;

        foreach (var memoryType in MemoryPriorityOrder)
        {
            if (!memories.Contains(memoryType))
                continue;

            var lines = NpcPersonalBondDialogue.GetPersonalMomentLines(memoryType, tier, npcName);
            if (lines.Length == 0)
                continue;

            return lines[(int)(variationSeed % (uint)lines.Length)];
        }

        return null;
    }

    /// <summary>Deterministic low-probability roll for personal moments.</summary>
    public static bool ShouldTriggerPersonalMoment(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 37 + npcEntityId * 19 + (uint)(totalGameMinutes % 991)) % 100;
        return roll < PersonalMomentChancePercent;
    }

    /// <summary>Short bond hint for the status command when NPCs are nearby.</summary>
    public static string? FormatBondHint(IReadOnlyCollection<NpcMemoryType> memories, RelationshipTier tier)
    {
        if (tier < RelationshipTier.Friend || memories.Count == 0)
            return null;

        if (memories.Contains(NpcMemoryType.FirstPreferredGiftReceived))
            return "remembers your kindness";

        if (memories.Contains(NpcMemoryType.FrequentGifter))
            return "feels close to you";

        if (memories.Contains(NpcMemoryType.HelpedVillageProject))
            return "appreciates what you've done for Bloomtown";

        return "warm bond";
    }

    private static string? TryGetAcquaintanceMemoryLine(
        NpcInteractionKind kind,
        IReadOnlyCollection<NpcMemoryType> memories,
        uint variationSeed)
    {
        foreach (var memoryType in MemoryPriorityOrder)
        {
            if (!memories.Contains(memoryType))
                continue;

            var lines = NpcPersonalBondDialogue.GetAcquaintanceMemoryLines(kind, memoryType);
            if (lines.Length == 0)
                continue;

            return lines[(int)(variationSeed % (uint)lines.Length)];
        }

        return null;
    }
}