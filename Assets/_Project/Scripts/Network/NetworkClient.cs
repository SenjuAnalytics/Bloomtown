using System;
using System.Net;
using System.Net.Sockets;
using Bloomtown.Shared.Protocol;
using LiteNetLib;
using UnityEngine;

namespace Bloomtown.Client.Network
{
    /// <summary>
    /// MonoBehaviour wrapper di sekitar LiteNetLib UDP client.
    ///
    /// SETUP — LiteNetLib belum ada di Unity project ini. Install via Package Manager:
    ///   Window > Package Manager > + > Add package from git URL >
    ///   https://github.com/RevenantX/LiteNetLib.git?path=LiteNetLib#master
    ///
    /// Alternatif: copy folder LiteNetLib/ dari repo GitHub ke Assets/Plugins/LiteNetLib/.
    ///
    /// Cara pakai:
    ///   - GameBootstrap membuat GameObject "Network" dan attach component ini
    ///   - PollEvents() dipanggil tiap Update() → semua event dijamin di main thread
    ///   - Kirim PlayerInput pakai SendPlayerInput(); game action pakai ReliableBuffer + SendReliable()
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkClient : MonoBehaviour, INetEventListener
    {
        // ── Inspector ───────────────────────────────────────────────────────
        [Header("Connection")]
        [SerializeField] private string _serverHost = "127.0.0.1";
        [SerializeField] private int    _serverPort = NetworkConstants.ServerPort;
        [SerializeField] private bool   _autoConnectOnStart = true;

        // ── Singleton ───────────────────────────────────────────────────────
        public static NetworkClient Instance { get; private set; }

        // ── State ───────────────────────────────────────────────────────────
        private NetManager _manager;
        private NetPeer    _server;

        // Pre-allocated buffers — zero alloc per frame
        // Packet paling besar di protokol: ~2100 byte (VillagePosition, MaxVillagePositionMessageBytes=2000)
        private readonly byte[] _unreliableBuf = new byte[256];   // PlayerInput = 17 bytes
        private readonly byte[] _reliableBuf   = new byte[2200];

        // ── Public API ──────────────────────────────────────────────────────
        public bool IsConnected => _server?.ConnectionState == ConnectionState.Connected;
        public int  Ping        => _server?.RoundTripTime ?? -1;

        /// <summary>
        /// Buffer yang bisa ditulis untuk reliable packet.
        /// Pola: serialize ke ReliableBuffer → panggil SendReliable(size).
        /// </summary>
        public byte[] ReliableBuffer => _reliableBuf;

        // ── Unity lifecycle ─────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _manager = new NetManager(this)
            {
                AutoRecycle              = true,   // recycle NetPacketReader otomatis setelah OnNetworkReceive
                UnconnectedMessagesEnabled = false,
                UpdateTime               = 15,     // poll interval ms (jauh lebih kecil dari sim tick 50ms)
                MaxConnectAttempts       = 10,
                ReconnectDelay           = 1000,
            };
            _manager.Start();

            if (_autoConnectOnStart)
                Connect();
        }

        private void Update()
        {
            _manager?.PollEvents();
        }

        private void OnDestroy()
        {
            _manager?.Stop();
            if (Instance == this) Instance = null;
        }

        // ── Connection control ──────────────────────────────────────────────
        public void Connect(string host = null, int port = 0)
        {
            if (host != null) _serverHost = host;
            if (port > 0)     _serverPort = port;
            Debug.Log($"[NetworkClient] Connecting → {_serverHost}:{_serverPort}");
            _manager.Connect(_serverHost, _serverPort, NetworkConstants.ConnectionKey);
        }

        public void Disconnect() => _server?.Disconnect();

        // ── Send helpers ────────────────────────────────────────────────────

        /// <summary>PlayerInput dikirim unreliable (drop OK, server punya seq untuk skip yang lama).</summary>
        public void SendPlayerInput(PlayerInput input)
        {
            if (!IsConnected) return;
            var size = PacketSerializer.WritePlayerInput(_unreliableBuf, input);
            _server.Send(_unreliableBuf, 0, size, DeliveryMethod.Unreliable);
        }

        /// <summary>Kirim reliable-ordered packet dari ReliableBuffer.</summary>
        public void SendReliable(int size)
        {
            if (!IsConnected) return;
            _server.Send(_reliableBuf, 0, size, DeliveryMethod.ReliableOrdered);
        }

        public void SendInteraction(NpcInteractionKind kind, uint targetNpcEntityId = 0)
        {
            if (!IsConnected) return;
            var size = PacketSerializer.WriteNpcInteractionRequest(
                _reliableBuf, new NpcInteractionRequest(kind, targetNpcEntityId));
            SendReliable(size);
        }

        public void SendClientQuery(ClientQueryKind kind)
        {
            if (!IsConnected) return;
            var size = PacketSerializer.WriteClientQueryRequest(
                _reliableBuf, new ClientQueryRequest(kind));
            SendReliable(size);
        }

