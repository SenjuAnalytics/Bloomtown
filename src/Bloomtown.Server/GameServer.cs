using System.Diagnostics;
using Bloomtown.Server.Networking;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Gathering;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Server.Simulation.Leadership;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server;

public class GameServer
{
    private const int TickRate = NetworkConstants.SimTickRate;
    private const double FixedDeltaSeconds = 1.0 / TickRate;

    // Prevent a long hitch from simulating too many catch-up ticks in one frame.
    private const double MaxAccumulatorSeconds = FixedDeltaSeconds * 5;
    private const double MaxFrameSeconds = 0.25;
    private const double MetricsLogIntervalSeconds = 5.0;

    private readonly WorldTimeSystem _worldTime;
    private readonly AoiSystem _aoiSystem;
    private readonly NpcManager _npcManager;
    private readonly NetworkServer _networkServer;
    private readonly PersistenceService _persistenceService;
    private readonly PlayerChestService _chestService;
    private readonly PlayerHousingService _housingService;
    private readonly CommunityProjectService _communityProjectService;
    private readonly VillageProjectStateService? _villageProjectStateService;
    private readonly IReadOnlyList<ISimulationSystem> _systems;

    private long _tickCount;
    private DateTime _lastMetricsLogUtc = DateTime.UtcNow;
    private DateTime _lastAutoSaveUtc = DateTime.UtcNow;

    public GameServer(
        WorldTimeSystem worldTime,
        AoiSystem aoiSystem,
        NpcManager npcManager,
        NpcLifeSystem npcLifeSystem,
        GatheringSystem gatheringSystem,
        PlayerEnergySystem energySystem,
        PlayerNeedsService needsService,
        NpcAmbientCommentSystem ambientCommentSystem,
        VillageProjectPassiveSystem villageProjectPassiveSystem,
        VillageAreaPassiveSystem villageAreaPassiveSystem,
        VillageBondRecognitionPassiveSystem villageBondRecognitionPassiveSystem,
        PlayerNpcRelationshipService relationshipService,
        ProjectVotingService votingService,
        VillageCouncilService councilService,
        VillagePositionService positionService,
        NetworkServer networkServer,
        PersistenceService persistenceService,
        PlayerChestService chestService,
        PlayerHousingService housingService,
        CommunityProjectService communityProjectService,
        VillageProjectStateService? villageProjectStateService = null)
    {
        _worldTime = worldTime;
        _aoiSystem = aoiSystem;
        _npcManager = npcManager;
        _networkServer = networkServer;
        _persistenceService = persistenceService;
        _chestService = chestService;
        _housingService = housingService;
        _communityProjectService = communityProjectService;
        _villageProjectStateService = villageProjectStateService;
        _systems = new ISimulationSystem[] { worldTime, npcLifeSystem, gatheringSystem, energySystem, needsService, ambientCommentSystem, villageProjectPassiveSystem, villageAreaPassiveSystem, villageBondRecognitionPassiveSystem, relationshipService, votingService, councilService, positionService };
    }

    public void Run(CancellationToken cancellationToken = default)
    {
        using var shutdownCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Log.Information("Shutdown signal received (Ctrl+C). Stopping gracefully...");
            shutdownCts.Cancel();
        };

        Log.Information(
            "GameServer started. Tick rate: {TickRate} Hz, fixed dt: {FixedDeltaMs} ms",
            TickRate,
            FixedDeltaSeconds * 1000);

        try
        {
            RunLoop(shutdownCts.Token);
            Log.Information("Server stopped gracefully after {TickCount} simulation ticks.", _tickCount);
        }
        catch (OperationCanceledException) when (shutdownCts.IsCancellationRequested)
        {
            Log.Information("Server shutdown complete after {TickCount} simulation ticks.", _tickCount);
        }
        finally
        {
            SavePersistenceSnapshot("shutdown");
            _networkServer.Stop();
        }
    }

    private void RunLoop(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        double previousElapsedSeconds = 0;
        double accumulatorSeconds = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            _networkServer.PollEvents();

            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var frameSeconds = elapsedSeconds - previousElapsedSeconds;
            previousElapsedSeconds = elapsedSeconds;

            if (frameSeconds > MaxFrameSeconds)
                frameSeconds = MaxFrameSeconds;

            accumulatorSeconds += frameSeconds;

            if (accumulatorSeconds > MaxAccumulatorSeconds)
                accumulatorSeconds = MaxAccumulatorSeconds;

            while (accumulatorSeconds >= FixedDeltaSeconds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Tick(FixedDeltaSeconds);
                accumulatorSeconds -= FixedDeltaSeconds;
            }

            Thread.Sleep(1);
        }
    }

    private void Tick(double deltaTimeSeconds)
    {
        _tickCount++;

        foreach (var system in _systems)
            system.Update(deltaTimeSeconds);

        _networkServer.Update(deltaTimeSeconds);
        _networkServer.SyncPositionsToAoi();
        _aoiSystem.Update(deltaTimeSeconds);
        _networkServer.BroadcastPositions();

        MaybeAutoSave();
        MaybeLogMetrics();
    }

    private void MaybeAutoSave()
    {
        var now = DateTime.UtcNow;
        if (now - _lastAutoSaveUtc < _persistenceService.AutoSaveInterval)
            return;

        _lastAutoSaveUtc = now;
        SavePersistenceSnapshot("auto-save");
    }

    private void SavePersistenceSnapshot(string reason)
    {
        try
        {
            _persistenceService
                .SaveAllAsync(
                    _worldTime,
                    _networkServer,
                    _npcManager,
                    _chestService,
                    _housingService,
                    _communityProjectService,
                    _villageProjectStateService)
                .GetAwaiter()
                .GetResult();

            Log.Information("Persistence save completed ({Reason}).", reason);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Persistence save failed ({Reason}).", reason);
        }
    }

    private void MaybeLogMetrics()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastMetricsLogUtc).TotalSeconds < MetricsLogIntervalSeconds)
            return;

        _lastMetricsLogUtc = now;

        Log.Information(
            "Sim metrics | ticks: {TickCount} | game time: Day {GameDay} {Hour:D2}:{Minute:D2}",
            _tickCount,
            _worldTime.GameDay,
            _worldTime.GameHour,
            _worldTime.MinuteOfHour);

        LogAoiState();
    }

    private void LogAoiState()
    {
        LogEntityAoiState(_networkServer.GetConnectedEntityIds());
        LogEntityAoiState(_networkServer.GetNpcEntityIds());
    }

    private void LogEntityAoiState(IEnumerable<uint> entityIds)
    {
        foreach (var entityId in entityIds)
        {
            var replicationView = _aoiSystem.GetReplicationView(entityId);
            if (replicationView is null)
                continue;

            var viewers = string.Join(", ", replicationView.ViewingPlayers);
            Log.Information(
                "AOI state | entity {EntityId} | viewers [{Viewers}]",
                entityId,
                viewers);
        }
    }
}