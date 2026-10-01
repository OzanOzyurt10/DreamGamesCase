using DreamCase.Core;
using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Converts grid coordinates to world positions. The cell size is the largest one that
    /// fits the whole board (plus its frame) into the given world area.
    /// </summary>
    public sealed class BoardLayout
    {
        private readonly Vector2 firstCellCenter;

        public float CellSize { get; }
        public Vector2 Center { get; }
        public Vector2 FrameSize { get; }

        public BoardLayout(int columns, int rows, Rect area, float framePaddingInCells)
        {
            float frameColumns = columns + 2f * framePaddingInCells;
            float frameRows = rows + 2f * framePaddingInCells;

            CellSize = Mathf.Min(area.width / frameColumns, area.height / frameRows);
            Center = area.center;
            FrameSize = new Vector2(frameColumns, frameRows) * CellSize;
            firstCellCenter = Center - new Vector2(columns - 1, rows - 1) * (CellSize / 2f);
        }

        public Vector2 CellCenter(GridPosition cell) => firstCellCenter + new Vector2(cell.X, cell.Y) * CellSize;

        /// <summary>The cell under a world position. It may be outside the board.</summary>
        public GridPosition WorldToCell(Vector2 worldPosition)
        {
            Vector2 offsetInCells = (worldPosition - firstCellCenter) / CellSize;
            return new GridPosition(Mathf.RoundToInt(offsetInCells.x), Mathf.RoundToInt(offsetInCells.y));
        }

        /// <summary>Center of the area an item covers; differs from its anchor cell for multi-cell items.</summary>
        public Vector2 ItemCenter(Item item) =>
            CellCenter(item.Position) + new Vector2(item.Width - 1, item.Height - 1) * (CellSize / 2f);
    }
}
