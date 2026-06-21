using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Memory;

/// <summary>
/// Player-initiated emotional bonding with Elsie, Harold, Mira, Tom, Greta, Nora, Elias, Ben, Lila, Rowan, Marcus, and Eleanor: action rules, cooldowns,
/// memory mapping, feedback, and archetype-linked warmth.
/// </summary>
public static class NpcEmotionalBondAgencyConfig
{
    /// <summary>Minimum gap between any two bonding actions with the same focus NPC.</summary>
    public const int GlobalBondingCooldownGameMinutes = 60;

    public const int CheckOnCooldownGameMinutes = 120;
    public const int SpendTimeCooldownGameMinutes = 135;
    public const int ShareMomentCooldownGameMinutes = 150;
    public const int HelpWithCooldownGameMinutes = 120;

    public const int CheckOnAffinityGain = 2;
    public const int SpendTimeAffinityGain = 3;
    public const int ShareMomentAffinityGain = 4;
    public const int HelpWithAffinityGain = 3;

    /// <summary>Chance the NPC adds a small remembrance line after a successful bonding action.</summary>
    public const int NpcRemembranceChancePercent = 35;

    public static bool IsValidTarget(uint npcEntityId) =>
        NpcEmotionalBondConfig.IsFocusNpc(npcEntityId);

    public static bool IsValidAction(EmotionalBondActionKind action) =>
        action is EmotionalBondActionKind.CheckOn
            or EmotionalBondActionKind.ShareMoment
            or EmotionalBondActionKind.HelpWith
            or EmotionalBondActionKind.SpendTime;

    public static RelationshipTier GetMinTier(EmotionalBondActionKind action) =>
        action switch
        {
            EmotionalBondActionKind.ShareMoment => RelationshipTier.Friend,
            _ => RelationshipTier.Acquaintance,
        };

    public static int GetAffinityGain(EmotionalBondActionKind action) =>
        action switch
        {
            EmotionalBondActionKind.CheckOn => CheckOnAffinityGain,
            EmotionalBondActionKind.SpendTime => SpendTimeAffinityGain,
            EmotionalBondActionKind.ShareMoment => ShareMomentAffinityGain,
            EmotionalBondActionKind.HelpWith => HelpWithAffinityGain,
            _ => 0,
        };

    public static int GetActionCooldownGameMinutes(EmotionalBondActionKind action) =>
        action switch
        {
            EmotionalBondActionKind.CheckOn => CheckOnCooldownGameMinutes,
            EmotionalBondActionKind.SpendTime => SpendTimeCooldownGameMinutes,
            EmotionalBondActionKind.ShareMoment => ShareMomentCooldownGameMinutes,
            EmotionalBondActionKind.HelpWith => HelpWithCooldownGameMinutes,
            _ => GlobalBondingCooldownGameMinutes,
        };

