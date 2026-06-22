using System.Collections.Generic;
using Bloomtown.Shared.Protocol;
using Bloomtown.Client.Network;
using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Mengelola semua entity remote di scene (NPC dan pemain lain).
    /// Entity pemain lokal TIDAK dikelola di sini — itu tanggung jawab GameBootstrap + LocalPlayer.
    ///
    /// Flow:
    ///   1. NetworkEvents.OnEntityDelta diterima
    ///   2. Kalau entity belum ada di registry → spawn dari prefab
    ///   3. Kalau entity sudah ada → push delta ke EntityInterpolator-nya
    ///
    /// EntityId ≥ NpcEntityIds.IdOffset (10000) → NPC; pakai _npcPrefab
    /// EntityId < NpcEntityIds.IdOffset         → remote player; pakai _remotePlayerPrefab
    /// </summary>
    public sealed class EntityRegistry : MonoBehaviour
    {
        // ── Inspector ───────────────────────────────────────────────────────
        [Header("Prefabs")]
        [Tooltip("Prefab untuk NPC (harus punya EntityInterpolator component)")]
        [SerializeField] private GameObject _npcPrefab;

        [Tooltip("Prefab untuk remote player (harus punya EntityInterpolator component)")]
        [SerializeField] private GameObject _remotePlayerPrefab;

        [Header("Debug")]
        [SerializeField] private bool _logSpawnDespawn = true;

        // ── State ───────────────────────────────────────────────────────────
        private readonly Dictionary<uint, EntityInterpolator> _entities = new();
        private uint _localEntityId; // set oleh GameBootstrap agar EntityDelta pemain lokal tidak di-spawn ulang

        // ── Lifecycle ───────────────────────────────────────────────────────
        private void OnEnable()
        {
            NetworkEvents.OnEntityDelta += OnEntityDelta;
            NetworkEvents.OnDisconnected += OnDisconnected;
        }

        private void OnDisable()
        {
            NetworkEvents.OnEntityDelta -= OnEntityDelta;
            NetworkEvents.OnDisconnected -= OnDisconnected;
        }

        // ── Public API ──────────────────────────────────────────────────────
        /// <summary>Dipanggil GameBootstrap setelah ConnectAccept agar entity lokal tidak di-spawn sebagai remote.</summary>
        public void SetLocalEntityId(uint id) => _localEntityId = id;

        /// <summary>Cek apakah entity dengan id tertentu sudah ada di registry.</summary>
        public bool Has(uint entityId) => _entities.ContainsKey(entityId);

        /// <summary>Hapus semua entity dari scene (misalnya saat disconnect).</summary>
        public void DespawnAll()
        {
            foreach (var kv in _entities)
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            _entities.Clear();
        }

        // ── Event handlers ───────────────────────────────────────────────────
        private void OnEntityDelta(EntityDelta delta)
        {
            // Jangan proses entity lokal sendiri di sini
            if (delta.EntityId == _localEntityId) return;

            if (!_entities.TryGetValue(delta.EntityId, out var interpolator))
            {
                interpolator = SpawnEntity(delta);
                if (interpolator == null) return; // prefab tidak di-assign
            }

            interpolator.PushDelta(delta);
        }

        private void OnDisconnected(string reason)
        {
            DespawnAll();
        }

        // ── Spawn ────────────────────────────────────────────────────────────
        private EntityInterpolator SpawnEntity(EntityDelta delta)
        {
            bool isNpc = NpcEntityIds.IsNpc(delta.EntityId);
            GameObject prefab = isNpc ? _npcPrefab : _remotePlayerPrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"[EntityRegistry] Prefab untuk {(isNpc ? "NPC" : "RemotePlayer")} belum di-assign di Inspector!");
                return null;
            }

            var spawnPos = EntityVisualConstants.ServerToVisual(delta.PositionX, delta.PositionY, delta.PositionZ);
            var spawnRot = Quaternion.Euler(0f, delta.RotationYaw, 0f);
            var go       = Instantiate(prefab, spawnPos, spawnRot);

            // Beri nama yang readable di Hierarchy
            string displayName = isNpc
                ? NpcNameLookup.GetDisplayNameOrDefault(delta.EntityId)
                : $"Player_{delta.EntityId}";
            go.name = displayName;

            var interpolator = go.GetComponent<EntityInterpolator>();
            if (interpolator == null)
            {
                Debug.LogError($"[EntityRegistry] Prefab '{prefab.name}' tidak punya EntityInterpolator component!");
                Destroy(go);
                return null;
            }

            interpolator.Init(delta.EntityId);
            CharacterCollisionBody.EnsureOn(go);
            _entities[delta.EntityId] = interpolator;

            if (isNpc)
                EntityBodyVisual.ApplyNpcBody(go, delta.EntityId);
            else
                EntityBodyVisual.ApplyRemotePlayerBody(go, delta.EntityId);

            var nameplate = go.GetComponent<EntityNameplate>() ?? go.AddComponent<EntityNameplate>();
            nameplate.SetStyle(isNpc ? EntityNameplate.Style.Npc : EntityNameplate.Style.RemotePlayer);
            nameplate.SetLabel(displayName);

            if (_logSpawnDespawn)
                Debug.Log($"[EntityRegistry] Spawned {(isNpc ? "NPC" : "RemotePlayer")} '{displayName}' (id={delta.EntityId}) @ {spawnPos}");

            return interpolator;
        }
    }
}
