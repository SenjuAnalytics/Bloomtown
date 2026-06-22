using Bloomtown.Shared.Protocol;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Bloomtown.Client.Scripts.Net;

/// <summary>
/// Minimal LiteNetLib client for movement replication testing.
/// Can be reused later from a Unity client assembly.
/// </summary>
public sealed class NetworkClient
{
    private const int PlayerInputPacketSize = PacketSerializer.PlayerInputPacketSize;

    private readonly EventBasedNetListener _listener;
    private readonly NetManager _netManager;
    private NetPeer? _serverPeer;

    private uint _inputSeq;
    private PlayerInput _pendingInput;

    public bool IsConnected => _serverPeer is { ConnectionState: ConnectionState.Connected };

    public uint LocalEntityId { get; private set; }

    public event Action<ConnectAccept>? OnConnected;
    public event Action<EntityDelta>? OnEntityDeltaReceived;
    public event Action<NpcInteractionResponse>? OnInteractionResponse;
    public event Action<EconomyResponse>? OnEconomyResponse;
    public event Action<GatheringResponse>? OnGatheringResponse;
    public event Action<GiftResponse>? OnGiftResponse;
    public event Action<CraftingResponse>? OnCraftingResponse;
    public event Action<ClientQueryResponse>? OnClientQueryResponse;
    public event Action<ChestResponse>? OnChestResponse;
    public event Action<HomeResponse>? OnHomeResponse;
    public event Action<CommunityProjectResponse>? OnCommunityProjectResponse;
    public event Action<MilestoneResponse>? OnMilestoneResponse;
    public event Action<string>? OnMilestoneNotification;
    public event Action<ProjectProposalResponse>? OnProjectProposalResponse;
    public event Action<string>? OnProjectProposalNotification;
    public event Action<VillagePositionResponse>? OnVillagePositionResponse;
    public event Action<string>? OnVillagePositionNotification;
    public event Action<uint, string>? OnNpcAmbientComment;
    public event Action<VillageAreaResponse>? OnVillageAreaResponse;
    public event Action<string>? OnVillageAreaNotification;
    public event Action<PersonalRoutineResponse>? OnPersonalRoutineResponse;
    public event Action<CommunityActivityResponse>? OnCommunityActivityResponse;
    public event Action<DailyVillageActivityResponse>? OnDailyVillageActivityResponse;
    public event Action<DailyRhythmResponse>? OnDailyRhythmResponse;
    public event Action<LegacyFocusResponse>? OnLegacyFocusResponse;
    public event Action<EmotionalBondResponse>? OnEmotionalBondResponse;
    public event Action<DisconnectInfo>? OnDisconnected;

    public NetworkClient()
    {
        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener)
        {
            AutoRecycle = true,
            IPv6Enabled = false,
        };

