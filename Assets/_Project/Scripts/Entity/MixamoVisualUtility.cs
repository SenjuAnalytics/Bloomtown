using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Penyesuaian visual model Mixamo: tapak kaki sejajar lantai (y = 0 relatif pivot entity).
    /// </summary>
    public static class MixamoVisualUtility
    {
        private static readonly int WalkStateHash = Animator.StringToHash("Walk");

        private static readonly HumanBodyBones[] FootBones =
        {
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
        };

        private static readonly string[] FootBoneNames =
        {
            "mixamorig:LeftFoot",
            "mixamorig:RightFoot",
            "mixamorig:LeftToeBase",
            "mixamorig:RightToeBase",
        };

        /// <summary>Sedikit turunkan model agar tapak tidak tampak melayang di atas lantai.</summary>
        private const float GroundContactBias = -0.03f;

        private static readonly float[] WalkContactSamples = { 0f, 0.25f, 0.5f, 0.75f };

        public static void ApplyToVisual(GameObject visual)
        {
            if (visual == null) return;
            AlignFeetToGround(visual);
        }

        public static void AlignFeetToGround(GameObject visual)
        {
            var pos = visual.transform.localPosition;
            pos.y = 0f;
            visual.transform.localPosition = pos;

            var lowestY = GetWalkCycleLowestFootLocalY(visual);
            if (float.IsPositiveInfinity(lowestY) || Mathf.Abs(lowestY) < 0.0005f)
                return;

            pos = visual.transform.localPosition;
            pos.y -= lowestY + GroundContactBias;
            visual.transform.localPosition = pos;
        }

        /// <summary>Sesuaikan offset Y model setiap frame agar kaki terendah menempel lantai pada pose saat ini.</summary>
        public static void ApplyRuntimeFootGrounding(GameObject visual, Animator animator)
        {
            if (visual == null || animator == null)
                return;

            var lowestY = GetCurrentPoseLowestFootLocalY(visual, animator);
            if (float.IsPositiveInfinity(lowestY))
                return;

            var pos = visual.transform.localPosition;
            pos.y = -lowestY + GroundContactBias;
            visual.transform.localPosition = pos;
        }

        private static float GetWalkCycleLowestFootLocalY(GameObject visual)
        {
            var animator = visual.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var walkY = SampleWalkContactFootY(visual.transform, animator);
                if (!float.IsPositiveInfinity(walkY))
                    return walkY;
            }

            return GetCurrentPoseLowestFootLocalY(visual, animator);
        }

        private static float GetCurrentPoseLowestFootLocalY(GameObject visual, Animator animator)
        {
            if (animator != null && animator.isHuman)
            {
                var humanoidY = GetHumanoidFootLowestLocalY(animator, visual.transform);
                if (!float.IsPositiveInfinity(humanoidY))
                    return humanoidY;
            }

            var boneY = GetNamedFootLowestLocalY(visual.transform);
            if (!float.IsPositiveInfinity(boneY))
                return boneY;

            return GetMeshBoundsLowestLocalY(visual);
        }

        private static float SampleWalkContactFootY(Transform visualRoot, Animator animator)
        {
            if (animator.runtimeAnimatorController == null)
                return float.PositiveInfinity;

            var wasSpeed = animator.speed;
            animator.speed = 0f;

            var minY = float.PositiveInfinity;
            foreach (var sample in WalkContactSamples)
            {
                animator.Play(WalkStateHash, 0, sample);
                animator.Update(0f);

                var footY = GetHumanoidFootLowestLocalY(animator, visualRoot);
                if (float.IsPositiveInfinity(footY))
                    footY = GetNamedFootLowestLocalY(visualRoot);

                if (footY < minY)
                    minY = footY;
            }

            animator.speed = wasSpeed;
            return minY;
        }

        private static float GetHumanoidFootLowestLocalY(Animator animator, Transform visualRoot)
        {
            if (animator == null || !animator.isHuman)
                return float.PositiveInfinity;

            var minY = float.PositiveInfinity;
            foreach (var bone in FootBones)
            {
                var t = animator.GetBoneTransform(bone);
                if (t == null) continue;

                var localY = visualRoot.InverseTransformPoint(t.position).y;
                if (localY < minY)
                    minY = localY;
            }

            return minY;
        }

        private static float GetNamedFootLowestLocalY(Transform visualRoot)
        {
            var minY = float.PositiveInfinity;
            foreach (var boneName in FootBoneNames)
            {
                var bone = FindChildRecursive(visualRoot, boneName);
                if (bone == null) continue;

                var localY = visualRoot.InverseTransformPoint(bone.position).y;
                if (localY < minY)
                    minY = localY;
            }

            return minY;
        }

        private static float GetMeshBoundsLowestLocalY(GameObject visual)
        {
            var renderers = visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length == 0)
                return float.PositiveInfinity;

            var minLocalY = float.PositiveInfinity;
            foreach (var smr in renderers)
            {
                var bounds = smr.localBounds;
                var bottom = bounds.center.y - bounds.extents.y;
                if (bottom < minLocalY)
                    minLocalY = bottom;
            }

            return minLocalY;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}