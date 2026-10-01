using System;

namespace DreamCase.Core
{
    /// <summary>Builds the starting board of a level from its JSON data.</summary>
    public sealed class BoardBuilder
    {
        private readonly ItemFactory factory;

        public BoardBuilder(ItemFactory factory)
        {
            this.factory = factory;
        }

        public Board Build(LevelData level)
        {
            int width = level.grid_width;
            int height = level.grid_height;
            if (level.grid == null || level.grid.Length != width * height)
                throw new ArgumentException($"Level {level.level_number}: grid must have {width * height} cells.");

            var board = new Board(width, height);
            for (int i = 0; i < level.grid.Length; i++)
            {
                var position = new GridPosition(i % width, i / width);

                // Already covered by a multi-cell item placed from an earlier (lower-left) cell.
                if (!board.IsEmpty(position))
                    continue;

                board.Place(factory.Create(level.grid[i]), position);
            }
            return board;
        }
    }
}
