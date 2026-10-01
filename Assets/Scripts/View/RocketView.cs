using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    public sealed class RocketView : ItemView
    {
        [SerializeField] private Sprite horizontal;
        [SerializeField] private Sprite vertical;

        public override void Bind(Item item)
        {
            var rocket = (Rocket)item;
            SpriteRenderer.sprite = rocket.Direction == RocketDirection.Horizontal ? horizontal : vertical;
        }
    }
}
