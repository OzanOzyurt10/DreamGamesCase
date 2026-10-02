namespace DreamCase.Core
{
    /// <summary>Hands out a unique id for every blast or explosion in a level.</summary>
    internal sealed class DamageSourceIds
    {
        private int lastId;

        public int Next() => ++lastId;
    }
}
