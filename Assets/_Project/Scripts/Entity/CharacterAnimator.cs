using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Sinkronkan siklus Walk dengan jarak tempuh — cadence ~jalan manusia normal.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterAnimator : MonoBehaviour
    {
        private static readonly int WalkStateHash = Animator.StringToHash("Walk");

        [Tooltip("Siklus Walk per detik pada kecepatan desain (0.8–1.0 = jalan santai).")]
        [SerializeField] private float _walkCadenceHz = 0.88f;

        [Tooltip("Kecepatan gerak (m/s) yang dipakai untuk menghitung panjang langkah.")]
        [SerializeField] private float _designMoveSpeed = 2f;

        [SerializeField] private float _moveThreshold = 0.08f;

        private Animator _animator;
        private float    _walkPhase;
        private float    _strideLength;
        private bool     _wasMoving;

        private void Awake()
        {
            BindAnimator();
        }

        public void RebindAnimator()
        {
            _animator  = null;
            _wasMoving = false;
            BindAnimator();
        }

        public void SetLocomotion(float speedMetersPerSecond, float deltaTime)
        {
            if (!BindAnimator()) return;

            var moving = speedMetersPerSecond > _moveThreshold;

            if (!moving)
            {
                _wasMoving = false;
                _animator.speed = 0f;
                return;
            }

            if (!_wasMoving)
            {
                _animator.speed = 0f;
                _animator.Play(WalkStateHash, 0, _walkPhase);
            }

            var stride = Mathf.Max(_strideLength, 0.5f);
            _walkPhase += (speedMetersPerSecond / stride) * Mathf.Max(deltaTime, 0f);
            if (_walkPhase >= 1f)
                _walkPhase %= 1f;

            _animator.speed = 0f;
            _animator.Play(WalkStateHash, 0, _walkPhase);
            _animator.Update(0f);
            _wasMoving = true;
        }

        private bool BindAnimator()
        {
            if (_animator != null) return true;

            _animator = GetComponentInChildren<Animator>();
            if (_animator == null) return false;

            _animator.applyRootMotion = false;
            _animator.cullingMode     = AnimatorCullingMode.AlwaysAnimate;
            RecalculateStride();
            return true;
        }

        private void RecalculateStride()
        {
            var cadence = Mathf.Clamp(_walkCadenceHz, 0.55f, 1.4f);
            var speed   = Mathf.Max(_designMoveSpeed, 0.5f);

            var clipLength = TryGetWalkClipLength();
            if (clipLength > 0.01f)
            {
                // Mixamo: satu siklus clip ≈ satu langkah penuh pada kecepatan ~1.3 m/s.
                const float mixamoAuthoredSpeed = 1.3f;
                var authoredStride = mixamoAuthoredSpeed * clipLength;
                var cadenceStride  = speed / cadence;
                _strideLength = Mathf.Max(authoredStride, cadenceStride);
            }
            else
            {
                _strideLength = speed / cadence;
            }
        }

        private float TryGetWalkClipLength()
        {
            if (_animator?.runtimeAnimatorController == null) return 0f;

            foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip == null || clip.length <= 0.01f) continue;
                if (clip.name.StartsWith("__")) continue;
                return clip.length;
            }

            return 0f;
        }
    }
}