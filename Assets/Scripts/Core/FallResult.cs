using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>How the board settled after a move: items that fell, and new cubes that dropped in from the top.</summary>
    public sealed class FallResult
    {
        public IReadOnlyList<ItemFall> Falls { get; }
        public IReadOnlyList<ItemFall> Spawns { get; }

        public FallResult(IReadOnlyList<ItemFall> falls, IReadOnlyList<ItemFall> spawns)
        {
            Falls = falls;
            Spawns = spawns;
        }
    }
}
