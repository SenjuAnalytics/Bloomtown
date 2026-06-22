using System;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client.Network
{
    /// <summary>
    /// Static event hub. NetworkClient fires events here; game systems subscribe from anywhere.
    /// Semua event dijamin dipanggil dari Unity main thread karena LiteNetLib di-poll dari Update().
    /// </summary>
    public static class NetworkEvents
    {
        // ── Connection lifecycle ────────────────────────────────────────────
        public static event Action OnConnected;
        public static event Action<string> OnDisconnected;

        // ── Server → Client: core movement ─────────────────────────────────
        public static event Action<ConnectAccept> OnConnectAccept;
        public static event Action<EntityDelta>   OnEntityDelta;

        // ── Server → Client: NPC ───────────────────────────────────────────
        public static event Action<NpcInteractionResponse> OnNpcInteractionResponse;
        public static event Action<uint, string>           OnNpcAmbientComment;   // (npcEntityId, message)

        // ── Server → Client: economy & inventory ──────────────────────────
        public static event Action<EconomyResponse>   OnEconomyResponse;
        public static event Action<GatheringResponse> OnGatheringResponse;
        public static event Action<GiftResponse>      OnGiftResponse;
        public static event Action<ChestResponse>     OnChestResponse;
        public static event Action<CraftingResponse>  OnCraftingResponse;
        public static event Action<HomeResponse>      OnHomeResponse;

        // ── Server → Client: community & village ──────────────────────────
        public static event Action<CommunityProjectResponse>    OnCommunityProjectResponse;
        public static event Action<CommunityActivityResponse>   OnCommunityActivityResponse;
        public static event Action<DailyVillageActivityResponse> OnDailyVillageActivityResponse;
        public static event Action<DailyRhythmResponse>         OnDailyRhythmResponse;

        // ── Server → Client: governance & leadership ──────────────────────
        public static event Action<ProjectProposalResponse> OnProjectProposalResponse;
        public static event Action<string>                  OnProjectProposalNotification;
        public static event Action<VillagePositionResponse> OnVillagePositionResponse;
        public static event Action<string>                  OnVillagePositionNotification;
        public static event Action<VillageAreaResponse>     OnVillageAreaResponse;
        public static event Action<string>                  OnVillageAreaNotification;

        // ── Server → Client: player progression ───────────────────────────
        public static event Action<MilestoneResponse>       OnMilestoneResponse;
        public static event Action<string>                  OnMilestoneNotification;
        public static event Action<LegacyFocusResponse>     OnLegacyFocusResponse;
        public static event Action<EmotionalBondResponse>   OnEmotionalBondResponse;
        public static event Action<PersonalRoutineResponse> OnPersonalRoutineResponse;

        // ── Server → Client: misc ──────────────────────────────────────────
        public static event Action<ClientQueryResponse> OnClientQueryResponse;

        // ── Internal fire methods (hanya NetworkClient yang boleh panggil) ─
        internal static void FireConnected()                            => OnConnected?.Invoke();
        internal static void FireDisconnected(string reason)           => OnDisconnected?.Invoke(reason);
        internal static void FireConnectAccept(ConnectAccept p)        => OnConnectAccept?.Invoke(p);
        internal static void FireEntityDelta(EntityDelta p)            => OnEntityDelta?.Invoke(p);
        internal static void FireNpcInteractionResponse(NpcInteractionResponse p)   => OnNpcInteractionResponse?.Invoke(p);
        internal static void FireNpcAmbientComment(uint id, string msg)             => OnNpcAmbientComment?.Invoke(id, msg);
        internal static void FireEconomyResponse(EconomyResponse p)    => OnEconomyResponse?.Invoke(p);
        internal static void FireGatheringResponse(GatheringResponse p) => OnGatheringResponse?.Invoke(p);
        internal static void FireGiftResponse(GiftResponse p)          => OnGiftResponse?.Invoke(p);
        internal static void FireChestResponse(ChestResponse p)        => OnChestResponse?.Invoke(p);
        internal static void FireCraftingResponse(CraftingResponse p)  => OnCraftingResponse?.Invoke(p);
        internal static void FireHomeResponse(HomeResponse p)          => OnHomeResponse?.Invoke(p);
        internal static void FireCommunityProjectResponse(CommunityProjectResponse p)       => OnCommunityProjectResponse?.Invoke(p);
        internal static void FireCommunityActivityResponse(CommunityActivityResponse p)     => OnCommunityActivityResponse?.Invoke(p);
        internal static void FireDailyVillageActivityResponse(DailyVillageActivityResponse p) => OnDailyVillageActivityResponse?.Invoke(p);
        internal static void FireDailyRhythmResponse(DailyRhythmResponse p)         => OnDailyRhythmResponse?.Invoke(p);
        internal static void FireProjectProposalResponse(ProjectProposalResponse p)  => OnProjectProposalResponse?.Invoke(p);
        internal static void FireProjectProposalNotification(string msg)             => OnProjectProposalNotification?.Invoke(msg);
        internal static void FireVillagePositionResponse(VillagePositionResponse p)  => OnVillagePositionResponse?.Invoke(p);
        internal static void FireVillagePositionNotification(string msg)             => OnVillagePositionNotification?.Invoke(msg);
        internal static void FireVillageAreaResponse(VillageAreaResponse p)          => OnVillageAreaResponse?.Invoke(p);
        internal static void FireVillageAreaNotification(string msg)                 => OnVillageAreaNotification?.Invoke(msg);
        internal static void FireMilestoneResponse(MilestoneResponse p)              => OnMilestoneResponse?.Invoke(p);
        internal static void FireMilestoneNotification(string msg)                   => OnMilestoneNotification?.Invoke(msg);
        internal static void FireLegacyFocusResponse(LegacyFocusResponse p)         => OnLegacyFocusResponse?.Invoke(p);
        internal static void FireEmotionalBondResponse(EmotionalBondResponse p)     => OnEmotionalBondResponse?.Invoke(p);
        internal static void FirePersonalRoutineResponse(PersonalRoutineResponse p)  => OnPersonalRoutineResponse?.Invoke(p);
        internal static void FireClientQueryResponse(ClientQueryResponse p)          => OnClientQueryResponse?.Invoke(p);
    }
}
