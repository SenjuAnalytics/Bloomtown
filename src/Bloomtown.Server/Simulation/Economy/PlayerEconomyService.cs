using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Contribution;
using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Needs;
using Serilog;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// In-memory wallet/inventory cache with database persistence.
/// </summary>
public sealed class PlayerEconomyService
{
    private readonly PlayerRepository _playerRepository;
    private readonly PlayerInventoryRepository _inventoryRepository;
    private readonly Dictionary<uint, PlayerEconomyState> _cache = new();

    public PlayerEconomyService(
        PlayerRepository playerRepository,
        PlayerInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task LoadPlayerAsync(uint playerEntityId, bool isReconnect)
    {
        var coins = isReconnect
            ? await _playerRepository.GetCoinsAsync(playerEntityId) ?? EconomyConfig.StartingCoins
            : EconomyConfig.StartingCoins;

        var villageReputation = isReconnect
            ? await _playerRepository.GetVillageReputationAsync(playerEntityId) ?? 0
            : 0;

        var energy = isReconnect
            ? await _playerRepository.GetEnergyAsync(playerEntityId) ?? VillageMilestoneConfig.DefaultPlayerEnergy
            : VillageMilestoneConfig.DefaultPlayerEnergy;

        var hunger = isReconnect
            ? await _playerRepository.GetHungerAsync(playerEntityId) ?? PlayerHungerConfig.DefaultHunger
            : PlayerHungerConfig.DefaultHunger;

        var mood = isReconnect
            ? await _playerRepository.GetMoodAsync(playerEntityId) ?? PlayerNeedsConfig.DefaultMood
            : PlayerNeedsConfig.DefaultMood;

        var fatigue = isReconnect
            ? await _playerRepository.GetFatigueAsync(playerEntityId) ?? PlayerNeedsConfig.DefaultFatigue
            : PlayerNeedsConfig.DefaultFatigue;

        var socialNeed = isReconnect
            ? await _playerRepository.GetSocialNeedAsync(playerEntityId) ?? PlayerNeedsConfig.DefaultSocialNeed
            : PlayerNeedsConfig.DefaultSocialNeed;

        var needsLastGameMinute = isReconnect
            ? await _playerRepository.GetNeedsLastGameMinuteAsync(playerEntityId) ?? 0L
            : 0L;

        var contributionScore = isReconnect
            ? await _playerRepository.GetVillageContributionScoreAsync(playerEntityId) ?? 0
            : 0;

        var villagePosition = VillagePosition.None;
        DateTime? positionAssignedAt = null;
        if (isReconnect)
        {
            var positionData = await _playerRepository.GetVillagePositionAsync(playerEntityId);
            if (positionData.HasValue)
            {
                villagePosition = positionData.Value.Position;
                positionAssignedAt = positionData.Value.AssignedAtUtc;
            }
        }

        var inventoryEntries = await _inventoryRepository.GetByPlayerAsync(playerEntityId);
        var state = new PlayerEconomyState
        {
            PlayerEntityId = playerEntityId,
            Coins = coins,
            VillageReputation = villageReputation,
            VillageContributionScore = contributionScore,
            VillageTitle = VillageTitleCalculator.GetTitle(contributionScore),
            VillagePosition = villagePosition,
            PositionAssignedAtUtc = positionAssignedAt,
            Energy = energy,
            Hunger = hunger,
            Mood = mood,
            Fatigue = fatigue,
            SocialNeed = socialNeed,
            LastNeedsUpdateTotalGameMinute = needsLastGameMinute,
        };

        state.Inventory.Load(inventoryEntries.Select(entry => new ItemStack(entry.ItemType, entry.Quantity)));
        _cache[playerEntityId] = state;

        var positionLabel = state.VillagePosition == VillagePosition.None
            ? "no position"
            : VillagePositionDisplay.GetName(state.VillagePosition);

        Log.Information(
            "Loaded economy for player {PlayerId}: {Coins} coins, village rep {VillageReputation}, contribution {ContributionScore} ({Title}), position {Position}, energy {Energy:F0}, hunger {Hunger:F0}, mood {Mood:F0}, fatigue {Fatigue:F0}, social {Social:F0}, {ItemCount} inventory stack(s).",
            playerEntityId,
            coins,
            villageReputation,
            state.VillageContributionScore,
            VillageTitleDisplay.GetName(state.VillageTitle),
            positionLabel,
            energy,
            hunger,
            mood,
            fatigue,
            socialNeed,
            inventoryEntries.Count);
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
    }

    public bool TryGetState(uint playerEntityId, out PlayerEconomyState state)
    {
        return _cache.TryGetValue(playerEntityId, out state!);
    }

    public IReadOnlyList<uint> GetCachedPlayerIds()
    {
        return _cache.Keys.ToList();
    }

    public string FormatInventory(PlayerEconomyState state)
    {
        var lines = new List<string> { $"Coins: {state.Coins}" };
        var stacks = state.Inventory.ToStacks().ToList();

        if (stacks.Count == 0)
        {
            lines.Add("Items: (empty)");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("Items:");
        foreach (var stack in stacks)
            lines.Add($"  - {ItemDatabase.GetDisplayName(stack.ItemType)} x{stack.Quantity}");

        return string.Join(Environment.NewLine, lines);
    }

    public async Task SavePlayerAsync(uint playerEntityId, PlayerRecord? positionRecord = null)
    {
        if (!_cache.TryGetValue(playerEntityId, out var state))
            return;

        if (positionRecord is not null)
        {
            await _playerRepository.UpsertAsync(new PlayerRecord
            {
                EntityId = positionRecord.EntityId,
                PositionX = positionRecord.PositionX,
                PositionY = positionRecord.PositionY,
                PositionZ = positionRecord.PositionZ,
                RotationYaw = positionRecord.RotationYaw,
                LastSeenUtc = positionRecord.LastSeenUtc,
                Coins = state.Coins,
                VillageReputation = state.VillageReputation,
                Energy = state.Energy,
                Hunger = state.Hunger,
                Mood = state.Mood,
                Fatigue = state.Fatigue,
                SocialNeed = state.SocialNeed,
                NeedsLastGameMinute = state.LastNeedsUpdateTotalGameMinute,
                VillageContributionScore = state.VillageContributionScore,
                VillageTitle = state.VillageTitle,
                VillagePosition = state.VillagePosition,
                PositionAssignedAtUtc = state.PositionAssignedAtUtc,
            });
        }
        else
        {
            await _playerRepository.UpdateEconomyAsync(
                playerEntityId,
                state.Coins,
                state.VillageReputation,
                state.Energy,
                state.Hunger,
                state.Mood,
                state.Fatigue,
                state.SocialNeed,
                state.LastNeedsUpdateTotalGameMinute,
                state.VillageContributionScore,
                state.VillageTitle,
                state.VillagePosition,
                state.PositionAssignedAtUtc);
        }

        await _inventoryRepository.ReplaceInventoryAsync(playerEntityId, state.Inventory.ToStacks());

        Log.Debug(
            "Saved economy for player {PlayerId}: {Coins} coins.",
            playerEntityId,
            state.Coins);
    }
}