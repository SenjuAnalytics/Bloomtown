using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Tracks recent project completions for boosted village growth reactions.
/// </summary>
public sealed class VillageReactivityService
{
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<byte, long> _completedAtGameMinute = new();

    public VillageReactivityService(WorldTimeSystem worldTime)
    {
        _worldTime = worldTime;
    }

    public void Reconcile(VillageProjectStateService projectStateService)
    {
        _ = projectStateService;
        _completedAtGameMinute.Clear();
    }

    public void OnVillageProjectCompleted(byte projectId)
    {
        _completedAtGameMinute[projectId] = _worldTime.TotalGameMinutes;
        Log.Information(
            "Village reactivity: project {ProjectId} entered completion reaction window ({WindowMinutes} game min).",
            projectId,
            VillageReactivityConfig.ProjectCompletionReactionWindowGameMinutes);
    }

    public IReadOnlyList<byte> GetProjectsInReactionWindow()
    {
        var currentMinute = _worldTime.TotalGameMinutes;
        var results = new List<byte>();

        foreach (var (projectId, completedAtMinute) in _completedAtGameMinute)
        {
            if (VillageReactivityConfig.IsWithinCompletionReactionWindow(completedAtMinute, currentMinute))
                results.Add(projectId);
        }

        return results;
    }

    public bool TryGetMostRecentReactionProject(out byte projectId)
    {
        projectId = 0;
        var inWindow = GetProjectsInReactionWindow();
        if (inWindow.Count == 0)
            return false;

        projectId = inWindow
            .OrderByDescending(id => _completedAtGameMinute.GetValueOrDefault(id, 0))
            .First();

        return true;
    }
}