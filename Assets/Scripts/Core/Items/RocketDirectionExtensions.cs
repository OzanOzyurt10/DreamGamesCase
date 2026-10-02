using System.Collections.Generic;

namespace DreamCase.Core.Items
{
    public static class RocketDirectionExtensions
    {
        private static readonly GridPosition[] HorizontalHalves = { GridPosition.Left, GridPosition.Right };
        private static readonly GridPosition[] VerticalHalves = { GridPosition.Down, GridPosition.Up };

        /// <summary>The two opposite directions a rocket's halves fly in.</summary>
        public static IReadOnlyList<GridPosition> HalfDirections(this RocketDirection direction) =>
            direction == RocketDirection.Horizontal ? HorizontalHalves : VerticalHalves;
    }
}
