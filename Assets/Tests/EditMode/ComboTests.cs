using System;
using DreamCase.Core;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class ComboTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        /// <summary>Checks every cell: cleared exactly where <paramref name="shouldBeCleared"/> says.</summary>
        private static void AssertClearedCells(Board board, Func<int, int, bool> shouldBeCleared)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                    Assert.AreEqual(shouldBeCleared(x, y), board.IsEmpty(At(x, y)), $"Cell {At(x, y)}");
            }
        }

        [Test]
        public void TwoRocketsExplodeAsAPlusShape()
        {
            // Both rockets are horizontal, yet the combo also clears the center column. The lower rocket's
            // own row stays: members don't explode individually.
            var session = TestBoards.Session(TestBoards.Filled(5, 5, "g", (2, 2, "hro"), (2, 1, "hro")));

            var result = session.TryTap(At(2, 2));

            Assert.AreEqual(2, result.Explosion.Combo.Members.Count);
            AssertClearedCells(session.Board, (x, y) => x == 2 || y == 2);
        }

        [Test]
        public void TwoTntsClearASevenBySevenArea()
        {
            var session = TestBoards.Session(TestBoards.Filled(9, 9, "r", (4, 4, "t"), (5, 4, "t")));

            session.TryTap(At(4, 4));

            AssertClearedCells(session.Board, (x, y) => Math.Abs(x - 4) <= 3 && Math.Abs(y - 4) <= 3);
        }

        [Test]
        public void TntAndRocketFireThreeRowsAndThreeColumns()
        {
            var session = TestBoards.Session(TestBoards.Filled(9, 9, "r", (4, 4, "t"), (4, 5, "vro")));

            session.TryTap(At(4, 4));

            AssertClearedCells(session.Board, (x, y) => Math.Abs(x - 4) <= 1 || Math.Abs(y - 4) <= 1);
        }

        [Test]
        public void TwoTntsMakeATntComboEvenWithRockets()
        {
            var session = TestBoards.Session(TestBoards.Filled(9, 9, "r", (4, 4, "t"), (5, 4, "t"), (3, 4, "hro")));

            session.TryTap(At(4, 4));

            AssertClearedCells(session.Board, (x, y) => Math.Abs(x - 4) <= 3 && Math.Abs(y - 4) <= 3);
        }

        [Test]
        public void DiagonalSpecialItemIsNotPartOfTheCombo()
        {
            var session = TestBoards.Session(TestBoards.Filled(3, 3, "r", (0, 0, "t"), (1, 1, "hro")));

            var result = session.TryTap(At(0, 0));

            Assert.IsNull(result.Explosion.Combo, "A single TNT explodes; the diagonal rocket only reacts to it");
        }

        [Test]
        public void ComboSpendsOneMove()
        {
            var session = TestBoards.Session(TestBoards.FromRows("r hro vro r"), moveCount: 5);

            session.TryTap(At(1, 0));

            Assert.AreEqual(4, session.MovesLeft);
        }
    }
}
