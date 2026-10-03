using System;
using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>
    /// Rearranges the cubes when the board has no move left. Not part of the case spec: added because Royal Match
    /// does the same. Only cubes move, and only among the cells cubes already occupy; everything else stays put.
    /// </summary>
    internal static class Shuffler
    {
        private const int RandomAttempts = 20;

        /// <summary>
        /// Tries random arrangements first; if none gives a move, places two same-coloured cubes side by side.
        /// If even that is impossible, the cubes are left shuffled and the board still has no move.
        /// Returns the cubes that changed cell.
        /// </summary>
        public static IReadOnlyList<ItemMove> Shuffle(Board board, Random random)
        {
            var cubes = board.Items.OfType<Cube>().ToList();
            var cells = cubes.Select(cube => cube.Position).ToList();
            var arrangement = new List<Cube>(cubes);

            foreach (var cube in cubes)
                board.Remove(cube);

            bool hasMove = false;
            for (int attempt = 0; attempt < RandomAttempts && !hasMove; attempt++)
            {
                Permute(arrangement, random);
                Place(board, cells, arrangement);
                hasMove = MoveFinder.HasAvailableMove(board);
                if (!hasMove)
                    Lift(board, arrangement);
            }

            if (!hasMove)
            {
                PairUp(cells, arrangement);
                Place(board, cells, arrangement);
            }

            var moves = new List<ItemMove>();
            for (int i = 0; i < cubes.Count; i++)
            {
                var cube = cubes[i];
                if (cube.Position != cells[i])
                    moves.Add(new ItemMove(cube, cells[i], cube.Position));
            }
            return moves;
        }

        /// <summary>Fisher–Yates: every order is equally likely.</summary>
        private static void Permute(List<Cube> cubes, Random random)
        {
            for (int i = cubes.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (cubes[i], cubes[j]) = (cubes[j], cubes[i]);
            }
        }

        /// <summary>
        /// Puts two cubes of the same colour on the first pair of neighbouring cube cells. Does nothing if no two
        /// cube cells touch, or if no colour has two cubes.
        /// </summary>
        private static void PairUp(List<GridPosition> cells, List<Cube> arrangement)
        {
            var color = arrangement.GroupBy(cube => cube.Color).FirstOrDefault(group => group.Count() >= 2)?.Key;
            if (color == null)
                return;

            for (int a = 0; a < cells.Count; a++)
            {
                for (int b = a + 1; b < cells.Count; b++)
                {
                    if (!AreNeighbours(cells[a], cells[b]))
                        continue;

                    Swap(arrangement, a, arrangement.FindIndex(cube => cube.Color == color));
                    int other = Enumerable.Range(0, arrangement.Count).First(i => i != a && arrangement[i].Color == color);
                    Swap(arrangement, b, other);
                    return;
                }
            }
        }

        private static bool AreNeighbours(GridPosition a, GridPosition b) =>
            Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) == 1;

        private static void Swap(List<Cube> cubes, int i, int j) => (cubes[i], cubes[j]) = (cubes[j], cubes[i]);

        private static void Place(Board board, List<GridPosition> cells, List<Cube> arrangement)
        {
            for (int i = 0; i < cells.Count; i++)
                board.Place(arrangement[i], cells[i]);
        }

        private static void Lift(Board board, List<Cube> arrangement)
        {
            foreach (var cube in arrangement)
                board.Remove(cube);
        }
    }
}
