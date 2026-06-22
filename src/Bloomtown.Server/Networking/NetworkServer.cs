using System.Net;
using System.Net.Sockets;
using Bloomtown.Server.Persistence;
using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Npc;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Crafting;
using Bloomtown.Server.Simulation.Gifting;
using Bloomtown.Server.Simulation.Console;
using Bloomtown.Server.Simulation.Gathering;
using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Milestone;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Server.Simulation.Village;
using Bloomtown.Server.Simulation.Proposal;
using Bloomtown.Server.Simulation.Leadership;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Npc.Interaction;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Leadership;
using Bloomtown.Shared.Protocol;
using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;

namespace Bloomtown.Server.Networking;

public sealed class NetworkServer : INetEventListener
{
    private const int PlayerInputPacketSize = 17;
    private const int EntityDeltaPacketSize = 25;

    private readonly AoiSystem _aoiSystem;
    private readonly NpcManager _npcManager;
    private readonly NpcInteractionHandler _interactionHandler;
    private readonly EconomyTransactionHandler _economyHandler;
    private readonly ChestTransactionHandler _chestHandler;
    private readonly HomeTransactionHandler _homeHandler;
    private readonly CommunityProjectHandler _communityProjectHandler;
    private readonly VillageProjectProposalHandler _proposalHandler;
    private readonly VillagePositionHandler _positionHandler;
    private readonly MilestoneInteractionHandler _milestoneHandler;
    private readonly VillageAreaInteractionHandler _villageAreaHandler;
    private readonly PersonalRoutineInteractionHandler _personalRoutineHandler;
    private readonly CommunityActivityHandler _communityActivityHandler;
    private readonly DailyVillageActivityHandler _dailyVillageActivityHandler;
    private readonly DailyRhythmHandler _dailyRhythmHandler;
    private readonly LegacyFocusHandler _legacyFocusHandler;
    private readonly EmotionalBondHandler _emotionalBondHandler;
    private readonly ResourceGatheringHandler _gatheringHandler;
    private readonly CraftingService _craftingService;
    private readonly NpcGiftingService _giftingService;
    private readonly ClientConsoleQueryHandler _consoleQueryHandler;
    private readonly PlayerNeedsService _needsService;
    private readonly NpcMemoryService _memoryService;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerChestService _chestService;
    private readonly PlayerHousingService _housingService;
    private readonly CommunityReputationService _communityReputationService;
    private readonly SocialDynamicsService _socialDynamicsService;
    private readonly PlayerLongTermGoalService _longTermGoalService;
    private readonly PlayerMilestoneService _playerMilestoneService;
    private readonly PersistenceService _persistenceService;
    private readonly NetManager _netManager;
    private readonly Dictionary<int, ConnectedPlayer> _players = new();
    private readonly Dictionary<uint, ConnectedPlayer> _playersByEntityId = new();
    private PlayerRecord? _pendingReconnectPlayer;
    private uint _nextEntityId = 1;
    private uint _stateSeq;

    public int ConnectedPlayerCount => _players.Count;

    public NetworkServer(
        AoiSystem aoiSystem,
        NpcManager npcManager,
        NpcInteractionHandler interactionHandler,
        EconomyTransactionHandler economyHandler,
        ChestTransactionHandler chestHandler,
        HomeTransactionHandler homeHandler,
        CommunityProjectHandler communityProjectHandler,
        VillageProjectProposalHandler proposalHandler,
        VillagePositionHandler positionHandler,
        MilestoneInteractionHandler milestoneHandler,
        VillageAreaInteractionHandler villageAreaHandler,
        PersonalRoutineInteractionHandler personalRoutineHandler,
        CommunityActivityHandler communityActivityHandler,
        DailyVillageActivityHandler dailyVillageActivityHandler,
        DailyRhythmHandler dailyRhythmHandler,
        LegacyFocusHandler legacyFocusHandler,
        EmotionalBondHandler emotionalBondHandler,
        ResourceGatheringHandler gatheringHandler,
        CraftingService craftingService,
        NpcGiftingService giftingService,
        ClientConsoleQueryHandler consoleQueryHandler,
        PlayerNeedsService needsService,
        NpcMemoryService memoryService,
        PlayerNpcRelationshipService relationshipService,
        PlayerEconomyService economyService,
        PlayerChestService chestService,
        PlayerHousingService housingService,
        CommunityReputationService communityReputationService,
        SocialDynamicsService socialDynamicsService,
        PlayerLongTermGoalService longTermGoalService,
        PlayerMilestoneService playerMilestoneService,
        PersistenceService persistenceService)
    {
        _aoiSystem = aoiSystem;
        _npcManager = npcManager;
        _interactionHandler = interactionHandler;
        _economyHandler = economyHandler;
        _chestHandler = chestHandler;
        _homeHandler = homeHandler;
        _communityProjectHandler = communityProjectHandler;
        _proposalHandler = proposalHandler;
        _positionHandler = positionHandler;
        _milestoneHandler = milestoneHandler;
        _villageAreaHandler = villageAreaHandler;
        _personalRoutineHandler = personalRoutineHandler;
        _communityActivityHandler = communityActivityHandler;
        _dailyVillageActivityHandler = dailyVillageActivityHandler;
        _dailyRhythmHandler = dailyRhythmHandler;
        _legacyFocusHandler = legacyFocusHandler;
        _emotionalBondHandler = emotionalBondHandler;
        _gatheringHandler = gatheringHandler;
        _craftingService = craftingService;
        _giftingService = giftingService;
        _consoleQueryHandler = consoleQueryHandler;
        _needsService = needsService;
        _memoryService = memoryService;
        _relationshipService = relationshipService;
        _economyService = economyService;
        _chestService = chestService;
        _housingService = housingService;
        _communityReputationService = communityReputationService;
        _socialDynamicsService = socialDynamicsService;
        _longTermGoalService = longTermGoalService;
        _playerMilestoneService = playerMilestoneService;
        _persistenceService = persistenceService;
        _netManager = new NetManager(this)
        {
            AutoRecycle = true,
            IPv6Enabled = false,
        };
    }

