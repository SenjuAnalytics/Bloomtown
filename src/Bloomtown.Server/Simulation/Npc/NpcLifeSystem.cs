using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Npc.Ai;
using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;
using Bloomtown.Server.Simulation.Npc.Utility;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Advances NPC needs, utility-based decisions, movement, AOI sync, and persistence hooks.
/// </summary>
public sealed class NpcLifeSystem : ISimulationSystem
{
    private readonly NpcManager _npcManager;
    private readonly WorldTimeSystem _worldTime;
    private readonly AoiSystem _aoiSystem;
    private readonly AoiGrid _aoiGrid = new();
    private readonly PersistenceService _persistenceService;
    private readonly NpcUtilityScorer _utilityScorer = new();
    private readonly double _gameMinutesPerRealSecond;
    private readonly Dictionary<uint, NpcLifeState> _lifeStates = new();

    public NpcLifeSystem(
        NpcManager npcManager,
        WorldTimeSystem worldTime,
        AoiSystem aoiSystem,
        PersistenceService persistenceService,
        double gameMinutesPerRealSecond = 1.0)
    {
        _npcManager = npcManager;
        _worldTime = worldTime;
        _aoiSystem = aoiSystem;
        _persistenceService = persistenceService;
        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;

        foreach (var state in _npcManager.SimulationStates)
            _lifeStates[state.Npc.EntityId] = new NpcLifeState();
    }

