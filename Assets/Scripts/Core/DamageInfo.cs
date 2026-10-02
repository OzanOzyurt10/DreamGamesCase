namespace DreamCase.Core
{
    /// <summary>One hit on one cell. Hits from the same blast or explosion share a SourceId.</summary>
    public readonly struct DamageInfo
    {
        public DamageType Type { get; }
        public int SourceId { get; }

        public DamageInfo(DamageType type, int sourceId)
        {
            Type = type;
            SourceId = sourceId;
        }
    }
}
