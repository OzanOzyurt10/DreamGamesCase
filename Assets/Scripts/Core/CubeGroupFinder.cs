using System.Collections.Generic;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    public static class CubeGroupFinder
    {
        /// <summary>Every cube of the same colour connected to <paramref name="start"/>, including itself.</summary>
        public static IReadOnlyList<Cube> FindGroup(Board board, Cube start) =>
            FloodFill.Find(board, start, cube => cube.Color == start.Color);
    }
}