    public void Update(double deltaTimeSeconds)
    {
        var deltaGameMinutes = deltaTimeSeconds * _gameMinutesPerRealSecond;

        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (!_lifeStates.TryGetValue(simulation.Npc.EntityId, out var lifeState))
            {
                lifeState = new NpcLifeState();
                _lifeStates[simulation.Npc.EntityId] = lifeState;
            }

            simulation.Needs.Decay(deltaGameMinutes);
            lifeState.MinutesInCurrentActivity += deltaGameMinutes;
            lifeState.MinutesSinceLastEvaluation += deltaGameMinutes;

            var shouldEvaluate = lifeState.ForceNextEvaluation ||
                lifeState.MinutesSinceLastEvaluation >= UtilityScoringConfig.EvaluationIntervalGameMinutes;

            var context = BuildEvaluationContext(simulation);
            var selection = _utilityScorer.Evaluate(
                in context,
                simulation.CurrentActivity,
                lifeState.MinutesInCurrentActivity,
                shouldEvaluate);

            if (shouldEvaluate)
            {
                lifeState.MinutesSinceLastEvaluation = 0;
                lifeState.ForceNextEvaluation = false;
            }

            if (selection.ActivityChanged)
            {
                simulation.CurrentActivity = selection.SelectedActivity;
                ApplyActivityMovement(simulation, selection.SelectedActivity);
                lifeState.WasAtDestination = false;
                lifeState.MinutesInCurrentActivity = 0;

                if (selection.SelectedActivity is NpcActivityType.Eat or NpcActivityType.Rest)
                    ApplyActivityEffects(simulation);

                SaveNpcState(simulation);

                var runnerUpName = selection.RunnerUpActivity?.ToString() ?? "none";
                var runnerUpScore = selection.RunnerUpScore ?? 0f;

                Log.Information(
                    "NPC {NpcName} chose {Activity} (score: {Score:F0}) over {RunnerUp} (score: {RunnerUpScore:F0}) because {Reason}",
                    simulation.Npc.Name,
                    selection.SelectedActivity,
                    selection.SelectedScore,
                    runnerUpName,
                    runnerUpScore,
                    selection.BuildLogReason());
            }

            var arrived = simulation.Movement.Update(simulation.Npc, deltaTimeSeconds);
            if (TrySyncAoiPosition(simulation.Npc, lifeState))
                lifeState.LastAoiSyncUtc = DateTime.UtcNow;

            if (!arrived)
            {
                lifeState.WasAtDestination = false;
                continue;
            }

            lifeState.WasAtDestination = true;
        }
    }

    public void InitializeAfterLoad()
    {
        foreach (var simulation in _npcManager.SimulationStates)
        {
            if (!_lifeStates.TryGetValue(simulation.Npc.EntityId, out var lifeState))
            {
                lifeState = new NpcLifeState();
                _lifeStates[simulation.Npc.EntityId] = lifeState;
            }

            lifeState.ForceNextEvaluation = true;
            lifeState.MinutesSinceLastEvaluation = UtilityScoringConfig.EvaluationIntervalGameMinutes;
            lifeState.MinutesInCurrentActivity = 0;

            var context = BuildEvaluationContext(simulation);
            var selection = _utilityScorer.Evaluate(
                in context,
                simulation.CurrentActivity,
                lifeState.MinutesInCurrentActivity,
                shouldEvaluate: true);

            simulation.CurrentActivity = selection.SelectedActivity;
            ApplyActivityMovement(simulation, selection.SelectedActivity);
            lifeState.MinutesSinceLastEvaluation = 0;
            lifeState.ForceNextEvaluation = false;

            Log.Information(
                "NPC {NpcName} initial activity {Activity} (score: {Score:F0}) because {Reason}",
                simulation.Npc.Name,
                selection.SelectedActivity,
                selection.SelectedScore,
                selection.BuildLogReason());

            _aoiSystem.SetEntityPosition(
                simulation.Npc.EntityId,
                simulation.Npc.PositionX,
                simulation.Npc.PositionZ);

            lifeState.LastSyncedCell = _aoiGrid.WorldToCell(simulation.Npc.PositionX, simulation.Npc.PositionZ);
            lifeState.LastAoiSyncUtc = DateTime.UtcNow;
        }
    }

    private UtilityEvaluationContext BuildEvaluationContext(NpcSimulationState simulation)
    {
        return new UtilityEvaluationContext(
            simulation.Npc.EntityId,
            simulation.Npc.PositionX,
            simulation.Npc.PositionZ,
            simulation.Needs,
            simulation.Schedule,
            simulation.Personality,
            _worldTime.GameMinute);
    }

    private static void ApplyActivityMovement(NpcSimulationState simulation, NpcActivityType activity)
    {
        var path = NpcActivityLocations.GetActivityPath(simulation.Npc.EntityId, activity);
        simulation.Movement.SetPatrol(activity, path);
        simulation.Movement.SyncTargetToNearestWaypoint(simulation.Npc.PositionX, simulation.Npc.PositionZ);
    }

    private static void ApplyActivityEffects(NpcSimulationState simulation)
    {
        switch (simulation.CurrentActivity)
        {
            case NpcActivityType.Eat:
                simulation.Needs.SatisfyHunger(NpcNeedsConfig.EatHungerReduction);
                Log.Information(
                    "NPC {NpcName} ate and reduced Hunger to {Hunger:F0}",
                    simulation.Npc.Name,
                    simulation.Needs.Hunger);
                break;

            case NpcActivityType.Rest:
                simulation.Needs.RestoreEnergy(NpcNeedsConfig.RestEnergyRecovery);
                Log.Information(
                    "NPC {NpcName} rested and restored Energy to {Energy:F0}",
                    simulation.Npc.Name,
                    simulation.Needs.Energy);
                break;
        }
    }

    private bool TrySyncAoiPosition(StaticNpc npc, NpcLifeState lifeState)
    {
        var currentCell = _aoiGrid.WorldToCell(npc.PositionX, npc.PositionZ);
        var elapsed = DateTime.UtcNow - lifeState.LastAoiSyncUtc;

        if (currentCell == lifeState.LastSyncedCell &&
            elapsed.TotalSeconds < Movement.NpcMovementConfig.AoiSyncIntervalSeconds)
        {
            return false;
        }

        _aoiSystem.SetEntityPosition(npc.EntityId, npc.PositionX, npc.PositionZ);
        lifeState.LastSyncedCell = currentCell;
        return true;
    }

    private void SaveNpcState(NpcSimulationState simulation)
    {
        try
        {
            _persistenceService.SaveNpcAsync(simulation).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save NPC {EntityId} state.", simulation.Npc.EntityId);
        }
    }

    private sealed class NpcLifeState
    {
        public CellCoord LastSyncedCell { get; set; }
        public DateTime LastAoiSyncUtc { get; set; } = DateTime.MinValue;
        public bool WasAtDestination { get; set; }
        public double MinutesSinceLastEvaluation { get; set; }
        public double MinutesInCurrentActivity { get; set; }
        public bool ForceNextEvaluation { get; set; }
    }
}