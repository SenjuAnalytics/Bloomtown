using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Npc;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Tracks Elsie–Tom interpersonal tone and applies rare, one-way evolution.
/// </summary>
public sealed class NpcInterpersonalRelationshipService
{
    private readonly WorldTimeSystem _worldTime;
    private NpcInterpersonalRelationship _elsieTomRelationship =
        NpcInterpersonalRelationshipConfig.DefaultRelationship;

    public NpcInterpersonalRelationshipService(WorldTimeSystem worldTime)
    {
        _worldTime = worldTime;
    }

    public NpcInterpersonalRelationship ElsieTomRelationship => _elsieTomRelationship;

    public bool IsElsieTomFriendly =>
        _elsieTomRelationship == NpcInterpersonalRelationship.Friendly;

    /// <summary>Status-line flavor describing how Elsie and Tom relate socially right now.</summary>
    public string FormatRelationshipStatus() =>
        NpcInterpersonalRelationshipConfig.FormatRelationshipStatus(_elsieTomRelationship);

    /// <summary>Reconciles relationship after load or when village state changes.</summary>
    public void Reconcile(VillageProjectStateService? projectStateService)
    {
        var completedProjects = projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        TryEvolve(completedProjects, trigger: "reconcile");
    }

    /// <summary>Called when a communal project completes — may warm Elsie and Tom's rapport.</summary>
    public void OnVillageProjectCompleted(byte projectId, VillageProjectStateService projectStateService)
    {
        var completedProjects = projectStateService.GetCompletedProjectIds();
        TryEvolve(completedProjects, trigger: $"project {projectId} completed");
    }

    /// <summary>Light time-based check — runs occasionally from ambient updates.</summary>
    public void TryEvolveFromTime(VillageProjectStateService? projectStateService)
    {
        var completedProjects = projectStateService?.GetCompletedProjectIds() ?? Array.Empty<byte>();
        TryEvolve(completedProjects, trigger: "time passage");
    }

    private void TryEvolve(IReadOnlyCollection<byte> completedProjectIds, string trigger)
    {
        var previous = _elsieTomRelationship;
        var next = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            _elsieTomRelationship,
            completedProjectIds,
            _worldTime.GameDay);

        if (next == previous)
            return;

        _elsieTomRelationship = next;
        Log.Information(
            "Elsie–Tom interpersonal relationship evolved {Previous} -> {Current} ({Trigger}).",
            NpcInterpersonalRelationshipConfig.GetDisplayName(previous),
            NpcInterpersonalRelationshipConfig.GetDisplayName(next),
            trigger);
    }
}