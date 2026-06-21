using System.Text;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Community;

/// <summary>
/// Tracks completed communal projects, village development level, and passive-benefit queries.
/// </summary>
public sealed class VillageProjectStateService
{
    private readonly CommunityProjectRepository _repository;
    private readonly WorldStateRepository _worldStateRepository;
    private readonly Dictionary<byte, DateTime> _completedAt = new();
    private readonly Dictionary<(uint PlayerId, byte ProjectId), DateTime> _siteCommentCooldowns = new();
    private readonly Dictionary<uint, DateTime> _atmosphereCommentCooldowns = new();

    public VillageProjectStateService(
        CommunityProjectRepository repository,
        WorldStateRepository worldStateRepository)
    {
        _repository = repository;
        _worldStateRepository = worldStateRepository;
    }

    public VillageDevelopmentLevel DevelopmentLevel { get; private set; } = VillageDevelopmentLevel.Quiet;

    /// <summary>Fired when the village development level advances after a project completes.</summary>
    public event Action<VillageDevelopmentLevel>? DevelopmentLevelAdvanced;

    /// <summary>Fired when a communal project is newly marked complete.</summary>
    public event Action<byte>? ProjectCompleted;

    public async Task LoadAsync()
    {
        _completedAt.Clear();
        var statuses = await _repository.GetAllStatusAsync();
        foreach (var status in statuses.Where(record => record.Status == (int)CommunityProjectStatus.Completed))
        {
            var completedAt = status.CompletedAtUtc ?? DateTime.UtcNow;
            _completedAt[(byte)status.ProjectId] = completedAt;
        }

        RecalculateDevelopmentLevel();

        var worldState = await _worldStateRepository.GetAsync();
        if (worldState is not null && worldState.VillageDevelopmentLevel != DevelopmentLevel)
        {
            Log.Information(
                "Reconciled village development level from completed projects ({ComputedLevel}) — persisted value was {PersistedLevel}.",
                VillageAtmosphereConfig.GetDisplayName(DevelopmentLevel),
                VillageAtmosphereConfig.GetDisplayName(worldState.VillageDevelopmentLevel));
        }

        await PersistDevelopmentLevelAsync();

        Log.Information(
            "Village project state loaded — {CompletedCount} completed project(s), atmosphere {DevelopmentLevel}.",
            _completedAt.Count,
            VillageAtmosphereConfig.GetDisplayName(DevelopmentLevel));

        LogActiveBonuses("server start");
    }

    /// <summary>Records completion time when a project finishes (called from CommunityProjectService).</summary>
    public void MarkCompleted(byte projectId, DateTime completedAtUtc)
    {
        if (_completedAt.ContainsKey(projectId))
            return;

        _completedAt[projectId] = completedAtUtc;

        var previousLevel = DevelopmentLevel;
        RecalculateDevelopmentLevel();

        Log.Information(
            "Village project {ProjectId} marked complete at {CompletedAt:u} — passive benefits now active.",
            projectId,
            completedAtUtc);

        if (DevelopmentLevel != previousLevel)
        {
            Log.Information(
                "Village development level advanced {PreviousLevel} -> {NewLevel} after project {ProjectId} completed.",
                VillageAtmosphereConfig.GetDisplayName(previousLevel),
                VillageAtmosphereConfig.GetDisplayName(DevelopmentLevel),
                projectId);

            LogActiveBonuses("development level advanced");
            PersistDevelopmentLevelAsync().GetAwaiter().GetResult();
            DevelopmentLevelAdvanced?.Invoke(DevelopmentLevel);
        }

        ProjectCompleted?.Invoke(projectId);
    }

    public VillageDevelopmentBonuses GetActiveBonuses() =>
        VillageAtmosphereConfig.GetActiveBonuses(DevelopmentLevel);

    public bool IsCompleted(byte projectId) => _completedAt.ContainsKey(projectId);

    public IReadOnlyList<byte> GetCompletedProjectIds() =>
        _completedAt.Keys.OrderBy(id => id).ToList();

    public IReadOnlyList<(byte ProjectId, DateTime CompletedAtUtc)> GetCompletedProjects()
    {
        return _completedAt
            .OrderBy(pair => pair.Key)
            .Select(pair => (pair.Key, pair.Value))
            .ToList();
    }

