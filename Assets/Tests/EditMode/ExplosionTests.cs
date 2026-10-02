using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class ExplosionTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        private static int TickOf(ExplosionResult result, Item item) =>
            result.Cleared.Single(clear => clear.Item == item).Tick;

        [Test]
        public void HorizontalRocketClearsItsWholeRow()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "g g g g g",
                "r r hro r b",
                "g g g g g"));

            session.TryTap(At(2, 1));

            for (int x = 0; x < 5; x++)
            {
                Assert.IsTrue(session.Board.IsEmpty(At(x, 1)), $"Row cell {At(x, 1)} should be cleared");
                Assert.IsFalse(session.Board.IsEmpty(At(x, 0)));
                Assert.IsFalse(session.Board.IsEmpty(At(x, 2)));
            }
        }

        [Test]
        public void VerticalRocketClearsItsWholeColumn()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "g r g",
                "g vro g",
                "g b g",
                "g y g"));

            session.TryTap(At(1, 2));

            for (int y = 0; y < 4; y++)
            {
                Assert.IsTrue(session.Board.IsEmpty(At(1, y)), $"Column cell {At(1, y)} should be cleared");
                Assert.IsFalse(session.Board.IsEmpty(At(0, y)));
                Assert.IsFalse(session.Board.IsEmpty(At(2, y)));
            }
        }

        [Test]
        public void RocketHitsNearCellsBeforeFarCells()
        {
            var board = TestBoards.FromRows("b r hro y g");
            var rocket = board.GetItem(At(2, 0));
            var near = board.GetItem(At(1, 0));
            var far = board.GetItem(At(0, 0));
            var session = TestBoards.Session(board);

            var result = session.TryTap(At(2, 0)).Explosion;

            Assert.AreEqual(0, TickOf(result, rocket));
            Assert.AreEqual(1, TickOf(result, near));
            Assert.AreEqual(2, TickOf(result, far));
        }

        [Test]
        public void RocketPassesThroughObstacles()
        {
            var session = TestBoards.Session(TestBoards.FromRows("hro s r"));

            session.TryTap(At(0, 0));

            Assert.IsTrue(session.Board.IsEmpty(At(2, 0)), "The cube behind the stone should be cleared");
        }

        [Test]
        public void TntClearsExactlyTheFiveByFiveAreaAroundIt()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "r r r r r r r",
                "r r r r r r r",
                "r r r r r r r",
                "r r r t r r r",
                "r r r r r r r",
                "r r r r r r r",
                "r r r r r r r"));

            var result = session.TryTap(At(3, 3)).Explosion;

            Assert.AreEqual(25, result.Cleared.Count);
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    bool inArea = System.Math.Abs(x - 3) <= 2 && System.Math.Abs(y - 3) <= 2;
                    Assert.AreEqual(inArea, session.Board.IsEmpty(At(x, y)), $"Cell {At(x, y)}");
                }
            }
        }

        [Test]
        public void RocketTriggersATntInItsPath()
        {
            var board = TestBoards.FromRows(
                "g g g g g",
                "g g g g g",
                "hro r r t r",
                "g g g g g",
                "g g g g g");
            var tnt = board.GetItem(At(3, 2));
            var session = TestBoards.Session(board);

            var result = session.TryTap(At(0, 2)).Explosion;

            Assert.AreEqual(3, TickOf(result, tnt), "The rocket reaches the TNT three cells away on tick 3");
            Assert.IsTrue(session.Board.IsEmpty(At(3, 0)), "Only the TNT's area reaches this cell");
            Assert.IsTrue(session.Board.IsEmpty(At(1, 4)), "Only the TNT's area reaches this cell");
            Assert.IsFalse(session.Board.IsEmpty(At(0, 0)), "Outside both the rocket's row and the TNT's area");
        }

        [Test]
        public void TappingASpecialItemSpendsOneMove()
        {
            var session = TestBoards.Session(TestBoards.FromRows("r t r"), moveCount: 5);

            var result = session.TryTap(At(1, 0));

            Assert.IsNotNull(result.Explosion);
            Assert.IsNull(result.Blast);
            Assert.AreEqual(4, session.MovesLeft);
        }
    }
}
