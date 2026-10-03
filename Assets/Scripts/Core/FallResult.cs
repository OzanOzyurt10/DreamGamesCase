using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>How the board settled after a move: items that fell, and new cubes that dropped in from the top.</summary>
    public sealed class FallResult
    {
        public IReadOnlyList<ItemMove> Falls { get; }
        public IReadOnlyList<ItemMove> Spawns { get; }

        public FallResult(IReadOnlyList<ItemMove> falls, IReadOnlyList<ItemMove> spawns)
        {
            Falls = falls;
            Spawns = spawns;
        }
    }
}
