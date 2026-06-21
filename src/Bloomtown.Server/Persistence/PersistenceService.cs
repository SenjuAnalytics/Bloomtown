using Bloomtown.Server.Networking;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Shared.Community;
using Serilog;

namespace Bloomtown.Server.Persistence;

/// <summary>
/// Coordinates loading and saving world time, players, and static NPC data.
/// </summary>
public sealed class PersistenceService
{
    private readonly PersistenceOptions _options;
    private readonly PlayerRepository _playerRepository;
    private readonly NpcRepository _npcRepository;
    private readonly WorldStateRepository _worldStateRepository;
    private readonly PlayerNpcRelationshipRepository _relationshipRepository;
    private readonly PlayerInventoryRepository _inventoryRepository;
    private readonly PlayerChestRepository _chestRepository;
    private readonly PlayerHousingRepository _housingRepository;
    private readonly PlayerHomeStorageRepository _homeStorageRepository;
    private readonly PlayerHouseFurnitureRepository _houseFurnitureRepository;
    private readonly CommunityProjectRepository _communityProjectRepository;
    private readonly CommunityProjectDefinitionRepository _communityProjectDefinitionRepository;
    private readonly VillageProjectProposalRepository _proposalRepository;
    private readonly ProjectVoteRepository _voteRepository;
    private readonly PositionElectionRepository _positionElectionRepository;
    private readonly PositionElectionVoteRepository _positionElectionVoteRepository;
    private readonly CouncilProposalVoteRepository _councilVoteRepository;
    private readonly ChiefAuthorityLogRepository _chiefAuthorityLogRepository;
    private readonly VillageMilestoneRepository _milestoneRepository;
    private readonly PlayerNpcMemoryRepository _memoryRepository;
    private readonly VillageAreaRepository _villageAreaRepository;
    private readonly CommunityReputationRepository _communityReputationRepository;
    private readonly PlayerLongTermGoalRepository _longTermGoalRepository;
    private readonly PlayerMilestoneRepository _playerMilestoneRepository;

    public PersistenceService(PersistenceOptions options)
    {
        _options = options;
        _playerRepository = new PlayerRepository(options.DatabasePath);
        _npcRepository = new NpcRepository(options.DatabasePath);
        _worldStateRepository = new WorldStateRepository(options.DatabasePath);
        _relationshipRepository = new PlayerNpcRelationshipRepository(options.DatabasePath);
        _inventoryRepository = new PlayerInventoryRepository(options.DatabasePath);
        _chestRepository = new PlayerChestRepository(options.DatabasePath);
        _housingRepository = new PlayerHousingRepository(options.DatabasePath);
        _homeStorageRepository = new PlayerHomeStorageRepository(options.DatabasePath);
        _houseFurnitureRepository = new PlayerHouseFurnitureRepository(options.DatabasePath);
        _communityProjectRepository = new CommunityProjectRepository(options.DatabasePath);
        _communityProjectDefinitionRepository = new CommunityProjectDefinitionRepository(options.DatabasePath);
        _proposalRepository = new VillageProjectProposalRepository(options.DatabasePath);
        _voteRepository = new ProjectVoteRepository(options.DatabasePath);
        _positionElectionRepository = new PositionElectionRepository(options.DatabasePath);
        _positionElectionVoteRepository = new PositionElectionVoteRepository(options.DatabasePath);
        _councilVoteRepository = new CouncilProposalVoteRepository(options.DatabasePath);
        _chiefAuthorityLogRepository = new ChiefAuthorityLogRepository(options.DatabasePath);
        _milestoneRepository = new VillageMilestoneRepository(options.DatabasePath);
        _memoryRepository = new PlayerNpcMemoryRepository(options.DatabasePath);
        _villageAreaRepository = new VillageAreaRepository(options.DatabasePath);
        _communityReputationRepository = new CommunityReputationRepository(options.DatabasePath);
        _longTermGoalRepository = new PlayerLongTermGoalRepository(options.DatabasePath);
        _playerMilestoneRepository = new PlayerMilestoneRepository(options.DatabasePath);
    }

