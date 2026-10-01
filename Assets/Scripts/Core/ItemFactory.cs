using System;
using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>
    /// Turns level codes into items. Adding a new item type means adding one entry to the registry.
    /// </summary>
    public sealed class ItemFactory
    {
        private static readonly CubeColor[] CubeColors = (CubeColor[])Enum.GetValues(typeof(CubeColor));

        private readonly Random random;
        private readonly Dictionary<string, Func<Item>> registry;

        public ItemFactory(Random random)
        {
            this.random = random;
            registry = new Dictionary<string, Func<Item>>
            {
                ["r"] = () => new Cube(CubeColor.Red),
                ["g"] = () => new Cube(CubeColor.Green),
                ["b"] = () => new Cube(CubeColor.Blue),
                ["y"] = () => new Cube(CubeColor.Yellow),
                ["rand"] = CreateRandomCube,
                ["vro"] = () => new Rocket(RocketDirection.Vertical),
                ["hro"] = () => new Rocket(RocketDirection.Horizontal),
                ["t"] = () => new Tnt(),
                ["s"] = () => new Stone(),
                ["v"] = () => new Vase(),
                // A multi-cell item is created from its bottom-left corner code.
                // The other corner codes (cbBR, cbTL, cbTR) land on cells the box already covers.
                ["cbBL"] = () => new ChaliceBox(),
            };
        }

        public Item Create(string code)
        {
            if (!registry.TryGetValue(code, out var create))
                throw new ArgumentException($"Unknown item code '{code}'.", nameof(code));
            return create();
        }

        public Cube CreateRandomCube() => new Cube(CubeColors[random.Next(CubeColors.Length)]);

        /// <summary>Creates the special item a blast earned. Rockets get a random direction.</summary>
        public SpecialItem CreateReward(BlastReward reward)
        {
            switch (reward)
            {
                case BlastReward.Rocket:
                    var direction = random.Next(2) == 0 ? RocketDirection.Horizontal : RocketDirection.Vertical;
                    return new Rocket(direction);
                case BlastReward.Tnt:
                    return new Tnt();
                default:
                    throw new ArgumentOutOfRangeException(nameof(reward), reward, "No special item for this reward.");
            }
        }
    }
}
