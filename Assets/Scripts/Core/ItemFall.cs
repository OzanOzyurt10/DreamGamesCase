using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>One item moving down a column. For a new cube, <see cref="From"/> is a cell above the board.</summary>
    public readonly struct ItemFall
    {
        public Item Item { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public ItemFall(Item item, GridPosition from, GridPosition to)
        {
            Item = item;
            From = from;
            To = to;
        }
    }
}
