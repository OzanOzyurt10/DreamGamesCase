using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class TapTests
    {
        // y=2:  r b r
        // y=1:  r r b
        // y=0:  b r b
        // The red group connected to (0, 2) has 4 cubes; the red at (2, 2) only touches it diagonally.
        private static Board CreateBoard() => TestBoards.FromRows(
            "r b r",
            "r r b",
            "b r b");

        [Test]
        public void GroupContainsOnlyOrthogonalNeighboursOfTheSameColor()
        {
            var board = CreateBoard();
            var start = (Cube)board.GetItem(new GridPosition(0, 2));

            var group = CubeGroupFinder.FindGroup(board, start);

            var cells = group.Select(cube => cube.Position).ToList();
            CollectionAssert.AreEquivalent(
                new[] { new GridPosition(0, 2), new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(1, 0) },
                cells);
        }

        [Test]
        public void ValidTapRemovesTheGroupAndSpendsOneMove()
        {
            var session = TestBoards.Session(CreateBoard(), moveCount: 5);

            var result = session.TryTap(new GridPosition(1, 0));

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Blast.BlastedCubes.Count);
            Assert.AreEqual(4, session.MovesLeft);
            Assert.IsTrue(session.Board.IsEmpty(new GridPosition(0, 2)));
            Assert.IsTrue(session.Board.IsEmpty(new GridPosition(0, 1)));
            Assert.IsTrue(session.Board.IsEmpty(new GridPosition(1, 1)));
            // A group of 4 leaves a rocket on the tapped cell instead of a cube.
            Assert.IsInstanceOf<Rocket>(session.Board.GetItem(new GridPosition(1, 0)));
            Assert.IsFalse(session.Board.IsEmpty(new GridPosition(2, 2)));
        }

        [Test]
        public void TapOnALonelyCubeDoesNothing()
        {
            var session = TestBoards.Session(CreateBoard(), moveCount: 5);

            var result = session.TryTap(new GridPosition(2, 2));

            Assert.IsNull(result);
            Assert.AreEqual(5, session.MovesLeft);
            Assert.IsFalse(session.Board.IsEmpty(new GridPosition(2, 2)));
        }

        [Test]
        public void TapOnAnObstacleOrOutsideTheBoardDoesNothing()
        {
            var session = TestBoards.Session(TestBoards.FromRows("s r r"), moveCount: 5);

            Assert.IsNull(session.TryTap(new GridPosition(0, 0)));
            Assert.IsNull(session.TryTap(new GridPosition(-1, 0)));
            Assert.IsNull(session.TryTap(new GridPosition(0, 5)));
            Assert.AreEqual(5, session.MovesLeft);
        }

        [Test]
        public void NoTapIsAcceptedWithoutMovesLeft()
        {
            var session = TestBoards.Session(TestBoards.FromRows("r r"), moveCount: 0);

            Assert.IsNull(session.TryTap(new GridPosition(0, 0)));
            Assert.IsFalse(session.Board.IsEmpty(new GridPosition(0, 0)));
        }
    }
}
