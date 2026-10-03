namespace DreamCase.Core
{
    /// <summary>Hands out a unique id for every blast or explosion in a level. Ids start at 1.</summary>
    internal sealed class DamageSourceIds
    {
        private int lastId;

        public int Next() => ++lastId;
    }
}