        _listener.PeerConnectedEvent += peer =>
        {
            _serverPeer = peer;
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            if (_serverPeer == peer)
                _serverPeer = null;

            OnDisconnected?.Invoke(info);
        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            try
            {
                HandlePacket(reader);
            }
            finally
            {
                reader.Recycle();
            }
        };
    }

    public bool Connect(string host, int port = NetworkConstants.ServerPort)
    {
        if (!_netManager.Start())
            return false;

        _netManager.Connect(host, port, NetworkConstants.ConnectionKey);
        return true;
    }

    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    public void SetInput(float moveDirX, float moveDirY, float lookYaw)
    {
        _pendingInput = new PlayerInput
        {
            Seq = _inputSeq,
            MoveDirX = moveDirX,
            MoveDirY = moveDirY,
            LookYaw = lookYaw,
        };
    }

    public void SendInput()
    {
        if (_serverPeer is null)
            return;

        _inputSeq++;

        var input = _pendingInput with { Seq = _inputSeq };

        var buffer = new byte[PlayerInputPacketSize];
        var length = PacketSerializer.WritePlayerInput(buffer, input);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.Unreliable);
    }

    public void SendInteraction(NpcInteractionKind kind, uint targetNpcEntityId = 0)
    {
        if (_serverPeer is null)
            return;

        var request = new NpcInteractionRequest(kind, targetNpcEntityId);
        var buffer = new byte[PacketSerializer.NpcInteractionRequestPacketSize];
        var length = PacketSerializer.WriteNpcInteractionRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendEconomyRequest(EconomyRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.EconomyRequestPacketSize];
        var length = PacketSerializer.WriteEconomyRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendGatheringRequest(GatheringRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.GatheringRequestPacketSize];
        var length = PacketSerializer.WriteGatheringRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendCraftingRequest(CraftingRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.CraftingRequestPacketSize];
        var length = PacketSerializer.WriteCraftingRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendGiftRequest(GiftRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.GiftRequestPacketSize];
        var length = PacketSerializer.WriteGiftRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendClientQuery(ClientQueryRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.ClientQueryRequestPacketSize];
        var length = PacketSerializer.WriteClientQueryRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendChestRequest(ChestRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.ChestRequestPacketSize];
        var length = PacketSerializer.WriteChestRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendHomeRequest(HomeRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.HomeRequestPacketSize];
        var length = PacketSerializer.WriteHomeRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendCommunityProjectRequest(CommunityProjectRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.CommunityProjectRequestPacketSize];
        var length = PacketSerializer.WriteCommunityProjectRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendMilestoneRequest(MilestoneRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.MilestoneRequestPacketSize];
        var length = PacketSerializer.WriteMilestoneRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendVillageAreaRequest(VillageAreaRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.VillageAreaRequestPacketSize];
        var length = PacketSerializer.WriteVillageAreaRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendPersonalRoutineRequest(PersonalRoutineRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.PersonalRoutineRequestPacketSize];
        var length = PacketSerializer.WritePersonalRoutineRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendCommunityActivityRequest(CommunityActivityRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.CommunityActivityRequestPacketSize];
        var length = PacketSerializer.WriteCommunityActivityRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendDailyVillageActivityRequest(DailyVillageActivityRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.DailyVillageActivityRequestPacketSize];
        var length = PacketSerializer.WriteDailyVillageActivityRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendDailyRhythmRequest(DailyRhythmRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.DailyRhythmRequestPacketSize];
        var length = PacketSerializer.WriteDailyRhythmRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendLegacyFocusRequest(LegacyFocusRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.LegacyFocusRequestPacketSize];
        var length = PacketSerializer.WriteLegacyFocusRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendEmotionalBondRequest(EmotionalBondRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.EmotionalBondRequestPacketSize];
        var length = PacketSerializer.WriteEmotionalBondRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendProjectProposalRequest(ProjectProposalRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[
            PacketSerializer.ProjectProposalRequestHeaderSize
            + PacketSerializer.MaxProjectProposalNameBytes
            + PacketSerializer.ProjectProposalRequestTailSize];
        var length = PacketSerializer.WriteProjectProposalRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void SendVillagePositionRequest(VillagePositionRequest request)
    {
        if (_serverPeer is null)
            return;

        var buffer = new byte[PacketSerializer.VillagePositionRequestPacketSize];
        var length = PacketSerializer.WriteVillagePositionRequest(buffer, request);
        _serverPeer.Send(buffer, 0, length, DeliveryMethod.ReliableOrdered);
    }

    public void Disconnect()
    {
        _serverPeer?.Disconnect();
        _netManager.Stop();
        _serverPeer = null;
    }

    private void HandlePacket(NetPacketReader reader)
    {
        if (reader.AvailableBytes < 1)
            return;

        var packetType = (PacketType)reader.GetByte();

        switch (packetType)
        {
            case PacketType.ConnectAccept:
                if (reader.AvailableBytes < 16)
                    return;

                var acceptPacket = new byte[17];
                acceptPacket[0] = (byte)packetType;
                reader.GetBytes(acceptPacket, 1, 16);

                var accept = PacketSerializer.ReadConnectAccept(acceptPacket);
                LocalEntityId = accept.EntityId;
                OnConnected?.Invoke(accept);
                break;

            case PacketType.EntityDelta:
                if (reader.AvailableBytes < 24)
                    return;

                var deltaPacket = new byte[25];
                deltaPacket[0] = (byte)packetType;
                reader.GetBytes(deltaPacket, 1, 24);

                var delta = PacketSerializer.ReadEntityDelta(deltaPacket);
                OnEntityDeltaReceived?.Invoke(delta);
                break;

            case PacketType.NpcInteractionResponse:
                if (reader.AvailableBytes < PacketSerializer.NpcInteractionResponseHeaderSize - 1)
                    return;

                var responsePacket = new byte[reader.AvailableBytes + 1];
                responsePacket[0] = (byte)packetType;
                reader.GetBytes(responsePacket, 1, reader.AvailableBytes);

                var response = PacketSerializer.ReadNpcInteractionResponse(responsePacket);
                OnInteractionResponse?.Invoke(response);
                break;

            case PacketType.EconomyResponse:
                if (reader.AvailableBytes < PacketSerializer.EconomyResponseHeaderSize - 1)
                    return;

                var economyPacket = new byte[reader.AvailableBytes + 1];
                economyPacket[0] = (byte)packetType;
                reader.GetBytes(economyPacket, 1, reader.AvailableBytes);

                var economyResponse = PacketSerializer.ReadEconomyResponse(economyPacket);
                OnEconomyResponse?.Invoke(economyResponse);
                break;

            case PacketType.GatheringResponse:
                if (reader.AvailableBytes < PacketSerializer.GatheringResponseHeaderSize + 2 - 1)
                    return;

                var gatheringPacket = new byte[reader.AvailableBytes + 1];
                gatheringPacket[0] = (byte)packetType;
                reader.GetBytes(gatheringPacket, 1, reader.AvailableBytes);

                var gatheringResponse = PacketSerializer.ReadGatheringResponse(gatheringPacket);
                OnGatheringResponse?.Invoke(gatheringResponse);
                break;

            case PacketType.CraftingResponse:
                if (reader.AvailableBytes < PacketSerializer.CraftingResponseHeaderSize - 1)
                    return;

                var craftingPacket = new byte[reader.AvailableBytes + 1];
                craftingPacket[0] = (byte)packetType;
                reader.GetBytes(craftingPacket, 1, reader.AvailableBytes);

                var craftingResponse = PacketSerializer.ReadCraftingResponse(craftingPacket);
                OnCraftingResponse?.Invoke(craftingResponse);
                break;

            case PacketType.GiftResponse:
                if (reader.AvailableBytes < PacketSerializer.GiftResponseHeaderSize - 1)
                    return;

                var giftPacket = new byte[reader.AvailableBytes + 1];
                giftPacket[0] = (byte)packetType;
                reader.GetBytes(giftPacket, 1, reader.AvailableBytes);

                var giftResponse = PacketSerializer.ReadGiftResponse(giftPacket);
                OnGiftResponse?.Invoke(giftResponse);
                break;

            case PacketType.ClientQueryResponse:
                if (reader.AvailableBytes < PacketSerializer.ClientQueryResponseHeaderSize - 1)
                    return;

                var queryPacket = new byte[reader.AvailableBytes + 1];
                queryPacket[0] = (byte)packetType;
                reader.GetBytes(queryPacket, 1, reader.AvailableBytes);

                var queryResponse = PacketSerializer.ReadClientQueryResponse(queryPacket);
                OnClientQueryResponse?.Invoke(queryResponse);
                break;

            case PacketType.ChestResponse:
                if (reader.AvailableBytes < PacketSerializer.ChestResponseHeaderSize - 1)
                    return;

                var chestPacket = new byte[reader.AvailableBytes + 1];
                chestPacket[0] = (byte)packetType;
                reader.GetBytes(chestPacket, 1, reader.AvailableBytes);

                var chestResponse = PacketSerializer.ReadChestResponse(chestPacket);
                OnChestResponse?.Invoke(chestResponse);
                break;

            case PacketType.CommunityProjectResponse:
                if (reader.AvailableBytes < PacketSerializer.CommunityProjectResponseHeaderSize - 1)
                    return;

                var projectPacket = new byte[reader.AvailableBytes + 1];
                projectPacket[0] = (byte)packetType;
                reader.GetBytes(projectPacket, 1, reader.AvailableBytes);

                var projectResponse = PacketSerializer.ReadCommunityProjectResponse(projectPacket);
                OnCommunityProjectResponse?.Invoke(projectResponse);
                break;

            case PacketType.MilestoneResponse:
                if (reader.AvailableBytes < PacketSerializer.MilestoneResponseHeaderSize - 1)
                    return;

                var milestonePacket = new byte[reader.AvailableBytes + 1];
                milestonePacket[0] = (byte)packetType;
                reader.GetBytes(milestonePacket, 1, reader.AvailableBytes);

                var milestoneResponse = PacketSerializer.ReadMilestoneResponse(milestonePacket);
                OnMilestoneResponse?.Invoke(milestoneResponse);
                break;

            case PacketType.MilestoneNotification:
                if (reader.AvailableBytes < PacketSerializer.MilestoneNotificationHeaderSize - 1)
                    return;

                var notificationPacket = new byte[reader.AvailableBytes + 1];
                notificationPacket[0] = (byte)packetType;
                reader.GetBytes(notificationPacket, 1, reader.AvailableBytes);

                var notification = PacketSerializer.ReadMilestoneNotification(notificationPacket);
                OnMilestoneNotification?.Invoke(notification);
                break;

            case PacketType.ProjectProposalResponse:
                if (reader.AvailableBytes < PacketSerializer.ProjectProposalResponseHeaderSize - 1)
                    return;

                var proposalPacket = new byte[reader.AvailableBytes + 1];
                proposalPacket[0] = (byte)packetType;
                reader.GetBytes(proposalPacket, 1, reader.AvailableBytes);

                var proposalResponse = PacketSerializer.ReadProjectProposalResponse(proposalPacket);
                OnProjectProposalResponse?.Invoke(proposalResponse);
                break;

            case PacketType.ProjectProposalNotification:
                if (reader.AvailableBytes < PacketSerializer.ProjectProposalNotificationHeaderSize - 1)
                    return;

                var proposalNotificationPacket = new byte[reader.AvailableBytes + 1];
                proposalNotificationPacket[0] = (byte)packetType;
                reader.GetBytes(proposalNotificationPacket, 1, reader.AvailableBytes);

                var proposalNotification = PacketSerializer.ReadProjectProposalNotification(proposalNotificationPacket);
                OnProjectProposalNotification?.Invoke(proposalNotification);
                break;

            case PacketType.VillagePositionResponse:
                if (reader.AvailableBytes < PacketSerializer.VillagePositionResponseHeaderSize - 1)
                    return;

                var positionPacket = new byte[reader.AvailableBytes + 1];
                positionPacket[0] = (byte)packetType;
                reader.GetBytes(positionPacket, 1, reader.AvailableBytes);

                var positionResponse = PacketSerializer.ReadVillagePositionResponse(positionPacket);
                OnVillagePositionResponse?.Invoke(positionResponse);
                break;

            case PacketType.VillagePositionNotification:
                if (reader.AvailableBytes < PacketSerializer.VillagePositionNotificationHeaderSize - 1)
                    return;

                var positionNotificationPacket = new byte[reader.AvailableBytes + 1];
                positionNotificationPacket[0] = (byte)packetType;
                reader.GetBytes(positionNotificationPacket, 1, reader.AvailableBytes);

                var positionNotification = PacketSerializer.ReadVillagePositionNotification(positionNotificationPacket);
                OnVillagePositionNotification?.Invoke(positionNotification);
                break;

            case PacketType.HomeResponse:
                if (reader.AvailableBytes < PacketSerializer.HomeResponseHeaderSize - 1)
                    return;

                var homePacket = new byte[reader.AvailableBytes + 1];
                homePacket[0] = (byte)packetType;
                reader.GetBytes(homePacket, 1, reader.AvailableBytes);

                var homeResponse = PacketSerializer.ReadHomeResponse(homePacket);
                OnHomeResponse?.Invoke(homeResponse);
                break;

            case PacketType.VillageAreaResponse:
                if (reader.AvailableBytes < PacketSerializer.VillageAreaResponseHeaderSize - 1)
                    return;

                var areaPacket = new byte[reader.AvailableBytes + 1];
                areaPacket[0] = (byte)packetType;
                reader.GetBytes(areaPacket, 1, reader.AvailableBytes);

                var areaResponse = PacketSerializer.ReadVillageAreaResponse(areaPacket);
                OnVillageAreaResponse?.Invoke(areaResponse);
                break;

            case PacketType.VillageAreaNotification:
                if (reader.AvailableBytes < PacketSerializer.VillageAreaNotificationHeaderSize - 1)
                    return;

                var areaNotificationPacket = new byte[reader.AvailableBytes + 1];
                areaNotificationPacket[0] = (byte)packetType;
                reader.GetBytes(areaNotificationPacket, 1, reader.AvailableBytes);

                var areaNotification = PacketSerializer.ReadVillageAreaNotification(areaNotificationPacket);
                OnVillageAreaNotification?.Invoke(areaNotification);
                break;

            case PacketType.PersonalRoutineResponse:
                if (reader.AvailableBytes < PacketSerializer.PersonalRoutineResponseHeaderSize - 1)
                    return;

                var routinePacket = new byte[reader.AvailableBytes + 1];
                routinePacket[0] = (byte)packetType;
                reader.GetBytes(routinePacket, 1, reader.AvailableBytes);

                var routineResponse = PacketSerializer.ReadPersonalRoutineResponse(routinePacket);
                OnPersonalRoutineResponse?.Invoke(routineResponse);
                break;

            case PacketType.CommunityActivityResponse:
                if (reader.AvailableBytes < PacketSerializer.CommunityActivityResponseHeaderSize - 1)
                    return;

                var activityPacket = new byte[reader.AvailableBytes + 1];
                activityPacket[0] = (byte)packetType;
                reader.GetBytes(activityPacket, 1, reader.AvailableBytes);

                var activityResponse = PacketSerializer.ReadCommunityActivityResponse(activityPacket);
                OnCommunityActivityResponse?.Invoke(activityResponse);
                break;

            case PacketType.DailyVillageActivityResponse:
                if (reader.AvailableBytes < PacketSerializer.DailyVillageActivityResponseHeaderSize - 1)
                    return;

                var dailyPacket = new byte[reader.AvailableBytes + 1];
                dailyPacket[0] = (byte)packetType;
                reader.GetBytes(dailyPacket, 1, reader.AvailableBytes);

                var dailyResponse = PacketSerializer.ReadDailyVillageActivityResponse(dailyPacket);
                OnDailyVillageActivityResponse?.Invoke(dailyResponse);
                break;

            case PacketType.DailyRhythmResponse:
                if (reader.AvailableBytes < PacketSerializer.DailyRhythmResponseHeaderSize - 1)
                    return;

                var rhythmPacket = new byte[reader.AvailableBytes + 1];
                rhythmPacket[0] = (byte)packetType;
                reader.GetBytes(rhythmPacket, 1, reader.AvailableBytes);

                var rhythmResponse = PacketSerializer.ReadDailyRhythmResponse(rhythmPacket);
                OnDailyRhythmResponse?.Invoke(rhythmResponse);
                break;

            case PacketType.LegacyFocusResponse:
                if (reader.AvailableBytes < PacketSerializer.LegacyFocusResponseHeaderSize - 1)
                    return;

                var focusPacket = new byte[reader.AvailableBytes + 1];
                focusPacket[0] = (byte)packetType;
                reader.GetBytes(focusPacket, 1, reader.AvailableBytes);

                var focusResponse = PacketSerializer.ReadLegacyFocusResponse(focusPacket);
                OnLegacyFocusResponse?.Invoke(focusResponse);
                break;

            case PacketType.EmotionalBondResponse:
                if (reader.AvailableBytes < PacketSerializer.EmotionalBondResponseHeaderSize - 1)
                    return;

                var bondPacket = new byte[reader.AvailableBytes + 1];
                bondPacket[0] = (byte)packetType;
                reader.GetBytes(bondPacket, 1, reader.AvailableBytes);

                var bondResponse = PacketSerializer.ReadEmotionalBondResponse(bondPacket);
                OnEmotionalBondResponse?.Invoke(bondResponse);
                break;

            case PacketType.NpcAmbientComment:
                if (reader.AvailableBytes < PacketSerializer.NpcAmbientCommentHeaderSize - 1)
                    return;

                var ambientPacket = new byte[reader.AvailableBytes + 1];
                ambientPacket[0] = (byte)packetType;
                reader.GetBytes(ambientPacket, 1, reader.AvailableBytes);

                var (npcEntityId, ambientMessage) = PacketSerializer.ReadNpcAmbientComment(ambientPacket);
                OnNpcAmbientComment?.Invoke(npcEntityId, ambientMessage);
                break;
        }
    }
}