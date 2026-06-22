using Bloomtown.Shared.Protocol;
using Bloomtown.Client.Network;
using Bloomtown.Client.Player;
using Bloomtown.Client.Entity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bloomtown.Client.Bootstrap
{
    /// <summary>
    /// Entry point utama game client. Taruh di scene awal (Bootstrap Scene).
    ///
    /// Cara setup di Unity:
    ///   1. Buat scene bernama "Bootstrap" (tambahkan ke Build Settings, index 0)
    ///   2. Buat empty GameObject "GameBootstrap" di scene itu, attach script ini
    ///   3. Assign _localPlayerPrefab di Inspector
    ///      → Prefab ini butuh: CharacterController + LocalPlayer component
    ///      → Opsional: child object Camera dengan tag "MainCamera"
    ///   4. Buat GameObject lain "Network", assign NetworkClient component
    ///   5. Buat GameObject "EntityRegistry", assign EntityRegistry component + dua prefab-nya
    ///   6. Play → GameBootstrap otomatis connect ke server dan spawn pemain setelah ConnectAccept diterima
    ///
    /// State machine sederhana: Idle → Connecting → Connected → InGame → Disconnected
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        // ── Inspector ───────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private NetworkClient  _networkClient;
        [SerializeField] private EntityRegistry _entityRegistry;

        [Header("Prefabs")]
        [Tooltip("Prefab pemain lokal. Harus ada: CharacterController + LocalPlayer")]
        [SerializeField] private GameObject _localPlayerPrefab;

        [Header("Scene")]
        [Tooltip("Nama scene game yang di-load setelah connect. Kosongkan kalau tidak pakai multi-scene.")]
        [SerializeField] private string _gameSceneName = "";

        private LocalPlayer _localPlayer;
        private bool _sceneLoaded;

        // ── Lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            SceneGroundBuilder.Apply();
            DontDestroyOnLoad(gameObject);
            ValidateReferences();
        }

        private void OnEnable()
        {
            NetworkEvents.OnConnected   += OnConnected;
            NetworkEvents.OnDisconnected += OnDisconnected;
            NetworkEvents.OnConnectAccept += OnConnectAccept;
        }

        private void OnDisable()
        {
            NetworkEvents.OnConnected    -= OnConnected;
            NetworkEvents.OnDisconnected -= OnDisconnected;
            NetworkEvents.OnConnectAccept -= OnConnectAccept;
        }

        private void Start()
        {
            Debug.Log("[GameBootstrap] State → Connecting");
            // NetworkClient._autoConnectOnStart = true di default-nya, jadi connect terjadi otomatis
        }

        // ── Network event handlers ───────────────────────────────────────────
        private void OnConnected()
        {
            Debug.Log("[GameBootstrap] State → Connected. Menunggu ConnectAccept dari server...");

            // Load game scene jika dikonfigurasi
            if (!string.IsNullOrEmpty(_gameSceneName) && !_sceneLoaded)
            {
                _sceneLoaded = true;
                SceneManager.LoadSceneAsync(_gameSceneName, LoadSceneMode.Additive);
            }
        }

        private void OnConnectAccept(ConnectAccept accept)
        {
            if (_localPlayerPrefab == null)
            {
                Debug.LogError("[GameBootstrap] _localPlayerPrefab belum di-assign di Inspector!");
                return;
            }

            // Spawn local player di spawn point dari server
            var spawnPos = new Vector3(accept.SpawnX, accept.SpawnY, accept.SpawnZ);
            var go       = Instantiate(_localPlayerPrefab, spawnPos, Quaternion.identity);
            go.name      = "LocalPlayer";

            _localPlayer = go.GetComponent<LocalPlayer>();
            if (_localPlayer == null)
            {
                Debug.LogError("[GameBootstrap] _localPlayerPrefab tidak punya component LocalPlayer!");
                Destroy(go);
                return;
            }

            _localPlayer.Init(accept);

            // Beritahu EntityRegistry ID pemain lokal agar tidak di-spawn sebagai remote entity
            _entityRegistry?.SetLocalEntityId(accept.EntityId);

            Debug.Log($"[GameBootstrap] State → InGame. LocalPlayer entity id = {accept.EntityId}");
        }

        private void OnDisconnected(string reason)
        {
            Debug.Log($"[GameBootstrap] State → Disconnected ({reason})");

            // Hapus local player
            if (_localPlayer != null)
            {
                Destroy(_localPlayer.gameObject);
                _localPlayer = null;
            }

            // Entity registry sudah dengarkan OnDisconnected sendiri → DespawnAll() otomatis
        }

        // ── Validation ───────────────────────────────────────────────────────
        private void ValidateReferences()
        {
            ResolveMissingPrefabs();

            if (_networkClient == null)
                Debug.LogError("[GameBootstrap] _networkClient belum di-assign!");
            if (_entityRegistry == null)
                Debug.LogWarning("[GameBootstrap] _entityRegistry belum di-assign (NPC/remote player tidak akan di-spawn).");
            if (_localPlayerPrefab == null)
                Debug.LogError("[GameBootstrap] _localPlayerPrefab belum di-assign! Jalankan Bloomtown → Rebuild Character Prefabs.");
        }

        private void ResolveMissingPrefabs()
        {
            if (_localPlayerPrefab != null) return;

            _localPlayerPrefab = Resources.Load<GameObject>("Prefabs/LocalPlayer");
            if (_localPlayerPrefab != null)
                Debug.LogWarning("[GameBootstrap] _localPlayerPrefab di-resolve dari Resources/Prefabs/LocalPlayer.");
        }
    }

    /// <summary>
    /// Membangun lantai dan suasana scene agar nyaman untuk pengembangan karakter.
    /// </summary>
    public static class SceneGroundBuilder
    {
        public static readonly Vector3 GroundCenter = new Vector3(14f, 0f, 12f);
        public static readonly Vector3 GroundScale  = new Vector3(22f, 1f, 22f);
        public const int TextureSize = 256;
        public const float TextureWorldRepeats = 9f;

        private static readonly Color SkyColor       = new Color(0.58f, 0.78f, 0.94f);
        private static readonly Color AmbientColor   = new Color(0.58f, 0.64f, 0.60f);
        private static readonly Color FogColor       = new Color(0.62f, 0.76f, 0.90f);
        private static readonly Color SunColor       = new Color(1.00f, 0.95f, 0.86f);

        public static void Apply()
        {
            ApplyGround();
            ApplyEnvironment();
            ApplyLighting();
        }

        public static Texture2D CreateGroundTexture()
        {
            var grassLight = new Color(0.40f, 0.58f, 0.32f);
            var grassMid   = new Color(0.33f, 0.50f, 0.27f);
            var grassDark  = new Color(0.27f, 0.42f, 0.22f);
            var dirt       = new Color(0.44f, 0.38f, 0.30f);
            var path       = new Color(0.50f, 0.44f, 0.34f);
            var gridLine   = new Color(0.20f, 0.30f, 0.18f);

            var tex = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, true)
            {
                wrapMode   = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name       = "BloomtownGroundTex",
            };

            const int gridCells = 8;
            var cell = TextureSize / gridCells;

            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    var nx = x / (float)TextureSize;
                    var ny = y / (float)TextureSize;

                    var n1 = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);
                    var n2 = Mathf.PerlinNoise(x * 0.19f + 17f, y * 0.19f + 31f);
                    var n3 = Mathf.PerlinNoise(x * 0.41f + 53f, y * 0.41f + 71f);

                    var color = Color.Lerp(grassMid, grassLight, n1);
                    color = Color.Lerp(color, grassDark, n2 * 0.45f);

                    if (n3 > 0.68f)
                        color = Color.Lerp(color, dirt, (n3 - 0.68f) * 1.6f);

                    var plaza = Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.5f));
                    if (plaza < 0.18f)
                        color = Color.Lerp(color, path, (0.18f - plaza) / 0.18f * 0.35f);

                    if (x % cell == 0 || y % cell == 0)
                        color = Color.Lerp(color, gridLine, 0.07f);

                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        public static Material CreateGroundMaterial(Texture2D texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return null;

            var mat = new Material(shader)
            {
                name = "BloomtownGroundMat",
            };

            if (shader.name.Contains("Universal"))
            {
                mat.SetTexture("_BaseMap", texture);
                mat.SetColor("_BaseColor", Color.white);
                mat.SetFloat("_Smoothness", 0.10f);
                mat.SetFloat("_Metallic", 0f);
            }
            else
            {
                mat.mainTexture = texture;
                mat.color = Color.white;
                mat.SetFloat("_Glossiness", 0.10f);
                mat.SetFloat("_Metallic", 0f);
            }

            var scale = new Vector2(TextureWorldRepeats, TextureWorldRepeats);
            if (mat.HasProperty("_BaseMap"))
                mat.SetTextureScale("_BaseMap", scale);
            else
                mat.mainTextureScale = scale;

            return mat;
        }

        private static void ApplyGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                Debug.LogWarning("[SceneGroundBuilder] Ground tidak ditemukan.");
                return;
            }

            ground.transform.position   = GroundCenter;
            ground.transform.localScale = GroundScale;

            var renderer = ground.GetComponent<Renderer>();
            if (renderer == null) return;

            var tex = CreateGroundTexture();
            var mat = CreateGroundMaterial(tex);
            if (mat != null)
                renderer.material = mat;
        }

        private static void ApplyEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 140f;
        }

        private static void ApplyLighting()
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type != LightType.Directional) continue;

                light.color     = SunColor;
                light.intensity = 1.12f;
                light.shadows   = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
                return;
            }
        }

        public static Color GetSkyColor() => SkyColor;
    }
}
