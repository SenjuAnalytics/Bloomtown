using Bloomtown.Server.Networking;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Crafting;
using Bloomtown.Server.Simulation.Gifting;
using Bloomtown.Server.Simulation.Console;
using Bloomtown.Server.Simulation.Gathering;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Contribution;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Server.Simulation.Leadership;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Legacy;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server;

internal class Program
{
    private const double GameMinutesPerRealSecond = 1.0;

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/server-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("=== Bloomtown Server Starting ===");

        try
        {
            var persistenceOptions = new PersistenceOptions();
            var persistenceService = new PersistenceService(persistenceOptions);
            await persistenceService.InitializeDatabaseAsync();

            var worldTime = new WorldTimeSystem(GameMinutesPerRealSecond);
            await persistenceService.LoadWorldTimeAsync(worldTime);

            worldTime.OnHourAdvanced += hour =>
                Log.Information("Game hour advanced to {Hour:D2}:00", hour);

            worldTime.OnDayRollover += day =>
                Log.Information("New game day began: Day {GameDay}", day);

            var aoiSystem = new AoiSystem();
            var npcManager = new NpcManager();
            npcManager.SpawnDefaults(aoiSystem);
            await persistenceService.ApplyPersistedNpcsAsync(npcManager, aoiSystem);

            var npcLifeSystem = new NpcLifeSystem(
                npcManager,
                worldTime,
                aoiSystem,
                persistenceService,
                GameMinutesPerRealSecond);
            npcLifeSystem.InitializeAfterLoad();

            var reconnectPlayer = await persistenceService.GetReconnectPlayerAsync();
            var nextEntityId = await persistenceService.GetNextPlayerEntityIdAsync();

            var relationshipService = new PlayerNpcRelationshipService(
                persistenceService.RelationshipRepository,
                worldTime);
            var memoryService = new NpcMemoryService(
                persistenceService.MemoryRepository,
                worldTime);
            var economyService = new PlayerEconomyService(
                persistenceService.PlayerRepository,
                persistenceService.InventoryRepository);
            var villageProjectStateService = new VillageProjectStateService(
                persistenceService.CommunityProjectRepository,
                persistenceService.WorldStateRepository);
            var needsService = new PlayerNeedsService(
                economyService,
                worldTime,
                GameMinutesPerRealSecond,
                villageProjectStateService);
            var chestService = new PlayerChestService(persistenceService.ChestRepository);
            var housingService = new PlayerHousingService(
                persistenceService.HousingRepository,
                persistenceService.HomeStorageRepository,
                persistenceService.HouseFurnitureRepository,
                economyService,
                needsService);
            var dailyRhythmTracker = new PlayerDailyRhythmTracker();
            var homeActivityService = new HomeActivityService(
                housingService,
                economyService,
                needsService,
                dailyRhythmTracker,
                worldTime,
                relationshipService);
            var economyHandler = new EconomyTransactionHandler(
                npcManager,
                aoiSystem,
                relationshipService,
                economyService,
                memoryService,
                worldTime);
            var chestHandler = new ChestTransactionHandler(chestService, economyService);
            var craftingService = new CraftingService(economyService);
            var giftingService = new NpcGiftingService(
                npcManager,
                aoiSystem,
                relationshipService,
                economyService,
                needsService,
                memoryService);
            var contributionService = new VillageContributionService(economyService);
            var milestoneService = new VillageMilestoneService(
                persistenceService.MilestoneRepository,
                persistenceService.CommunityProjectRepository,
                economyService,
                needsService);
            await milestoneService.LoadAsync();
            var communityProjectService = new CommunityProjectService(
                persistenceService.CommunityProjectRepository,
                persistenceService.CommunityProjectDefinitionRepository,
                persistenceService.PlayerRepository,
                economyService,
                milestoneService,
                villageProjectStateService,
                contributionService,
                memoryService,
                relationshipService);
            await communityProjectService.LoadDynamicDefinitionsAsync();
            await communityProjectService.LoadAsync();
            await villageProjectStateService.LoadAsync();
            var interpersonalRelationshipService = new NpcInterpersonalRelationshipService(worldTime);
            interpersonalRelationshipService.Reconcile(villageProjectStateService);
            villageProjectStateService.ProjectCompleted += projectId =>
                interpersonalRelationshipService.OnVillageProjectCompleted(projectId, villageProjectStateService);
            var reactivityService = new VillageReactivityService(worldTime);
            reactivityService.Reconcile(villageProjectStateService);
            villageProjectStateService.ProjectCompleted += reactivityService.OnVillageProjectCompleted;
            var communityReputationService = new CommunityReputationService(
                persistenceService.CommunityReputationRepository,
                worldTime);
            var socialDynamicsService = new SocialDynamicsService(worldTime);
            var legacyService = new PlayerLegacyService(
                economyService,
                memoryService,
                communityProjectService,
                worldTime,
                communityReputationService);
            var longTermGoalService = new PlayerLongTermGoalService(
                persistenceService.LongTermGoalRepository,
                economyService,
                communityReputationService,
                relationshipService,
                legacyService,
                communityProjectService,
                worldTime);
            var playerMilestoneService = new PlayerMilestoneService(
                persistenceService.PlayerMilestoneRepository,
                economyService,
                housingService,
                communityReputationService,
                relationshipService,
                worldTime);
            communityProjectService.ConfigureLongTermGoalService(longTermGoalService);
            var homeHandler = new HomeTransactionHandler(
                housingService,
                economyService,
                homeActivityService,
                playerMilestoneService);
            var emotionalBondService = new NpcEmotionalBondService(
                npcManager,
                aoiSystem,
                memoryService,
                relationshipService,
                longTermGoalService,
                needsService,
                economyService,
                worldTime);
            var interactionHandler = new NpcInteractionHandler(
                npcManager,
                aoiSystem,
                relationshipService,
                economyService,
                needsService,
                memoryService,
                villageProjectStateService,
                legacyService,
                communityReputationService,
                socialDynamicsService,
                longTermGoalService,
                emotionalBondService,
                worldTime,
                playerMilestoneService);
            var villageAreaService = new VillageAreaService(
                persistenceService.VillageAreaRepository,
                economyService);
            await villageAreaService.LoadAsync();
            await villageAreaService.ReconcileUnlocksAsync(
                villageProjectStateService.DevelopmentLevel,
                announce: false);
            await milestoneService.ReconcileFromCompletedProjectsAsync();
            var communityProjectHandler = new CommunityProjectHandler(communityProjectService);
            var votingService = new ProjectVotingService(
                persistenceService.ProposalRepository,
                persistenceService.VoteRepository,
                communityProjectService,
                economyService,
                worldTime);
            await votingService.LoadAsync();
            var councilService = new VillageCouncilService(
                persistenceService.ProposalRepository,
                persistenceService.CouncilVoteRepository,
                persistenceService.ChiefAuthorityLogRepository,
                communityProjectService,
                economyService,
                persistenceService.PlayerRepository,
                worldTime);
            await councilService.LoadAsync();
            var positionService = new VillagePositionService(
                persistenceService.PlayerRepository,
                persistenceService.PositionElectionRepository,
                persistenceService.PositionElectionVoteRepository,
                councilService,
                economyService,
                worldTime);
            await positionService.LoadAsync();
            var proposalService = new VillageProjectProposalService(
                persistenceService.ProposalRepository,
                votingService,
                councilService,
                positionService,
                economyService);
            var proposalHandler = new VillageProjectProposalHandler(proposalService);
            var positionHandler = new VillagePositionHandler(positionService);
            var milestoneHandler = new MilestoneInteractionHandler(milestoneService);
            var villageAreaHandler = new VillageAreaInteractionHandler(villageAreaService);
            var playerRoutineService = new PlayerRoutineService(economyService, needsService, worldTime);
            var personalRoutineHandler = new PersonalRoutineInteractionHandler(playerRoutineService);
            NetworkServer networkServer = null!;
            var communityActivityService = new CommunityActivityService(
                economyService,
                needsService,
                villageAreaService,
                villageProjectStateService,
                npcManager,
                aoiSystem,
                interpersonalRelationshipService,
                communityReputationService,
                longTermGoalService,
                memoryService,
                relationshipService,
                worldTime,
                (playerEntityId, npcEntityId, message) =>
                    networkServer.SendNpcAmbientComment(playerEntityId, npcEntityId, message),
                dailyRhythmTracker,
                playerMilestoneService);
            var communityActivityHandler = new CommunityActivityHandler(communityActivityService);
            var dailyRhythmService = new DailyRhythmService(
                economyService,
                needsService,
                dailyRhythmTracker,
                worldTime,
                playerMilestoneService);
            var dailyRhythmHandler = new DailyRhythmHandler(dailyRhythmService);
            var dailyVillageActivityService = new DailyVillageActivityService(
                economyService,
                needsService,
                dailyRhythmTracker,
                worldTime,
                relationshipService,
                playerMilestoneService);
            var dailyVillageActivityHandler = new DailyVillageActivityHandler(dailyVillageActivityService);
            var legacyFocusHandler = new LegacyFocusHandler(longTermGoalService, npcManager, aoiSystem);
            var emotionalBondHandler = new EmotionalBondHandler(emotionalBondService);
            var gatheringHandler = new ResourceGatheringHandler(
                aoiSystem,
                economyService,
                needsService,
                (playerEntityId, response) => networkServer.SendGatheringResponse(playerEntityId, response),
                relationshipService,
                worldTime);
            var gatheringSystem = new GatheringSystem(gatheringHandler, GameMinutesPerRealSecond);
            var energySystem = new PlayerEnergySystem(economyService, needsService, GameMinutesPerRealSecond);
            NpcAmbientCommentSystem? ambientCommentSystem = null;

            var consoleQueryHandler = new ClientConsoleQueryHandler(
                npcManager,
                aoiSystem,
                economyService,
                energySystem,
                housingService,
                gatheringHandler,
                relationshipService,
                villageProjectStateService,
                villageAreaService,
                worldTime,
                playerRoutineService,
                communityActivityService,
                dailyVillageActivityService,
                dailyRhythmService,
                memoryService,
                legacyService,
                interpersonalRelationshipService,
                communityReputationService,
                longTermGoalService,
                playerMilestoneService);

            networkServer = new NetworkServer(
                aoiSystem,
                npcManager,
                interactionHandler,
                economyHandler,
                chestHandler,
                homeHandler,
                communityProjectHandler,
                proposalHandler,
                positionHandler,
                milestoneHandler,
                villageAreaHandler,
                personalRoutineHandler,
                communityActivityHandler,
                dailyVillageActivityHandler,
                dailyRhythmHandler,
                legacyFocusHandler,
                emotionalBondHandler,
                gatheringHandler,
                craftingService,
                giftingService,
                consoleQueryHandler,
                needsService,
                memoryService,
                relationshipService,
                economyService,
                chestService,
                housingService,
                communityReputationService,
                socialDynamicsService,
                longTermGoalService,
                playerMilestoneService,
                persistenceService);
            var villageProjectPassiveSystem = new VillageProjectPassiveSystem(
                villageProjectStateService,
                needsService,
                () => networkServer.GetConnectedPlayerPositions());
            var villageAreaPassiveSystem = new VillageAreaPassiveSystem(
                villageAreaService,
                needsService,
                () => networkServer.GetConnectedPlayerPositions(),
                GameMinutesPerRealSecond);
            var villageBondRecognitionPassiveSystem = new VillageBondRecognitionPassiveSystem(
                housingService,
                relationshipService,
                memoryService,
                needsService,
                () => networkServer.GetConnectedPlayerPositions(),
                GameMinutesPerRealSecond);
            ambientCommentSystem = new NpcAmbientCommentSystem(
                npcManager,
                aoiSystem,
                relationshipService,
                memoryService,
                worldTime,
                () => networkServer.GetConnectedPlayerPositions(),
                networkServer.SendNpcAmbientComment,
                villageProjectStateService,
                villageAreaService,
                legacyService,
                communityReputationService,
                socialDynamicsService,
                longTermGoalService,
                interpersonalRelationshipService,
                reactivityService);
            milestoneService.MilestoneUnlocked += networkServer.BroadcastMilestoneNotification;
            villageProjectStateService.DevelopmentLevelAdvanced += level =>
                villageAreaService.ReconcileUnlocksAsync(level, announce: true).GetAwaiter().GetResult();
            villageAreaService.AreaUnlocked += networkServer.BroadcastVillageAreaNotification;
            votingService.VotingStarted += networkServer.BroadcastProjectProposalNotification;
            votingService.VotingEnded += networkServer.BroadcastProjectProposalNotification;
            councilService.CouncilReviewStarted += networkServer.BroadcastProjectProposalNotification;
            councilService.CouncilReviewEnded += networkServer.BroadcastProjectProposalNotification;
            councilService.ChiefAuthorityUsed += networkServer.BroadcastVillagePositionNotification;
            positionService.ElectionStarted += networkServer.BroadcastVillagePositionNotification;
            positionService.ElectionEnded += networkServer.BroadcastVillagePositionNotification;
            networkServer.ConfigureEntityIdAllocation(nextEntityId, reconnectPlayer);
            networkServer.Start(NetworkConstants.ServerPort);

            var server = new GameServer(
                worldTime,
                aoiSystem,
                npcManager,
                npcLifeSystem,
                gatheringSystem,
                energySystem,
                needsService,
                ambientCommentSystem,
                villageProjectPassiveSystem,
                villageAreaPassiveSystem,
                villageBondRecognitionPassiveSystem,
                relationshipService,
                votingService,
                councilService,
                positionService,
                networkServer,
                persistenceService,
                chestService,
                housingService,
                communityProjectService,
                villageProjectStateService);
            server.Run();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Fatal(ex, "Server crashed unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}