    public string FormatStatusBenefits()
    {
        if (_completedAt.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        builder.AppendLine("Village projects (active):");

        foreach (var (projectId, _) in GetCompletedProjects())
        {
            var name = VillageProjectBenefitConfig.FormatProjectDisplayName(projectId);
            var benefit = VillageProjectBenefitConfig.FormatStatusBenefit(projectId);
            builder.AppendLine($"  {name} — {benefit}");
        }

        return builder.ToString().TrimEnd();
    }

    public string FormatStatusAtmosphere()
    {
        var builder = new StringBuilder();
        builder.AppendLine(VillageAtmosphereConfig.GetStatusHeadline(DevelopmentLevel));
        builder.AppendLine(VillageAtmosphereConfig.GetStatusFlavor(DevelopmentLevel));
        builder.AppendLine(VillageReactivityConfig.FormatVisibleGrowthSummary(
            DevelopmentLevel,
            GetCompletedProjectIds()));

        var bonusLines = VillageAtmosphereConfig.FormatActiveBonusLines(DevelopmentLevel);
        if (bonusLines.Count > 0)
        {
            builder.AppendLine("Village bonuses (active):");
            foreach (var line in bonusLines)
                builder.AppendLine($"  {line}");
        }

        return builder.ToString().TrimEnd();
    }

    /// <summary>Returns the nearest completed project site within comment radius, if any.</summary>
    public bool TryGetNearestCompletedSite(
        float playerX,
        float playerZ,
        out byte projectId,
        out float distanceMeters)
    {
        projectId = 0;
        distanceMeters = float.MaxValue;

        foreach (var (id, _) in GetCompletedProjects())
        {
            if (!VillageMilestoneRegistry.TryGetForProject(id, out var milestone))
                continue;

            var distance = GetDistance(playerX, playerZ, milestone.WorldX, milestone.WorldZ);
            if (distance > VillageProjectBenefitConfig.ProjectSiteCommentRadiusMeters)
                continue;

            if (distance >= distanceMeters)
                continue;

            projectId = id;
            distanceMeters = distance;
        }

        return projectId != 0;
    }

    public bool IsNearBridge(float playerX, float playerZ)
    {
        if (!IsCompleted(VillageProjectBenefitConfig.BridgeProjectId))
            return false;

        if (!VillageMilestoneRegistry.TryGetForProject(VillageProjectBenefitConfig.BridgeProjectId, out var milestone))
            return false;

        return GetDistance(playerX, playerZ, milestone.WorldX, milestone.WorldZ)
               <= VillageProjectBenefitConfig.BridgePassiveRadiusMeters;
    }

    public bool TryConsumeProjectSiteCommentCooldown(uint playerEntityId, byte projectId)
    {
        var key = (playerEntityId, projectId);
        if (_siteCommentCooldowns.TryGetValue(key, out var lastUsed)
            && DateTime.UtcNow - lastUsed < VillageProjectBenefitConfig.ProjectSiteCommentCooldown)
        {
            return false;
        }

        _siteCommentCooldowns[key] = DateTime.UtcNow;
        return true;
    }

    public bool TryConsumeAtmosphereCommentCooldown(uint playerEntityId)
    {
        if (_atmosphereCommentCooldowns.TryGetValue(playerEntityId, out var lastUsed)
            && DateTime.UtcNow - lastUsed < VillageAtmosphereConfig.AtmosphereCommentCooldown)
        {
            return false;
        }

        _atmosphereCommentCooldowns[playerEntityId] = DateTime.UtcNow;
        return true;
    }

    public async Task PersistDevelopmentLevelAsync()
    {
        var worldState = await _worldStateRepository.GetAsync();
        var record = new Persistence.Models.WorldStateRecord
        {
            GameDay = worldState?.GameDay ?? 1,
            GameMinute = worldState?.GameMinute ?? 0,
            LastSavedUtc = worldState?.LastSavedUtc ?? DateTime.UtcNow,
            VillageDevelopmentLevel = DevelopmentLevel,
        };

        await _worldStateRepository.UpsertAsync(record);
    }

    /// <summary>
    /// Recomputes development level from the number of completed communal projects.
    /// </summary>
    private void RecalculateDevelopmentLevel()
    {
        DevelopmentLevel = VillageAtmosphereConfig.CalculateLevel(_completedAt.Count);
    }

    private void LogActiveBonuses(string reason)
    {
        var bonuses = GetActiveBonuses();
        if (!bonuses.HasPassiveBonus)
        {
            Log.Information("Village development bonuses ({Reason}): none — atmosphere is Quiet.", reason);
            return;
        }

        foreach (var line in VillageAtmosphereConfig.FormatActiveBonusLines(DevelopmentLevel))
            Log.Information("Village development bonus active ({Reason}): {Bonus}", reason, line);
    }

    private static float GetDistance(float x1, float z1, float x2, float z2)
    {
        var dx = x2 - x1;
        var dz = z2 - z1;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}