    public PlayerRepository PlayerRepository => _playerRepository;
    public PlayerNpcRelationshipRepository RelationshipRepository => _relationshipRepository;
    public PlayerNpcMemoryRepository MemoryRepository => _memoryRepository;
    public PlayerInventoryRepository InventoryRepository => _inventoryRepository;
    public PlayerChestRepository ChestRepository => _chestRepository;
    public PlayerHousingRepository HousingRepository => _housingRepository;
    public PlayerHomeStorageRepository HomeStorageRepository => _homeStorageRepository;
    public PlayerHouseFurnitureRepository HouseFurnitureRepository => _houseFurnitureRepository;
    public CommunityProjectRepository CommunityProjectRepository => _communityProjectRepository;
    public CommunityProjectDefinitionRepository CommunityProjectDefinitionRepository => _communityProjectDefinitionRepository;
    public VillageProjectProposalRepository ProposalRepository => _proposalRepository;
    public ProjectVoteRepository VoteRepository => _voteRepository;
    public PositionElectionRepository PositionElectionRepository => _positionElectionRepository;
    public PositionElectionVoteRepository PositionElectionVoteRepository => _positionElectionVoteRepository;
    public CouncilProposalVoteRepository CouncilVoteRepository => _councilVoteRepository;
    public ChiefAuthorityLogRepository ChiefAuthorityLogRepository => _chiefAuthorityLogRepository;
    public VillageMilestoneRepository MilestoneRepository => _milestoneRepository;
    public WorldStateRepository WorldStateRepository => _worldStateRepository;
    public VillageAreaRepository VillageAreaRepository => _villageAreaRepository;
    public CommunityReputationRepository CommunityReputationRepository => _communityReputationRepository;
    public PlayerLongTermGoalRepository LongTermGoalRepository => _longTermGoalRepository;
    public PlayerMilestoneRepository PlayerMilestoneRepository => _playerMilestoneRepository;

    public TimeSpan AutoSaveInterval => _options.AutoSaveInterval;

    public Task InitializeDatabaseAsync()
    {
        DatabaseInitializer.Initialize(_options);
        Log.Information("SQLite database initialized at {DatabasePath}", _options.DatabasePath);
        return Task.CompletedTask;
    }

    public async Task LoadWorldTimeAsync(WorldTimeSystem worldTime)
    {
        var record = await _worldStateRepository.GetAsync();
        if (record is null)
        {
            Log.Information("No saved world time found. Using default world time.");
            return;
        }

        worldTime.LoadState(record.GameDay, record.GameMinute);
        Log.Information(
            "Loaded world time: Day {GameDay}, minute {GameMinute} (saved at {LastSavedUtc:u})",
            record.GameDay,
            record.GameMinute,
            record.LastSavedUtc);
    }

    public async Task ApplyPersistedNpcsAsync(NpcManager npcManager, AoiSystem aoiSystem)
    {
        var records = await _npcRepository.GetAllAsync();
        if (records.Count == 0)
        {
            Log.Information("No saved NPC state found. Using default NPC spawn values.");
            return;
        }

        npcManager.ApplyPersistedState(records, aoiSystem);
        Log.Information("Loaded {NpcCount} NPC record(s) from database.", records.Count);
    }

    public async Task<uint> GetNextPlayerEntityIdAsync()
    {
        var maxEntityId = await _playerRepository.GetMaxEntityIdAsync();
        return maxEntityId.HasValue ? maxEntityId.Value + 1 : 1;
    }

    public async Task<PlayerRecord?> GetReconnectPlayerAsync()
    {
        return await _playerRepository.GetMostRecentAsync();
    }

    public async Task SavePlayerAsync(PlayerRecord record)
    {
        await _playerRepository.UpsertAsync(record);
        Log.Information(
            "Saved player {EntityId} at ({X:F1}, {Y:F1}, {Z:F1})",
            record.EntityId,
            record.PositionX,
            record.PositionY,
            record.PositionZ);
    }

