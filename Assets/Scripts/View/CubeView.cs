using System;
using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    public sealed class CubeView : ItemView
    {
        [Serializable]
        private struct ColorSprites
        {
            public CubeColor Color;
            public Sprite Default;
        }

        [SerializeField] private ColorSprites[] sprites;

        public override void Bind(Item item)
        {
            SpriteRenderer.sprite = SpritesFor(((Cube)item).Color).Default;
        }

        private ColorSprites SpritesFor(CubeColor color)
        {
            foreach (var entry in sprites)
            {
                if (entry.Color == color)
                    return entry;
            }
            throw new InvalidOperationException($"{name} has no sprites for {color}.");
        }
    }
}
