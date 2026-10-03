using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    internal static class BlastDamage
    {
        /// <summary>
        /// Damages every item next to the blasted cubes, once per item, with the number of blasted cubes
        /// touching it. Each item decides what a blast does to it: cubes and special items ignore it.
        /// Run it after the blasted cubes are removed. Cleared items are removed from the board.
        /// </summary>
        public static (IReadOnlyList<Item> damaged, IReadOnlyList<Item> cleared) Apply(
            Board board, IReadOnlyList<Cube> blastedCubes, int sourceId)
        {
            var touchingCubes = new Dictionary<Item, int>();
            foreach (var cube in blastedCubes)
            {
                foreach (var direction in GridPosition.OrthogonalDirections)
                {
                    var cell = cube.Position + direction;
                    if (!board.IsInside(cell) || board.IsEmpty(cell))
                        continue;

                    var neighbour = board.GetItem(cell);
                    touchingCubes.TryGetValue(neighbour, out int count);
                    touchingCubes[neighbour] = count + 1;
                }
            }

            var damaged = new List<Item>();
            var cleared = new List<Item>();
            foreach (var pair in touchingCubes)
            {
                switch (pair.Key.TakeDamage(new DamageInfo(DamageType.Blast, sourceId, pair.Value)))
                {
                    case DamageOutcome.Damaged:
                        damaged.Add(pair.Key);
                        break;
                    case DamageOutcome.Cleared:
                        board.Remove(pair.Key);
                        cleared.Add(pair.Key);
                        break;
                }
            }
            return (damaged, cleared);
        }
    }
}
