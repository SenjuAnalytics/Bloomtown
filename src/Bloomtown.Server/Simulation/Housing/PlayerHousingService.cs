using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Loads home locations, tiers, home storage, upgrades, and sleep-at-home recovery.
/// </summary>
public sealed class PlayerHousingService
{
    private readonly PlayerHousingRepository _housingRepository;
    private readonly PlayerHomeStorageRepository _homeStorageRepository;
    private readonly PlayerHouseFurnitureRepository _furnitureRepository;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly Dictionary<uint, PlayerHouseState> _cache = new();

    public PlayerHousingService(
        PlayerHousingRepository housingRepository,
        PlayerHomeStorageRepository homeStorageRepository,
        PlayerHouseFurnitureRepository furnitureRepository,
        PlayerEconomyService economyService,
        PlayerNeedsService needsService)
    {
        _housingRepository = housingRepository;
        _homeStorageRepository = homeStorageRepository;
        _furnitureRepository = furnitureRepository;
        _economyService = economyService;
        _needsService = needsService;
    }

    public async Task LoadPlayerAsync(uint playerEntityId)
    {
        var housing = await _housingRepository.GetAsync(playerEntityId);
        if (housing is null)
        {
            var (houseX, houseZ) = PlayerHousingConfig.ComputeDefaultLocation(playerEntityId);
            housing = new PlayerHousingRecord
            {
                PlayerEntityId = playerEntityId,
                HouseX = houseX,
                HouseZ = houseZ,
                HouseTier = HouseTier.Basic,
            };
            await _housingRepository.UpsertAsync(housing);

            Log.Information(
                "Assigned home for player {PlayerId} at ({HouseX:F0}, {HouseZ:F0}).",
                playerEntityId,
                houseX,
                houseZ);
        }

        var storageEntries = await _homeStorageRepository.GetByPlayerAsync(playerEntityId);
        var furnitureEntries = await _furnitureRepository.GetByPlayerAsync(playerEntityId);
        var state = new PlayerHouseState
        {
            PlayerEntityId = playerEntityId,
            HouseX = housing.HouseX,
            HouseZ = housing.HouseZ,
            HouseTier = housing.HouseTier,
        };
        state.HomeStorage.Load(storageEntries.Select(entry => new ItemStack(entry.ItemType, entry.Quantity)));
        foreach (var entry in furnitureEntries)
            state.PlacedFurniture[entry.FurnitureType] = entry.Quantity;
        _cache[playerEntityId] = state;

        Log.Information(
            "Loaded home for player {PlayerId} at ({HouseX:F0}, {HouseZ:F0}), tier {Tier}, comfort {Comfort}, {FurnitureCount} furniture piece(s), {ItemCount} storage stack(s).",
            playerEntityId,
            state.HouseX,
            state.HouseZ,
            HouseTierDisplay.GetName(state.HouseTier),
            state.ComfortScore,
            furnitureEntries.Count,
            storageEntries.Count);
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
    }

    public bool TryGetState(uint playerEntityId, out PlayerHouseState state)
    {
        return _cache.TryGetValue(playerEntityId, out state!);
    }

