using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    public sealed class VaseView : ItemView
    {
        [SerializeField] private Sprite intact;
        [SerializeField] private Sprite cracked;

        public override void Bind(Item item)
        {
            var vase = (Vase)item;
            SpriteRenderer.sprite = vase.Health == Vase.MaxHealth ? intact : cracked;
        }
    }
}
