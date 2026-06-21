using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Thresholds, chance rolls, passive benefits, and formatting for village recognition of emotional bonds
/// with focus NPCs (Elsie, Harold, Mira, Tom, Greta). Keeps acknowledgment rare, warm, and lightly meaningful.
/// </summary>
public static class VillageBondRecognitionConfig
{
    /// <summary>Minimum focus close friends before village ambient recognition can appear.</summary>
    public const int MinCloseFriendsForAmbientRecognition = 1;

    /// <summary>Minimum focus close friends before cross-NPC bond awareness can surface.</summary>
    public const int MinCloseFriendsForCrossNpcRecognition = 2;

    /// <summary>Focus close friends needed before the village-wide bond memory is recorded.</summary>
    public const int MinCloseFriendsForVillageMemory = 2;

    /// <summary>Minimum focus close friends before passive village mood recovery applies.</summary>
    public const int MinCloseFriendsForPassiveBenefit = 2;

    /// <summary>Base ambient chance with one focus close friend.</summary>
    public const int AmbientRecognitionBaseChancePercent = 7;

    /// <summary>Extra ambient chance per additional focus close friend (single-bond tier).</summary>
    public const int AmbientRecognitionBonusChancePerFriendPercent = 5;

    /// <summary>Warmer ambient base chance when the player has multiple close focus bonds.</summary>
    public const int MultiBondAmbientBaseChancePercent = 10;

    /// <summary>Extra ambient chance per close friend when multiple bonds are established.</summary>
    public const int MultiBondAmbientBonusChancePerFriendPercent = 6;

    public const int MultiBondAmbientMaxChancePercent = 28;
    public const int SingleBondAmbientMaxChancePercent = 22;

    /// <summary>Chance during talk/greet when a focus NPC notices another close bond.</summary>
    public const int CrossNpcRecognitionChancePercent = 20;

    /// <summary>Rare village appreciation from a focus NPC when multiple bonds are visible.</summary>
    public const int VillageAppreciationChancePercent = 12;

    /// <summary>Chance a village appreciation moment also grants a small item.</summary>
    public const int VillageAppreciationItemChancePercent = 15;

    public const int AmbientRecognitionCooldownGameMinutes = 70;
    public const int MultiBondAmbientCooldownGameMinutes = 55;
    public const int CrossNpcRecognitionCooldownGameMinutes = 48;
    public const int VillageAppreciationCooldownGameMinutes = 90;

    /// <summary>Very light passive mood recovery per game minute while in the village.</summary>
    public const float PassiveMoodRecoveryPerGameMinute = 0.04f;

    /// <summary>Small extra recovery once the village has formally noticed the player's bonds.</summary>
    public const float PassiveMoodRecoveryNoticedBonusPerGameMinute = 0.01f;

    private static readonly uint[] FocusNpcIds =
    [
        NpcEntityIds.Elsie,
        NpcEntityIds.Harold,
        NpcEntityIds.Mira,
        NpcEntityIds.Tom,
        NpcEntityIds.Greta,
        NpcEntityIds.Nora,
        NpcEntityIds.Elias,
        NpcEntityIds.Ben,
        NpcEntityIds.Lila,
        NpcEntityIds.Rowan,
        NpcEntityIds.Marcus,
        NpcEntityIds.Eleanor,
    ];

    public static int CountFocusCloseFriends(Func<uint, RelationshipTier> getTier)
    {
        var count = 0;
        foreach (var npcId in FocusNpcIds)
        {
            if (getTier(npcId) >= RelationshipTier.CloseFriend)
                count++;
        }

        return count;
    }

    public static IReadOnlyList<uint> GetFocusCloseFriendNpcIds(Func<uint, RelationshipTier> getTier)
    {
        var results = new List<uint>();
        foreach (var npcId in FocusNpcIds)
        {
            if (getTier(npcId) >= RelationshipTier.CloseFriend)
                results.Add(npcId);
        }

        return results;
    }

    public static bool IsEligibleForAmbientRecognition(int focusCloseFriendCount) =>
        focusCloseFriendCount >= MinCloseFriendsForAmbientRecognition;

    public static bool IsEligibleForPassiveBenefit(int focusCloseFriendCount) =>
        focusCloseFriendCount >= MinCloseFriendsForPassiveBenefit;

    public static bool IsEligibleForCrossNpcRecognition(
        uint speakingNpcEntityId,
        RelationshipTier speakingNpcTier,
        int focusCloseFriendCount) =>
        NpcEmotionalBondConfig.IsFocusNpc(speakingNpcEntityId)
        && speakingNpcTier >= RelationshipTier.CloseFriend
        && focusCloseFriendCount >= MinCloseFriendsForCrossNpcRecognition;

