using System.Collections.Generic;
using UnityEngine;

namespace Bloomtown.Client.Entity
{
    /// <summary>
    /// Menyelesaikan overlap horizontal antar karakter setelah semua gerakan frame selesai.
    /// </summary>
    public sealed class CharacterCollisionSystem : MonoBehaviour
    {
        private static readonly List<CharacterCollisionBody> Bodies = new();
        private static CharacterCollisionSystem _instance;

        private const int SeparationPasses = 2;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null)
                return;

            var go = new GameObject(nameof(CharacterCollisionSystem));
            _instance = go.AddComponent<CharacterCollisionSystem>();
            DontDestroyOnLoad(go);
        }

        public static void Register(CharacterCollisionBody body)
        {
            if (body == null || Bodies.Contains(body))
                return;

            Bodies.Add(body);
        }

        public static void Unregister(CharacterCollisionBody body)
        {
            if (body == null)
                return;

            Bodies.Remove(body);
        }

        private void LateUpdate()
        {
            ResolveOverlaps();
        }

        private static void ResolveOverlaps()
        {
            var count = Bodies.Count;
            if (count < 2)
                return;

            var pushes = new Vector3[count];

            for (var pass = 0; pass < SeparationPasses; pass++)
            {
                for (var i = 0; i < count; i++)
                    pushes[i] = Vector3.zero;

                for (var i = 0; i < count; i++)
                {
                    var a = Bodies[i];
                    if (a == null || !a.isActiveAndEnabled)
                        continue;

                    var posA = a.FeetPosition;

                    for (var j = i + 1; j < count; j++)
                    {
                        var b = Bodies[j];
                        if (b == null || !b.isActiveAndEnabled)
                            continue;

                        var posB = b.FeetPosition;
                        var delta = posA - posB;
                        delta.y = 0f;

                        var dist    = delta.magnitude;
                        var minDist = a.Radius + b.Radius;
                        if (dist >= minDist)
                            continue;

                        Vector3 sep;
                        if (dist > 0.001f)
                            sep = delta * ((minDist - dist) / dist * 0.5f);
                        else
                            sep = Vector3.right * (minDist * 0.5f);

                        pushes[i] += sep;
                        pushes[j] -= sep;
                    }
                }

                for (var i = 0; i < count; i++)
                {
                    var body = Bodies[i];
                    if (body == null || !body.isActiveAndEnabled)
                        continue;

                    body.ApplyHorizontalDelta(pushes[i]);
                }
            }
        }
    }
}