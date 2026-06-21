using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Serilog;

namespace Bloomtown.Server.Simulation.Relationship;

/// <summary>
/// Manages player–NPC affinity: load/cache, interaction growth, and slow neglect decay.
/// </summary>
public sealed class PlayerNpcRelationshipService : ISimulationSystem
{
    private readonly PlayerNpcRelationshipRepository _repository;
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<uint, Dictionary<uint, PlayerNpcRelationship>> _cache = new();
    private int _lastDecayGameDay = -1;

    public PlayerNpcRelationshipService(
        PlayerNpcRelationshipRepository repository,
        WorldTimeSystem worldTime)
    {
        _repository = repository;
        _worldTime = worldTime;
    }

    public async Task LoadPlayerAsync(uint playerEntityId)
    {
        var records = await _repository.GetByPlayerAsync(playerEntityId);
        var npcMap = new Dictionary<uint, PlayerNpcRelationship>();

        foreach (var record in records)
        {
            npcMap[record.NpcEntityId] = new PlayerNpcRelationship
            {
                PlayerEntityId = record.PlayerEntityId,
                NpcEntityId = record.NpcEntityId,
                AffinityValue = record.AffinityValue,
                LastInteractionGameDay = record.LastInteractionGameDay,
                LastInteractionUtc = record.LastInteractionUtc,
            };
        }

        _cache[playerEntityId] = npcMap;

        if (records.Count == 0)
        {
            Log.Information("Loaded relationships for player {PlayerId}: no saved records (all Stranger).", playerEntityId);
            return;
        }

        foreach (var record in records)
        {
            var npcName = NpcNameLookup.GetDisplayNameOrDefault(record.NpcEntityId);
            var tier = RelationshipTierCalculator.GetTier(record.AffinityValue);
            Log.Information(
                "Loaded relationship for player {PlayerId} with {NpcName}: affinity {Affinity} ({Tier})",
                playerEntityId,
                npcName,
                record.AffinityValue,
                RelationshipTierDisplay.GetName(tier));
        }
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
    }

    public IReadOnlyList<uint> GetCachedPlayerIds()
    {
        return _cache.Keys.ToList();
    }

