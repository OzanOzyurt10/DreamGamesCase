using DG.Tweening;
using DreamCase.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace DreamCase.View
{
    /// <summary>
    /// Flies the two halves of an exploding rocket, each leaving a smoke and spark trail. Half visuals are plain
    /// sprites kept in a pool, each with its own trail.
    /// </summary>
    public sealed class RocketEffect : MonoBehaviour
    {
        private sealed class Half
        {
            public SpriteRenderer Sprite;
            public ParticleSystem Trail;
        }

        [SerializeField] private Sprite leftHalf;
        [SerializeField] private Sprite rightHalf;
        [SerializeField] private Sprite bottomHalf;
        [SerializeField] private Sprite topHalf;

        [Tooltip("Emits behind a flying half. Authored for a 1-unit cell; world space, so the trail stays where it was left.")]
        [SerializeField] private ParticleSystem trailPrefab;

        [Tooltip("Draws the halves above every item.")]
        [SerializeField] private int sortingOrder = 100;

        private ObjectPool<Half> pool;
        private float trailLifetime;

        private void Awake()
        {
            foreach (var system in trailPrefab.GetComponentsInChildren<ParticleSystem>())
                trailLifetime = Mathf.Max(trailLifetime, system.main.startLifetime.constantMax);
            pool = new ObjectPool<Half>(
                createFunc: CreateHalf,
                actionOnGet: half => half.Sprite.gameObject.SetActive(true),
                actionOnRelease: half => half.Sprite.gameObject.SetActive(false),
                actionOnDestroy: half => Destroy(half.Sprite.gameObject));
        }

        /// <summary>
        /// Moves one half in a straight line at constant speed, so it reaches each cell exactly when the model
        /// hits it. The half stays hidden until the returned tween starts playing. Once it arrives, its trail
        /// stops emitting and the half returns to the pool after the last trail particles fade.
        /// </summary>
        public Tween LaunchHalf(GridPosition direction, Vector2 from, Vector2 to, float duration, float cellSize)
        {
            var half = pool.Get();
            half.Sprite.gameObject.SetActive(false);
            half.Sprite.sprite = SpriteFor(direction);
            half.Sprite.transform.position = from;

            float spriteWidth = half.Sprite.sprite.bounds.size.x;
            half.Sprite.transform.localScale = Vector3.one * (cellSize / spriteWidth);
            // Undo the half's own scale so the trail's world size is one cell per unit, like every other effect.
            half.Trail.transform.localScale = Vector3.one * spriteWidth;

            return DOTween.Sequence()
                .AppendCallback(() =>
                {
                    half.Sprite.gameObject.SetActive(true);
                    half.Trail.Clear();
                    half.Trail.Play();
                })
                .Append(half.Sprite.transform.DOMove(to, duration).SetEase(Ease.Linear))
                .AppendCallback(() =>
                {
                    half.Trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    DOVirtual.DelayedCall(trailLifetime, () => pool.Release(half));
                });
        }

        private Sprite SpriteFor(GridPosition direction)
        {
            if (direction == GridPosition.Left) return leftHalf;
            if (direction == GridPosition.Right) return rightHalf;
            if (direction == GridPosition.Down) return bottomHalf;
            return topHalf;
        }

        private Half CreateHalf()
        {
            var sprite = new GameObject("RocketHalf").AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(transform, false);
            sprite.sortingOrder = sortingOrder;
            // Disappears at the board's edge like the items.
            sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            var trail = Instantiate(trailPrefab, sprite.transform);
            return new Half { Sprite = sprite, Trail = trail };
        }
    }
}
