using System;
using DreamCase.Core;
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
            [Tooltip("Shown when the cube's group would create a rocket.")]
            public Sprite Rocket;
            [Tooltip("Shown when the cube's group would create a TNT.")]
            public Sprite Tnt;
        }

        [SerializeField] private ColorSprites[] sprites;

        private ColorSprites colorSprites;

        public override void Bind(Item item)
        {
            colorSprites = SpritesFor(((Cube)item).Color);
            ShowHint(BlastReward.None);
        }

        public override void ShowHint(BlastReward reward)
        {
            SpriteRenderer.sprite = reward switch
            {
                BlastReward.Rocket => colorSprites.Rocket,
                BlastReward.Tnt => colorSprites.Tnt,
                _ => colorSprites.Default,
            };
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
