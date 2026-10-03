using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>
    /// One item going from one cell to another: a fall, a new cube dropping in (then <see cref="From"/> is a
    /// cell above the board), or a cube moved by a shuffle.
    /// </summary>
    public readonly struct ItemMove
    {
        public Item Item { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public ItemMove(Item item, GridPosition from, GridPosition to)
        {
            Item = item;
            From = from;
            To = to;
        }
    }
}
