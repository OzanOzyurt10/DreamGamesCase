using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class GravityTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        [Test]
        public void ItemsFallIntoEveryGapBelowThem()
        {
            var board = TestBoards.FromRows(
                "r",
                ".",
                "g",
                ".",
                ".");
            var red = board.GetItem(At(0, 4));
            var green = board.GetItem(At(0, 2));

            var falls = Gravity.Apply(board);

            Assert.AreSame(green, board.GetItem(At(0, 0)));
            Assert.AreSame(red, board.GetItem(At(0, 1)));
            Assert.AreEqual(2, falls.Count);
            Assert.IsTrue(falls.Any(f => f.Item == red && f.From == At(0, 4) && f.To == At(0, 1)));
        }

        [Test]
        public void StoneDoesNotFallAndHoldsUpItemsAbove()
        {
            var board = TestBoards.FromRows(
                "r",
                "s",
                ".");

            var falls = Gravity.Apply(board);

            Assert.IsEmpty(falls);
            Assert.IsInstanceOf<Stone>(board.GetItem(At(0, 1)));
            Assert.IsInstanceOf<Cube>(board.GetItem(At(0, 2)));
            Assert.IsTrue(board.IsEmpty(At(0, 0)));
        }

        [Test]
        public void VaseFalls()
        {
            var board = TestBoards.FromRows(
                "v",
                ".");

            Gravity.Apply(board);

            Assert.IsInstanceOf<Vase>(board.GetItem(At(0, 0)));
        }

        [Test]
        public void ChaliceBoxDoesNotFallAndHoldsUpBothColumns()
        {
            var board = TestBoards.FromRows(
                "r g",
                "cbTL cbTR",
                "cbBL cbBR",
                ". .");
            var box = board.GetItem(At(0, 1));

            var result = TestBoards.Session(board).Settle();

            Assert.IsEmpty(result.Falls);
            Assert.IsEmpty(result.Spawns);
            Assert.AreSame(box, board.GetItem(At(1, 2)));
            Assert.AreEqual(At(0, 1), box.Position);
            Assert.IsTrue(board.IsEmpty(At(0, 0)));
            Assert.IsTrue(board.IsEmpty(At(1, 0)));
        }

        [Test]
        public void RefillOnlyFillsCellsOpenToTheTop()
        {
            var board = TestBoards.FromRows(
                ".",
                "s",
                ".");

            var result = TestBoards.Session(board).Settle();

            Assert.AreEqual(1, result.Spawns.Count);
            Assert.IsInstanceOf<Cube>(board.GetItem(At(0, 2)));
            Assert.IsTrue(board.IsEmpty(At(0, 0)));
        }

        [Test]
        public void NewCubesStartStackedAboveTheBoard()
        {
            var board = TestBoards.FromRows(
                ".",
                ".",
                "r");

            var spawns = TestBoards.Session(board).Settle().Spawns;

            // Two empty cells: each new cube starts two rows above its target, so they fall as one stack.
            CollectionAssert.AreEquivalent(new[] { At(0, 3), At(0, 4) }, spawns.Select(s => s.From));
            CollectionAssert.AreEquivalent(new[] { At(0, 1), At(0, 2) }, spawns.Select(s => s.To));
        }

        [Test]
        public void BoardIsFullAfterABlastWhenNothingBlocksTheColumns()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "r r b",
                "b b g",
                "g r r"));

            session.TryTap(At(0, 1));
            var result = session.Settle();

            Assert.AreEqual(2, result.Falls.Count);
            Assert.AreEqual(2, result.Spawns.Count);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                    Assert.IsFalse(session.Board.IsEmpty(At(x, y)), $"Cell {At(x, y)} is empty");
            }
        }
    }
}
