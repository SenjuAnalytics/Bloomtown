using System.Collections.Generic;
using Bloomtown.Shared.Protocol;
using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Diattach ke setiap remote entity (NPC atau player lain).
    /// Menerima EntityDelta dari EntityRegistry, menyimpannya di buffer,
    /// dan menginterpolasi posisi/rotasi secara halus antara dua snapshot terakhir.
    ///
    /// Render lag: sengaja di-delay sedikit (InterpolationDelay) agar selalu ada dua titik
    /// untuk diinterpolasi. Ini pola standar yang sama dipakai Valve (Source Engine) dan Unity Netcode.
    /// </summary>
    public sealed class EntityInterpolator : MonoBehaviour
    {
        // ── Inspector ───────────────────────────────────────────────────────
        [Header("Interpolation")]
        [Tooltip("Berapa detik di belakang realtime. Harus > (1 / server tick rate). Default 0.1s untuk 20Hz server.")]
        [SerializeField] private float _interpolationDelay = 0.1f;

        [Tooltip("Jarak maksimum sebelum hard-teleport daripada lerp.")]
        [SerializeField] private float _teleportThreshold = 5f;

        [Tooltip("Derajat/detik saat berdiri — rotasi mengikuti yaw server.")]
        [SerializeField] private float _idleTurnRateDegPerSec = 360f;

        [Tooltip("Kecepatan minimum (m/s) agar tubuh menghadap arah langkah.")]
        [SerializeField] private float _facingMoveSpeedThreshold = 0.08f;

        // ── Snapshot buffer ─────────────────────────────────────────────────
        private struct Snapshot
        {
            public float   Time;
            public Vector3 Position;
            public float   Yaw;
        }

        // Circular buffer kecil — cukup simpan beberapa detik terakhir
        private readonly LinkedList<Snapshot> _buffer = new LinkedList<Snapshot>();
        private const int MaxBufferSize = 30; // 30 snapshot @ 20Hz = 1.5 detik history

        private CharacterAnimator _characterAnimator;
        private Vector3 _lastAnimPosition;
        private bool _hasAnimPosition;

        // ── Public API ──────────────────────────────────────────────────────
        public uint EntityId { get; private set; }
        public bool HasReceivedAnyDelta { get; private set; }

        public void Init(uint entityId)
        {
            EntityId = entityId;
            _characterAnimator = GetComponent<CharacterAnimator>();
            _hasAnimPosition   = false;
        }

        /// <summary>Dipanggil oleh EntityRegistry setiap kali server mengirim EntityDelta untuk entity ini.</summary>
        public void PushDelta(EntityDelta delta)
        {
            HasReceivedAnyDelta = true;

            // Kalau buffer penuh, hapus yang paling lama
            if (_buffer.Count >= MaxBufferSize)
                _buffer.RemoveFirst();

            _buffer.AddLast(new Snapshot
            {
                Time     = Time.time,
                Position = EntityVisualConstants.ServerToVisual(delta.PositionX, delta.PositionY, delta.PositionZ),
                Yaw      = delta.RotationYaw,
            });
        }

        // ── Update: interpolasi posisi ───────────────────────────────────────
        private void Update()
        {
            if (_buffer.Count < 2) return;

            float renderTime = Time.time - _interpolationDelay;

            // Cari dua snapshot yang mengapit renderTime
            Snapshot from = default;
            Snapshot to   = default;
            bool found = false;

            var node = _buffer.First;
            while (node?.Next != null)
            {
                if (node.Value.Time <= renderTime && node.Next.Value.Time >= renderTime)
                {
                    from  = node.Value;
                    to    = node.Next.Value;
                    found = true;
                    break;
                }
                node = node.Next;
            }

            if (!found)
            {
                // renderTime lebih baru dari semua snapshot → pakai snapshot terbaru
                to   = _buffer.Last.Value;
                from = _buffer.Last.Previous?.Value ?? to;
            }

            // Hitung t (0..1) antara from dan to
            float span = to.Time - from.Time;
            float t    = span > 0.0001f ? Mathf.Clamp01((renderTime - from.Time) / span) : 1f;

            // Posisi interpolasi
            Vector3 targetPos = Vector3.Lerp(from.Position, to.Position, t);
            float   dist      = Vector3.Distance(transform.position, targetPos);
            var     prevPos   = transform.position;

            // FIX: Teleport threshold benar-benar berbeda dengan smooth lerp
            if (dist > _teleportThreshold)
            {
                // Hard snap langsung ke posisi server terbaru (entity pindah jauh / respawn)
                transform.position = to.Position;
            }
            else
            {
                // Smooth interpolasi menuju posisi yang sudah di-lerp antara dua snapshot
                transform.position = targetPos;
            }

            UpdateFacing(prevPos, from, to, t);
            UpdateLocomotionAnimation();
        }

        private void UpdateFacing(Vector3 previousPosition, Snapshot from, Snapshot to, float t)
        {
            var delta = transform.position - previousPosition;
            delta.y = 0f;

            var dt = Mathf.Max(Time.deltaTime, 0.0001f);
            var moveSpeed = delta.magnitude / dt;

            if (moveSpeed >= _facingMoveSpeedThreshold)
            {
                // Hadap arah langkah — cegah animasi jalan "mundur" (moonwalk).
                var moveYaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, moveYaw, 0f);
                return;
            }

            var serverYaw = Mathf.LerpAngle(from.Yaw, to.Yaw, t);
            var currentYaw = transform.eulerAngles.y;
            var maxStep = _idleTurnRateDegPerSec * dt;
            var idleYaw = Mathf.MoveTowardsAngle(currentYaw, serverYaw, maxStep);
            transform.rotation = Quaternion.Euler(0f, idleYaw, 0f);
        }

        private void UpdateLocomotionAnimation()
        {
            if (_characterAnimator == null) return;

            if (!_hasAnimPosition)
            {
                _lastAnimPosition = transform.position;
                _hasAnimPosition  = true;
                _characterAnimator.SetLocomotion(0f, Time.deltaTime);
                return;
            }

            var speed = (transform.position - _lastAnimPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _characterAnimator.SetLocomotion(speed, Time.deltaTime);
            _lastAnimPosition = transform.position;
        }

        // Hapus snapshot yang sudah terlalu lama di tiap LateUpdate
        private void LateUpdate()
        {
            float cutoff = Time.time - _interpolationDelay - 1f; // simpan 1 detik ekstra sebagai buffer
            while (_buffer.Count > 2 && _buffer.First.Value.Time < cutoff)
                _buffer.RemoveFirst();
        }
    }
}
