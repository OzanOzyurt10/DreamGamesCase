using System;
using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>A cell coordinate on the board. (0, 0) is the bottom-left cell.</summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public static readonly GridPosition Up = new GridPosition(0, 1);
        public static readonly GridPosition Down = new GridPosition(0, -1);
        public static readonly GridPosition Left = new GridPosition(-1, 0);
        public static readonly GridPosition Right = new GridPosition(1, 0);

        /// <summary>The four direct neighbours: no diagonals.</summary>
        public static readonly IReadOnlyList<GridPosition> OrthogonalDirections = new[] { Up, Down, Left, Right };

        public readonly int X;
        public readonly int Y;

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridPosition operator +(GridPosition a, GridPosition b) => new GridPosition(a.X + b.X, a.Y + b.Y);

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);

        public override string ToString() => $"({X}, {Y})";
    }
}
