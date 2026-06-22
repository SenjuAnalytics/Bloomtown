using Bloomtown.Shared.Protocol;
using Bloomtown.Client.Bootstrap;
using Bloomtown.Client.Entity;
using Bloomtown.Client.Network;
using Bloomtown.Client.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using ProtocolPlayerInput = Bloomtown.Shared.Protocol.PlayerInput;

namespace Bloomtown.Client.Player
{
    /// <summary>
    /// Diattach ke GameObject pemain lokal.
    ///
    /// Tanggung jawab:
    ///   1. Baca input keyboard/mouse tiap frame (New Input System)
    ///   2. Gerakkan karakter secara lokal (client-side prediction)
    ///   3. Kirim PlayerInput ke server pada rate 20Hz (NetSendRate)
    ///   4. Terima koreksi posisi dari EntityDelta (reconciliation sederhana)
    ///
    /// Requires: CharacterController di GameObject yang sama.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class LocalPlayer : MonoBehaviour
    {
        // ── Inspector ───────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float _moveSpeed    = NetworkConstants.PlayerMoveSpeed;
        [SerializeField] private float _gravity      = -9.81f;
        [SerializeField] private float _groundDist   = 0.1f;
        [SerializeField] private LayerMask _groundMask = ~0;

        [Header("Camera Look")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _initialPitch = 20f;
        [SerializeField] private Transform _cameraRig;
        [Tooltip("Offset kamera relatif terhadap CameraRig (third-person). " +
                 "Z negatif = di belakang karakter. Atur di Inspector untuk tweaking.")]
        [SerializeField] private Vector3 _thirdPersonOffset = new Vector3(0f, 1f, -14f);
        [SerializeField] private float _cameraFov = 42f;

        [Header("Network")]
        [SerializeField] private float _serverCorrectionAlpha = 0.2f;

        // ── Runtime state ───────────────────────────────────────────────────
        private CharacterController _cc;
        private uint   _localEntityId;
        private uint   _inputSeq;
        private float  _sendTimer;
        private float  _sendInterval;
        private float  _yaw;
        private float  _pitch;
        private float  _verticalVelocity;
        private bool   _grounded;
        private Vector3 _serverPosition = Vector3.zero;
        private bool    _hasServerPosition;
        private Vector2 _lastMoveDir;
        private CharacterAnimator _characterAnimator;

        // ── Public API ──────────────────────────────────────────────────────
        public uint EntityId => _localEntityId;

        // ── Init ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _cc           = GetComponent<CharacterController>();
            _sendInterval = 1f / NetworkConstants.NetSendRate;

#if UNITY_EDITOR
            // Input langsung ke Game view saat Play (hindari Keyboard.current null di Editor).
            if (InputSystem.settings != null)
                InputSystem.settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
        }

        public void Init(ConnectAccept accept)
        {
            _localEntityId = accept.EntityId;

            _cc.enabled = false;
            transform.position = new Vector3(accept.SpawnX, accept.SpawnY, accept.SpawnZ);
            _cc.enabled = true;

            LockCursor();
            ApplyThirdPersonCamera();

            EntityBodyVisual.ApplyLocalPlayerBody(gameObject);
            CharacterCollisionBody.EnsureOn(gameObject);
            _characterAnimator = GetComponent<CharacterAnimator>();
            _characterAnimator?.RebindAnimator();

            var nameplate = GetComponent<EntityNameplate>() ?? gameObject.AddComponent<EntityNameplate>();
            nameplate.SetStyle(EntityNameplate.Style.LocalPlayer);
            nameplate.SetLabel("★ KAMU");

            ApplyInitialLookTowardVillage(accept.SpawnX, accept.SpawnZ);

            Debug.Log($"[LocalPlayer] Spawned entity {_localEntityId} at ({accept.SpawnX:F1}, {accept.SpawnY:F1}, {accept.SpawnZ:F1})");

            NetworkEvents.OnEntityDelta += OnEntityDelta;
        }

        private void OnDestroy()
        {
            NetworkEvents.OnEntityDelta -= OnEntityDelta;
            UnlockCursor();
        }

        // ── Update ──────────────────────────────────────────────────────────
        private void Update()
        {
            if (IsGameplayInputBlocked())
            {
                _lastMoveDir = Vector2.zero;
                _characterAnimator?.SetLocomotion(0f, Time.deltaTime);
                HandleNetworkSend();
                return;
            }

            HandleCursorToggle();
            HandleCameraLook();
            HandleMovement();
            HandleNetworkSend();
            ApplyServerCorrection();
        }

        private static bool IsGameplayInputBlocked()
        {
            return GameHUD.Instance != null && GameHUD.Instance.IsCommandOpen;
        }

        private void ApplyInitialLookTowardVillage(float spawnX, float spawnZ)
        {
            var toVillageX = NetworkConstants.VillageCenterX - spawnX;
            var toVillageZ = NetworkConstants.VillageCenterZ - spawnZ;

            if (toVillageX * toVillageX + toVillageZ * toVillageZ < 0.01f)
                _yaw = 0f;
            else
                _yaw = Mathf.Atan2(toVillageX, toVillageZ) * Mathf.Rad2Deg;

            _pitch = _initialPitch;
            transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);

            if (_cameraRig != null)
                _cameraRig.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void ApplyThirdPersonCamera()
        {
            if (_cameraRig == null)
            {
                Debug.LogWarning("[LocalPlayer] _cameraRig belum di-assign!");
                return;
            }

            var cam = _cameraRig.GetComponentInChildren<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("[LocalPlayer] Tidak ada Camera di bawah CameraRig!");
                return;
            }

            cam.transform.localPosition = _thirdPersonOffset;
            cam.fieldOfView             = _cameraFov;
            ApplyCameraSky(cam);
        }

