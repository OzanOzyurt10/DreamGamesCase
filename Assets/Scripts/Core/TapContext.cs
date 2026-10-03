namespace DreamCase.Core
{
    /// <summary>What an item needs to resolve a tap on itself.</summary>
    internal sealed class TapContext
    {
        public Board Board { get; }
        public ItemFactory ItemFactory { get; }
        public DamageSourceIds SourceIds { get; }

        public TapContext(Board board, ItemFactory itemFactory, DamageSourceIds sourceIds)
        {
            Board = board;
            ItemFactory = itemFactory;
            SourceIds = sourceIds;
        }
    }
}
