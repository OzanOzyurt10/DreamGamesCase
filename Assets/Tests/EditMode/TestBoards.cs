using System;
using System.Linq;
using DreamCase.Core;

namespace DreamCase.Tests
{
    /// <summary>
    /// Builds small boards for tests from rows written top to bottom, the way they look on screen.
    /// Codes are the level file codes; "." is an empty cell.
    /// </summary>
    public static class TestBoards
    {
        public const string Empty = ".";

        public static ItemFactory Factory() => new ItemFactory(new Random(0));

        public static Board FromRows(params string[] rowsTopToBottom)
        {
            var rows = rowsTopToBottom
                .Reverse()
                .Select(row => row.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToArray();

            var factory = Factory();
            var board = new Board(rows[0].Length, rows.Length);
            for (int y = 0; y < rows.Length; y++)
            {
                for (int x = 0; x < rows[y].Length; x++)
                {
                    var cell = new GridPosition(x, y);
                    // Skip the other corners of a multi-cell item already placed from its bottom-left corner.
                    if (rows[y][x] != Empty && board.IsEmpty(cell))
                        board.Place(factory.Create(rows[y][x]), cell);
                }
            }
            return board;
        }

        /// <summary>A board full of <paramref name="fill"/>, with some cells (board coordinates, y = 0 at the bottom) set to other codes.</summary>
        public static Board Filled(int width, int height, string fill, params (int x, int y, string code)[] cells)
        {
            var codes = new string[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    codes[y, x] = fill;
            }
            foreach (var (x, y, code) in cells)
                codes[y, x] = code;

            var rowsTopToBottom = Enumerable.Range(0, height)
                .Select(row => height - 1 - row)
                .Select(y => string.Join(" ", Enumerable.Range(0, width).Select(x => codes[y, x])))
                .ToArray();
            return FromRows(rowsTopToBottom);
        }

        public static LevelSession Session(Board board, int moveCount = 5) =>
            new LevelSession(board, moveCount, Factory(), new Random(0));
    }
}
