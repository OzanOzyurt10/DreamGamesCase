using System.Collections.Generic;

namespace DreamCase.Core
{
    public static class Gravity
    {
        /// <summary>
        /// Drops every falling item as far down its column as it can go. Items are only moved vertically
        /// and never pass through another item; an item that can't fall holds up everything above it.
        /// Falling items are all 1x1, so each column is handled on its own.
        /// </summary>
        public static IReadOnlyList<ItemMove> Apply(Board board)
        {
            var falls = new List<ItemMove>();
            for (int x = 0; x < board.Width; x++)
            {
                // Lowest empty cell an item above could land on; -1 when there is none.
                int landingY = -1;

                for (int y = 0; y < board.Height; y++)
                {
                    var cell = new GridPosition(x, y);
                    var item = board.GetItem(cell);

                    if (item == null)
                    {
                        if (landingY < 0)
                            landingY = y;
                    }
                    else if (!item.CanFall)
                    {
                        landingY = -1;
                    }
                    else if (landingY >= 0)
                    {
                        var target = new GridPosition(x, landingY);
                        board.Move(item, target);
                        falls.Add(new ItemMove(item, cell, target));
                        // Everything between the target and the old cell is empty, so the next landing spot is right above.
                        landingY++;
                    }
                }
            }
            return falls;
        }
    }
}
