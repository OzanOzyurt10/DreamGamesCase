using DG.Tweening;
using DreamCase.Core;
using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Visual of one item. Items with a single fixed sprite (stone, TNT) use this class as is, with the sprite
    /// set on the prefab. Items whose look depends on model state (cube, rocket, vase, chalice box) override Bind.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemView : MonoBehaviour
    {
        [Header("Effects (optional)")]
        [Tooltip("Played when the item takes damage and survives.")]
        [SerializeField] private ParticleSystem hitEffect;
        [Tooltip("Played when the item leaves the board.")]
        [SerializeField] private ParticleSystem breakEffect;

        /// <summary>The scale set by FitWidth: the size the view returns to after any squash.</summary>
        private Vector3 fittedScale;

        protected SpriteRenderer SpriteRenderer { get; private set; }

        /// <summary>The effect for a hit the item survives, or null for none.</summary>
        public virtual ParticleSystem HitEffect => hitEffect;

        /// <summary>The effect for the item leaving the board, or null for none.</summary>
        public virtual ParticleSystem BreakEffect => breakEffect;

        protected virtual void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            // Only visible inside the board's mask, so new cubes appear to slide in from the board's top edge.
            SpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        /// <summary>Updates the visual to match the model item.</summary>
        public virtual void Bind(Item item)
        {
        }

        /// <summary>Shows which special item tapping this item would create. Only cubes show hints.</summary>
        public virtual void ShowHint(BlastReward reward)
        {
        }

        /// <summary>Scales the view so the sprite spans the given world width, keeping its aspect ratio.</summary>
        public void FitWidth(float worldWidth)
        {
            float spriteWidth = SpriteRenderer.sprite.bounds.size.x;
            fittedScale = Vector3.one * (worldWidth / spriteWidth);
            transform.localScale = fittedScale;
        }

        public virtual void SetSortingOrder(int order) => SpriteRenderer.sortingOrder = order;

        /// <summary>
        /// A short swell when the item takes damage but survives. It always ends at the fitted size, so
        /// overlapping hits can't leave the view bigger than it should be.
        /// </summary>
        public Tween PlayHit(float duration) =>
            DOTween.Sequence()
                .Append(transform.DOScale(fittedScale * 1.15f, duration * 0.3f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(fittedScale, duration * 0.7f).SetEase(Ease.OutBack));

        /// <summary>Shrinks the view to nothing, with a small swell first.</summary>
        public Tween PlayBlast(float duration) => transform.DOScale(0f, duration).SetEase(Ease.InBack);

        /// <summary>Slides into the target cell, pulling back slightly first, to merge into a special item.</summary>
        public Tween PlayMerge(Vector2 target, float duration) => transform.DOMove(target, duration).SetEase(Ease.InBack);

        /// <summary>Hides the view at once, then pops it up to its current size when the tween plays.</summary>
        public Tween PlayAppear(float duration)
        {
            Vector3 fullScale = transform.localScale;
            transform.localScale = Vector3.zero;
            return transform.DOScale(fullScale, duration).SetEase(Ease.OutBack);
        }

        /// <summary>Jumps along an arc to a new cell, for a shuffle.</summary>
        public Tween PlayShuffle(Vector2 target, float arcHeight, float duration) =>
            transform.DOJump(target, arcHeight, 1, duration).SetEase(Ease.InOutQuad);

        /// <summary>Falls to the target speeding up like under gravity, then bounces lightly on landing.</summary>
        public Tween PlayFall(Vector2 target, float duration, float bounceHeight, float bounceDuration) =>
            DOTween.Sequence()
                .Append(transform.DOMove(target, duration).SetEase(Ease.InQuad))
                .Append(transform.DOMoveY(target.y + bounceHeight, bounceDuration / 2f).SetEase(Ease.OutQuad))
                .Append(transform.DOMoveY(target.y, bounceDuration / 2f).SetEase(Ease.InQuad));
    }
}
