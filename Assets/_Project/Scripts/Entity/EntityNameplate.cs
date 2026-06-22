using UnityEngine;
using UnityEngine.Rendering;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Label nama di atas kepala entity — kontras tinggi, bayangan hitam, ukuran per tipe.
    /// </summary>
    public sealed class EntityNameplate : MonoBehaviour
    {
        public enum Style
        {
            LocalPlayer,
            Npc,
            RemotePlayer,
        }

        private static readonly Vector3 ShadowOffset = new Vector3(0.008f, -0.008f, 0.004f);

        private const float BaseTextSize = 0.031f;

        // Semua karakter: membesar sedikit saat jauh agar tetap terbaca di kamera third-person.
        private const float DistanceScaleNearMeters = 4f;
        private const float DistanceScaleFarMeters  = 20f;
        private const float DistanceScaleMin        = 1f;
        private const float DistanceScaleMax        = 2.3f;

        [SerializeField] private Vector3 _localOffset = new Vector3(0f, EntityVisualConstants.NameplateOffsetFeetPivot, 0f);

        private Transform _labelRoot;
        private TextMesh _textMesh;
        private TextMesh _shadowMesh;
        private Style _style = Style.Npc;

        public void SetLabel(string label)
        {
            EnsureBuilt();
            _localOffset = GetOffsetForStyle(_style);
            var text = label ?? string.Empty;
            _textMesh.text   = text;
            _shadowMesh.text = text;
        }

        public void SetStyle(Style style)
        {
            _style = style;
            _localOffset = GetOffsetForStyle(style);

            if (_textMesh == null) return;
            ApplyStyle();
        }

        private static Vector3 GetOffsetForStyle(Style style)
        {
            return new Vector3(0f, EntityVisualConstants.NameplateOffsetFeetPivot, 0f);
        }

        private void LateUpdate()
        {
            if (_labelRoot == null) return;

            _labelRoot.localPosition = _localOffset;

            var cam = Camera.main;
            if (cam == null) return;

            var toCamera = _labelRoot.position - cam.transform.position;
            var distance = toCamera.magnitude;

            if (distance > 0.0001f)
                _labelRoot.rotation = Quaternion.LookRotation(toCamera);

            _labelRoot.localScale = Vector3.one * GetDistanceScale(distance);
        }

        private static float GetDistanceScale(float distanceFromCamera)
        {
            var t = Mathf.InverseLerp(DistanceScaleNearMeters, DistanceScaleFarMeters, distanceFromCamera);
            return Mathf.Lerp(DistanceScaleMin, DistanceScaleMax, t);
        }

        private void EnsureBuilt()
        {
            if (_labelRoot != null) return;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject("Nameplate");
            root.transform.SetParent(transform, false);
            _labelRoot = root.transform;

            _shadowMesh = CreateTextChild(root.transform, "Shadow", font, 0);
            _textMesh    = CreateTextChild(root.transform, "Label", font, 1);

            ApplyStyle();
        }

        private static TextMesh CreateTextChild(Transform parent, string name, Font font, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var mesh = go.AddComponent<TextMesh>();
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.font = font;
            mesh.fontSize = 40;
            mesh.characterSize = BaseTextSize;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sortingOrder      = sortingOrder;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows    = false;
            ApplyBrightMaterial(renderer, font, Color.white);

            return mesh;
        }

        private void ApplyStyle()
        {
            Color main;
            Color shadow = new Color(0f, 0f, 0f, 1f);
            FontStyle fontStyle;

            switch (_style)
            {
                case Style.LocalPlayer:
                    main      = new Color(1f, 0.96f, 0.35f);
                    fontStyle = FontStyle.Bold;
                    break;

                case Style.RemotePlayer:
                    main      = new Color(0.65f, 0.98f, 1f);
                    fontStyle = FontStyle.Bold;
                    break;

                default: // Npc
                    main      = new Color(1f, 1f, 1f);
                    fontStyle = FontStyle.Normal;
                    break;
            }

            ApplyToMesh(_textMesh, BaseTextSize, main, fontStyle, Vector3.zero);
            ApplyToMesh(_shadowMesh, BaseTextSize, shadow, fontStyle, ShadowOffset);
        }

        private static void ApplyBrightMaterial(MeshRenderer renderer, Font font, Color color)
        {
            if (renderer == null)
                return;

            var shader = Shader.Find("GUI/Text Shader") ?? Shader.Find("Unlit/Color");
            Material mat;

            if (font != null && font.material != null)
            {
                mat = new Material(font.material);
                if (font.material.mainTexture != null)
                    mat.mainTexture = font.material.mainTexture;
            }
            else if (shader != null)
            {
                mat = new Material(shader);
            }
            else
            {
                return;
            }

            if (shader != null && mat.shader.name != "GUI/Text Shader")
                mat.shader = shader;

            SetMaterialColor(mat, color);
            renderer.material = mat;
        }

        private static void ApplyToMesh(TextMesh mesh, float size, Color color, FontStyle fontStyle, Vector3 localPos)
        {
            if (mesh == null) return;

            mesh.transform.localPosition = localPos;
            mesh.characterSize = size;
            mesh.color = color;
            mesh.fontStyle = fontStyle;

            var renderer = mesh.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
                SetMaterialColor(renderer.material, color);
        }

        private static void SetMaterialColor(Material mat, Color color)
        {
            mat.color = color;
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
        }
    }
}