        // ── INetEventListener ───────────────────────────────────────────────
        public void OnPeerConnected(NetPeer peer)
        {
            _server = peer;
            Debug.Log($"[NetworkClient] ✓ Connected to {peer.Address}:{peer.Port}");
            NetworkEvents.FireConnected();
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            Debug.Log($"[NetworkClient] Disconnected: {info.Reason}");
            _server = null;
            NetworkEvents.FireDisconnected(info.Reason.ToString());
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            => Debug.LogError($"[NetworkClient] Socket error {socketError} from {endPoint}");

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
        {
            DispatchPacket(reader.GetRemainingBytesSpan());
            // AutoRecycle = true → tidak perlu panggil reader.Recycle() manual
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remote, NetPacketReader reader, UnconnectedMessageType type) { }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public void OnConnectionRequest(ConnectionRequest request) => request.Reject(); // client tidak menerima koneksi masuk

        // ── Packet dispatch (zero alloc, semua pakai stack-based ReadOnlySpan) ──
        private void DispatchPacket(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0) return;
            var type = (PacketType)data[0];
            try
            {
                switch (type)
                {
                    // ── Core movement ──
                    case PacketType.ConnectAccept:
                        NetworkEvents.FireConnectAccept(PacketSerializer.ReadConnectAccept(data));
                        break;
                    case PacketType.EntityDelta:
                        NetworkEvents.FireEntityDelta(PacketSerializer.ReadEntityDelta(data));
                        break;

                    // ── NPC ──
                    case PacketType.NpcInteractionResponse:
                        NetworkEvents.FireNpcInteractionResponse(PacketSerializer.ReadNpcInteractionResponse(data));
                        break;
                    case PacketType.NpcAmbientComment:
                    {
                        var ambient = PacketSerializer.ReadNpcAmbientComment(data);
                        NetworkEvents.FireNpcAmbientComment(ambient.NpcEntityId, ambient.Message);
                        break;
                    }

                    // ── Economy & inventory ──
                    case PacketType.EconomyResponse:
                        NetworkEvents.FireEconomyResponse(PacketSerializer.ReadEconomyResponse(data));
                        break;
                    case PacketType.GatheringResponse:
                        NetworkEvents.FireGatheringResponse(PacketSerializer.ReadGatheringResponse(data));
                        break;
                    case PacketType.GiftResponse:
                        NetworkEvents.FireGiftResponse(PacketSerializer.ReadGiftResponse(data));
                        break;
                    case PacketType.ChestResponse:
                        NetworkEvents.FireChestResponse(PacketSerializer.ReadChestResponse(data));
                        break;
                    case PacketType.CraftingResponse:
                        NetworkEvents.FireCraftingResponse(PacketSerializer.ReadCraftingResponse(data));
                        break;
                    case PacketType.HomeResponse:
                        NetworkEvents.FireHomeResponse(PacketSerializer.ReadHomeResponse(data));
                        break;

                    // ── Community & village ──
                    case PacketType.CommunityProjectResponse:
                        NetworkEvents.FireCommunityProjectResponse(PacketSerializer.ReadCommunityProjectResponse(data));
                        break;
                    case PacketType.CommunityActivityResponse:
                        NetworkEvents.FireCommunityActivityResponse(PacketSerializer.ReadCommunityActivityResponse(data));
                        break;
                    case PacketType.DailyVillageActivityResponse:
                        NetworkEvents.FireDailyVillageActivityResponse(PacketSerializer.ReadDailyVillageActivityResponse(data));
                        break;
                    case PacketType.DailyRhythmResponse:
                        NetworkEvents.FireDailyRhythmResponse(PacketSerializer.ReadDailyRhythmResponse(data));
                        break;

                    // ── Governance ──
                    case PacketType.ProjectProposalResponse:
                        NetworkEvents.FireProjectProposalResponse(PacketSerializer.ReadProjectProposalResponse(data));
                        break;
                    case PacketType.ProjectProposalNotification:
                        NetworkEvents.FireProjectProposalNotification(PacketSerializer.ReadProjectProposalNotification(data));
                        break;
                    case PacketType.VillagePositionResponse:
                        NetworkEvents.FireVillagePositionResponse(PacketSerializer.ReadVillagePositionResponse(data));
                        break;
                    case PacketType.VillagePositionNotification:
                        NetworkEvents.FireVillagePositionNotification(PacketSerializer.ReadVillagePositionNotification(data));
                        break;
                    case PacketType.VillageAreaResponse:
                        NetworkEvents.FireVillageAreaResponse(PacketSerializer.ReadVillageAreaResponse(data));
                        break;
                    case PacketType.VillageAreaNotification:
                        NetworkEvents.FireVillageAreaNotification(PacketSerializer.ReadVillageAreaNotification(data));
                        break;

                    // ── Player progression ──
                    case PacketType.MilestoneResponse:
                        NetworkEvents.FireMilestoneResponse(PacketSerializer.ReadMilestoneResponse(data));
                        break;
                    case PacketType.MilestoneNotification:
                        NetworkEvents.FireMilestoneNotification(PacketSerializer.ReadMilestoneNotification(data));
                        break;
                    case PacketType.LegacyFocusResponse:
                        NetworkEvents.FireLegacyFocusResponse(PacketSerializer.ReadLegacyFocusResponse(data));
                        break;
                    case PacketType.EmotionalBondResponse:
                        NetworkEvents.FireEmotionalBondResponse(PacketSerializer.ReadEmotionalBondResponse(data));
                        break;
                    case PacketType.PersonalRoutineResponse:
                        NetworkEvents.FirePersonalRoutineResponse(PacketSerializer.ReadPersonalRoutineResponse(data));
                        break;

                    // ── Misc ──
                    case PacketType.ClientQueryResponse:
                        NetworkEvents.FireClientQueryResponse(PacketSerializer.ReadClientQueryResponse(data));
                        break;

                    default:
                        Debug.LogWarning($"[NetworkClient] Unhandled packet type: {type} (0x{data[0]:X2})");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkClient] Exception dispatching {type}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