    public void Start(int port = NetworkConstants.ServerPort)
    {
        if (!_netManager.Start(port))
            throw new InvalidOperationException($"Failed to start network server on port {port}.");

        Log.Information("Network server listening on UDP port {Port}", port);
    }

    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    public void SyncPositionsToAoi()
    {
        foreach (var player in _players.Values)
            _aoiSystem.SetEntityPosition(player.EntityId, player.PositionX, player.PositionZ);
    }

    public IEnumerable<uint> GetConnectedEntityIds()
    {
        foreach (var player in _players.Values)
            yield return player.EntityId;
    }

    public IEnumerable<uint> GetNpcEntityIds()
    {
        foreach (var npc in _npcManager.Npcs)
            yield return npc.EntityId;
    }

    public IReadOnlyList<(uint EntityId, float X, float Z)> GetConnectedPlayerPositions()
    {
        return _players.Values
            .Select(player => (player.EntityId, player.PositionX, player.PositionZ))
            .ToList();
    }

    public void SendNpcAmbientComment(uint playerEntityId, uint npcEntityId, string message)
    {
        if (!_playersByEntityId.TryGetValue(playerEntityId, out var player))
            return;

        var buffer = new byte[PacketSerializer.NpcAmbientCommentHeaderSize + PacketSerializer.MaxNpcAmbientCommentBytes];
        var length = PacketSerializer.WriteNpcAmbientComment(buffer, npcEntityId, message);
        player.Peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Debug(
            "Sent ambient NPC comment to player {PlayerId} from NPC {NpcId}: {Message}",
            playerEntityId,
            npcEntityId,
            message);
    }

    public IEnumerable<PlayerPersistenceSnapshot> GetConnectedPlayerSnapshots()
    {
        foreach (var player in _players.Values)
        {
            var coins = EconomyConfig.StartingCoins;
            var villageReputation = 0;
            var energy = VillageMilestoneConfig.DefaultPlayerEnergy;
            var hunger = PlayerHungerConfig.DefaultHunger;
            var mood = PlayerNeedsConfig.DefaultMood;
            var fatigue = PlayerNeedsConfig.DefaultFatigue;
            var socialNeed = PlayerNeedsConfig.DefaultSocialNeed;
            var needsLastGameMinute = 0L;
            var contributionScore = 0;
            var villageTitle = VillageTitle.Newcomer;
            var villagePosition = VillagePosition.None;
            DateTime? positionAssignedAt = null;
            IReadOnlyList<ItemStack> stacks = Array.Empty<ItemStack>();

            if (_economyService.TryGetState(player.EntityId, out var economy))
            {
                coins = economy.Coins;
                villageReputation = economy.VillageReputation;
                energy = economy.Energy;
                hunger = economy.Hunger;
                mood = economy.Mood;
                fatigue = economy.Fatigue;
                socialNeed = economy.SocialNeed;
                needsLastGameMinute = economy.LastNeedsUpdateTotalGameMinute;
                contributionScore = economy.VillageContributionScore;
                villageTitle = economy.VillageTitle;
                villagePosition = economy.VillagePosition;
                positionAssignedAt = economy.PositionAssignedAtUtc;
                stacks = economy.Inventory.ToStacks().ToList();
            }

            yield return new PlayerPersistenceSnapshot(
                player.EntityId,
                player.PositionX,
                player.PositionY,
                player.PositionZ,
                player.RotationYaw,
                coins,
                villageReputation,
                energy,
                hunger,
                mood,
                fatigue,
                socialNeed,
                needsLastGameMinute,
                contributionScore,
                villageTitle,
                villagePosition,
                positionAssignedAt,
                stacks);
        }
    }

