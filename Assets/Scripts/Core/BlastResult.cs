using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>What a valid tap changed on the board, so the view can animate it afterwards.</summary>
    public sealed class BlastResult
    {
        public GridPosition TappedCell { get; }
        public IReadOnlyList<Cube> BlastedCubes { get; }

        /// <summary>The special item created on the tapped cell, or null if the group was too small.</summary>
        public SpecialItem CreatedItem { get; }

        /// <summary>Neighbouring obstacles that took damage but stayed on the board.</summary>
        public IReadOnlyList<Item> DamagedObstacles { get; }

        /// <summary>Neighbouring obstacles the blast destroyed; they are already off the board.</summary>
        public IReadOnlyList<Item> ClearedObstacles { get; }

        public BlastResult(
            GridPosition tappedCell,
            IReadOnlyList<Cube> blastedCubes,
            SpecialItem createdItem,
            IReadOnlyList<Item> damagedObstacles,
            IReadOnlyList<Item> clearedObstacles)
        {
            TappedCell = tappedCell;
            BlastedCubes = blastedCubes;
            CreatedItem = createdItem;
            DamagedObstacles = damagedObstacles;
            ClearedObstacles = clearedObstacles;
        }
    }
}
