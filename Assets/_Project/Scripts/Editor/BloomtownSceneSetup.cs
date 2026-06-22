#if UNITY_EDITOR
using System.IO;
using Bloomtown.Client.Bootstrap;
using Bloomtown.Client.Entity;
using Bloomtown.Client.Network;
using Bloomtown.Client.Player;
using Bloomtown.Client.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bloomtown.Client.Editor
{
    /// <summary>
    /// Satu klik: buat prefab + scene Bootstrap siap Play.
    /// Menu: Bloomtown → Setup Playable Scene
    /// </summary>
    public static class BloomtownSceneSetup
    {
        private const string PrefabDir = "Assets/_Project/Prefabs";
        private const string ScenePath = "Assets/_Project/Scenes/Bootstrap.unity";

        private const string LocalPlayerPrefabPath  = PrefabDir + "/LocalPlayer.prefab";
        private const string LocalPlayerMatPath     = PrefabDir + "/LocalPlayer.mat";
        private const string NpcPrefabPath          = PrefabDir + "/NPC.prefab";
        private const string RemotePrefabPath       = PrefabDir + "/RemotePlayer.prefab";
        private const string GroundMatPath          = PrefabDir + "/Ground.mat";
        private const string GroundTexPath          = PrefabDir + "/GroundTexture.asset";

        [MenuItem("Bloomtown/Rebuild Character Prefabs")]
        public static void RebuildCharacterPrefabs()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            BloomtownCharacterImport.ConfigureCharacterAssets();
            BloomtownCharacterImport.PublishCharacterModelsToResources();

            var localPlayerPrefab = CreateOrLoadLocalPlayerPrefab();
            var npcPrefab         = CreateOrLoadHumanoidPrefab(NpcPrefabPath, "NPC");
            var remotePrefab      = CreateOrLoadHumanoidPrefab(RemotePrefabPath, "RemotePlayer");
            BloomtownCharacterImport.PublishGameplayPrefabsToResources(localPlayerPrefab, npcPrefab, remotePrefab);
            WireBootstrapSceneReferences(localPlayerPrefab, npcPrefab, remotePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var localOk  = AssetDatabase.LoadAssetAtPath<GameObject>(LocalPlayerPrefabPath) != null;
            var npcOk    = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath) != null;
            var remoteOk = AssetDatabase.LoadAssetAtPath<GameObject>(RemotePrefabPath) != null;

            if (localOk && npcOk && remoteOk)
            {
                Debug.Log("[Bloomtown] Prefab karakter di-rebuild dengan model Mixamo FBX.");
                EditorUtility.DisplayDialog(
                    "Prefab karakter diperbarui",
                    "LocalPlayer, NPC, dan RemotePlayer sekarang memakai PlayerModel.fbx / NpcModel.fbx.\n\n" +
                    "Tekan Play di Bootstrap.unity untuk melihat model baru.",
                    "OK");
            }
            else
            {
                Debug.LogError("[Bloomtown] Rebuild prefab gagal sebagian. Cek Console untuk error missing script / compile.");
                EditorUtility.DisplayDialog(
                    "Rebuild prefab gagal",
                    "Satu atau lebih prefab tidak tersimpan.\n\n" +
                    "1. Tunggu Unity selesai compile (tidak ada error merah di Console)\n" +
                    "2. Jalankan lagi: Bloomtown → Rebuild Character Prefabs",
                    "OK");
            }
        }

        [MenuItem("Bloomtown/Setup Playable Scene")]
        public static void SetupPlayableScene()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Scenes");

            BloomtownCharacterImport.ConfigureCharacterAssets();
            BloomtownCharacterImport.PublishCharacterModelsToResources();

            var localPlayerPrefab = CreateOrLoadLocalPlayerPrefab();
            var npcPrefab         = CreateOrLoadHumanoidPrefab(NpcPrefabPath, "NPC");
            var remotePrefab      = CreateOrLoadHumanoidPrefab(RemotePrefabPath, "RemotePlayer");
            BloomtownCharacterImport.PublishGameplayPrefabsToResources(localPlayerPrefab, npcPrefab, remotePrefab);

            BuildBootstrapScene(localPlayerPrefab, npcPrefab, remotePrefab);
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Bloomtown] Setup selesai. Buka Bootstrap.unity → jalankan server → tekan Play.");
            EditorUtility.DisplayDialog(
                "Bloomtown setup selesai",
                "1. Jalankan server:\n   dotnet run --project src/Bloomtown.Server\n\n" +
                "2. Buka scene Bootstrap.unity\n\n" +
                "3. Tekan Play\n\n" +
                "Kontrol: WASD gerak, Space lompat, mouse lihat, Esc lepas kursor, Ctrl kunci kursor.\n\n" +
                "Model 3D: PlayerModel.fbx + NpcModel.fbx (Mixamo).",
                "OK");
        }

        // ── Folder helper ────────────────────────────────────────────────────
        internal static void EnsureFolderPublic(string path) => EnsureFolder(path);

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folder = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folder)) return;

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folder);
        }

        // ── Shader helper (FIX: null check agar tidak crash di HDRP) ─────────
        private static Shader FindShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard")
                      ?? Shader.Find("Diffuse");

            if (shader == null)
                Debug.LogWarning("[BloomtownSceneSetup] Tidak ada shader URP/Lit maupun Standard. Material warna tidak akan diterapkan.");

            return shader;
        }

        // ── Material helper: buat material asset, bukan instance ─────────────
        // FIX: material HARUS disimpan sebagai Asset (CreateAsset) sebelum di-assign
        // ke prefab. Kalau pakai renderer.material langsung tanpa save, Unity akan
        // kehilangan material tersebut saat prefab di-reimport atau project di-restart.
        private static Material CreateOrLoadMaterialAsset(string matPath, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing != null)
            {
                existing.color = color;
                return existing;
            }

            var shader = FindShader();
            if (shader == null) return null;

            var mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
        }

        // ── LocalPlayer prefab ───────────────────────────────────────────────
        private static GameObject CreateOrLoadLocalPlayerPrefab()
        {
            var controller = BloomtownCharacterImport.LoadLocomotionController();
            var existing   = AssetDatabase.LoadAssetAtPath<GameObject>(LocalPlayerPrefabPath);

            var root = existing != null
                ? PrefabUtility.LoadPrefabContents(LocalPlayerPrefabPath)
                : BuildLocalPlayerRoot();

            EnsureLocalPlayerComponents(root);

            if (BloomtownCharacterImport.AttachModelVisual(root, BloomtownCharacterImport.PlayerModelPath, controller) == null)
                EntityBodyVisual.ApplyLocalPlayerBody(root);

            return SavePrefabContents(root, LocalPlayerPrefabPath);
        }

        private static GameObject BuildLocalPlayerRoot()
        {
            var root = new GameObject("LocalPlayer");
            root.AddComponent<CharacterController>();
            root.AddComponent<LocalPlayer>();

            var cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(root.transform, false);

            var cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(cameraRig.transform, false);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();

            return root;
        }

        private static void EnsureLocalPlayerComponents(GameObject root)
        {
            var cc = root.GetComponent<CharacterController>() ?? root.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 1f, 0f);

            var player = root.GetComponent<LocalPlayer>() ?? root.AddComponent<LocalPlayer>();
            CharacterCollisionBody.EnsureOn(root);

            var cameraRig = root.transform.Find("CameraRig");
            if (cameraRig == null)
            {
                var cameraRigGo = new GameObject("CameraRig");
                cameraRigGo.transform.SetParent(root.transform, false);
                cameraRig = cameraRigGo.transform;
            }

            cameraRig.localPosition = new Vector3(0f, 1.5f, 0f);

            var cameraTransform = cameraRig.Find("Camera");
            if (cameraTransform == null)
            {
                var cameraGo = new GameObject("Camera");
                cameraGo.transform.SetParent(cameraRig, false);
                cameraTransform = cameraGo.transform;
            }

            cameraTransform.localPosition = new Vector3(0f, 1f, -14f);
            cameraTransform.gameObject.tag = "MainCamera";

            var camera = cameraTransform.GetComponent<Camera>() ?? cameraTransform.gameObject.AddComponent<Camera>();
            camera.fieldOfView = 42f;

            var so = new SerializedObject(player);
            so.FindProperty("_cameraRig").objectReferenceValue = cameraRig;
            so.FindProperty("_thirdPersonOffset").vector3Value = new Vector3(0f, 1f, -14f);
            so.FindProperty("_cameraFov").floatValue           = 42f;
            so.FindProperty("_initialPitch").floatValue          = 20f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── NPC / RemotePlayer prefab ────────────────────────────────────────
        private static GameObject CreateOrLoadHumanoidPrefab(string prefabPath, string objName)
        {
            var controller = BloomtownCharacterImport.LoadLocomotionController();
            var modelPath  = objName == "NPC"
                ? BloomtownCharacterImport.NpcModelPath
                : BloomtownCharacterImport.PlayerModelPath;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var root = existing != null
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : new GameObject(objName);

            if (root.GetComponent<EntityInterpolator>() == null)
                root.AddComponent<EntityInterpolator>();

            CharacterCollisionBody.EnsureOn(root);

            CharacterVisualFactory.HideLegacyRootMesh(root);

            if (BloomtownCharacterImport.AttachModelVisual(root, modelPath, controller) == null)
            {
                if (objName == "NPC")
                    EntityBodyVisual.ApplyNpcBody(root, 10001);
                else
                    EntityBodyVisual.ApplyRemotePlayerBody(root, 2);
            }

            return SavePrefabContents(root, prefabPath);
        }

        private static GameObject SavePrefabContents(GameObject root, string prefabPath)
        {
            PreparePrefabForSave(root);

            var success = false;
            var prefab  = PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
            PrefabUtility.UnloadPrefabContents(root);

            if (!success)
                Debug.LogError($"[Bloomtown] Gagal menyimpan prefab: {prefabPath}");

            return prefab;
        }

        private static void PreparePrefabForSave(GameObject root)
        {
            StripMissingScriptsRecursive(root);
            if (root.GetComponent<CharacterAnimator>() == null)
                root.AddComponent<CharacterAnimator>();
        }

        private static void StripMissingScriptsRecursive(GameObject root)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.gameObject == root) continue;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            }
        }

        private static void WireBootstrapSceneReferences(
            GameObject localPlayerPrefab,
            GameObject npcPrefab,
            GameObject remotePrefab)
        {
            var sceneAssetPath = Path.Combine(Application.dataPath, "..", ScenePath).Replace('\\', '/');
            if (!File.Exists(sceneAssetPath))
            {
                Debug.LogWarning("[Bloomtown] Bootstrap.unity tidak ditemukan — lewati wiring scene.");
                return;
            }

            var scene    = EditorSceneManager.GetSceneByPath(ScenePath);
            var wasOpen  = scene.isLoaded;
            if (!wasOpen)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                var bootstrapSo = new SerializedObject(bootstrap);
                bootstrapSo.FindProperty("_localPlayerPrefab").objectReferenceValue = localPlayerPrefab;
                bootstrapSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var registry = Object.FindFirstObjectByType<EntityRegistry>();
            if (registry != null)
            {
                var registrySo = new SerializedObject(registry);
                registrySo.FindProperty("_npcPrefab").objectReferenceValue          = npcPrefab;
                registrySo.FindProperty("_remotePlayerPrefab").objectReferenceValue = remotePrefab;
                registrySo.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (!wasOpen)
                EditorSceneManager.CloseScene(scene, true);
        }

        // ── Bootstrap scene ──────────────────────────────────────────────────
        private static void BuildBootstrapScene(
            GameObject localPlayerPrefab,
            GameObject npcPrefab,
            GameObject remotePrefab)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ground — tekstur rumput procedural + area desa lebih luas
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = SceneGroundBuilder.GroundScale;
            ground.transform.position   = SceneGroundBuilder.GroundCenter;

            var groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                var groundTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GroundTexPath);
                if (groundTex == null)
                {
                    groundTex = SceneGroundBuilder.CreateGroundTexture();
                    AssetDatabase.CreateAsset(groundTex, GroundTexPath);
                }

                var groundMat = AssetDatabase.LoadAssetAtPath<Material>(GroundMatPath);
                if (groundMat == null)
                {
                    groundMat = SceneGroundBuilder.CreateGroundMaterial(groundTex);
                    if (groundMat != null)
                        AssetDatabase.CreateAsset(groundMat, GroundMatPath);
                }

                if (groundMat != null)
                    groundRenderer.sharedMaterial = groundMat;
                else
                    groundRenderer.sharedMaterial = SceneGroundBuilder.CreateGroundMaterial(groundTex);
            }

            // Directional light
            var lightGo = new GameObject("Directional Light");
            var light   = lightGo.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1.12f;
            light.color     = new Color(1f, 0.95f, 0.86f);
            light.shadows   = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            // Network
            var networkGo     = new GameObject("Network");
            var networkClient = networkGo.AddComponent<NetworkClient>();
            var networkSo     = new SerializedObject(networkClient);
            networkSo.FindProperty("_serverHost").stringValue         = "127.0.0.1";
            networkSo.FindProperty("_serverPort").intValue            = 7777;
            networkSo.FindProperty("_autoConnectOnStart").boolValue   = true;
            networkSo.ApplyModifiedPropertiesWithoutUndo();

            // EntityRegistry
            var registryGo = new GameObject("EntityRegistry");
            var registry   = registryGo.AddComponent<EntityRegistry>();
            var registrySo = new SerializedObject(registry);
            registrySo.FindProperty("_npcPrefab").objectReferenceValue          = npcPrefab;
            registrySo.FindProperty("_remotePlayerPrefab").objectReferenceValue = remotePrefab;
            registrySo.ApplyModifiedPropertiesWithoutUndo();

            // GameBootstrap
            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap   = bootstrapGo.AddComponent<GameBootstrap>();
            var bootstrapSo = new SerializedObject(bootstrap);
            bootstrapSo.FindProperty("_networkClient").objectReferenceValue   = networkClient;
            bootstrapSo.FindProperty("_entityRegistry").objectReferenceValue  = registry;
            bootstrapSo.FindProperty("_localPlayerPrefab").objectReferenceValue = localPlayerPrefab;
            bootstrapSo.FindProperty("_gameSceneName").stringValue            = "";
            bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

            // HUD
            var hudGo = new GameObject("GameHUD");
            hudGo.AddComponent<GameHUD>();

            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
        }

        // ── Build Settings ────────────────────────────────────────────────────
        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            var found = false;
            for (var i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path != scenePath) continue;
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                found = true;
                break;
            }

            if (!found)
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    /// <summary>
    /// Konfigurasi import FBX Mixamo + Animator Controller locomotion.
    /// </summary>
    public static class BloomtownCharacterImport
    {
        public const string PlayerModelPath = "Assets/_Project/Art/Characters/Models/PlayerModel.fbx";
        public const string NpcModelPath    = "Assets/_Project/Art/Characters/Models/NpcModel.fbx";
        public const string ControllerPath  = "Assets/_Project/Art/Characters/Animators/CharacterLocomotion.controller";

        [MenuItem("Bloomtown/Configure Character Assets")]
        public static void ConfigureCharacterAssets()
        {
            EnsureArtFolder("Assets/_Project/Art/Characters/Models");
            EnsureArtFolder("Assets/_Project/Art/Characters/Animators");

            MixamoMaterialUtility.ConfigureModelMaterials(PlayerModelPath);
            MixamoMaterialUtility.ConfigureModelMaterials(NpcModelPath);

            var walkClip = FindWalkClip(PlayerModelPath) ?? FindWalkClip(NpcModelPath);
            if (walkClip == null)
            {
                Debug.LogWarning("[Bloomtown] Walk clip tidak ditemukan di FBX. Animator mungkin kosong.");
                return;
            }

            CreateLocomotionController(walkClip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Bloomtown] Character assets dikonfigurasi (Humanoid rig + CharacterLocomotion.controller).");
        }

        public static RuntimeAnimatorController LoadLocomotionController()
        {
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        }

        public static GameObject AttachModelVisual(
            GameObject root,
            string modelPath,
            RuntimeAnimatorController controller,
            float modelYawOffset = 0f)
        {
            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelPrefab == null)
            {
                Debug.LogWarning($"[Bloomtown] Model tidak ditemukan: {modelPath}");
                return null;
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

            CharacterVisualFactory.RemoveExistingVisual(root);
            CharacterVisualFactory.EnsureSingleVisual(root);
            CharacterVisualFactory.HideLegacyRootMesh(root);

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, root.transform);
            visual.name = CharacterVisualFactory.CharacterVisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, modelYawOffset, 0f);
            visual.transform.localScale    = Vector3.one;

            foreach (var col in visual.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(col);

            var animator = visual.GetComponent<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();

            animator.applyRootMotion           = false;
            animator.runtimeAnimatorController = controller;

            if (root.GetComponent<CharacterAnimator>() == null)
                root.AddComponent<CharacterAnimator>();

            MixamoVisualUtility.ApplyToVisual(visual);

            return visual;
        }

        public static void PublishGameplayPrefabsToResources(
            GameObject localPlayerPrefab,
            GameObject npcPrefab,
            GameObject remotePrefab)
        {
            BloomtownSceneSetup.EnsureFolderPublic("Assets/_Project/Resources/Prefabs");
            CopyAssetToResources(localPlayerPrefab, "Assets/_Project/Resources/Prefabs/LocalPlayer.prefab");
            CopyAssetToResources(npcPrefab, "Assets/_Project/Resources/Prefabs/NPC.prefab");
            CopyAssetToResources(remotePrefab, "Assets/_Project/Resources/Prefabs/RemotePlayer.prefab");
        }

        public static void PublishCharacterModelsToResources()
        {
            BloomtownSceneSetup.EnsureFolderPublic("Assets/_Project/Resources");
            BloomtownSceneSetup.EnsureFolderPublic("Assets/_Project/Resources/Characters");

            SaveModelPrefabToResources(PlayerModelPath, "Assets/_Project/Resources/Characters/PlayerModel.prefab");
            SaveModelPrefabToResources(NpcModelPath, "Assets/_Project/Resources/Characters/NpcModel.prefab");

            var controller = LoadLocomotionController();
            if (controller != null)
                CopyAssetToResources(controller, "Assets/_Project/Resources/Characters/CharacterLocomotion.controller");
        }

        private static void SaveModelPrefabToResources(string modelPath, string resourcePrefabPath)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (source == null)
            {
                Debug.LogWarning($"[Bloomtown] Lewati publish Resources — model tidak bisa dimuat: {modelPath}");
                return;
            }

            var resourceDir = Path.GetDirectoryName(resourcePrefabPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(resourceDir))
                BloomtownSceneSetup.EnsureFolderPublic(resourceDir);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(resourcePrefabPath) != null)
                AssetDatabase.DeleteAsset(resourcePrefabPath);

            var temp = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                var success = false;
                PrefabUtility.SaveAsPrefabAsset(temp, resourcePrefabPath, out success);
                if (!success)
                    Debug.LogError($"[Bloomtown] Gagal menyimpan prefab Resources: {resourcePrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(temp);
            }
        }

        private static void CopyAssetToResources(Object source, string destinationPath)
        {
            if (source == null) return;

            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogWarning($"[Bloomtown] Gagal menyalin asset ke Resources — source tanpa path: {destinationPath}");
                return;
            }

            var resourceDir = Path.GetDirectoryName(destinationPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(resourceDir))
                BloomtownSceneSetup.EnsureFolderPublic(resourceDir);

            if (AssetDatabase.LoadAssetAtPath<Object>(destinationPath) != null)
                AssetDatabase.DeleteAsset(destinationPath);

            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
                Debug.LogWarning($"[Bloomtown] Gagal menyalin asset ke Resources: {destinationPath}");
        }

        private static AnimationClip FindWalkClip(string modelPath)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (asset is not AnimationClip clip) continue;
                if (clip.name.StartsWith("__")) continue;
                return clip;
            }

            return null;
        }

        private static void CreateLocomotionController(AnimationClip walkClip)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

            var root = controller.layers[0].stateMachine;
            var walk = root.AddState("Walk", new Vector3(300f, 0f, 0f));
            walk.motion = walkClip;
            root.defaultState = walk;

            EditorUtility.SetDirty(controller);
        }

        private static void EnsureArtFolder(string path)
        {
            BloomtownSceneSetup.EnsureFolderPublic(path);
        }
    }
}
#endif