    public void ConfigureEntityIdAllocation(uint nextEntityId, PlayerRecord? reconnectPlayer)
    {
        _nextEntityId = nextEntityId;
        _pendingReconnectPlayer = reconnectPlayer;
    }

    public void Update(double deltaTimeSeconds)
    {
        foreach (var player in _players.Values)
        {
            var input = player.LastInput;
            var dirX = input.MoveDirX;
            var dirY = input.MoveDirY;
            var magnitude = MathF.Sqrt(dirX * dirX + dirY * dirY);

            if (magnitude <= 0.0001f)
                continue;

            dirX /= magnitude;
            dirY /= magnitude;

            player.RotationYaw = input.LookYaw;

            // Client mengirim arah gerak sudah dalam world-space (X/Z).
            var distance = NetworkConstants.PlayerMoveSpeed * (float)deltaTimeSeconds;
            player.PositionX += dirX * distance;
            player.PositionZ += dirY * distance;
        }
    }

    public void BroadcastPositions()
    {
        if (_players.Count == 0)
            return;

        _stateSeq++;
        var packetBuffer = new byte[EntityDeltaPacketSize];

        foreach (var sourcePlayer in _players.Values)
        {
            BroadcastEntity(
                sourcePlayer.EntityId,
                sourcePlayer.PositionX,
                sourcePlayer.PositionY,
                sourcePlayer.PositionZ,
                sourcePlayer.RotationYaw,
                packetBuffer);
        }

        foreach (var npc in _npcManager.Npcs)
        {
            BroadcastEntity(
                npc.EntityId,
                npc.PositionX,
                npc.PositionY,
                npc.PositionZ,
                npc.RotationYaw,
                packetBuffer);
        }
    }

    private void BroadcastEntity(
        uint entityId,
        float positionX,
        float positionY,
        float positionZ,
        float rotationYaw,
        byte[] packetBuffer)
    {
        var replicationView = _aoiSystem.GetReplicationView(entityId);
        if (replicationView is null)
            return;

        var delta = new EntityDelta
        {
            EntityId = entityId,
            Seq = _stateSeq,
            PositionX = positionX,
            PositionY = positionY,
            PositionZ = positionZ,
            RotationYaw = rotationYaw,
        };

        var length = PacketSerializer.WriteEntityDelta(packetBuffer, delta);

        foreach (var viewerEntityId in replicationView.ViewingPlayers)
        {
            if (viewerEntityId == entityId)
                continue;

            if (!_playersByEntityId.TryGetValue(viewerEntityId, out var viewer))
                continue;

            viewer.Peer.Send(packetBuffer, 0, length, DeliveryMethod.Unreliable);
        }
    }

    public void Stop()
    {
        _netManager.Stop();
        _players.Clear();
        _playersByEntityId.Clear();
        Log.Information("Network server stopped.");
    }

