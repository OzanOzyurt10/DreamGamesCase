using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    public static class HintFinder
    {
        /// <summary>
        /// The special item each cube's group would create if tapped now. Cubes in groups too small
        /// for a special item are left out.
        /// </summary>
        public static IReadOnlyDictionary<Cube, BlastReward> Find(Board board)
        {
            var hints = new Dictionary<Cube, BlastReward>();
            var visited = new HashSet<Cube>();

            foreach (var cube in board.Items.OfType<Cube>())
            {
                if (visited.Contains(cube))
                    continue;

                var group = CubeGroupFinder.FindGroup(board, cube);
                visited.UnionWith(group);

                var reward = BlastRewards.ForGroupSize(group.Count);
                if (reward == BlastReward.None)
                    continue;

                foreach (var member in group)
                    hints[member] = reward;
            }
            return hints;
        }
    }
}
