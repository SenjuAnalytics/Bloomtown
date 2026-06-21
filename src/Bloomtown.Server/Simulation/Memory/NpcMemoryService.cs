using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Memory;

/// <summary>
/// Loads, records, and queries simple player–NPC memories; evaluates memory triggers.
/// </summary>
public sealed class NpcMemoryService
{
    private readonly PlayerNpcMemoryRepository _repository;
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<uint, HashSet<(uint NpcEntityId, NpcMemoryType Type)>> _cache = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), (int GameDay, int GiftCount)> _giftCountsByDay = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastAmbientCommentGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastPersonalMomentGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalMomentGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalArchetypeBondGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalAmbientGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), int> _focusNpcInteractionCounts = new();
    private readonly Dictionary<(uint PlayerEntityId, CommunityActivityKind), int> _focusAreaHelpCounts = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastGlobalBondingGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId, EmotionalBondActionKind Action), long> _lastBondingActionGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalBondInfoGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalBondFavorGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastEmotionalBondAppreciationGameMinute = new();
    private readonly Dictionary<uint, long> _lastVillageBondRecognitionAmbientGameMinute = new();
    private readonly Dictionary<uint, long> _lastVillageSocialStandingAmbientGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastCrossNpcBondRecognitionGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastVillageBondAppreciationGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastWellLikedStandingPrivilegeGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastVillageSocialStandingAwarenessGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastSocialLegacyNpcMentionGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastSocialStandingFavorGameMinute = new();
    private readonly Dictionary<(uint PlayerEntityId, uint NpcEntityId), long> _lastSocialInfluenceGameMinute = new();
    private readonly Dictionary<uint, int> _haroldSocialInfluenceProjectBacking = new();
    private readonly Dictionary<uint, int> _elsieSocialInfluenceGardenBacking = new();
    private readonly Dictionary<uint, int> _tomSocialInfluenceLumberBacking = new();
    private readonly Dictionary<uint, int> _noraSocialInfluenceHerbalBacking = new();
    private readonly Dictionary<uint, int> _eliasSocialInfluenceSmithingBacking = new();
    private readonly Dictionary<uint, int> _benSocialInfluenceGuardBacking = new();
    private readonly Dictionary<uint, int> _lilaSocialInfluenceYouthBacking = new();
    private readonly Dictionary<uint, int> _rowanSocialInfluenceStoryBacking = new();
    private readonly Dictionary<uint, int> _marcusSocialInfluenceCraftingBacking = new();
    private readonly Dictionary<uint, int> _eleanorSocialInfluenceLegacyBacking = new();
    private readonly Dictionary<uint, (bool BuyPrivilege, int RemainingUses)> _miraSocialInfluenceTradePrivilege = new();

    public NpcMemoryService(PlayerNpcMemoryRepository repository, WorldTimeSystem worldTime)
    {
        _repository = repository;
        _worldTime = worldTime;
    }

    public async Task LoadPlayerAsync(uint playerEntityId)
    {
        var records = await _repository.GetByPlayerAsync(playerEntityId);
        var memorySet = new HashSet<(uint, NpcMemoryType)>();

        foreach (var record in records)
            memorySet.Add((record.NpcEntityId, record.MemoryType));

        _cache[playerEntityId] = memorySet;

        if (records.Count == 0)
        {
            Log.Information("Loaded memories for player {PlayerId}: none recorded yet.", playerEntityId);
            return;
        }

        foreach (var record in records)
        {
            var npcLabel = record.NpcEntityId == NpcMemoryConfig.VillageWideNpcEntityId
                ? "village"
                : NpcNameLookup.GetDisplayNameOrDefault(record.NpcEntityId);
            Log.Information(
                "Loaded memory for player {PlayerId} with {NpcLabel}: {MemoryType} at {OccurredAt:u}",
                playerEntityId,
                npcLabel,
                record.MemoryType,
                record.OccurredAtUtc);
        }
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);

        var giftKeys = _giftCountsByDay.Keys.Where(key => key.PlayerEntityId == playerEntityId).ToList();
        foreach (var key in giftKeys)
            _giftCountsByDay.Remove(key);

        var ambientKeys = _lastAmbientCommentGameMinute.Keys.Where(key => key.PlayerEntityId == playerEntityId).ToList();
        foreach (var key in ambientKeys)
            _lastAmbientCommentGameMinute.Remove(key);

        var momentKeys = _lastPersonalMomentGameMinute.Keys.Where(key => key.PlayerEntityId == playerEntityId).ToList();
        foreach (var key in momentKeys)
            _lastPersonalMomentGameMinute.Remove(key);

        ClearPlayerKeyedDictionary(_lastEmotionalMomentGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_lastEmotionalArchetypeBondGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_lastEmotionalAmbientGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_focusNpcInteractionCounts, playerEntityId);
        ClearPlayerAreaHelpCounts(playerEntityId);
        ClearPlayerKeyedDictionary(_lastGlobalBondingGameMinute, playerEntityId);
        ClearPlayerBondingActionCooldowns(playerEntityId);
        ClearPlayerKeyedDictionary(_lastEmotionalBondInfoGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_lastEmotionalBondFavorGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_lastEmotionalBondAppreciationGameMinute, playerEntityId);
        _lastVillageBondRecognitionAmbientGameMinute.Remove(playerEntityId);
        ClearPlayerKeyedDictionary(_lastCrossNpcBondRecognitionGameMinute, playerEntityId);
        ClearPlayerKeyedDictionary(_lastVillageBondAppreciationGameMinute, playerEntityId);
    }

    public bool HasMemory(uint playerEntityId, uint npcEntityId, NpcMemoryType memoryType)
    {
        if (!_cache.TryGetValue(playerEntityId, out var memorySet))
            return false;

        var storageNpcId = NpcMemoryConfig.GetStorageNpcEntityId(npcEntityId, memoryType);
        return memorySet.Contains((storageNpcId, memoryType));
    }

    public IReadOnlyList<NpcMemoryType> GetMemoriesForNpc(uint playerEntityId, uint npcEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var memorySet))
            return Array.Empty<NpcMemoryType>();

        var results = new List<NpcMemoryType>();
        foreach (var (storedNpcId, memoryType) in memorySet)
        {
            if (storedNpcId == npcEntityId || storedNpcId == NpcMemoryConfig.VillageWideNpcEntityId)
                results.Add(memoryType);
        }

        return results;
    }

    /// <summary>
    /// Records a memory if the player does not already have it. Returns true when newly stored.
    /// </summary>
    public async Task<bool> TryRecordMemoryAsync(uint playerEntityId, uint npcEntityId, NpcMemoryType memoryType)
    {
        var storageNpcId = NpcMemoryConfig.GetStorageNpcEntityId(npcEntityId, memoryType);
        if (HasMemory(playerEntityId, storageNpcId, memoryType))
            return false;

        if (!_cache.TryGetValue(playerEntityId, out var memorySet))
        {
            memorySet = new HashSet<(uint, NpcMemoryType)>();
            _cache[playerEntityId] = memorySet;
        }

        memorySet.Add((storageNpcId, memoryType));
        var occurredAt = DateTime.UtcNow;

        await _repository.InsertAsync(new PlayerNpcMemoryRecord
        {
            PlayerEntityId = playerEntityId,
            NpcEntityId = storageNpcId,
            MemoryType = memoryType,
            OccurredAtUtc = occurredAt,
        });

        var npcLabel = storageNpcId == NpcMemoryConfig.VillageWideNpcEntityId
            ? "village"
            : NpcNameLookup.GetDisplayNameOrDefault(storageNpcId);

        Log.Information(
            "Recorded new memory for player {PlayerId} with {NpcLabel}: {MemoryType} at {OccurredAt:u}",
            playerEntityId,
            npcLabel,
            memoryType,
            occurredAt);

        return true;
    }

    /// <summary>
    /// Trigger: favorite gift bonding memory for focus NPCs, first preferred gift for others,
    /// and frequent gifter after enough gifts on the same game day.
    /// </summary>
    public async Task<NpcMemoryType?> OnGiftGivenAsync(uint playerEntityId, uint npcEntityId, bool isPreferred)
    {
        NpcMemoryType? newlyRecorded = null;

        if (isPreferred
            && NpcEmotionalBondConfig.IsFocusNpc(npcEntityId)
            && NpcEmotionalBondGiftConfig.GetFavoriteGiftMemoryForNpc(npcEntityId) is { } bondGiftMemory
            && await TryRecordMemoryAsync(playerEntityId, npcEntityId, bondGiftMemory))
        {
            newlyRecorded = bondGiftMemory;
            Log.Information(
                "Player {PlayerId} recorded emotional-bond favorite gift memory {Memory} with focus NPC {NpcId}.",
                playerEntityId,
                bondGiftMemory,
                npcEntityId);
        }
        else if (isPreferred
            && await TryRecordMemoryAsync(playerEntityId, npcEntityId, NpcMemoryType.FirstPreferredGiftReceived))
        {
            newlyRecorded = NpcMemoryType.FirstPreferredGiftReceived;
        }

        var key = (playerEntityId, npcEntityId);
        var gameDay = _worldTime.GameDay;
        if (_giftCountsByDay.TryGetValue(key, out var tracker) && tracker.GameDay == gameDay)
            _giftCountsByDay[key] = (gameDay, tracker.GiftCount + 1);
        else
            _giftCountsByDay[key] = (gameDay, 1);

        var giftCount = _giftCountsByDay[key].GiftCount;
        if (giftCount >= NpcMemoryConfig.FrequentGifterGiftThreshold
            && await TryRecordMemoryAsync(playerEntityId, npcEntityId, NpcMemoryType.FrequentGifter))
        {
            newlyRecorded = NpcMemoryType.FrequentGifter;
        }

        return newlyRecorded;
    }

    /// <summary>
    /// Trigger: first village project contribution (village-wide memory).
    /// </summary>
    public Task<bool> OnVillageProjectContributedAsync(uint playerEntityId)
    {
        return TryRecordMemoryAsync(
            playerEntityId,
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryType.HelpedVillageProject);
    }

    /// <summary>
    /// Trigger: player has multiple close focus-NPC bonds — village-wide recognition memory.
    /// </summary>
    public Task<bool> TryRecordVillageNoticedBondsIfEligibleAsync(uint playerEntityId, int focusCloseFriendCount)
    {
        if (!VillageBondRecognitionConfig.ShouldRecordVillageMemory(focusCloseFriendCount))
            return Task.FromResult(false);

        return TryRecordMemoryAsync(
            playerEntityId,
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryType.VillageNoticedYourBonds);
    }

    public bool HasVillageNoticedBondsMemory(uint playerEntityId) =>
        HasMemory(
            playerEntityId,
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryType.VillageNoticedYourBonds);

    public bool TryConsumeAmbientCommentCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastAmbientCommentGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcMemoryConfig.AmbientCommentCooldownGameMinutes)
        {
            return false;
        }

        _lastAmbientCommentGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>
    /// Records focus-NPC interactions — after enough visits, Elsie, Harold, or Mira remembers the player as a companion.
    /// </summary>
    public async Task<NpcMemoryType?> OnFocusNpcInteractionAsync(uint playerEntityId, uint npcEntityId)
    {
        if (!NpcEmotionalBondConfig.IsFocusNpc(npcEntityId))
            return null;

        var companionMemory = NpcEmotionalBondConfig.GetCompanionMemoryForNpc(npcEntityId);
        if (companionMemory is null)
            return null;

        var key = (playerEntityId, npcEntityId);
        var count = _focusNpcInteractionCounts.TryGetValue(key, out var current) ? current + 1 : 1;
        _focusNpcInteractionCounts[key] = count;

        if (count < NpcEmotionalBondConfig.FrequentCompanionInteractionThreshold)
            return null;

        if (await TryRecordMemoryAsync(playerEntityId, npcEntityId, companionMemory.Value))
            return companionMemory;

        return null;
    }

    /// <summary>
    /// Records garden/well/market/lumber community help — focus NPC remembers steady area-specific help.
    /// </summary>
    public async Task<NpcMemoryType?> OnFocusAreaCommunityHelpAsync(
        uint playerEntityId,
        CommunityActivityKind activity)
    {
        var areaMemory = NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(activity);
        var focusNpc = NpcEmotionalBondConfig.GetFocusNpcForActivity(activity);
        if (areaMemory is null || focusNpc == 0)
            return null;

        var key = (playerEntityId, activity);
        var count = _focusAreaHelpCounts.TryGetValue(key, out var current) ? current + 1 : 1;
        _focusAreaHelpCounts[key] = count;

        if (count < NpcEmotionalBondConfig.FrequentAreaHelpThreshold)
            return null;

        if (await TryRecordMemoryAsync(playerEntityId, focusNpc, areaMemory.Value))
            return areaMemory;

        return null;
    }

    /// <summary>Separate, longer cooldown so emotional personal moments stay rare.</summary>
    public bool TryConsumeEmotionalMomentCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalMomentGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondConfig.EmotionalMomentCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalMomentGameMinute[key] = currentMinute;
        return true;
    }

    public bool TryConsumeEmotionalArchetypeBondCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalArchetypeBondGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondConfig.EmotionalArchetypeBondCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalArchetypeBondGameMinute[key] = currentMinute;
        return true;
    }

    public bool TryConsumeEmotionalAmbientCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalAmbientGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondConfig.EmotionalAmbientCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalAmbientGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Global cooldown between any bonding actions with the same focus NPC.</summary>
    public bool TryConsumeGlobalBondingCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastGlobalBondingGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondAgencyConfig.GlobalBondingCooldownGameMinutes)
        {
            return false;
        }

        _lastGlobalBondingGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Per-action cooldown so conscious bonding stays rare and meaningful.</summary>
    public bool TryConsumeBondingActionCooldown(
        uint playerEntityId,
        uint npcEntityId,
        EmotionalBondActionKind action)
    {
        var key = (playerEntityId, npcEntityId, action);
        var currentMinute = _worldTime.TotalGameMinutes;
        var cooldown = NpcEmotionalBondAgencyConfig.GetActionCooldownGameMinutes(action);

        if (_lastBondingActionGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < cooldown)
        {
            return false;
        }

        _lastBondingActionGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare emotional-bond village tips from Elsie or Harold.</summary>
    public bool TryConsumeEmotionalBondInfoCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalBondInfoGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondImpactConfig.EmotionalBondInfoCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalBondInfoGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare helpful favors when the emotional bond is close.</summary>
    public bool TryConsumeEmotionalBondFavorCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalBondFavorGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondImpactConfig.EmotionalBondFavorCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalBondFavorGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for personal appreciation lines that show the NPC values the player's presence.</summary>
    public bool TryConsumeEmotionalBondAppreciationCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastEmotionalBondAppreciationGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcEmotionalBondImpactConfig.EmotionalBondAppreciationCooldownGameMinutes)
        {
            return false;
        }

        _lastEmotionalBondAppreciationGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare villager ambient comments tied to social standing from close focus bonds.</summary>
    public bool TryConsumeVillageSocialStandingAmbientCooldown(
        uint playerEntityId,
        VillageSocialStandingTier tier)
    {
        var currentMinute = _worldTime.TotalGameMinutes;
        var cooldown = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(tier);

        if (_lastVillageSocialStandingAmbientGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < cooldown)
        {
            return false;
        }

        _lastVillageSocialStandingAmbientGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare Well-liked standing privileges from focus NPCs.</summary>
    public bool TryConsumeWellLikedStandingPrivilegeCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastWellLikedStandingPrivilegeGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < VillageSocialStandingImpactConfig.WellLikedPrivilegeCooldownGameMinutes)
        {
            return false;
        }

        _lastWellLikedStandingPrivilegeGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare Well-liked legacy journey lines from Harold, Greta, or Elsie.</summary>
    public bool TryConsumeSocialLegacyNpcMentionCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastSocialLegacyNpcMentionGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < SocialLegacyConfig.LegacyNpcMentionCooldownGameMinutes)
        {
            return false;
        }

        _lastSocialLegacyNpcMentionGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for focus NPCs noting wider village recognition of the player's social standing.</summary>
    public bool TryConsumeVillageSocialStandingAwarenessCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastVillageSocialStandingAwarenessGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < VillageSocialStandingImpactConfig.VillageAwarenessCooldownGameMinutes)
        {
            return false;
        }

        _lastVillageSocialStandingAwarenessGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for active social-standing favor requests per focus NPC.</summary>
    public bool TryConsumeSocialStandingFavorCooldown(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;
        var cooldown = SocialStandingActionConfig.GetCooldownGameMinutes(tier);

        if (_lastSocialStandingFavorGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < cooldown)
        {
            return false;
        }

        _lastSocialStandingFavorGameMinute[key] = currentMinute;
        return true;
    }

    public int GetSocialStandingFavorCooldownRemainingMinutes(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier)
    {
        var key = (playerEntityId, npcEntityId);
        var cooldown = SocialStandingActionConfig.GetCooldownGameMinutes(tier);

        if (!_lastSocialStandingFavorGameMinute.TryGetValue(key, out var lastMinute))
            return 0;

        var elapsed = _worldTime.TotalGameMinutes - lastMinute;
        var remaining = cooldown - elapsed;
        return remaining > 0 ? (int)remaining : 0;
    }

    /// <summary>Cooldown for active social-influence calls on focus NPCs (Respected and Well-liked).</summary>
    public bool TryConsumeSocialInfluenceCooldown(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;
        var cooldown = SocialInfluenceActionConfig.GetCooldownGameMinutes(npcEntityId, tier);

        if (_lastSocialInfluenceGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < cooldown)
        {
            return false;
        }

        _lastSocialInfluenceGameMinute[key] = currentMinute;
        return true;
    }

    public int GetSocialInfluenceCooldownRemainingMinutes(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier)
    {
        var key = (playerEntityId, npcEntityId);
        var cooldown = SocialInfluenceActionConfig.GetCooldownGameMinutes(npcEntityId, tier);

        if (!_lastSocialInfluenceGameMinute.TryGetValue(key, out var lastMinute))
            return 0;

        var elapsed = _worldTime.TotalGameMinutes - lastMinute;
        var remaining = cooldown - elapsed;
        return remaining > 0 ? (int)remaining : 0;
    }

    public void GrantHaroldSocialInfluenceProjectBacking(uint playerEntityId, int progressBonus) =>
        _haroldSocialInfluenceProjectBacking[playerEntityId] = progressBonus;

    public bool HasHaroldSocialInfluenceProjectBacking(uint playerEntityId) =>
        _haroldSocialInfluenceProjectBacking.ContainsKey(playerEntityId);

    public bool TryGetHaroldSocialInfluenceProjectBacking(uint playerEntityId, out int progressBonus) =>
        _haroldSocialInfluenceProjectBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeHaroldSocialInfluenceProjectBacking(uint playerEntityId, out int progressBonus) =>
        _haroldSocialInfluenceProjectBacking.Remove(playerEntityId, out progressBonus);

    public void GrantElsieSocialInfluenceGardenBacking(uint playerEntityId, int progressBonus) =>
        _elsieSocialInfluenceGardenBacking[playerEntityId] = progressBonus;

    public bool HasElsieSocialInfluenceGardenBacking(uint playerEntityId) =>
        _elsieSocialInfluenceGardenBacking.ContainsKey(playerEntityId);

    public bool TryGetElsieSocialInfluenceGardenBacking(uint playerEntityId, out int progressBonus) =>
        _elsieSocialInfluenceGardenBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeElsieSocialInfluenceGardenBacking(uint playerEntityId, out int progressBonus) =>
        _elsieSocialInfluenceGardenBacking.Remove(playerEntityId, out progressBonus);

    public void GrantMiraSocialInfluenceTradePrivilege(uint playerEntityId, bool buyPrivilege) =>
        _miraSocialInfluenceTradePrivilege[playerEntityId] =
            (buyPrivilege, SocialInfluenceActionConfig.MiraTradePrivilegeTransactionCount);

    public bool HasMiraSocialInfluenceTradePrivilege(uint playerEntityId) =>
        _miraSocialInfluenceTradePrivilege.TryGetValue(playerEntityId, out var state)
        && state.RemainingUses > 0;

    public bool TryGetMiraSocialInfluenceTradePrivilegeIsBuy(uint playerEntityId, out bool buyPrivilege)
    {
        if (_miraSocialInfluenceTradePrivilege.TryGetValue(playerEntityId, out var state)
            && state.RemainingUses > 0)
        {
            buyPrivilege = state.BuyPrivilege;
            return true;
        }

        buyPrivilege = false;
        return false;
    }

    public int GetMiraSocialInfluenceTradePrivilegeRemainingUses(uint playerEntityId) =>
        _miraSocialInfluenceTradePrivilege.TryGetValue(playerEntityId, out var state)
            ? state.RemainingUses
            : 0;

    public void GrantTomSocialInfluenceLumberBacking(uint playerEntityId, int progressBonus) =>
        _tomSocialInfluenceLumberBacking[playerEntityId] = progressBonus;

    public bool HasTomSocialInfluenceLumberBacking(uint playerEntityId) =>
        _tomSocialInfluenceLumberBacking.ContainsKey(playerEntityId);

    public bool TryGetTomSocialInfluenceLumberBacking(uint playerEntityId, out int progressBonus) =>
        _tomSocialInfluenceLumberBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeTomSocialInfluenceLumberBacking(uint playerEntityId, out int progressBonus) =>
        _tomSocialInfluenceLumberBacking.Remove(playerEntityId, out progressBonus);

    public void GrantNoraSocialInfluenceHerbalBacking(uint playerEntityId, int progressBonus) =>
        _noraSocialInfluenceHerbalBacking[playerEntityId] = progressBonus;

    public bool HasNoraSocialInfluenceHerbalBacking(uint playerEntityId) =>
        _noraSocialInfluenceHerbalBacking.ContainsKey(playerEntityId);

    public bool TryGetNoraSocialInfluenceHerbalBacking(uint playerEntityId, out int progressBonus) =>
        _noraSocialInfluenceHerbalBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeNoraSocialInfluenceHerbalBacking(uint playerEntityId, out int progressBonus) =>
        _noraSocialInfluenceHerbalBacking.Remove(playerEntityId, out progressBonus);

    public void GrantEliasSocialInfluenceSmithingBacking(uint playerEntityId, int progressBonus) =>
        _eliasSocialInfluenceSmithingBacking[playerEntityId] = progressBonus;

    public bool HasEliasSocialInfluenceSmithingBacking(uint playerEntityId) =>
        _eliasSocialInfluenceSmithingBacking.ContainsKey(playerEntityId);

    public bool TryGetEliasSocialInfluenceSmithingBacking(uint playerEntityId, out int progressBonus) =>
        _eliasSocialInfluenceSmithingBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeEliasSocialInfluenceSmithingBacking(uint playerEntityId, out int progressBonus) =>
        _eliasSocialInfluenceSmithingBacking.Remove(playerEntityId, out progressBonus);

    public void GrantBenSocialInfluenceGuardBacking(uint playerEntityId, int progressBonus) =>
        _benSocialInfluenceGuardBacking[playerEntityId] = progressBonus;

    public bool HasBenSocialInfluenceGuardBacking(uint playerEntityId) =>
        _benSocialInfluenceGuardBacking.ContainsKey(playerEntityId);

    public bool TryGetBenSocialInfluenceGuardBacking(uint playerEntityId, out int progressBonus) =>
        _benSocialInfluenceGuardBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeBenSocialInfluenceGuardBacking(uint playerEntityId, out int progressBonus) =>
        _benSocialInfluenceGuardBacking.Remove(playerEntityId, out progressBonus);

    public void GrantLilaSocialInfluenceYouthBacking(uint playerEntityId, int progressBonus) =>
        _lilaSocialInfluenceYouthBacking[playerEntityId] = progressBonus;

    public bool HasLilaSocialInfluenceYouthBacking(uint playerEntityId) =>
        _lilaSocialInfluenceYouthBacking.ContainsKey(playerEntityId);

    public bool TryGetLilaSocialInfluenceYouthBacking(uint playerEntityId, out int progressBonus) =>
        _lilaSocialInfluenceYouthBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeLilaSocialInfluenceYouthBacking(uint playerEntityId, out int progressBonus) =>
        _lilaSocialInfluenceYouthBacking.Remove(playerEntityId, out progressBonus);

    public void GrantRowanSocialInfluenceStoryBacking(uint playerEntityId, int progressBonus) =>
        _rowanSocialInfluenceStoryBacking[playerEntityId] = progressBonus;

    public bool HasRowanSocialInfluenceStoryBacking(uint playerEntityId) =>
        _rowanSocialInfluenceStoryBacking.ContainsKey(playerEntityId);

    public bool TryGetRowanSocialInfluenceStoryBacking(uint playerEntityId, out int progressBonus) =>
        _rowanSocialInfluenceStoryBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeRowanSocialInfluenceStoryBacking(uint playerEntityId, out int progressBonus) =>
        _rowanSocialInfluenceStoryBacking.Remove(playerEntityId, out progressBonus);

    public void GrantMarcusSocialInfluenceCraftingBacking(uint playerEntityId, int progressBonus) =>
        _marcusSocialInfluenceCraftingBacking[playerEntityId] = progressBonus;

    public bool HasMarcusSocialInfluenceCraftingBacking(uint playerEntityId) =>
        _marcusSocialInfluenceCraftingBacking.ContainsKey(playerEntityId);

    public bool TryGetMarcusSocialInfluenceCraftingBacking(uint playerEntityId, out int progressBonus) =>
        _marcusSocialInfluenceCraftingBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeMarcusSocialInfluenceCraftingBacking(uint playerEntityId, out int progressBonus) =>
        _marcusSocialInfluenceCraftingBacking.Remove(playerEntityId, out progressBonus);

    public void GrantEleanorSocialInfluenceLegacyBacking(uint playerEntityId, int progressBonus) =>
        _eleanorSocialInfluenceLegacyBacking[playerEntityId] = progressBonus;

    public bool HasEleanorSocialInfluenceLegacyBacking(uint playerEntityId) =>
        _eleanorSocialInfluenceLegacyBacking.ContainsKey(playerEntityId);

    public bool TryGetEleanorSocialInfluenceLegacyBacking(uint playerEntityId, out int progressBonus) =>
        _eleanorSocialInfluenceLegacyBacking.TryGetValue(playerEntityId, out progressBonus);

    public bool TryConsumeEleanorSocialInfluenceLegacyBacking(uint playerEntityId, out int progressBonus) =>
        _eleanorSocialInfluenceLegacyBacking.Remove(playerEntityId, out progressBonus);

    public bool TryConsumeMiraSocialInfluenceTradePrivilege(uint playerEntityId, out bool buyPrivilege)
    {
        if (!_miraSocialInfluenceTradePrivilege.TryGetValue(playerEntityId, out var state)
            || state.RemainingUses <= 0)
        {
            buyPrivilege = false;
            return false;
        }

        buyPrivilege = state.BuyPrivilege;
        state.RemainingUses--;
        if (state.RemainingUses <= 0)
            _miraSocialInfluenceTradePrivilege.Remove(playerEntityId);
        else
            _miraSocialInfluenceTradePrivilege[playerEntityId] = state;

        return true;
    }

    /// <summary>Cooldown for rare village-wide ambient recognition of emotional bonds.</summary>
    public bool TryConsumeVillageBondRecognitionAmbientCooldown(
        uint playerEntityId,
        int focusCloseFriendCount)
    {
        var currentMinute = _worldTime.TotalGameMinutes;
        var cooldown = VillageBondRecognitionConfig.GetAmbientRecognitionCooldownGameMinutes(focusCloseFriendCount);

        if (_lastVillageBondRecognitionAmbientGameMinute.TryGetValue(playerEntityId, out var lastMinute)
            && currentMinute - lastMinute < cooldown)
        {
            return false;
        }

        _lastVillageBondRecognitionAmbientGameMinute[playerEntityId] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for rare village-wide appreciation from a focus NPC.</summary>
    public bool TryConsumeVillageBondAppreciationCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastVillageBondAppreciationGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < VillageBondRecognitionConfig.VillageAppreciationCooldownGameMinutes)
        {
            return false;
        }

        _lastVillageBondAppreciationGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Cooldown for focus NPCs noticing the player's other close bonds during talk.</summary>
    public bool TryConsumeCrossNpcBondRecognitionCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastCrossNpcBondRecognitionGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < VillageBondRecognitionConfig.CrossNpcRecognitionCooldownGameMinutes)
        {
            return false;
        }

        _lastCrossNpcBondRecognitionGameMinute[key] = currentMinute;
        return true;
    }

    /// <summary>Separate, longer cooldown so personal moments stay rare and special.</summary>
    public bool TryConsumePersonalMomentCooldown(uint playerEntityId, uint npcEntityId)
    {
        var key = (playerEntityId, npcEntityId);
        var currentMinute = _worldTime.TotalGameMinutes;

        if (_lastPersonalMomentGameMinute.TryGetValue(key, out var lastMinute)
            && currentMinute - lastMinute < NpcMemoryConfig.PersonalMomentCooldownGameMinutes)
        {
            return false;
        }

        _lastPersonalMomentGameMinute[key] = currentMinute;
        return true;
    }

    private static void ClearPlayerKeyedDictionary<TValue>(
        Dictionary<(uint PlayerEntityId, uint NpcEntityId), TValue> dictionary,
        uint playerEntityId)
    {
        var keys = dictionary.Keys.Where(key => key.PlayerEntityId == playerEntityId).ToList();
        foreach (var key in keys)
            dictionary.Remove(key);
    }

    private void ClearPlayerAreaHelpCounts(uint playerEntityId)
    {
        var keys = _focusAreaHelpCounts.Keys.Where(key => key.PlayerEntityId == playerEntityId).ToList();
        foreach (var key in keys)
            _focusAreaHelpCounts.Remove(key);
    }

    private void ClearPlayerBondingActionCooldowns(uint playerEntityId)
    {
        var keys = _lastBondingActionGameMinute.Keys
            .Where(key => key.PlayerEntityId == playerEntityId)
            .ToList();
        foreach (var key in keys)
            _lastBondingActionGameMinute.Remove(key);
    }
}