    public static bool IsEligibleForVillageAppreciation(
        uint npcEntityId,
        RelationshipTier tier,
        int focusCloseFriendCount) =>
        NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
        && tier >= RelationshipTier.CloseFriend
        && focusCloseFriendCount >= MinCloseFriendsForCrossNpcRecognition;

    public static bool ShouldRecordVillageMemory(int focusCloseFriendCount) =>
        focusCloseFriendCount >= MinCloseFriendsForVillageMemory;

    public static int GetAmbientRecognitionChancePercent(int focusCloseFriendCount)
    {
        if (!IsEligibleForAmbientRecognition(focusCloseFriendCount))
            return 0;

        if (focusCloseFriendCount >= MinCloseFriendsForCrossNpcRecognition)
        {
            var chance = MultiBondAmbientBaseChancePercent
                + (focusCloseFriendCount - 1) * MultiBondAmbientBonusChancePerFriendPercent;
            return Math.Min(chance, MultiBondAmbientMaxChancePercent);
        }

        var singleChance = AmbientRecognitionBaseChancePercent
            + (focusCloseFriendCount - 1) * AmbientRecognitionBonusChancePerFriendPercent;
        return Math.Min(singleChance, SingleBondAmbientMaxChancePercent);
    }

    public static int GetAmbientRecognitionCooldownGameMinutes(int focusCloseFriendCount) =>
        focusCloseFriendCount >= MinCloseFriendsForCrossNpcRecognition
            ? MultiBondAmbientCooldownGameMinutes
            : AmbientRecognitionCooldownGameMinutes;

    public static bool ShouldTriggerAmbientRecognition(
        uint playerEntityId,
        int focusCloseFriendCount,
        long totalGameMinutes,
        uint attemptCounter)
    {
        if (!IsEligibleForAmbientRecognition(focusCloseFriendCount))
            return false;

        var chance = GetAmbientRecognitionChancePercent(focusCloseFriendCount);
        var roll = (playerEntityId * 97 + (uint)(totalGameMinutes % 919) + attemptCounter * 11) % 100;
        return roll < chance;
    }

    public static bool ShouldTriggerCrossNpcRecognition(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes)
    {
        var roll = (playerEntityId * 101 + npcEntityId * 37 + (uint)(totalGameMinutes % 907)) % 100;
        return roll < CrossNpcRecognitionChancePercent;
    }

    public static bool ShouldTriggerVillageAppreciation(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes)
    {
        var roll = (playerEntityId * 107 + npcEntityId * 43 + (uint)(totalGameMinutes % 893)) % 100;
        return roll < VillageAppreciationChancePercent;
    }

    /// <summary>Passive mood recovery rate while the player is in the village with multiple close bonds.</summary>
    public static float GetPassiveMoodRecoveryPerGameMinute(int focusCloseFriendCount, bool villageNoticedMemory)
    {
        if (!IsEligibleForPassiveBenefit(focusCloseFriendCount))
            return 0f;

        var rate = PassiveMoodRecoveryPerGameMinute;
        if (villageNoticedMemory)
            rate += PassiveMoodRecoveryNoticedBonusPerGameMinute;

        if (focusCloseFriendCount > MinCloseFriendsForPassiveBenefit)
            rate += Math.Min(focusCloseFriendCount - MinCloseFriendsForPassiveBenefit, 2) * 0.005f;

        return rate;
    }

    public static string? TryGetVillageAmbientComment(
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (focusCloseFriendNpcIds.Count == 0)
            return null;

        return VillageBondRecognitionDialogue.TryGetVillageAmbientComment(
            focusCloseFriendNpcIds,
            villageNoticedMemory,
            variationSeed);
    }

