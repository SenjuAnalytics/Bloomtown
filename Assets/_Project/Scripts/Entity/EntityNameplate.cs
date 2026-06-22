using UnityEngine;

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

        private static readonly Vector3 ShadowOffset = new Vector3(0.02f, -0.02f, 0.01f);

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
            if (toCamera.sqrMagnitude > 0.0001f)
                _labelRoot.rotation = Quaternion.LookRotation(toCamera);
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
            mesh.fontSize = 64;
            mesh.characterSize = 0.08f;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sortingOrder = sortingOrder;

            return mesh;
        }

        private void ApplyStyle()
        {
            float size;
            Color main;
            Color shadow = new Color(0f, 0f, 0f, 0.9f);
            FontStyle fontStyle;

            switch (_style)
            {
                case Style.LocalPlayer:
                    size      = 0.088f;
                    main      = new Color(1f, 0.85f, 0.1f);
                    fontStyle = FontStyle.Bold;
                    break;

                case Style.RemotePlayer:
                    size      = 0.09f;
                    main      = new Color(0.4f, 0.85f, 1f);
                    fontStyle = FontStyle.Bold;
                    break;

                default: // Npc
                    size      = 0.085f;
                    main      = Color.white;
                    fontStyle = FontStyle.Normal;
                    break;
            }

            ApplyToMesh(_textMesh, size, main, fontStyle, Vector3.zero);
            ApplyToMesh(_shadowMesh, size, shadow, fontStyle, ShadowOffset);
        }

        private static void ApplyToMesh(TextMesh mesh, float size, Color color, FontStyle fontStyle, Vector3 localPos)
        {
            if (mesh == null) return;

            mesh.transform.localPosition = localPos;
            mesh.characterSize = size;
            mesh.color = color;
            mesh.fontStyle = fontStyle;
        }
    }
}