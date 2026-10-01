using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    public static class CubeGroupFinder
    {
        /// <summary>
        /// Flood fill: every cube of the same colour connected to <paramref name="start"/> through
        /// orthogonal neighbours, including <paramref name="start"/> itself.
        /// </summary>
        public static IReadOnlyList<Cube> FindGroup(Board board, Cube start)
        {
            var group = new List<Cube> { start };
            var visited = new HashSet<Cube> { start };
            var frontier = new Queue<Cube>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                var cube = frontier.Dequeue();
                foreach (var direction in GridPosition.OrthogonalDirections)
                {
                    var neighbourCell = cube.Position + direction;
                    if (!board.IsInside(neighbourCell))
                        continue;

                    if (board.GetItem(neighbourCell) is Cube neighbour
                        && neighbour.Color == start.Color
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
