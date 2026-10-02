using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>An item an explosion removed, and the tick it happened on.</summary>
    public readonly struct ClearedItem
    {
        public Item Item { get; }
        public int Tick { get; }

        public ClearedItem(Item item, int tick)
        {
            Item = item;
            Tick = tick;
        }
    }
}