        private static void ApplyCameraSky(Camera camera)
        {
            if (camera == null) return;

            camera.clearFlags      = CameraClearFlags.SolidColor;
            camera.backgroundColor = SceneGroundBuilder.GetSkyColor();
            RenderSettings.skybox  = null;
        }

        // ── Input: cursor ───────────────────────────────────────────────────
        private void HandleCursorToggle()
        {
            if (GameInput.WasPressed(Key.Escape))
                UnlockCursor();

            if (GameInput.WasPressed(Key.LeftCtrl) && Cursor.lockState != CursorLockMode.Locked)
                LockCursor();
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── Input: kamera ───────────────────────────────────────────────────
        private void HandleCameraLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            GameInput.ReadMouseLook(_mouseSensitivity, out var mouseX, out var mouseY);

            _yaw   += mouseX;
            _pitch  = Mathf.Clamp(_pitch - mouseY, -80f, 80f);

            transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);
            if (_cameraRig != null)
                _cameraRig.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // ── Input: movement + gravity ───────────────────────────────────────
        private void HandleMovement()
        {
            // FIX: Harus tambahkan _cc.center agar sphere check berada di dasar kapsul yang benar.
            // Tanpa _cc.center, dengan center=(0,1,0) height=2 radius=0.3:
            //   Posisi salah: transform.pos + Vector3.down*(1-0.3) = transform.pos - (0,0.7,0)  ← di bawah tanah
            //   Posisi benar: transform.pos + (0,1,0) + Vector3.down*(1-0.3) = transform.pos + (0,0.3,0)  ← tepat di dasar kapsul
            _grounded = Physics.CheckSphere(
                transform.position + _cc.center + Vector3.down * (_cc.height * 0.5f - _cc.radius),
                _groundDist + _cc.radius,
                _groundMask,
                QueryTriggerInteraction.Ignore);

            if (_grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _lastMoveDir = ReadMoveInput();

            Vector3 horizontal = transform.right * _lastMoveDir.x + transform.forward * _lastMoveDir.y;

            _verticalVelocity += _gravity * Time.deltaTime;
            Vector3 move = horizontal * (_moveSpeed * Time.deltaTime)
                         + Vector3.up * (_verticalVelocity * Time.deltaTime);

            _cc.Move(move);

            var animSpeed = _lastMoveDir.sqrMagnitude > 0.01f
                ? _lastMoveDir.magnitude * _moveSpeed
                : new Vector3(_cc.velocity.x, 0f, _cc.velocity.z).magnitude;
            _characterAnimator?.SetLocomotion(animSpeed, Time.deltaTime);
        }

        private static Vector2 ReadMoveInput() => GameInput.ReadMove();

        // ── Network send (rate-limited ke 20Hz) ────────────────────────────
        private void HandleNetworkSend()
        {
            _sendTimer += Time.deltaTime;
            if (_sendTimer < _sendInterval) return;
            _sendTimer -= _sendInterval;

            if (NetworkClient.Instance == null || !NetworkClient.Instance.IsConnected) return;

            unchecked { _inputSeq++; }

            var worldDir = Vector3.zero;
            if (_lastMoveDir.sqrMagnitude > 0.01f)
            {
                worldDir = transform.right * _lastMoveDir.x + transform.forward * _lastMoveDir.y;
                worldDir.y = 0f;
                worldDir.Normalize();
            }

            var input = new ProtocolPlayerInput
            {
                Seq      = _inputSeq,
                MoveDirX = worldDir.x,
                MoveDirY = worldDir.z,
                LookYaw  = _yaw,
            };
            NetworkClient.Instance.SendPlayerInput(input);
        }

        // ── Server reconciliation ───────────────────────────────────────────
        private void OnEntityDelta(EntityDelta delta)
        {
            if (delta.EntityId != _localEntityId) return;
            _serverPosition    = new Vector3(delta.PositionX, delta.PositionY, delta.PositionZ);
            _hasServerPosition = true;
        }

        private void ApplyServerCorrection()
        {
            if (!_hasServerPosition) return;

            // Jangan tarik posisi saat input gerak aktif — itu penyebab karakter "terseret".
            if (_lastMoveDir.sqrMagnitude > 0.01f)
                return;

            float dist = Vector3.Distance(transform.position, _serverPosition);
            if (dist <= 0.05f)
                return;

            if (dist > 2f)
            {
                _cc.enabled = false;
                transform.position = _serverPosition;
                _cc.enabled = true;
                Debug.Log($"[LocalPlayer] Hard correction: {dist:F1}m");
                return;
            }

            Vector3 corrected = Vector3.Lerp(transform.position, _serverPosition, _serverCorrectionAlpha);
            _cc.enabled = false;
            transform.position = corrected;
            _cc.enabled = true;
        }
    }

    /// <summary>
    /// Pembaca input terpusat via New Input System (project pakai Input System Package).
    /// </summary>
    public static class GameInput
    {
        private const float MouseDeltaScale = 0.1f;

        public static bool WasPressed(Key key)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        public static bool IsPressed(Key key)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].isPressed;
        }

        public static Vector2 ReadMove()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            float x = 0f, y = 0f;
            if (IsPressed(Key.A) || IsPressed(Key.LeftArrow))  x -= 1f;
            if (IsPressed(Key.D) || IsPressed(Key.RightArrow)) x += 1f;
            if (IsPressed(Key.S) || IsPressed(Key.DownArrow))  y -= 1f;
            if (IsPressed(Key.W) || IsPressed(Key.UpArrow))    y += 1f;

            var dir = new Vector2(x, y);
            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }

        public static void ReadMouseLook(float sensitivity, out float yawDelta, out float pitchDelta)
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                yawDelta   = 0f;
                pitchDelta = 0f;
                return;
            }

            var delta = mouse.delta.ReadValue();
            yawDelta   = delta.x * sensitivity * MouseDeltaScale;
            pitchDelta = delta.y * sensitivity * MouseDeltaScale;
        }
    }
}
