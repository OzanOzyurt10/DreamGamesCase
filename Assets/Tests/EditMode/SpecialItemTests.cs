using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class SpecialItemTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        [TestCase(2, BlastReward.None)]
        [TestCase(3, BlastReward.None)]
        [TestCase(4, BlastReward.Rocket)]
        [TestCase(5, BlastReward.Rocket)]
        [TestCase(6, BlastReward.Tnt)]
        [TestCase(9, BlastReward.Tnt)]
        public void GroupSizeDecidesTheReward(int groupSize, BlastReward expected)
        {
            Assert.AreEqual(expected, BlastRewards.ForGroupSize(groupSize));
        }

        [Test]
        public void SmallGroupCreatesNoSpecialItem()
        {
            var session = TestBoards.Session(TestBoards.FromRows("r r r b"));

            var result = session.TryTap(At(1, 0));

            Assert.IsNull(result.Blast.CreatedItem);
            Assert.IsTrue(session.Board.IsEmpty(At(1, 0)));
        }

        [TestCase("r r r r b")]
        [TestCase("r r r r r")]
        public void FourOrFiveCubesCreateARocketOnTheTappedCell(string row)
        {
            var session = TestBoards.Session(TestBoards.FromRows(row));

            var result = session.TryTap(At(2, 0));

            Assert.IsInstanceOf<Rocket>(result.Blast.CreatedItem);
            Assert.AreSame(result.Blast.CreatedItem, session.Board.GetItem(At(2, 0)));
            Assert.AreEqual(At(2, 0), result.Blast.TappedCell);
        }

        [Test]
        public void SixCubesCreateATntOnTheTappedCell()
        {
            var session = TestBoards.Session(TestBoards.FromRows(
                "b r r b",
                "b r r r",
                "b b r b"));

            var result = session.TryTap(At(2, 0));

            Assert.AreEqual(6, result.Blast.BlastedCubes.Count);
            Assert.IsInstanceOf<Tnt>(result.Blast.CreatedItem);
            Assert.AreSame(result.Blast.CreatedItem, session.Board.GetItem(At(2, 0)));
        }

        [Test]
        public void HintsShowTheRewardOfEachCubesGroup()
        {
            var board = TestBoards.FromRows(
                "r r r r r b",
                "g g g g g g",
                "y y y b y b");

            var hints = HintFinder.Find(board);

            var reds = board.Items.OfType<Cube>().Where(c => c.Color == CubeColor.Red);
            var greens = board.Items.OfType<Cube>().Where(c => c.Color == CubeColor.Green);
            Assert.IsTrue(reds.All(c => hints[c] == BlastReward.Rocket));
            Assert.IsTrue(greens.All(c => hints[c] == BlastReward.Tnt));
            Assert.AreEqual(11, hints.Count, "Only the 5 reds and 6 greens get a hint.");
        }
    }
}
