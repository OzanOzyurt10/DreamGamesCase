using System;
using System.Linq;
using DreamCase.Core;

namespace DreamCase.Tests
{
    /// <summary>Builds small boards for tests from rows written top to bottom, the way they look on screen.</summary>
    public static class TestBoards
    {
        public static Board FromRows(params string[] rowsTopToBottom)
        {
            var rows = rowsTopToBottom
                .Reverse()
                .Select(row => row.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToArray();

            var level = new LevelData
            {
                grid_width = rows[0].Length,
                grid_height = rows.Length,
                grid = rows.SelectMany(row => row).ToArray(),
            };
            return new BoardBuilder(new ItemFactory(new Random(0))).Build(level);
        }
    }
}
