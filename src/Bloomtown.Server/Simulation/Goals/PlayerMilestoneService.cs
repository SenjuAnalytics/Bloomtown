using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Goals;

/// <summary>
/// Tracks optional personal milestones, applies light rewards, and surfaces compact status feedback.
/// </summary>
public sealed class PlayerMilestoneService
{
    private readonly PlayerMilestoneRepository _repository;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerHousingService _housingService;
    private readonly CommunityReputationService _reputationService;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly WorldTimeSystem _worldTime;

    private readonly Dictionary<uint, PlayerMilestoneProgress> _cache = new();
    private readonly Dictionary<uint, PlayerMilestoneKind?> _pendingFeedback = new();

    public PlayerMilestoneService(
        PlayerMilestoneRepository repository,
        PlayerEconomyService economyService,
        PlayerHousingService housingService,
        CommunityReputationService reputationService,
        PlayerNpcRelationshipService relationshipService,
        WorldTimeSystem worldTime)
    {
        _repository = repository;
        _economyService = economyService;
        _housingService = housingService;
        _reputationService = reputationService;
        _relationshipService = relationshipService;
        _worldTime = worldTime;
    }

    public void UnloadPlayer(uint playerEntityId)
    {
        if (_cache.ContainsKey(playerEntityId))
            SaveAsync(playerEntityId).GetAwaiter().GetResult();

        _cache.Remove(playerEntityId);
        _pendingFeedback.Remove(playerEntityId);
    }

    public async Task LoadAsync(uint playerEntityId)
    {
        var record = await _repository.GetAsync(playerEntityId);
        _cache[playerEntityId] = record is null
            ? PlayerMilestoneConfig.CreateDefault()
            : FromRecord(record);

        Log.Debug(
            "Loaded personal milestones for player {PlayerId}: {Completed}/{Total} complete.",
            playerEntityId,
            _cache[playerEntityId].CompletedCount,
            PlayerMilestoneConfig.TotalMilestoneCount);
    }

    public PlayerMilestoneProgress GetProgress(uint playerEntityId) =>
        _cache.TryGetValue(playerEntityId, out var progress)
            ? progress
            : PlayerMilestoneConfig.CreateDefault();

    public void RecordRhythmAgencyDay(uint playerEntityId, int gameDay)
    {
        var progress = GetOrCreate(playerEntityId);
        progress.RhythmAgencyDays.Add(gameDay);
    }

    public void RecordDailyVillageActivity(uint playerEntityId, int gameDay)
    {
        var progress = GetOrCreate(playerEntityId);
        progress.DailyActivityCount++;
        progress.DailyActivityDays.Add(gameDay);
    }

    public async Task<IReadOnlyList<PlayerMilestoneKind>> ReconcileAsync(uint playerEntityId)
    {
        var progress = GetOrCreate(playerEntityId);
        var snapshot = BuildSnapshot(playerEntityId, progress);
        var newlyCompleted = PlayerMilestoneConfig.EvaluateNewMilestones(progress, snapshot);

        if (newlyCompleted.Count == 0)
            return newlyCompleted;

        foreach (var milestone in newlyCompleted)
        {
            progress.Completed.Add(milestone);
            ApplyReward(playerEntityId, milestone);
            _pendingFeedback[playerEntityId] = milestone;

            Log.Information(
                "Player {PlayerId} earned personal milestone {Milestone}.",
                playerEntityId,
                milestone);
        }

        await SaveAsync(playerEntityId);
        return newlyCompleted;
    }

    public bool TryConsumePendingFeedback(uint playerEntityId, out string? feedback)
    {
        feedback = null;

        if (!_pendingFeedback.TryGetValue(playerEntityId, out var milestone)
            || milestone is null)
        {
            return false;
        }

        feedback = PlayerMilestoneConfig.GetCompletionFeedback(milestone.Value);
        _pendingFeedback.Remove(playerEntityId);

        if (string.IsNullOrWhiteSpace(feedback))
            return false;

        Log.Information(
            "Delivered personal milestone feedback ({Milestone}) to player {PlayerId}: \"{Feedback}\"",
            milestone,
            playerEntityId,
            feedback);

        return true;
    }

    public string FormatStatusLine(uint playerEntityId)
    {
        var progress = GetProgress(playerEntityId);
        var snapshot = BuildSnapshot(playerEntityId, progress);
        return PlayerMilestoneConfig.FormatStatusLine(progress, snapshot);
    }

    private PlayerMilestoneSnapshot BuildSnapshot(uint playerEntityId, PlayerMilestoneProgress progress)
    {
        var placedFurniture = 0;
        var comfortScore = 0;
        if (_housingService.TryGetState(playerEntityId, out var house))
        {
            placedFurniture = house.PlacedFurniture.Values.Sum();
            comfortScore = house.ComfortScore;
        }

        var reputation = _reputationService.GetState(playerEntityId);
        var focusCloseFriends = VillageBondRecognitionConfig.CountFocusCloseFriends(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));

        return new PlayerMilestoneSnapshot(
            placedFurniture,
            comfortScore,
            reputation.TotalHelpCount,
            focusCloseFriends,
            progress.RhythmAgencyDays.Count,
            progress.DailyActivityCount,
            progress.DailyActivityDays.Count);
    }

    private void ApplyReward(uint playerEntityId, PlayerMilestoneKind milestone)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + PlayerMilestoneConfig.MilestoneMoodReward);

        if (milestone is PlayerMilestoneKind.HelpingHand or PlayerMilestoneKind.RespectedNeighbor)
            economy.VillageReputation += PlayerMilestoneConfig.MilestoneReputationReward;

        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
    }

    public async Task SaveAsync(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var progress))
            return;

        await _repository.UpsertAsync(ToRecord(playerEntityId, progress));
    }

    private PlayerMilestoneProgress GetOrCreate(uint playerEntityId)
    {
        if (!_cache.TryGetValue(playerEntityId, out var progress))
        {
            progress = PlayerMilestoneConfig.CreateDefault();
            _cache[playerEntityId] = progress;
        }

        return progress;
    }

    private static PlayerMilestoneProgress FromRecord(PlayerMilestoneRecord record)
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        foreach (var kind in PlayerMilestoneConfig.AllMilestones)
        {
            if ((record.CompletedBitmask & (1 << (int)kind)) != 0)
                progress.Completed.Add(kind);
        }

        foreach (var day in ParseDays(record.RhythmAgencyDays))
            progress.RhythmAgencyDays.Add(day);

        progress.DailyActivityCount = record.DailyActivityCount;
        foreach (var day in ParseDays(record.DailyActivityDays))
            progress.DailyActivityDays.Add(day);

        return progress;
    }

    private static PlayerMilestoneRecord ToRecord(uint playerEntityId, PlayerMilestoneProgress progress)
    {
        var bitmask = 0;
        foreach (var kind in progress.Completed)
            bitmask |= 1 << (int)kind;

        return new PlayerMilestoneRecord
        {
            PlayerEntityId = playerEntityId,
            CompletedBitmask = bitmask,
            RhythmAgencyDays = FormatDays(progress.RhythmAgencyDays),
            DailyActivityCount = progress.DailyActivityCount,
            DailyActivityDays = FormatDays(progress.DailyActivityDays),
        };
    }

    private static IEnumerable<int> ParseDays(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            yield break;

        foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var day) && day > 0)
                yield return day;
        }
    }

    private static string FormatDays(IEnumerable<int> days) =>
        string.Join(',', days.OrderBy(day => day));
}