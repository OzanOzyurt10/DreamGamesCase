using System;
using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    public static class FloodFill
    {
        /// <summary>
        /// Every item of type <typeparamref name="T"/> connected to <paramref name="start"/> through orthogonal
        /// neighbours that pass <paramref name="belongsToGroup"/>, including <paramref name="start"/> itself.
        /// Grouped items (cubes, special items) are all 1x1.
        /// </summary>
        public static IReadOnlyList<T> Find<T>(Board board, T start, Func<T, bool> belongsToGroup) where T : Item
        {
            var group = new List<T> { start };
            var visited = new HashSet<T> { start };
            var frontier = new Queue<T>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                var item = frontier.Dequeue();
                foreach (var direction in GridPosition.OrthogonalDirections)
                {
                    var neighbourCell = item.Position + direction;
                    if (!board.IsInside(neighbourCell))
                        continue;

                    if (board.GetItem(neighbourCell) is T neighbour
                        && belongsToGroup(neighbour)
                        && visited.Add(neighbour))
                    {
                        group.Add(neighbour);
                        frontier.Enqueue(neighbour);
                    }
                }
            }
            return group;
        }
    }
}
