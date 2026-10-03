using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace DreamCase.View
{
    /// <summary>
    /// Plays particle effects from per-prefab pools: a finished effect is deactivated and reused, not destroyed.
    /// Effects are authored for a 1-unit cell and scaled to the board's cell size when played.
    /// </summary>
    public sealed class EffectPool : MonoBehaviour
    {
        private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> pools =
            new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();
        private readonly Dictionary<ParticleSystem, float> lifetimes = new Dictionary<ParticleSystem, float>();

        /// <summary>Plays the effect at a position. A null prefab means the item has no effect for this event.</summary>
        public void Play(ParticleSystem prefab, Vector2 position, float cellSize)
        {
            if (prefab == null)
                return;

            var pool = PoolFor(prefab);
            var effect = pool.Get();
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * cellSize;
            effect.Play(true);
            DOVirtual.DelayedCall(lifetimes[prefab], () => pool.Release(effect)).SetLink(effect.gameObject);
        }

        private ObjectPool<ParticleSystem> PoolFor(ParticleSystem prefab)
        {
            if (pools.TryGetValue(prefab, out var pool))
                return pool;

            pool = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: effect => effect.gameObject.SetActive(true),
                actionOnRelease: effect =>
                {
                    effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    effect.gameObject.SetActive(false);
                },
                actionOnDestroy: effect => Destroy(effect.gameObject));
            pools.Add(prefab, pool);
            lifetimes.Add(prefab, LongestLifetime(prefab));
            return pool;
        }

        /// <summary>How long until every particle of the effect, children included, has died.</summary>
        private static float LongestLifetime(ParticleSystem prefab)
        {
            float longest = 0f;
            foreach (var system in prefab.GetComponentsInChildren<ParticleSystem>())
                longest = Mathf.Max(longest, system.main.duration + system.main.startLifetime.constantMax);
            return longest;
        }
    }
}
