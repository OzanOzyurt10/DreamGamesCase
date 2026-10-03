namespace DreamCase.Core
{
    public enum DamageOutcome
    {
        /// <summary>The hit had no effect on the item.</summary>
        None,

        /// <summary>The item took damage but stays on the board.</summary>
        Damaged,

        /// <summary>The item is destroyed and must leave the board.</summary>
        Cleared
    }
}
