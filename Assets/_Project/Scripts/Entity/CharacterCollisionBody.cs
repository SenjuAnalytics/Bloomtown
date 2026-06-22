using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Mendaftarkan tubuh karakter ke <see cref="CharacterCollisionSystem"/> agar
    /// pemain lokal, NPC, dan remote player tidak saling menembus di sumbu XZ.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterCollisionBody : MonoBehaviour
    {
        public const float DefaultRadius = 0.3f;
        public const float DefaultHeight = 2f;
        public static readonly Vector3 DefaultCenter = new Vector3(0f, 1f, 0f);

        [SerializeField] private float _radius = DefaultRadius;

        private CharacterController _characterController;

        public float Radius => _radius;

        public static CharacterCollisionBody EnsureOn(GameObject go)
        {
            return go.GetComponent<CharacterCollisionBody>() ?? go.AddComponent<CharacterCollisionBody>();
        }

        public Vector3 FeetPosition
        {
            get
            {
                var pos = transform.position;
                pos.y = 0f;
                return pos;
            }
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            EnsurePhysicsCollider();
        }

        private void OnEnable()  => CharacterCollisionSystem.Register(this);
        private void OnDisable() => CharacterCollisionSystem.Unregister(this);

        /// <summary>Pakai CapsuleCollider untuk entitas tanpa CharacterController agar CC pemain lokal bisa menabrak.</summary>
        private void EnsurePhysicsCollider()
        {
            if (_characterController != null)
                return;

            var capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = gameObject.AddComponent<CapsuleCollider>();

            capsule.height    = DefaultHeight;
            capsule.radius    = _radius;
            capsule.center    = DefaultCenter;
            capsule.isTrigger = false;
        }

        public void ApplyHorizontalDelta(Vector3 delta)
        {
            delta.y = 0f;
            if (delta.sqrMagnitude < 1e-8f)
                return;

            if (_characterController != null)
            {
                _characterController.enabled = false;
                transform.position += delta;
                _characterController.enabled = true;
                return;
            }

            transform.position += delta;
        }
    }
}