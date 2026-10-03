using System;
using DreamCase.Core;
using DreamCase.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace DreamCase.View
{
    public sealed class CubeView : ItemView
    {
        [Serializable]
        private struct ColorVisuals
        {
            public CubeColor Color;
            public Sprite Default;
            [Tooltip("Shown when the cube's group would create a rocket.")]
            public Sprite Rocket;
            [Tooltip("Shown when the cube's group would create a TNT.")]
            public Sprite Tnt;
            [Tooltip("Pieces in this colour, played when the cube is destroyed.")]
            public ParticleSystem BreakEffect;
        }

        [FormerlySerializedAs("sprites")]
        [SerializeField] private ColorVisuals[] colorVisuals;

        private ColorVisuals visuals;

        public override ParticleSystem BreakEffect => visuals.BreakEffect;

        public override void Bind(Item item)
        {
            visuals = VisualsFor(((Cube)item).Color);
            ShowHint(BlastReward.None);
        }

        public override void ShowHint(BlastReward reward)
        {
            SpriteRenderer.sprite = reward switch
            {
                BlastReward.Rocket => visuals.Rocket,
                BlastReward.Tnt => visuals.Tnt,
                _ => visuals.Default,
            };
        }

        private ColorVisuals VisualsFor(CubeColor color)
        {
            foreach (var entry in colorVisuals)
            {
                if (entry.Color == color)
                    return entry;
            }
            throw new InvalidOperationException($"{name} has no visuals for {color}.");
        }
    }
}
