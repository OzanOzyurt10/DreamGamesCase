namespace DreamCase.Core.Items
{
    /// <summary>Base class of everything that can occupy a board cell.</summary>
    public abstract class Item
    {
        /// <summary>Bottom-left cell of the item. Only the board moves items.</summary>
        public GridPosition Position { get; internal set; }

        /// <summary>Number of columns the item covers.</summary>
        public virtual int Width => 1;

        /// <summary>Number of rows the item covers.</summary>
        public virtual int Height => 1;

        /// <summary>Whether gravity pulls the item down. Items that don't fall also hold up everything above them.</summary>
        public virtual bool CanFall => true;

        /// <summary>Whether tapping this item right now is a valid move. By default an item can't be tapped.</summary>
        internal virtual bool CanBeTapped(Board board) => false;

        /// <summary>Resolves a tap on this item. Only called when <see cref="CanBeTapped"/> is true.</summary>
        internal virtual TapResult Tap(TapContext context) =>
            throw new System.InvalidOperationException($"{GetType().Name} can't be tapped.");

        /// <summary>Applies one damage event. By default an item is not affected.</summary>
        internal virtual DamageOutcome TakeDamage(DamageInfo damage) => DamageOutcome.None;

        /// <summary>Called right after an explosion cleared the item. Special items detonate here.</summary>
        internal virtual void OnCleared(ExplosionResolver explosions, int tick)
        {
        }
    }
}
