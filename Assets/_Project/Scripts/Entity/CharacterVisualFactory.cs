using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Menempelkan model FBX Mixamo sebagai child "CharacterVisual".
    /// Dipakai saat runtime (Resources) dan oleh editor setup (AssetDatabase).
    /// </summary>
    public static class CharacterVisualFactory
    {
        public const string PlayerModelResource    = "Characters/PlayerModel";
        public const string NpcModelResource       = "Characters/NpcModel";
        public const string LocomotionResource     = "Characters/CharacterLocomotion";
        public const string CharacterVisualName    = "CharacterVisual";

        public static GameObject AttachFromResources(GameObject root, bool useNpcModel, float modelYawOffset = 0f)
        {
            var modelPath = useNpcModel ? NpcModelResource : PlayerModelResource;
            var modelPrefab = Resources.Load<GameObject>(modelPath);
            if (modelPrefab == null)
                return null;

            var controller = Resources.Load<RuntimeAnimatorController>(LocomotionResource);
            return AttachInstance(root, modelPrefab, controller, modelYawOffset);
        }

        public static GameObject AttachInstance(
            GameObject root,
            GameObject modelPrefab,
            RuntimeAnimatorController controller,
            float modelYawOffset = 0f)
        {
            if (modelPrefab == null)
                return null;

            RemoveExistingVisual(root);
            HideLegacyRootMesh(root);

            var visual = Object.Instantiate(modelPrefab, root.transform);
            visual.name = CharacterVisualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, modelYawOffset, 0f);
            visual.transform.localScale    = Vector3.one;

            foreach (var col in visual.GetComponentsInChildren<Collider>())
                Object.Destroy(col);

            var animator = visual.GetComponent<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();

            animator.applyRootMotion           = false;
            animator.runtimeAnimatorController = controller;

            var charAnim = root.GetComponent<CharacterAnimator>();
            if (charAnim == null)
                charAnim = root.AddComponent<CharacterAnimator>();
            else
                charAnim.RebindAnimator();

            MixamoVisualUtility.ApplyToVisual(visual);

            return visual;
        }

        public static void EnsureSingleVisual(GameObject root)
        {
            Transform keep = null;

            for (var i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i);
                if (child.name != CharacterVisualName)
                    continue;

                if (keep == null)
                {
                    keep = child;
                    continue;
                }

                DestroyObject(child.gameObject);
            }
        }

        public static void RemoveExistingVisual(GameObject root)
        {
            for (var i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i);
                if (child.name != CharacterVisualName)
                    continue;

                DestroyObject(child.gameObject);
            }
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

        public static void HideLegacyRootMesh(GameObject root)
        {
            var rootRenderer = root.GetComponent<Renderer>();
            if (rootRenderer != null)
                rootRenderer.enabled = false;

            var rootCollider = root.GetComponent<Collider>();
            if (rootCollider != null && rootCollider is not CharacterController)
                rootCollider.enabled = false;
        }
    }
}