    public static string? TryGetCrossNpcRecognitionLine(
        uint speakingNpcEntityId,
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        uint variationSeed)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(speakingNpcEntityId))
            return null;

        var otherCloseFriends = focusCloseFriendNpcIds
            .Where(id => id != speakingNpcEntityId)
            .ToList();

        if (otherCloseFriends.Count == 0)
            return null;

        var otherNpcId = otherCloseFriends[(int)(variationSeed % (uint)otherCloseFriends.Count)];
        return VillageBondRecognitionDialogue.TryGetCrossNpcRecognitionLine(
            speakingNpcEntityId,
            otherNpcId,
            variationSeed);
    }

    public static string? TryGetVillageAppreciationLine(
        uint npcEntityId,
        int focusCloseFriendCount,
        bool villageNoticedMemory,
        uint variationSeed)
    {
        if (!IsEligibleForVillageAppreciation(npcEntityId, RelationshipTier.CloseFriend, focusCloseFriendCount))
            return null;

        return VillageBondRecognitionDialogue.TryGetVillageAppreciationLine(
            npcEntityId,
            focusCloseFriendCount,
            villageNoticedMemory,
            variationSeed);
    }

    public static EmotionalBondFavorGrant? TryGetVillageAppreciationItemGrant(
        uint npcEntityId,
        uint variationSeed)
    {
        var itemRoll = (variationSeed * 103 + npcEntityId * 47) % 100;
        if (itemRoll >= VillageAppreciationItemChancePercent)
            return null;

        return npcEntityId switch
        {
            NpcEntityIds.Elsie => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            NpcEntityIds.Harold => new EmotionalBondFavorGrant(ItemType.Wood, 1),
            NpcEntityIds.Mira => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            NpcEntityIds.Tom => new EmotionalBondFavorGrant(ItemType.Plank, 1),
            NpcEntityIds.Greta => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            NpcEntityIds.Nora => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            NpcEntityIds.Elias => new EmotionalBondFavorGrant(ItemType.Wood, 1),
            NpcEntityIds.Ben => new EmotionalBondFavorGrant(ItemType.Tool, 1),
            NpcEntityIds.Lila => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            NpcEntityIds.Rowan => new EmotionalBondFavorGrant(ItemType.Wood, 1),
            NpcEntityIds.Marcus => new EmotionalBondFavorGrant(ItemType.Plank, 1),
            NpcEntityIds.Eleanor => new EmotionalBondFavorGrant(ItemType.Apple, 1),
            _ => null,
        };
    }

    /// <summary>Status command summary of how the village sees the player's emotional bonds.</summary>
    public static string? FormatRecognitionStatus(
        IReadOnlyList<uint> focusCloseFriendNpcIds,
        bool villageNoticedMemory,
        bool passiveBenefitActive = false)
    {
        if (focusCloseFriendNpcIds.Count == 0)
            return null;

        var lines = new List<string>
        {
            VillageBondRecognitionDialogue.FormatRecognitionStatus(
                focusCloseFriendNpcIds,
                villageNoticedMemory)!,
        };

        if (passiveBenefitActive)
        {
            var passiveHint = VillageBondRecognitionDialogue.FormatPassiveBenefitStatusHint(
                focusCloseFriendNpcIds.Count,
                villageNoticedMemory);
            if (!string.IsNullOrWhiteSpace(passiveHint))
                lines.Add(passiveHint);
        }

        return string.Join(" ", lines);
    }

    /// <summary>Clear positive feedback when village ambient recognition fires.</summary>
    public static string FormatAmbientRecognitionFeedback(IReadOnlyList<uint> focusCloseFriendNpcIds) =>
        $"[The village has noticed {FormatBondNamesForFeedback(focusCloseFriendNpcIds)}.]";

    /// <summary>Clear positive feedback when a focus NPC references another close bond.</summary>
    public static string FormatCrossNpcRecognitionFeedback(string npcDisplayName, string recognitionLine) =>
        $"[{npcDisplayName} has noticed your other close bonds too:] {recognitionLine}";

    /// <summary>Feedback when a focus NPC offers village-wide appreciation.</summary>
    public static string FormatVillageAppreciationFeedback(string npcDisplayName, string appreciationLine) =>
        $"[Bloomtown's regard reaches you through {npcDisplayName}:] {appreciationLine}";

    /// <summary>Feedback when village appreciation includes a small item.</summary>
    public static string FormatVillageAppreciationItemFeedback(
        string npcDisplayName,
        EmotionalBondFavorGrant grant)
    {
        var itemLabel = grant.Quantity == 1
            ? ItemDatabase.GetDisplayName(grant.ItemType)
            : $"{grant.Quantity} {ItemDatabase.GetDisplayName(grant.ItemType)}";

        return $"[{npcDisplayName} offers {itemLabel} — a small token of how Bloomtown values you.]";
    }

    private static string FormatBondNamesForFeedback(IReadOnlyList<uint> focusCloseFriendNpcIds) =>
        focusCloseFriendNpcIds.Count switch
        {
            1 => $"how close you've become with {NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0])}",
            2 =>
                $"your close bond with {NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[0])} and {NpcNameLookup.GetDisplayNameOrDefault(focusCloseFriendNpcIds[1])}",
            _ => "your close friendships around Bloomtown",
        };
}