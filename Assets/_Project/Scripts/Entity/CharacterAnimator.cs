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

        [Tooltip("Pose Walk saat lompat dari berdiri (kaki sedikit menekuk).")]
        [SerializeField] private float _idleAirbornePhase = 0.14f;

        [Tooltip("Pose Walk saat lompat sambil jalan (satu kaki terangkat).")]
        [SerializeField] private float _movingAirbornePhase = 0.20f;

        [Tooltip("Pose Walk saat berdiri — kaki menapak di lantai.")]
        [SerializeField] private float _idleGroundPhase = 0f;

        private Animator _animator;
        private Transform _visualRoot;
        private float    _walkPhase;
        private float    _airborneHoldPhase;
        private float    _strideLength;
        private bool     _wasMoving;
        private bool     _isAirborne;

        private void Awake()
        {
            BindAnimator();
        }

        public void RebindAnimator()
        {
            _animator     = null;
            _visualRoot   = null;
            _wasMoving    = false;
            _isAirborne   = false;
            BindAnimator();
        }

        public void SetLocomotion(float speedMetersPerSecond, float deltaTime, bool isGrounded = true)
        {
            if (!BindAnimator()) return;

            if (!isGrounded)
            {
                EnterAirborne(speedMetersPerSecond);
                HoldAirbornePose();
                return;
            }

            if (_isAirborne)
            {
                _isAirborne = false;
                _walkPhase  = _airborneHoldPhase;
            }

            var moving = speedMetersPerSecond > _moveThreshold;

            if (!moving)
            {
                _wasMoving = false;
                _walkPhase = _idleGroundPhase;
                _animator.speed = 0f;
                _animator.Play(WalkStateHash, 0, _idleGroundPhase);
                _animator.Update(0f);
                ApplyFootGrounding();
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
            ApplyFootGrounding();
        }

        private void EnterAirborne(float speedMetersPerSecond)
        {
            if (_isAirborne)
                return;

            _isAirborne = true;
            _wasMoving  = false;

            _airborneHoldPhase = speedMetersPerSecond > _moveThreshold
                ? _movingAirbornePhase
                : _idleAirbornePhase;
        }

        private void HoldAirbornePose()
        {
            _animator.speed = 0f;
            _animator.Play(WalkStateHash, 0, _airborneHoldPhase);
            _animator.Update(0f);
        }

        private void ApplyFootGrounding()
        {
            if (_visualRoot == null || _animator == null)
                return;

            MixamoVisualUtility.ApplyRuntimeFootGrounding(_visualRoot.gameObject, _animator);
        }

        private bool BindAnimator()
        {
            if (_animator != null) return true;

            _animator = GetComponentInChildren<Animator>();
            if (_animator == null) return false;

            _visualRoot = _animator.transform;
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