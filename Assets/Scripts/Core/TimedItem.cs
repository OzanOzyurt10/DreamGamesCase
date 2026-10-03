using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>Something that happened to an item on a given explosion tick.</summary>
    public readonly struct TimedItem
    {
        public Item Item { get; }
        public int Tick { get; }

        public TimedItem(Item item, int tick)
        {
            Item = item;
            Tick = tick;
        }
    }
}