    public static NpcMemoryType? GetMemoryForAction(uint npcEntityId, EmotionalBondActionKind action) =>
        (npcEntityId, action) switch
        {
            (NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnElsie,
            (NpcEntityIds.Harold, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnHarold,
            (NpcEntityIds.Elsie, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithElsie,
            (NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithHarold,
            (NpcEntityIds.Elsie, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedElsie,
            (NpcEntityIds.Harold, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedHarold,
            (NpcEntityIds.Mira, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnMira,
            (NpcEntityIds.Mira, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithMira,
            (NpcEntityIds.Mira, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedMira,
            (NpcEntityIds.Tom, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnTom,
            (NpcEntityIds.Tom, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithTom,
            (NpcEntityIds.Tom, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedTom,
            (NpcEntityIds.Elsie, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithElsie,
            (NpcEntityIds.Harold, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithHarold,
            (NpcEntityIds.Mira, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithMira,
            (NpcEntityIds.Tom, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithTom,
            (NpcEntityIds.Greta, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnGreta,
            (NpcEntityIds.Greta, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithGreta,
            (NpcEntityIds.Greta, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedGreta,
            (NpcEntityIds.Greta, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithGreta,
            (NpcEntityIds.Nora, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnNora,
            (NpcEntityIds.Nora, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithNora,
            (NpcEntityIds.Nora, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedNora,
            (NpcEntityIds.Nora, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithNora,
            (NpcEntityIds.Elias, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnElias,
            (NpcEntityIds.Elias, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithElias,
            (NpcEntityIds.Elias, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedElias,
            (NpcEntityIds.Elias, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithElias,
            (NpcEntityIds.Ben, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnBen,
            (NpcEntityIds.Ben, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithBen,
            (NpcEntityIds.Ben, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedBen,
            (NpcEntityIds.Ben, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithBen,
            (NpcEntityIds.Lila, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnLila,
            (NpcEntityIds.Lila, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithLila,
            (NpcEntityIds.Lila, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedLila,
            (NpcEntityIds.Lila, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithLila,
            (NpcEntityIds.Rowan, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnRowan,
            (NpcEntityIds.Rowan, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithRowan,
            (NpcEntityIds.Rowan, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedRowan,
            (NpcEntityIds.Rowan, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithRowan,
            (NpcEntityIds.Marcus, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnMarcus,
            (NpcEntityIds.Marcus, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithMarcus,
            (NpcEntityIds.Marcus, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedMarcus,
            (NpcEntityIds.Marcus, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithMarcus,
            (NpcEntityIds.Eleanor, EmotionalBondActionKind.CheckOn) => NpcMemoryType.CheckedOnEleanor,
            (NpcEntityIds.Eleanor, EmotionalBondActionKind.ShareMoment) => NpcMemoryType.SharedMomentWithEleanor,
            (NpcEntityIds.Eleanor, EmotionalBondActionKind.HelpWith) => NpcMemoryType.ConsciouslyHelpedEleanor,
            (NpcEntityIds.Eleanor, EmotionalBondActionKind.SpendTime) => NpcMemoryType.SpentQuietTimeWithEleanor,
            _ => null,
        };

    /// <summary>Immediate NPC response to the player's conscious bonding action.</summary>
    public static string? TryGetBondingActionResponse(
        uint npcEntityId,
        EmotionalBondActionKind action,
        RelationshipTier tier,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsValidTarget(npcEntityId) || !IsValidAction(action))
            return null;

        var lines = NpcEmotionalBondAgencyDialogue.GetBondingActionLines(npcEntityId, action, tier, archetype);
        return PickLine(lines, variationSeed);
    }

    /// <summary>
    /// Rare follow-up where the NPC recalls something personal about the player — shows the bond is two-way.
    /// </summary>
    public static string? TryGetNpcRemembranceLine(
        uint npcEntityId,
        IReadOnlyCollection<NpcMemoryType> memories,
        RelationshipTier tier,
        LegacyArchetype archetype,
        uint variationSeed)
    {
        if (!IsValidTarget(npcEntityId) || tier < NpcEmotionalBondConfig.MinEmotionalInteractionTier)
            return null;

        var memoryType = NpcEmotionalBondConfig.GetActiveEmotionalMemory(npcEntityId, memories);
        if (memoryType is null)
            return null;

        var lines = NpcEmotionalBondAgencyDialogue.GetNpcRemembranceLines(
            npcEntityId,
            memoryType.Value,
            tier,
            archetype);

        return PickLine(lines, variationSeed);
    }

    public static bool ShouldTriggerNpcRemembrance(uint playerEntityId, uint npcEntityId, long totalGameMinutes)
    {
        var roll = (playerEntityId * 67 + npcEntityId * 47 + (uint)(totalGameMinutes % 907)) % 100;
        return roll < NpcRemembranceChancePercent;
    }

    /// <summary>Clear player-facing feedback when a bonding action deepens the relationship.</summary>
    public static string BuildBondDeepenedFeedback(
        string npcDisplayName,
        EmotionalBondActionKind action,
        bool recordedNewMemory,
        bool tierIncreased,
        RelationshipTier newTier)
    {
        var actionLabel = action switch
        {
            EmotionalBondActionKind.CheckOn => "checking on",
            EmotionalBondActionKind.SpendTime => "spending quiet time with",
            EmotionalBondActionKind.ShareMoment => "sharing a moment with",
            EmotionalBondActionKind.HelpWith => "helping",
            _ => "reaching out to",
        };

        var memoryNote = recordedNewMemory
            ? " They'll remember this."
            : " Your bond feels a little deeper.";

        var tierNote = tierIncreased
            ? $" You're now {RelationshipTierDisplay.GetName(newTier)} with {npcDisplayName}."
            : string.Empty;

        if (action == EmotionalBondActionKind.SpendTime)
        {
            return $"You spend a quiet moment with {npcDisplayName}. They seem to appreciate the company.{memoryNote}{tierNote}";
        }

        return $"You feel closer to {npcDisplayName} after {actionLabel} them.{memoryNote}{tierNote}";
    }

    /// <summary>Personal nuance from the player's legacy archetype after a conscious bonding action.</summary>
    public static string? TryGetArchetypeBondingHint(
        LegacyArchetype archetype,
        EmotionalBondActionKind action,
        uint variationSeed)
    {
        if (archetype == LegacyArchetype.None || !IsValidAction(action))
            return null;

        var lines = NpcEmotionalBondAgencyDialogue.GetArchetypeBondingHintLines(archetype, action);
        return PickLine(lines, variationSeed);
    }

    public static string FormatBondingCooldownHint(EmotionalBondActionKind action) =>
        $"Give it time — {GetActionLabel(action)} with the same person needs about {GetActionCooldownGameMinutes(action)} game minutes.";

    public static string GetActionLabel(EmotionalBondActionKind action) =>
        action switch
        {
            EmotionalBondActionKind.CheckOn => "checking on someone",
            EmotionalBondActionKind.SpendTime => "spending quiet time",
            EmotionalBondActionKind.ShareMoment => "sharing a moment",
            EmotionalBondActionKind.HelpWith => "offering hands-on help",
            _ => "bonding",
        };

    private static string? PickLine(string[] lines, uint variationSeed)
    {
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}