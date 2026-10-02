using DG.Tweening;
using DreamCase.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace DreamCase.View
{
    /// <summary>Flies the two halves of an exploding rocket. Half visuals are plain sprites kept in a pool.</summary>
    public sealed class RocketEffect : MonoBehaviour
    {
        [SerializeField] private Sprite leftHalf;
        [SerializeField] private Sprite rightHalf;
        [SerializeField] private Sprite bottomHalf;
        [SerializeField] private Sprite topHalf;

        [Tooltip("Draws the halves above every item.")]
        [SerializeField] private int sortingOrder = 100;

        private ObjectPool<SpriteRenderer> pool;

        private void Awake()
        {
            pool = new ObjectPool<SpriteRenderer>(
                createFunc: CreateHalf,
                actionOnGet: half => half.gameObject.SetActive(true),
                actionOnRelease: half => half.gameObject.SetActive(false),
                actionOnDestroy: half => Destroy(half.gameObject));
        }

        /// <summary>
        /// Moves one half in a straight line at constant speed, so it reaches each cell exactly when the model
        /// hits it. The half stays hidden until the returned tween starts playing.
        /// </summary>
        public Tween LaunchHalf(GridPosition direction, Vector2 from, Vector2 to, float duration, float cellSize)
        {
            var half = pool.Get();
            half.gameObject.SetActive(false);
            half.sprite = SpriteFor(direction);
            half.transform.position = from;
            half.transform.localScale = Vector3.one * (cellSize / half.sprite.bounds.size.x);

            return DOTween.Sequence()
                .AppendCallback(() => half.gameObject.SetActive(true))
                .Append(half.transform.DOMove(to, duration).SetEase(Ease.Linear))
                .AppendCallback(() => pool.Release(half));
        }

        private Sprite SpriteFor(GridPosition direction)
        {
            if (direction == GridPosition.Left) return leftHalf;
            if (direction == GridPosition.Right) return rightHalf;
            if (direction == GridPosition.Down) return bottomHalf;
            return topHalf;
        }

        private SpriteRenderer CreateHalf()
        {
            var half = new GameObject("RocketHalf").AddComponent<SpriteRenderer>();
            half.transform.SetParent(transform, false);
            half.sortingOrder = sortingOrder;
            // Disappears at the board's edge like the items.
            half.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            return half;
        }
    }
}
