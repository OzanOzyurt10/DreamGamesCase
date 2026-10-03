using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class LevelEndTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        [Test]
        public void LevelIsWonWhenTheLastObstacleIsCleared()
        {
            var session = TestBoards.Session(TestBoards.FromRows("g g v r r"));

            session.TryTap(At(0, 0));
            Assert.IsFalse(session.IsWon, "The vase is only cracked");

            session.TryTap(At(3, 0));
            session.Settle();
            Assert.IsTrue(session.IsWon);
            Assert.IsFalse(session.IsLost);
        }

        [Test]
        public void LevelIsLostWhenMovesRunOutWithObstaclesLeft()
        {
            var session = TestBoards.Session(TestBoards.FromRows("s r r g g"), moveCount: 1);

            session.TryTap(At(1, 0));
            session.Settle();

            Assert.AreEqual(0, session.MovesLeft);
            Assert.IsTrue(session.IsLost);
        }

        [Test]
        public void ChainReactionOnTheLastMoveStillWins()
        {
            // The TNT's area reaches the rocket, and only the rocket reaches the stone at the far end.
            var session = TestBoards.Session(TestBoards.FromRows("t r hro r r s"), moveCount: 1);

            session.TryTap(At(0, 0));
            session.Settle();

            Assert.AreEqual(0, session.MovesLeft);
            Assert.IsTrue(session.IsWon);
            Assert.IsFalse(session.IsLost);
        }

        [Test]
        public void RemainingObstaclesAreCountedPerType()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "r r r cbTL cbTR",
                "v v s cbBL cbBR"));

            var remaining = session.RemainingObstacles();

            Assert.AreEqual(3, remaining.Count);
            Assert.AreEqual(2, remaining[typeof(Vase)]);
            Assert.AreEqual(1, remaining[typeof(Stone)]);
            Assert.AreEqual(1, remaining[typeof(ChaliceBox)], "A chalice box counts once, not once per cell");
        }
    }
}