    public bool CanUpgradeHouse(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var house))
            return false;

        if (HouseUpgradeConfig.IsMaxTier(house.HouseTier))
            return false;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return false;

        return FindFirstMissingMaterial(economy.Inventory, house.HouseTier) is null;
    }

    public IReadOnlyDictionary<ItemType, int>? GetUpgradeRequirements(HouseTier currentTier)
    {
        return HouseUpgradeConfig.TryGetUpgradeRequirements(currentTier, out _, out var requirements)
            ? requirements
            : null;
    }

    public string FormatFurnitureStatus(PlayerHouseState house)
    {
        var sleepBonus = FurnitureComfortConfig.GetCombinedSleepRecovery(house.HouseTier, house.ComfortScore);
        var lines = new List<string>
        {
            $"Comfort score: {house.ComfortScore} ({FurnitureComfortConfig.FormatComfortTierLabel(house.ComfortScore)})",
            $"Sleep bonus: +{sleepBonus:F0} Energy (best of house tier or comfort)",
        };

        if (house.PlacedFurniture.Count == 0)
        {
            lines.Add("Furniture: (none placed)");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("Furniture:");
        foreach (var (furnitureType, quantity) in house.PlacedFurniture.OrderBy(pair => pair.Key))
        {
            lines.Add(
                $"  - {FurnitureTypeDisplay.GetName(furnitureType)} x{quantity} (+{FurnitureCatalog.GetComfortValue(furnitureType) * quantity} comfort)");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string FormatUpgradeStatus(PlayerHouseState house)
    {
        var sleepBonus = FurnitureComfortConfig.GetCombinedSleepRecovery(house.HouseTier, house.ComfortScore);
        var lines = new List<string>
        {
            $"House tier: {HouseTierDisplay.GetName(house.HouseTier)} (sleep up to +{sleepBonus:F0} Energy)",
        };

        if (HouseUpgradeConfig.IsMaxTier(house.HouseTier))
        {
            lines.Add("Home is fully upgraded.");
            return string.Join(Environment.NewLine, lines);
        }

        if (HouseUpgradeConfig.TryGetUpgradeRequirements(house.HouseTier, out var nextTier, out var requirements))
        {
            lines.Add(
                $"Next upgrade → {HouseTierDisplay.GetName(nextTier)}: {HouseUpgradeConfig.FormatRequirements(requirements)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string FormatHomeStorage(PlayerHouseState house)
    {
        var lines = new List<string>
        {
            $"Home storage at ({house.HouseX:F0}, {house.HouseZ:F0}):",
            FormatUpgradeStatus(house),
            FormatFurnitureStatus(house),
        };

        var stacks = house.HomeStorage.ToStacks().ToList();
        if (stacks.Count == 0)
        {
            lines.Add("  (empty)");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("Items:");
        foreach (var stack in stacks)
            lines.Add($"  - {ItemDatabase.GetDisplayName(stack.ItemType)} x{stack.Quantity}");

        return string.Join(Environment.NewLine, lines);
    }

    public async Task SavePlayerAsync(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var state))
            return;

        await _housingRepository.UpsertAsync(new PlayerHousingRecord
        {
            PlayerEntityId = playerEntityId,
            HouseX = state.HouseX,
            HouseZ = state.HouseZ,
            HouseTier = state.HouseTier,
        });
        await _homeStorageRepository.ReplaceStorageAsync(playerEntityId, state.HomeStorage.ToStacks());
        await _furnitureRepository.ReplaceFurnitureAsync(playerEntityId, state.PlacedFurniture);
        Log.Debug(
            "Saved home for player {PlayerId} (tier {Tier}, comfort {Comfort}).",
            playerEntityId,
            HouseTierDisplay.GetName(state.HouseTier),
            state.ComfortScore);
    }

    public IEnumerable<(uint PlayerEntityId, IReadOnlyList<ItemStack> Stacks)> GetCachedSnapshots()
    {
        foreach (var (playerEntityId, state) in _cache)
            yield return (playerEntityId, state.HomeStorage.ToStacks().ToList());
    }

    public IEnumerable<PlayerHousingRecord> GetCachedHousingRecords()
    {
        foreach (var (_, state) in _cache)
        {
            yield return new PlayerHousingRecord
            {
                PlayerEntityId = state.PlayerEntityId,
                HouseX = state.HouseX,
                HouseZ = state.HouseZ,
                HouseTier = state.HouseTier,
            };
        }
    }

    public IEnumerable<(uint PlayerEntityId, IReadOnlyDictionary<FurnitureType, int> Furniture)> GetCachedFurnitureSnapshots()
    {
        foreach (var (playerEntityId, state) in _cache)
            yield return (playerEntityId, new Dictionary<FurnitureType, int>(state.PlacedFurniture));
    }

    /// <summary>
    /// Validates location and materials, then places furniture and updates comfort score.
    /// </summary>
    public HomeResponse TryPlaceFurniture(uint playerEntityId, float playerX, float playerZ, FurnitureType furnitureType)
    {
        if (!FurnitureCatalog.TryGet(furnitureType, out var definition))
        {
            return Fail(HomeRequestKind.PlaceFurniture, HomeFailureReason.UnknownFurniture, "Unknown furniture type.");
        }

        if (!_cache.TryGetValue(playerEntityId, out var house))
        {
            return Fail(HomeRequestKind.PlaceFurniture, HomeFailureReason.HomeUnavailable, "Home data is unavailable.");
        }

        if (!PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ))
        {
            var distance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            return Fail(
                HomeRequestKind.PlaceFurniture,
                HomeFailureReason.NotAtHome,
                $"You must be at your home ({house.HouseX:F0}, {house.HouseZ:F0}) to place furniture ({distance:F1}m away). " +
                $"Move within {PlayerHousingConfig.AccessRadiusMeters:F0}m.");
        }

        var currentCount = house.PlacedFurniture.GetValueOrDefault(furnitureType, 0);
        if (currentCount >= definition.MaxPerHome)
        {
            return Fail(
                HomeRequestKind.PlaceFurniture,
                HomeFailureReason.FurnitureAlreadyPlaced,
                $"{FurnitureTypeDisplay.GetName(furnitureType)} is already placed in your home.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(HomeRequestKind.PlaceFurniture, HomeFailureReason.HomeUnavailable, "Player inventory is unavailable.");
        }

        var missingMaterial = FindFirstMissingFurnitureMaterial(economy.Inventory, definition.MaterialRequirements);
        if (missingMaterial is not null)
        {
            var (itemType, required, owned) = missingMaterial.Value;
            var itemName = ItemDatabase.GetDisplayName(itemType);

            Log.Information(
                "Player {PlayerId} furniture placement failed: need {Required} {Item}, have {Owned} for {Furniture}.",
                playerEntityId,
                required,
                itemName,
                owned,
                FurnitureTypeDisplay.GetName(furnitureType));

            return Fail(
                HomeRequestKind.PlaceFurniture,
                HomeFailureReason.InsufficientMaterials,
                $"Not enough {itemName} to place {FurnitureTypeDisplay.GetName(furnitureType)} " +
                $"(need {required}, have {owned}). Required: {FurnitureCatalog.FormatMaterialRequirements(definition.MaterialRequirements)}.");
        }

        foreach (var (itemType, quantity) in definition.MaterialRequirements)
            economy.Inventory.RemoveItem(itemType, quantity);

        var previousComfort = house.ComfortScore;
        house.PlacedFurniture[furnitureType] = currentCount + 1;
        var newComfort = house.ComfortScore;
        _needsService.ApplyPlaceFurniture(economy);

        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var sleepBonus = FurnitureComfortConfig.GetCombinedSleepRecovery(house.HouseTier, newComfort);
        var message =
            $"Placed {FurnitureTypeDisplay.GetName(furnitureType)}! Comfort {previousComfort} → {newComfort} " +
            $"(+{definition.ComfortValue}). Sleep bonus up to +{sleepBonus:F0} Energy.";

        Log.Information(
            "Player {PlayerId} placed {Furniture} at home ({HouseX:F0}, {HouseZ:F0}). Comfort {Previous}->{New} using {Materials}.",
            playerEntityId,
            FurnitureTypeDisplay.GetName(furnitureType),
            house.HouseX,
            house.HouseZ,
            previousComfort,
            newComfort,
            FurnitureCatalog.FormatMaterialRequirements(definition.MaterialRequirements));

        return new HomeResponse(true, HomeRequestKind.PlaceFurniture, HomeFailureReason.None, message);
    }

    /// <summary>
    /// Validates materials, consumes them from inventory, and raises the house tier.
    /// </summary>
    public HomeResponse TryUpgradeHouse(uint playerEntityId, float playerX, float playerZ)
    {
        if (!_cache.TryGetValue(playerEntityId, out var house))
        {
            return Fail(HomeRequestKind.Upgrade, HomeFailureReason.HomeUnavailable, "Home data is unavailable.");
        }

        if (!PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ))
        {
            var distance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            return Fail(
                HomeRequestKind.Upgrade,
                HomeFailureReason.NotAtHome,
                $"You must be at your home ({house.HouseX:F0}, {house.HouseZ:F0}) to upgrade it ({distance:F1}m away). " +
                $"Move within {PlayerHousingConfig.AccessRadiusMeters:F0}m.");
        }

        if (HouseUpgradeConfig.IsMaxTier(house.HouseTier))
        {
            return Fail(
                HomeRequestKind.Upgrade,
                HomeFailureReason.MaxTierReached,
                $"Your home is already {HouseTierDisplay.GetName(house.HouseTier)} — fully upgraded.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(HomeRequestKind.Upgrade, HomeFailureReason.HomeUnavailable, "Player inventory is unavailable.");
        }

        if (!HouseUpgradeConfig.TryGetUpgradeRequirements(house.HouseTier, out var nextTier, out var requirements))
        {
            return Fail(HomeRequestKind.Upgrade, HomeFailureReason.MaxTierReached, "Your home is already fully upgraded.");
        }

        var missingMaterial = FindFirstMissingMaterial(economy.Inventory, house.HouseTier);
        if (missingMaterial is not null)
        {
            var (itemType, required, owned) = missingMaterial.Value;
            var itemName = ItemDatabase.GetDisplayName(itemType);

            Log.Information(
                "Player {PlayerId} home upgrade failed: need {Required} {Item}, have {Owned} (current tier {Tier}).",
                playerEntityId,
                required,
                itemName,
                owned,
                HouseTierDisplay.GetName(house.HouseTier));

            return Fail(
                HomeRequestKind.Upgrade,
                HomeFailureReason.InsufficientMaterials,
                $"Not enough {itemName} to upgrade to {HouseTierDisplay.GetName(nextTier)} " +
                $"(need {required}, have {owned}). Required: {HouseUpgradeConfig.FormatRequirements(requirements)}.");
        }

        foreach (var (itemType, quantity) in requirements)
            economy.Inventory.RemoveItem(itemType, quantity);

        var previousTier = house.HouseTier;
        house.HouseTier = nextTier;
        _needsService.ApplyHouseUpgrade(economy);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var sleepBonus = HouseUpgradeConfig.GetSleepEnergyRecovery(house.HouseTier);
        var message =
            $"Home upgraded from {HouseTierDisplay.GetName(previousTier)} to {HouseTierDisplay.GetName(nextTier)}! " +
            $"Sleep now restores +{sleepBonus:F0} Energy.";

        Log.Information(
            "Player {PlayerId} upgraded home {PreviousTier} -> {NewTier} at ({HouseX:F0}, {HouseZ:F0}) using {Materials}.",
            playerEntityId,
            HouseTierDisplay.GetName(previousTier),
            HouseTierDisplay.GetName(nextTier),
            house.HouseX,
            house.HouseZ,
            HouseUpgradeConfig.FormatRequirements(requirements));

        return new HomeResponse(true, HomeRequestKind.Upgrade, HomeFailureReason.None, message);
    }

    /// <summary>
    /// Sleep at home restores energy based on house tier; requires being near your house.
    /// </summary>
    public ClientQueryResponse SleepAtHome(uint playerEntityId, float playerX, float playerZ)
    {
        if (!_cache.TryGetValue(playerEntityId, out var house))
        {
            return new ClientQueryResponse(
                ClientQueryKind.Sleep,
                false,
                "Home data is unavailable.");
        }

        if (!PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ))
        {
            var distance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            return new ClientQueryResponse(
                ClientQueryKind.Sleep,
                false,
                $"You are not at your home ({distance:F1}m away). Your home is at ({house.HouseX:F0}, {house.HouseZ:F0}). " +
                $"Move within {PlayerHousingConfig.AccessRadiusMeters:F0}m and try 'sleep' again.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return new ClientQueryResponse(
                ClientQueryKind.Sleep,
                false,
                "Player state is unavailable.");
        }

        if (economy.Energy >= PlayerEnergyConfig.MaxValue)
        {
            return new ClientQueryResponse(
                ClientQueryKind.Sleep,
                true,
                $"You are already fully rested (Energy {economy.Energy:F0}/{PlayerEnergyConfig.MaxValue:F0}).");
        }

        var sleepRecovery = FurnitureComfortConfig.GetCombinedSleepRecovery(house.HouseTier, house.ComfortScore);
        var before = economy.Energy;
        economy.Energy = PlayerEnergyConfig.Clamp(economy.Energy + sleepRecovery);
        _needsService.ApplySleep(economy, house.ComfortScore);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var restored = economy.Energy - before;

        Log.Information(
            "Player {PlayerId} slept at {Tier} home (comfort {Comfort}, +{Recovery:F0} energy) at ({HouseX:F0}, {HouseZ:F0}) — recovered +{Restore:F0} (now {Energy:F0}/{Max:F0}).",
            playerEntityId,
            HouseTierDisplay.GetName(house.HouseTier),
            house.ComfortScore,
            sleepRecovery,
            house.HouseX,
            house.HouseZ,
            restored,
            economy.Energy,
            PlayerEnergyConfig.MaxValue);

        return new ClientQueryResponse(
            ClientQueryKind.Sleep,
            true,
            $"You sleep soundly in your {HouseTierDisplay.GetName(house.HouseTier).ToLowerInvariant()} home " +
            $"(comfort {house.ComfortScore}) for about {PlayerHousingConfig.HomeSleepDurationGameMinutes} game minutes. " +
            $"Energy +{restored:F0} (now {economy.Energy:F0}/{PlayerEnergyConfig.MaxValue:F0}). " +
            $"(Outdoor 'rest' only restores +{PlayerHousingConfig.RestEnergyRecovery:F0}.)");
    }

    /// <summary>
    /// Returns the first material missing for the upgrade from the current tier.
    /// </summary>
    private static (ItemType ItemType, int Required, int Owned)? FindFirstMissingMaterial(
        Inventory inventory,
        HouseTier currentTier)
    {
        if (!HouseUpgradeConfig.TryGetUpgradeRequirements(currentTier, out _, out var requirements))
            return null;

        foreach (var (itemType, required) in requirements.OrderBy(pair => pair.Key))
        {
            var owned = inventory.GetItemCount(itemType);
            if (owned < required)
                return (itemType, required, owned);
        }

        return null;
    }

    private static (ItemType ItemType, int Required, int Owned)? FindFirstMissingFurnitureMaterial(
        Inventory inventory,
        IReadOnlyDictionary<ItemType, int> requirements)
    {
        foreach (var (itemType, required) in requirements.OrderBy(pair => pair.Key))
        {
            var owned = inventory.GetItemCount(itemType);
            if (owned < required)
                return (itemType, required, owned);
        }

        return null;
    }

    private static HomeResponse Fail(HomeRequestKind kind, HomeFailureReason reason, string message)
    {
        Log.Information("Home request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new HomeResponse(false, kind, reason, message);
    }
}