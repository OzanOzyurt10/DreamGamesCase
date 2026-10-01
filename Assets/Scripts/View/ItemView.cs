using DG.Tweening;
using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Visual of one item. Items with a single fixed sprite (stone, vase, TNT, chalice box) use this
    /// class as is, with the sprite set on the prefab. Items whose sprite depends on model state override Bind.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemView : MonoBehaviour
    {
        protected SpriteRenderer SpriteRenderer { get; private set; }

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            // Only visible inside the board's mask, so new cubes appear to slide in from the board's top edge.
            SpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        /// <summary>Updates the visual to match the model item.</summary>
        public virtual void Bind(Item item)
        {
        }

        /// <summary>Scales the view so the sprite spans the given world width, keeping its aspect ratio.</summary>
        public void FitWidth(float worldWidth)
        {
            float spriteWidth = SpriteRenderer.sprite.bounds.size.x;
            transform.localScale = Vector3.one * (worldWidth / spriteWidth);
        }

        public void SetSortingOrder(int order) => SpriteRenderer.sortingOrder = order;

        /// <summary>Shrinks the view to nothing, with a small swell first.</summary>
        public Tween PlayBlast(float duration) => transform.DOScale(0f, duration).SetEase(Ease.InBack);

        /// <summary>Falls to the target speeding up like under gravity, then bounces lightly on landing.</summary>
        public Tween PlayFall(Vector2 target, float duration, float bounceHeight, float bounceDuration) =>
            DOTween.Sequence()
                .Append(transform.DOMove(target, duration).SetEase(Ease.InQuad))
                .Append(transform.DOMoveY(target.y + bounceHeight, bounceDuration / 2f).SetEase(Ease.OutQuad))
                .Append(transform.DOMoveY(target.y, bounceDuration / 2f).SetEase(Ease.InQuad));
    }
}
