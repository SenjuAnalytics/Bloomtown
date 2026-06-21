using Bloomtown.Server.Persistence;
using Bloomtown.Shared.Items;
using Serilog;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Loads, caches, and persists per-player personal chest contents.
/// </summary>
public sealed class PlayerChestService
{
    private readonly PlayerChestRepository _chestRepository;
    private readonly Dictionary<uint, PlayerChestState> _cache = new();

    public PlayerChestService(PlayerChestRepository chestRepository)
    {
        _chestRepository = chestRepository;
    }

    public async Task LoadPlayerAsync(uint playerEntityId)
    {
        var entries = await _chestRepository.GetByPlayerAsync(playerEntityId);
        var state = new PlayerChestState { PlayerEntityId = playerEntityId };
        state.Storage.Load(entries.Select(entry => new ItemStack(entry.ItemType, entry.Quantity)));
        _cache[playerEntityId] = state;

        Log.Information(
            "Loaded personal chest for player {PlayerId}: {ItemCount} stack(s).",
            playerEntityId,
            entries.Count);
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        _cache.Remove(playerEntityId);
    }

    public bool TryGetState(uint playerEntityId, out PlayerChestState state)
    {
        return _cache.TryGetValue(playerEntityId, out state!);
    }

    public string FormatChest(PlayerChestState state)
    {
        var lines = new List<string>
        {
            $"Personal Chest at ({PersonalChestConfig.WorldX:F0}, {PersonalChestConfig.WorldZ:F0}):",
        };

        var stacks = state.Storage.ToStacks().ToList();
        if (stacks.Count == 0)
        {
            lines.Add("  (empty)");
            return string.Join(Environment.NewLine, lines);
        }

        foreach (var stack in stacks)
            lines.Add($"  - {ItemDatabase.GetDisplayName(stack.ItemType)} x{stack.Quantity}");

        return string.Join(Environment.NewLine, lines);
    }

    public async Task SavePlayerAsync(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var state))
            return;

        await _chestRepository.ReplaceChestAsync(playerEntityId, state.Storage.ToStacks());

        Log.Debug("Saved personal chest for player {PlayerId}.", playerEntityId);
    }

    public IEnumerable<(uint PlayerEntityId, IReadOnlyList<ItemStack> Stacks)> GetCachedSnapshots()
    {
        foreach (var (playerEntityId, state) in _cache)
            yield return (playerEntityId, state.Storage.ToStacks().ToList());
    }
}