    public async Task SaveNpcAsync(NpcSimulationState simulation)
    {
        await _npcRepository.UpsertAsync(new NpcRecord
        {
            EntityId = simulation.Npc.EntityId,
            Name = simulation.Npc.Name,
            PositionX = simulation.Npc.PositionX,
            PositionY = simulation.Npc.PositionY,
            PositionZ = simulation.Npc.PositionZ,
            Hunger = simulation.Needs.Hunger,
            Energy = simulation.Needs.Energy,
            Social = simulation.Needs.Social,
        });

        Log.Debug(
            "Saved NPC {NpcName} (entity {EntityId}) pos=({X:F1},{Z:F1}) hunger={Hunger:F0} energy={Energy:F0} social={Social:F0}",
            simulation.Npc.Name,
            simulation.Npc.EntityId,
            simulation.Npc.PositionX,
            simulation.Npc.PositionZ,
            simulation.Needs.Hunger,
            simulation.Needs.Energy,
            simulation.Needs.Social);
    }

    public async Task SaveAllAsync(
        WorldTimeSystem worldTime,
        NetworkServer networkServer,
        NpcManager npcManager,
        PlayerChestService? chestService = null,
        PlayerHousingService? housingService = null,
        CommunityProjectService? communityProjectService = null,
        VillageProjectStateService? villageProjectStateService = null)
    {
        var savedAt = DateTime.UtcNow;

        await _worldStateRepository.UpsertAsync(new WorldStateRecord
        {
            GameDay = worldTime.GameDay,
            GameMinute = worldTime.GameMinute,
            LastSavedUtc = savedAt,
            VillageDevelopmentLevel = villageProjectStateService?.DevelopmentLevel ?? VillageDevelopmentLevel.Quiet,
        });

        foreach (var snapshot in networkServer.GetConnectedPlayerSnapshots())
        {
            await _playerRepository.UpsertAsync(new PlayerRecord
            {
                EntityId = snapshot.EntityId,
                PositionX = snapshot.PositionX,
                PositionY = snapshot.PositionY,
                PositionZ = snapshot.PositionZ,
                RotationYaw = snapshot.RotationYaw,
                LastSeenUtc = savedAt,
                Coins = snapshot.Coins,
                VillageReputation = snapshot.VillageReputation,
                Energy = snapshot.Energy,
                Hunger = snapshot.Hunger,
                Mood = snapshot.Mood,
                Fatigue = snapshot.Fatigue,
                SocialNeed = snapshot.SocialNeed,
                NeedsLastGameMinute = snapshot.NeedsLastGameMinute,
                VillageContributionScore = snapshot.VillageContributionScore,
                VillageTitle = snapshot.VillageTitle,
                VillagePosition = snapshot.VillagePosition,
                PositionAssignedAtUtc = snapshot.PositionAssignedAtUtc,
            });

            await _inventoryRepository.ReplaceInventoryAsync(snapshot.EntityId, snapshot.InventoryStacks);
        }

        if (communityProjectService is not null)
            await communityProjectService.SaveAllAsync();

        if (chestService is not null)
        {
            foreach (var (playerEntityId, stacks) in chestService.GetCachedSnapshots())
                await _chestRepository.ReplaceChestAsync(playerEntityId, stacks);
        }

        if (housingService is not null)
        {
            foreach (var record in housingService.GetCachedHousingRecords())
                await _housingRepository.UpsertAsync(record);

            foreach (var (playerEntityId, stacks) in housingService.GetCachedSnapshots())
                await _homeStorageRepository.ReplaceStorageAsync(playerEntityId, stacks);

            foreach (var (playerEntityId, furniture) in housingService.GetCachedFurnitureSnapshots())
                await _houseFurnitureRepository.ReplaceFurnitureAsync(playerEntityId, furniture);
        }

        foreach (var simulation in npcManager.SimulationStates)
        {
            await _npcRepository.UpsertAsync(new NpcRecord
            {
                EntityId = simulation.Npc.EntityId,
                Name = simulation.Npc.Name,
                PositionX = simulation.Npc.PositionX,
                PositionY = simulation.Npc.PositionY,
                PositionZ = simulation.Npc.PositionZ,
                Hunger = simulation.Needs.Hunger,
                Energy = simulation.Needs.Energy,
                Social = simulation.Needs.Social,
            });
        }

        Log.Information(
            "Persistence snapshot saved at {SavedAt:u} | players={PlayerCount}, npcs={NpcCount}, world=Day {GameDay} {GameMinute}",
            savedAt,
            networkServer.ConnectedPlayerCount,
            npcManager.Npcs.Count,
            worldTime.GameDay,
            worldTime.GameMinute);
    }
}