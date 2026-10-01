using System;
using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The grid of cells. A multi-cell item is stored in every cell it covers.</summary>
    public sealed class Board
    {
        private readonly Item[,] cells;

        public int Width { get; }
        public int Height { get; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new Item[width, height];
        }

        public bool IsInside(GridPosition position) =>
            position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;

        /// <summary>Returns the item covering the cell, or null if the cell is empty.</summary>
        public Item GetItem(GridPosition position) => cells[position.X, position.Y];

        public bool IsEmpty(GridPosition position) => GetItem(position) == null;

        /// <summary>Puts the item with its bottom-left corner on <paramref name="anchor"/>.</summary>
        public void Place(Item item, GridPosition anchor)
        {
            for (int dx = 0; dx < item.Width; dx++)
            {
                for (int dy = 0; dy < item.Height; dy++)
                {
                    var cell = new GridPosition(anchor.X + dx, anchor.Y + dy);
                    if (!IsInside(cell) || !IsEmpty(cell))
                        throw new InvalidOperationException($"Cannot place {item.GetType().Name} at {anchor}: cell {cell} is outside or occupied.");
                }
            }

            item.Position = anchor;
            for (int dx = 0; dx < item.Width; dx++)
            {
                for (int dy = 0; dy < item.Height; dy++)
                    cells[anchor.X + dx, anchor.Y + dy] = item;
            }
        }

        /// <summary>Clears every cell the item covers.</summary>
        public void Remove(Item item)
        {
            if (!IsInside(item.Position) || GetItem(item.Position) != item)
                throw new InvalidOperationException($"{item.GetType().Name} is not on the board at {item.Position}.");

            for (int dx = 0; dx < item.Width; dx++)
            {
                for (int dy = 0; dy < item.Height; dy++)
                    cells[item.Position.X + dx, item.Position.Y + dy] = null;
            }
        }

        /// <summary>Every item on the board exactly once, even if it covers several cells.</summary>
        public IEnumerable<Item> Items
        {
            get
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var item = cells[x, y];
                        // A multi-cell item is reported only from its anchor cell.
                        if (item != null && item.Position == new GridPosition(x, y))
                            yield return item;
                    }
                }
            }
        }
    }
}
