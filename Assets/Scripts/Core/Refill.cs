using System.Collections.Generic;

namespace DreamCase.Core
{
    public static class Refill
    {
        /// <summary>
        /// Fills the empty cells at the top of each column with new random cubes. Filling stops at the first
        /// occupied cell, so holes under an item that can't fall stay empty. Run it after gravity.
        /// </summary>
        public static IReadOnlyList<ItemFall> Apply(Board board, ItemFactory factory)
        {
            var spawns = new List<ItemFall>();
            for (int x = 0; x < board.Width; x++)
            {
                int emptyCount = CountEmptyCellsFromTop(board, x);

                // The new cubes start stacked right above the board and all fall the same distance.
                for (int y = board.Height - emptyCount; y < board.Height; y++)
                {
                    var cube = factory.CreateRandomCube();
                    var target = new GridPosition(x, y);
                    board.Place(cube, target);
                    spawns.Add(new ItemFall(cube, new GridPosition(x, y + emptyCount), target));
                }
            }
            return spawns;
        }

        private static int CountEmptyCellsFromTop(Board board, int x)
        {
            int count = 0;
            while (count < board.Height && board.IsEmpty(new GridPosition(x, board.Height - 1 - count)))
                count++;
            return count;
        }
    }
}
