using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Konstanta visual karakter manusia (~1.9m tinggi, pivot di kaki).
    /// </summary>
    public static class EntityVisualConstants
    {
        public const float CharacterHeight = 1.9f;
        public const float NameplateGapAboveHead = 0.2f;

        public static float NameplateOffsetFeetPivot =>
            CharacterHeight + NameplateGapAboveHead;

        public static Vector3 ServerToVisual(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
    }

    public struct HumanoidColors
    {
        public Color Skin;
        public Color Shirt;
        public Color Pants;

        public static HumanoidColors FromAccent(Color accent)
        {
            return new HumanoidColors
            {
                Skin  = new Color(0.93f, 0.78f, 0.64f),
                Shirt = accent,
                Pants = Color.Lerp(accent, new Color(0.12f, 0.10f, 0.14f), 0.55f),
            };
        }

        public static HumanoidColors LocalPlayer => FromAccent(new Color(1f, 0.45f, 0.1f));

        public static HumanoidColors Npc(uint entityId)
        {
            var hue = (entityId % 12) / 12f;
            return FromAccent(Color.HSVToRGB(hue, 0.42f, 0.92f));
        }

        public static HumanoidColors RemotePlayer =>
            FromAccent(new Color(0.25f, 0.45f, 0.95f));
    }

    /// <summary>
    /// Membangun tubuh manusia sederhana (kepala, torso, lengan, kaki) dari primitive Unity.
    /// </summary>
    public static class HumanoidBodyBuilder
    {
        public static void Build(GameObject root, HumanoidColors colors)
        {
            ClearLegacyVisuals(root);

            var rootRenderer = root.GetComponent<Renderer>();
            if (rootRenderer != null)
                rootRenderer.enabled = false;

            var humanoid = new GameObject("Humanoid");
            humanoid.transform.SetParent(root.transform, false);
            humanoid.transform.localPosition = Vector3.zero;
            humanoid.transform.localRotation = Quaternion.identity;

            CreatePart(humanoid.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.68f, 0f), new Vector3(0.36f, 0.36f, 0.36f), colors.Skin);

            CreatePart(humanoid.transform, "Torso", PrimitiveType.Cube,
                new Vector3(0f, 1.20f, 0f), new Vector3(0.50f, 0.62f, 0.28f), colors.Shirt);

            CreatePart(humanoid.transform, "UpperArm_L", PrimitiveType.Cube,
                new Vector3(-0.36f, 1.34f, 0f), new Vector3(0.26f, 0.12f, 0.12f), colors.Shirt);
            CreatePart(humanoid.transform, "LowerArm_L", PrimitiveType.Cube,
                new Vector3(-0.54f, 1.16f, 0f), new Vector3(0.22f, 0.10f, 0.10f), colors.Skin);

            CreatePart(humanoid.transform, "UpperArm_R", PrimitiveType.Cube,
                new Vector3(0.36f, 1.34f, 0f), new Vector3(0.26f, 0.12f, 0.12f), colors.Shirt);
            CreatePart(humanoid.transform, "LowerArm_R", PrimitiveType.Cube,
                new Vector3(0.54f, 1.16f, 0f), new Vector3(0.22f, 0.10f, 0.10f), colors.Skin);

            CreatePart(humanoid.transform, "UpperLeg_L", PrimitiveType.Cube,
                new Vector3(-0.14f, 0.78f, 0f), new Vector3(0.16f, 0.40f, 0.16f), colors.Pants);
            CreatePart(humanoid.transform, "LowerLeg_L", PrimitiveType.Cube,
                new Vector3(-0.14f, 0.36f, 0f), new Vector3(0.14f, 0.36f, 0.14f), colors.Pants);

            CreatePart(humanoid.transform, "UpperLeg_R", PrimitiveType.Cube,
                new Vector3(0.14f, 0.78f, 0f), new Vector3(0.16f, 0.40f, 0.16f), colors.Pants);
            CreatePart(humanoid.transform, "LowerLeg_R", PrimitiveType.Cube,
                new Vector3(0.14f, 0.36f, 0f), new Vector3(0.14f, 0.36f, 0.14f), colors.Pants);

            CreatePart(humanoid.transform, "Foot_L", PrimitiveType.Cube,
                new Vector3(-0.14f, 0.06f, 0.05f), new Vector3(0.16f, 0.08f, 0.24f), colors.Pants);
            CreatePart(humanoid.transform, "Foot_R", PrimitiveType.Cube,
                new Vector3(0.14f, 0.06f, 0.05f), new Vector3(0.16f, 0.08f, 0.24f), colors.Pants);
        }

        private static void ClearLegacyVisuals(GameObject root)
        {
            var humanoid = root.transform.Find("Humanoid");
            if (humanoid != null)
                DestroyObject(humanoid.gameObject);

            var body = root.transform.Find("Body");
            if (body != null)
                DestroyObject(body.gameObject);
        }

        private static void DestroyObject(Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(obj);
                return;
            }
#endif
            Object.Destroy(obj);
        }

        private static void CreatePart(
            Transform parent,
            string name,
            PrimitiveType type,
            Vector3 localPos,
            Vector3 localScale,
            Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                DestroyObject(collider);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                TintRenderer(renderer, color);
        }

        private static void TintRenderer(Renderer renderer, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;

            renderer.material = new Material(shader) { color = color };
        }
    }

    /// <summary>
    /// Warna tubuh entity agar mudah dibedakan di scene.
    /// </summary>
    public static class EntityBodyVisual
    {
        public static void ApplyLocalPlayerBody(GameObject root)
        {
            if (TryApplyFbxVisual(root, useNpcModel: false)) return;
            HumanoidBodyBuilder.Build(root, HumanoidColors.LocalPlayer);
        }

        public static void ApplyNpcBody(GameObject root, uint entityId)
        {
            if (TryApplyFbxVisual(root, useNpcModel: true)) return;
            HumanoidBodyBuilder.Build(root, HumanoidColors.Npc(entityId));
        }

        public static void ApplyRemotePlayerBody(GameObject root, uint entityId)
        {
            if (TryApplyFbxVisual(root, useNpcModel: false)) return;
            HumanoidBodyBuilder.Build(root, HumanoidColors.RemotePlayer);
        }

        public static bool HasFbxVisual(GameObject root)
        {
            return root.transform.Find(CharacterVisualFactory.CharacterVisualName) != null;
        }

        private static bool TryApplyFbxVisual(GameObject root, bool useNpcModel)
        {
            if (HasFbxVisual(root))
            {
                var visual = root.transform.Find(CharacterVisualFactory.CharacterVisualName);
                if (visual != null)
                    MixamoVisualUtility.ApplyToVisual(visual.gameObject);
                return true;
            }

            return CharacterVisualFactory.AttachFromResources(root, useNpcModel) != null;
        }
    }
}