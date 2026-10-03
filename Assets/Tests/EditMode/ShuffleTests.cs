using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class ShuffleTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        // Available moves

        [Test]
        public void SameColouredNeighboursAreAMove()
        {
            Assert.IsTrue(MoveFinder.HasAvailableMove(TestBoards.FromRows("r r g")));
        }

        [Test]
        public void ALoneSpecialItemIsAMove()
        {
            Assert.IsTrue(MoveFinder.HasAvailableMove(TestBoards.FromRows("r hro g")));
        }

        [Test]
        public void ACheckerboardHasNoMove()
        {
            Assert.IsFalse(MoveFinder.HasAvailableMove(TestBoards.FromRows(
                "r g",
                "g r")));
        }

        // Shuffle

        [Test]
        public void NoShuffleWhileAMoveExists()
        {
            var session = TestBoards.Session(TestBoards.FromRows("r r g"));

            Assert.IsFalse(session.TryShuffle(out var moves));
            CollectionAssert.IsEmpty(moves);
        }

        [Test]
        public void ShuffleCreatesAMoveAndOnlyRearrangesCubes()
        {
            // No two neighbouring cubes share a colour; the stone must stay where it is.
            var board = TestBoards.FromRows(
                "r g b y",
                "g s y b",
                "b y r g");
            var stone = board.GetItem(At(1, 1));
            var coloursBefore = ColourCounts(board);
            var session = TestBoards.Session(board);

            Assert.IsTrue(session.TryShuffle(out _));

            Assert.IsTrue(MoveFinder.HasAvailableMove(board));
            Assert.AreSame(stone, board.GetItem(At(1, 1)));
            CollectionAssert.AreEquivalent(coloursBefore, ColourCounts(board));
            Assert.AreEqual(11, board.Items.OfType<Cube>().Count());
        }

        [Test]
        public void ShuffleAlwaysPairsUpCubesWhenTwoCubeCellsTouch()
        {
            // Stones leave a single pair of neighbouring cube cells, (6, 0) and (7, 0); random tries can easily miss it.
            var board = TestBoards.FromRows("r s g s b s r g");
            var session = TestBoards.Session(board);

            session.TryShuffle(out _);

            Assert.IsTrue(MoveFinder.HasAvailableMove(board));
            Assert.IsFalse(session.IsLost);
        }

        [Test]
        public void LevelIsLostWhenNoShuffleCanCreateAMove()
        {
            // Two cubes of different colours: no arrangement makes a group.
            var session = TestBoards.Session(TestBoards.FromRows("r g s"), moveCount: 5);

            session.TryShuffle(out _);

            Assert.AreEqual(5, session.MovesLeft);
            Assert.IsTrue(session.IsLost);
        }

        private static string[] ColourCounts(Board board) =>
            board.Items.OfType<Cube>()
                .GroupBy(cube => cube.Color)
                .Select(group => $"{group.Key}={group.Count()}")
                .ToArray();
    }
}