    public int GetAffinity(uint playerEntityId, uint npcEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var npcMap))
            return 0;

        return npcMap.TryGetValue(npcEntityId, out var relationship)
            ? relationship.AffinityValue
            : 0;
    }

    public RelationshipTier GetTier(uint playerEntityId, uint npcEntityId)
    {
        return RelationshipTierCalculator.GetTier(GetAffinity(playerEntityId, npcEntityId));
    }

    /// <summary>
    /// Returns relationships sorted by affinity (highest first) for status display.
    /// </summary>
    public IReadOnlyList<PlayerNpcRelationship> GetRelationships(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var npcMap))
            return Array.Empty<PlayerNpcRelationship>();

        return npcMap.Values
            .OrderByDescending(relationship => relationship.AffinityValue)
            .ThenBy(relationship => relationship.NpcEntityId)
            .ToList();
    }

    /// <summary>
    /// Applies affinity growth after a successful Greet or Talk and persists the update.
    /// </summary>
    public Task<RelationshipChangeResult> ApplyInteractionGainAsync(
        uint playerEntityId,
        uint npcEntityId,
        NpcInteractionKind kind)
    {
        var gain = kind switch
        {
            NpcInteractionKind.Greet => RelationshipConfig.GreetAffinityGain,
            NpcInteractionKind.Talk => RelationshipConfig.TalkAffinityGain,
            _ => 0,
        };

        return ApplyAffinityGainAsync(playerEntityId, npcEntityId, gain);
    }

    /// <summary>
    /// Applies a custom affinity gain (gifts, interactions) and persists the update.
    /// </summary>
    public async Task<RelationshipChangeResult> ApplyAffinityGainAsync(
        uint playerEntityId,
        uint npcEntityId,
        int affinityGain)
    {
        if (affinityGain <= 0)
        {
            var currentValue = GetAffinity(playerEntityId, npcEntityId);
            var currentTier = RelationshipTierCalculator.GetTier(currentValue);
            return new RelationshipChangeResult(currentValue, currentValue, currentTier, currentTier);
        }

        var previousValue = GetAffinity(playerEntityId, npcEntityId);
        var previousTier = RelationshipTierCalculator.GetTier(previousValue);
        var newValue = Math.Clamp(previousValue + affinityGain, 0, RelationshipConfig.MaxAffinity);
        var newTier = RelationshipTierCalculator.GetTier(newValue);
        var now = DateTime.UtcNow;
        var gameDay = _worldTime.GameDay;

        if (!_cache.TryGetValue(playerEntityId, out var npcMap))
        {
            npcMap = new Dictionary<uint, PlayerNpcRelationship>();
            _cache[playerEntityId] = npcMap;
        }

        npcMap[npcEntityId] = new PlayerNpcRelationship
        {
            PlayerEntityId = playerEntityId,
            NpcEntityId = npcEntityId,
            AffinityValue = newValue,
            LastInteractionGameDay = gameDay,
            LastInteractionUtc = now,
        };

        await _repository.UpsertAsync(new PlayerNpcRelationshipRecord
        {
            PlayerEntityId = playerEntityId,
            NpcEntityId = npcEntityId,
            AffinityValue = newValue,
            LastInteractionGameDay = gameDay,
            LastInteractionUtc = now,
        });

        return new RelationshipChangeResult(previousValue, newValue, previousTier, newTier);
    }

    /// <summary>
    /// Once per game day, decays affinity for relationships neglected past the grace period.
    /// </summary>
    public void Update(double deltaTimeSeconds)
    {
        var currentDay = _worldTime.GameDay;
        if (currentDay == _lastDecayGameDay)
            return;

        _lastDecayGameDay = currentDay;
        ApplyDailyDecayAsync(currentDay).GetAwaiter().GetResult();
    }

    private async Task ApplyDailyDecayAsync(int currentGameDay)
    {
        foreach (var (playerEntityId, npcMap) in _cache)
        {
            foreach (var (npcEntityId, relationship) in npcMap.ToList())
            {
                if (relationship.AffinityValue <= 0)
                    continue;

                var idleDays = currentGameDay - relationship.LastInteractionGameDay;
                if (idleDays <= RelationshipConfig.DecayGraceGameDays)
                    continue;

                var previousValue = relationship.AffinityValue;
                var previousTier = RelationshipTierCalculator.GetTier(previousValue);
                var newValue = Math.Max(0, previousValue - RelationshipConfig.DecayPerIdleGameDay);
                var newTier = RelationshipTierCalculator.GetTier(newValue);

                relationship.AffinityValue = newValue;
                npcMap[npcEntityId] = relationship;

                await _repository.UpsertAsync(new PlayerNpcRelationshipRecord
                {
                    PlayerEntityId = playerEntityId,
                    NpcEntityId = npcEntityId,
                    AffinityValue = newValue,
                    LastInteractionGameDay = relationship.LastInteractionGameDay,
                    LastInteractionUtc = relationship.LastInteractionUtc,
                });

                var npcName = NpcNameLookup.GetDisplayNameOrDefault(npcEntityId);
                Log.Information(
                    "Player {PlayerId} affinity with {NpcName} decayed {Previous}->{New} ({PreviousTier}->{NewTier}) after {IdleDays} idle game day(s).",
                    playerEntityId,
                    npcName,
                    previousValue,
                    newValue,
                    RelationshipTierDisplay.GetName(previousTier),
                    RelationshipTierDisplay.GetName(newTier),
                    idleDays);
            }
        }
    }
}

public readonly record struct RelationshipChangeResult(
    int PreviousAffinity,
    int NewAffinity,
    RelationshipTier PreviousTier,
    RelationshipTier NewTier);