    public void OnPeerConnected(NetPeer peer)
    {
        uint entityId;
        float spawnX;
        float spawnY;
        float spawnZ;
        float rotationYaw;

        var reconnect = ResolveReconnectPlayer();
        if (reconnect is not null)
        {
            entityId = reconnect.EntityId;
            spawnX = reconnect.PositionX;
            spawnY = reconnect.PositionY;
            spawnZ = reconnect.PositionZ;
            rotationYaw = reconnect.RotationYaw;

            Log.Information(
                "Restored player entity {EntityId} from persistence at ({X:F1}, {Y:F1}, {Z:F1})",
                entityId,
                spawnX,
                spawnY,
                spawnZ);
        }
        else
        {
            entityId = _nextEntityId++;
            var spawnIndex = _players.Count;
            spawnX = NetworkConstants.DefaultSpawnX + (spawnIndex % 3 - 1) * 2f;
            spawnY = NetworkConstants.DefaultSpawnY;
            spawnZ = NetworkConstants.DefaultSpawnZ + spawnIndex / 3 * 2f;
            rotationYaw = 0f;
        }

        var player = new ConnectedPlayer
        {
            Peer = peer,
            EntityId = entityId,
            PositionX = spawnX,
            PositionY = spawnY,
            PositionZ = spawnZ,
            RotationYaw = rotationYaw,
        };

        _players[peer.Id] = player;
        _playersByEntityId[entityId] = player;

        var isReconnect = reconnect is not null;
        _relationshipService.LoadPlayerAsync(entityId).GetAwaiter().GetResult();
        _memoryService.LoadPlayerAsync(entityId).GetAwaiter().GetResult();
        _economyService.LoadPlayerAsync(entityId, isReconnect).GetAwaiter().GetResult();
        _chestService.LoadPlayerAsync(entityId).GetAwaiter().GetResult();
        _housingService.LoadPlayerAsync(entityId).GetAwaiter().GetResult();
        _communityReputationService.LoadAsync(entityId).GetAwaiter().GetResult();
        _longTermGoalService.LoadAsync(entityId).GetAwaiter().GetResult();
        _longTermGoalService.ReconcileAsync(entityId).GetAwaiter().GetResult();
        _playerMilestoneService.LoadAsync(entityId).GetAwaiter().GetResult();
        _playerMilestoneService.ReconcileAsync(entityId).GetAwaiter().GetResult();
        _needsService.OnPlayerConnected(entityId);

        _aoiSystem.RegisterEntity(entityId, EntityKind.Player, spawnX, spawnZ);
        Log.Information(
            "AOI registered player entity {EntityId} at world ({X:F1}, {Z:F1})",
            entityId,
            spawnX,
            spawnZ);

        var acceptBuffer = new byte[17];
        var accept = new ConnectAccept
        {
            EntityId = entityId,
            SpawnX = spawnX,
            SpawnY = spawnY,
            SpawnZ = spawnZ,
        };
        var length = PacketSerializer.WriteConnectAccept(acceptBuffer, accept);
        peer.Send(acceptBuffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Information(
            "Client connected: peer={PeerId}, entity={EntityId}, spawn=({X:F1}, {Y:F1}, {Z:F1})",
            peer.Id,
            entityId,
            spawnX,
            0f,
            spawnZ);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (_players.Remove(peer.Id, out var player))
        {
            _playersByEntityId.Remove(player.EntityId);
            _relationshipService.UnloadPlayer(player.EntityId);
            _memoryService.UnloadPlayer(player.EntityId);
            _needsService.ClearPlayer(player.EntityId);

            _economyService.SavePlayerAsync(
                player.EntityId,
                new PlayerRecord
                {
                    EntityId = player.EntityId,
                    PositionX = player.PositionX,
                    PositionY = player.PositionY,
                    PositionZ = player.PositionZ,
                    RotationYaw = player.RotationYaw,
                    LastSeenUtc = DateTime.UtcNow,
                }).GetAwaiter().GetResult();

            _chestService.SavePlayerAsync(player.EntityId).GetAwaiter().GetResult();
            _housingService.SavePlayerAsync(player.EntityId).GetAwaiter().GetResult();
            _economyService.UnloadPlayer(player.EntityId);
            _chestService.UnloadPlayer(player.EntityId);
            _housingService.UnloadPlayer(player.EntityId);
            _communityReputationService.UnloadPlayer(player.EntityId);
            _socialDynamicsService.UnloadPlayer(player.EntityId);
            _longTermGoalService.UnloadPlayer(player.EntityId);
            _playerMilestoneService.UnloadPlayer(player.EntityId);
            _gatheringHandler.CancelPlayerSession(player.EntityId);
            _aoiSystem.RemoveEntity(player.EntityId);

            Log.Information(
                "Client disconnected: peer={PeerId}, entity={EntityId}, reason={Reason}",
                peer.Id,
                player.EntityId,
                disconnectInfo.Reason);
            Log.Information("AOI removed player entity {EntityId}", player.EntityId);
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        try
        {
            if (reader.AvailableBytes < 1)
                return;

            var packetType = (PacketType)reader.GetByte();

            switch (packetType)
            {
                case PacketType.PlayerInput:
                    HandlePlayerInput(peer, reader);
                    break;

                case PacketType.NpcInteractionRequest:
                    HandleNpcInteractionRequest(peer, reader);
                    break;

                case PacketType.EconomyRequest:
                    HandleEconomyRequest(peer, reader);
                    break;

                case PacketType.GatheringRequest:
                    HandleGatheringRequest(peer, reader);
                    break;

                case PacketType.CraftingRequest:
                    HandleCraftingRequest(peer, reader);
                    break;

                case PacketType.GiftRequest:
                    HandleGiftRequest(peer, reader);
                    break;

                case PacketType.ClientQueryRequest:
                    HandleClientQueryRequest(peer, reader);
                    break;

                case PacketType.ChestRequest:
                    HandleChestRequest(peer, reader);
                    break;

                case PacketType.CommunityProjectRequest:
                    HandleCommunityProjectRequest(peer, reader);
                    break;

                case PacketType.MilestoneRequest:
                    HandleMilestoneRequest(peer, reader);
                    break;

                case PacketType.VillageAreaRequest:
                    HandleVillageAreaRequest(peer, reader);
                    break;

                case PacketType.PersonalRoutineRequest:
                    HandlePersonalRoutineRequest(peer, reader);
                    break;

                case PacketType.CommunityActivityRequest:
                    HandleCommunityActivityRequest(peer, reader);
                    break;

                case PacketType.DailyVillageActivityRequest:
                    HandleDailyVillageActivityRequest(peer, reader);
                    break;

                case PacketType.DailyRhythmRequest:
                    HandleDailyRhythmRequest(peer, reader);
                    break;

                case PacketType.LegacyFocusRequest:
                    HandleLegacyFocusRequest(peer, reader);
                    break;

                case PacketType.EmotionalBondRequest:
                    HandleEmotionalBondRequest(peer, reader);
                    break;

                case PacketType.ProjectProposalRequest:
                    HandleProjectProposalRequest(peer, reader);
                    break;

                case PacketType.VillagePositionRequest:
                    HandleVillagePositionRequest(peer, reader);
                    break;

                case PacketType.HomeRequest:
                    HandleHomeRequest(peer, reader);
                    break;
            }
        }
        finally
        {
            reader.Recycle();
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Log.Warning("Network error from {EndPoint}: {SocketError}", endPoint, socketError);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        reader.Recycle();
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        // AcceptIfKey reads request.Data internally — do not read it beforehand.
        if (request.AcceptIfKey(NetworkConstants.ConnectionKey) is null)
            Log.Warning("Connection rejected: invalid connection key from {EndPoint}", request.RemoteEndPoint);
    }

    private void HandlePlayerInput(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PlayerInputPacketSize - 1)
            return;

        var packet = new byte[PlayerInputPacketSize];
        packet[0] = (byte)PacketType.PlayerInput;
        reader.GetBytes(packet, 1, PlayerInputPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        player.LastInput = PacketSerializer.ReadPlayerInput(packet);
    }

    private void HandleNpcInteractionRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.NpcInteractionRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.NpcInteractionRequestPacketSize];
        packet[0] = (byte)PacketType.NpcInteractionRequest;
        reader.GetBytes(packet, 1, PacketSerializer.NpcInteractionRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadNpcInteractionRequest(packet);
        var response = _interactionHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendInteractionResponse(peer, response);
    }

    private static void SendInteractionResponse(NetPeer peer, NpcInteractionResponse response)
    {
        var buffer = new byte[PacketSerializer.NpcInteractionResponseHeaderSize + PacketSerializer.MaxInteractionMessageBytes];
        var length = PacketSerializer.WriteNpcInteractionResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleEconomyRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.EconomyRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.EconomyRequestPacketSize];
        packet[0] = (byte)PacketType.EconomyRequest;
        reader.GetBytes(packet, 1, PacketSerializer.EconomyRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadEconomyRequest(packet);
        var response = _economyHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendEconomyResponse(peer, response);
    }

    private static void SendEconomyResponse(NetPeer peer, EconomyResponse response)
    {
        var buffer = new byte[PacketSerializer.EconomyResponseHeaderSize + PacketSerializer.MaxEconomyMessageBytes];
        var length = PacketSerializer.WriteEconomyResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendGatheringResponse(uint playerEntityId, GatheringResponse response)
    {
        if (!_playersByEntityId.TryGetValue(playerEntityId, out var player))
            return;

        SendGatheringResponse(player.Peer, response);
    }

    private void HandleGatheringRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.GatheringRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.GatheringRequestPacketSize];
        packet[0] = (byte)PacketType.GatheringRequest;
        reader.GetBytes(packet, 1, PacketSerializer.GatheringRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadGatheringRequest(packet);
        var response = _gatheringHandler.TryStartGather(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendGatheringResponse(peer, response);
    }

    private static void SendGatheringResponse(NetPeer peer, GatheringResponse response)
    {
        var buffer = new byte[PacketSerializer.GatheringResponseHeaderSize + 2 + PacketSerializer.MaxGatheringMessageBytes];
        var length = PacketSerializer.WriteGatheringResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleCraftingRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.CraftingRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.CraftingRequestPacketSize];
        packet[0] = (byte)PacketType.CraftingRequest;
        reader.GetBytes(packet, 1, PacketSerializer.CraftingRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadCraftingRequest(packet);
        var response = _craftingService.Handle(player.EntityId, request);
        SendCraftingResponse(peer, response);
    }

    private static void SendCraftingResponse(NetPeer peer, CraftingResponse response)
    {
        var buffer = new byte[PacketSerializer.CraftingResponseHeaderSize + PacketSerializer.MaxCraftingMessageBytes];
        var length = PacketSerializer.WriteCraftingResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleGiftRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.GiftRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.GiftRequestPacketSize];
        packet[0] = (byte)PacketType.GiftRequest;
        reader.GetBytes(packet, 1, PacketSerializer.GiftRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadGiftRequest(packet);
        var response = _giftingService.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendGiftResponse(peer, response);
    }

    private static void SendGiftResponse(NetPeer peer, GiftResponse response)
    {
        var buffer = new byte[
            PacketSerializer.GiftResponseHeaderSize
            + PacketSerializer.MaxGiftDialogueBytes
            + 2
            + PacketSerializer.MaxGiftMessageBytes];
        var length = PacketSerializer.WriteGiftResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleClientQueryRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.ClientQueryRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.ClientQueryRequestPacketSize];
        packet[0] = (byte)PacketType.ClientQueryRequest;
        reader.GetBytes(packet, 1, PacketSerializer.ClientQueryRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadClientQueryRequest(packet);
        var response = _consoleQueryHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendClientQueryResponse(peer, response);
    }

    private static void SendClientQueryResponse(NetPeer peer, ClientQueryResponse response)
    {
        var buffer = new byte[PacketSerializer.ClientQueryResponseHeaderSize + PacketSerializer.MaxClientQueryMessageBytes];
        var length = PacketSerializer.WriteClientQueryResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleChestRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.ChestRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.ChestRequestPacketSize];
        packet[0] = (byte)PacketType.ChestRequest;
        reader.GetBytes(packet, 1, PacketSerializer.ChestRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadChestRequest(packet);
        var response = _chestHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendChestResponse(peer, response);
    }

    private static void SendChestResponse(NetPeer peer, ChestResponse response)
    {
        var buffer = new byte[PacketSerializer.ChestResponseHeaderSize + PacketSerializer.MaxChestMessageBytes];
        var length = PacketSerializer.WriteChestResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleHomeRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.HomeRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.HomeRequestPacketSize];
        packet[0] = (byte)PacketType.HomeRequest;
        reader.GetBytes(packet, 1, PacketSerializer.HomeRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadHomeRequest(packet);
        var response = _homeHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendHomeResponse(peer, response);
    }

    private static void SendHomeResponse(NetPeer peer, HomeResponse response)
    {
        var buffer = new byte[PacketSerializer.HomeResponseHeaderSize + PacketSerializer.MaxHomeMessageBytes];
        var length = PacketSerializer.WriteHomeResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleCommunityProjectRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.CommunityProjectRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.CommunityProjectRequestPacketSize];
        packet[0] = (byte)PacketType.CommunityProjectRequest;
        reader.GetBytes(packet, 1, PacketSerializer.CommunityProjectRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadCommunityProjectRequest(packet);
        var response = _communityProjectHandler.Handle(player.EntityId, request);
        SendCommunityProjectResponse(peer, response);
    }

    private static void SendCommunityProjectResponse(NetPeer peer, CommunityProjectResponse response)
    {
        var buffer = new byte[PacketSerializer.CommunityProjectResponseHeaderSize + PacketSerializer.MaxCommunityProjectMessageBytes];
        var length = PacketSerializer.WriteCommunityProjectResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleMilestoneRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.MilestoneRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.MilestoneRequestPacketSize];
        packet[0] = (byte)PacketType.MilestoneRequest;
        reader.GetBytes(packet, 1, PacketSerializer.MilestoneRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadMilestoneRequest(packet);
        var response = _milestoneHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendMilestoneResponse(peer, response);
    }

    private static void SendMilestoneResponse(NetPeer peer, MilestoneResponse response)
    {
        var buffer = new byte[PacketSerializer.MilestoneResponseHeaderSize + PacketSerializer.MaxMilestoneMessageBytes];
        var length = PacketSerializer.WriteMilestoneResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void BroadcastMilestoneNotification(string message)
    {
        var buffer = new byte[PacketSerializer.MilestoneNotificationHeaderSize + PacketSerializer.MaxMilestoneMessageBytes];
        var length = PacketSerializer.WriteMilestoneNotification(buffer, message);

        foreach (var player in _players.Values)
            player.Peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Information("Broadcast milestone notification to {PlayerCount} online player(s): {Message}", _players.Count, message);
    }

    private void HandlePersonalRoutineRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.PersonalRoutineRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.PersonalRoutineRequestPacketSize];
        packet[0] = (byte)PacketType.PersonalRoutineRequest;
        reader.GetBytes(packet, 1, PacketSerializer.PersonalRoutineRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadPersonalRoutineRequest(packet);
        var response = _personalRoutineHandler.Handle(player.EntityId, request);
        SendPersonalRoutineResponse(peer, response);
    }

    private static void SendPersonalRoutineResponse(NetPeer peer, PersonalRoutineResponse response)
    {
        var buffer = new byte[PacketSerializer.PersonalRoutineResponseHeaderSize + PacketSerializer.MaxPersonalRoutineMessageBytes];
        var length = PacketSerializer.WritePersonalRoutineResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleCommunityActivityRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.CommunityActivityRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.CommunityActivityRequestPacketSize];
        packet[0] = (byte)PacketType.CommunityActivityRequest;
        reader.GetBytes(packet, 1, PacketSerializer.CommunityActivityRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadCommunityActivityRequest(packet);
        var response = _communityActivityHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);
        SendCommunityActivityResponse(peer, response);
    }

    private static void SendCommunityActivityResponse(NetPeer peer, CommunityActivityResponse response)
    {
        var buffer = new byte[PacketSerializer.CommunityActivityResponseHeaderSize + PacketSerializer.MaxCommunityActivityMessageBytes];
        var length = PacketSerializer.WriteCommunityActivityResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleDailyVillageActivityRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.DailyVillageActivityRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.DailyVillageActivityRequestPacketSize];
        packet[0] = (byte)PacketType.DailyVillageActivityRequest;
        reader.GetBytes(packet, 1, PacketSerializer.DailyVillageActivityRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadDailyVillageActivityRequest(packet);
        var response = _dailyVillageActivityHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);
        SendDailyVillageActivityResponse(peer, response);
    }

    private static void SendDailyVillageActivityResponse(NetPeer peer, DailyVillageActivityResponse response)
    {
        var buffer = new byte[PacketSerializer.DailyVillageActivityResponseHeaderSize + PacketSerializer.MaxDailyVillageActivityMessageBytes];
        var length = PacketSerializer.WriteDailyVillageActivityResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleDailyRhythmRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.DailyRhythmRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.DailyRhythmRequestPacketSize];
        packet[0] = (byte)PacketType.DailyRhythmRequest;
        reader.GetBytes(packet, 1, PacketSerializer.DailyRhythmRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadDailyRhythmRequest(packet);
        var response = _dailyRhythmHandler.Handle(player.EntityId, request);
        SendDailyRhythmResponse(peer, response);
    }

    private static void SendDailyRhythmResponse(NetPeer peer, DailyRhythmResponse response)
    {
        var buffer = new byte[PacketSerializer.DailyRhythmResponseHeaderSize + PacketSerializer.MaxDailyRhythmMessageBytes];
        var length = PacketSerializer.WriteDailyRhythmResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleLegacyFocusRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.LegacyFocusRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.LegacyFocusRequestPacketSize];
        packet[0] = (byte)PacketType.LegacyFocusRequest;
        reader.GetBytes(packet, 1, PacketSerializer.LegacyFocusRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadLegacyFocusRequest(packet);
        var response = _legacyFocusHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);
        SendLegacyFocusResponse(peer, response);
    }

