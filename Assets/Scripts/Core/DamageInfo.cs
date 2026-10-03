namespace DreamCase.Core
{
    /// <summary>
    /// One damage event on one item. Damage from the same blast or explosion shares a SourceId.
    /// </summary>
    public readonly struct DamageInfo
    {
        public DamageType Type { get; }
        public int SourceId { get; }

        /// <summary>
        /// How much of the source reaches the item: the number of blasted cubes next to it for a blast,
        /// or 1 for each cell an explosion hits.
        /// </summary>
        public int AffectedCells { get; }

        public DamageInfo(DamageType type, int sourceId, int affectedCells)
        {
            Type = type;
            SourceId = sourceId;
            AffectedCells = affectedCells;
        }
    }
}