    private static void SendLegacyFocusResponse(NetPeer peer, LegacyFocusResponse response)
    {
        var buffer = new byte[PacketSerializer.LegacyFocusResponseHeaderSize + PacketSerializer.MaxLegacyFocusMessageBytes];
        var length = PacketSerializer.WriteLegacyFocusResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleEmotionalBondRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.EmotionalBondRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.EmotionalBondRequestPacketSize];
        packet[0] = (byte)PacketType.EmotionalBondRequest;
        reader.GetBytes(packet, 1, PacketSerializer.EmotionalBondRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadEmotionalBondRequest(packet);
        var response = _emotionalBondHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);
        SendEmotionalBondResponse(peer, response);
    }

    private static void SendEmotionalBondResponse(NetPeer peer, EmotionalBondResponse response)
    {
        var buffer = new byte[PacketSerializer.EmotionalBondResponseHeaderSize + PacketSerializer.MaxEmotionalBondMessageBytes];
        var length = PacketSerializer.WriteEmotionalBondResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    private void HandleVillageAreaRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.VillageAreaRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.VillageAreaRequestPacketSize];
        packet[0] = (byte)PacketType.VillageAreaRequest;
        reader.GetBytes(packet, 1, PacketSerializer.VillageAreaRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadVillageAreaRequest(packet);
        var response = _villageAreaHandler.Handle(
            player.EntityId,
            player.PositionX,
            player.PositionZ,
            request);

        SendVillageAreaResponse(peer, response);
    }

    private static void SendVillageAreaResponse(NetPeer peer, VillageAreaResponse response)
    {
        var buffer = new byte[PacketSerializer.VillageAreaResponseHeaderSize + PacketSerializer.MaxVillageAreaMessageBytes];
        var length = PacketSerializer.WriteVillageAreaResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void BroadcastVillageAreaNotification(string message)
    {
        var buffer = new byte[PacketSerializer.VillageAreaNotificationHeaderSize + PacketSerializer.MaxVillageAreaMessageBytes];
        var length = PacketSerializer.WriteVillageAreaNotification(buffer, message);

        foreach (var player in _players.Values)
            player.Peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Information("Broadcast village area notification to {PlayerCount} online player(s): {Message}", _players.Count, message);
    }

    private void HandleProjectProposalRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.ProjectProposalRequestHeaderSize - 1)
            return;

        var packet = new byte[reader.AvailableBytes + 1];
        packet[0] = (byte)PacketType.ProjectProposalRequest;
        reader.GetBytes(packet, 1, reader.AvailableBytes);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadProjectProposalRequest(packet);
        var response = _proposalHandler.HandleAsync(player.EntityId, request).GetAwaiter().GetResult();
        SendProjectProposalResponse(peer, response);
    }

    private static void SendProjectProposalResponse(NetPeer peer, ProjectProposalResponse response)
    {
        var buffer = new byte[PacketSerializer.ProjectProposalResponseHeaderSize + PacketSerializer.MaxProjectProposalMessageBytes];
        var length = PacketSerializer.WriteProjectProposalResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void BroadcastProjectProposalNotification(string message)
    {
        var buffer = new byte[PacketSerializer.ProjectProposalNotificationHeaderSize + PacketSerializer.MaxProjectProposalMessageBytes];
        var length = PacketSerializer.WriteProjectProposalNotification(buffer, message);

        foreach (var player in _players.Values)
            player.Peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Information(
            "Broadcast project proposal notification to {PlayerCount} online player(s): {Message}",
            _players.Count,
            message);
    }

    private void HandleVillagePositionRequest(NetPeer peer, NetPacketReader reader)
    {
        if (reader.AvailableBytes < PacketSerializer.VillagePositionRequestPacketSize - 1)
            return;

        var packet = new byte[PacketSerializer.VillagePositionRequestPacketSize];
        packet[0] = (byte)PacketType.VillagePositionRequest;
        reader.GetBytes(packet, 1, PacketSerializer.VillagePositionRequestPacketSize - 1);

        if (!_players.TryGetValue(peer.Id, out var player))
            return;

        var request = PacketSerializer.ReadVillagePositionRequest(packet);
        var response = _positionHandler.HandleAsync(player.EntityId, request).GetAwaiter().GetResult();
        SendVillagePositionResponse(peer, response);
    }

    private static void SendVillagePositionResponse(NetPeer peer, VillagePositionResponse response)
    {
        var buffer = new byte[PacketSerializer.VillagePositionResponseHeaderSize + PacketSerializer.MaxVillagePositionMessageBytes];
        var length = PacketSerializer.WriteVillagePositionResponse(buffer, response);
        peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void BroadcastVillagePositionNotification(string message)
    {
        var buffer = new byte[PacketSerializer.VillagePositionNotificationHeaderSize + PacketSerializer.MaxVillagePositionMessageBytes];
        var length = PacketSerializer.WriteVillagePositionNotification(buffer, message);

        foreach (var player in _players.Values)
            player.Peer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);

        Log.Information(
            "Broadcast village position notification to {PlayerCount} online player(s): {Message}",
            _players.Count,
            message);
    }

    private PlayerRecord? ResolveReconnectPlayer()
    {
        var candidate = _pendingReconnectPlayer;
        _pendingReconnectPlayer = null;

        candidate ??= _persistenceService.GetReconnectPlayerAsync().GetAwaiter().GetResult();
        if (candidate is null)
            return null;

        if (_playersByEntityId.ContainsKey(candidate.EntityId))
            return null;

        return candidate;
    }

    private sealed class ConnectedPlayer
    {
        public required NetPeer Peer { get; init; }
        public required uint EntityId { get; init; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationYaw { get; set; }
        public PlayerInput LastInput { get; set; }
    }
}