using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;

namespace DreamCase.Tests
{
    public class ObstacleTests
    {
        private static GridPosition At(int x, int y) => new GridPosition(x, y);

        // Vase

        [Test]
        public void VaseCracksFromAnAdjacentBlast()
        {
            var board = TestBoards.FromRows("v r r");
            var vase = (Vase)board.GetItem(At(0, 0));
            var session = TestBoards.Session(board);

            var result = session.TryTap(At(1, 0));

            Assert.AreEqual(1, vase.Health);
            Assert.AreSame(vase, board.GetItem(At(0, 0)));
            CollectionAssert.Contains(result.Blast.DamagedObstacles, vase);
        }

        [Test]
        public void VaseTakesOneDamageFromABlastTouchingItTwice()
        {
            var board = TestBoards.FromRows(
                "r v",
                "r r");
            var vase = (Vase)board.GetItem(At(1, 1));

            TestBoards.Session(board).TryTap(At(0, 0));

            Assert.AreEqual(1, vase.Health);
        }

        [Test]
        public void VaseIsClearedByTwoDifferentBlasts()
        {
            var board = TestBoards.FromRows("g g v r r");
            var vase = board.GetItem(At(2, 0));
            var session = TestBoards.Session(board);

            session.TryTap(At(0, 0));
            var second = session.TryTap(At(3, 0));

            Assert.IsTrue(board.IsEmpty(At(2, 0)));
            CollectionAssert.Contains(second.Blast.ClearedObstacles, vase);
        }

        [Test]
        public void VaseTakesOneDamageFromACombo()
        {
            // The TNT + rocket combo's row and column rockets both cross (5, 5).
            var board = TestBoards.Filled(9, 9, "r", (4, 4, "t"), (4, 3, "vro"), (5, 5, "v"));
            var vase = (Vase)board.GetItem(At(5, 5));

            TestBoards.Session(board).TryTap(At(4, 4));

            Assert.AreEqual(1, vase.Health);
            Assert.AreSame(vase, board.GetItem(At(5, 5)));
        }

        // Stone

        [Test]
        public void StoneIgnoresBlasts()
        {
            var board = TestBoards.FromRows("s r r");
            var stone = board.GetItem(At(0, 0));

            var result = TestBoards.Session(board).TryTap(At(1, 0));

            Assert.AreSame(stone, board.GetItem(At(0, 0)));
            CollectionAssert.IsEmpty(result.Blast.DamagedObstacles);
        }

        [Test]
        public void StoneIsClearedByAnExplosion()
        {
            var board = TestBoards.FromRows("s hro r");

            TestBoards.Session(board).TryTap(At(1, 0));

            Assert.IsTrue(board.IsEmpty(At(0, 0)));
        }

        // Chalice box

        private static Board ChaliceBoxBoard() => TestBoards.FromRows(
            "g g g g",
            "r cbTL cbTR b",
            "r cbBL cbBR b",
            "y y y y");

        [Test]
        public void ChaliceBoxDoorTakesExactlyOneDamageFromABlast()
        {
            var board = ChaliceBoxBoard();
            var box = (ChaliceBox)board.GetItem(At(1, 1));

            // Two red cubes touch the box, but the door phase takes one damage per source.
            TestBoards.Session(board).TryTap(At(0, 1));

            Assert.IsTrue(box.IsDoorOpen);
            Assert.AreEqual(ChaliceBox.ChaliceCount, box.ChalicesLeft);
        }

        [Test]
        public void OpenChaliceBoxLosesOneChalicePerAdjacentBlastedCube()
        {
            var board = ChaliceBoxBoard();
            var box = (ChaliceBox)board.GetItem(At(1, 1));
            var session = TestBoards.Session(board);

            session.TryTap(At(0, 1));
            session.TryTap(At(0, 0)); // Four yellow cubes; two of them touch the box.

            Assert.AreEqual(ChaliceBox.ChaliceCount - 2, box.ChalicesLeft);
        }

        [Test]
        public void ChaliceBoxLosesOneChalicePerCellAnExplosionHits()
        {
            // The lower TNT's area covers the whole box and opens the door: that is its one damage.
            // It also triggers the upper TNT, a new source, whose area hits all four cells: four chalices.
            var board = TestBoards.FromRows(
                "r r r r",
                "t cbTL cbTR r",
                "r cbBL cbBR r",
                "t r r r");
            var box = (ChaliceBox)board.GetItem(At(1, 1));

            TestBoards.Session(board).TryTap(At(0, 0));

            Assert.IsTrue(box.IsDoorOpen);
            Assert.AreEqual(ChaliceBox.ChaliceCount - 4, box.ChalicesLeft);
        }

        [Test]
        public void ChaliceBoxIsClearedWhenTheLastChaliceIsTaken()
        {
            var box = new ChaliceBox();
            box.TakeDamage(new DamageInfo(DamageType.Explosion, 1, 1));

            Assert.AreEqual(DamageOutcome.None, box.TakeDamage(new DamageInfo(DamageType.Explosion, 1, 1)),
                "The source that opened the door deals no more damage");
            Assert.AreEqual(DamageOutcome.Damaged, box.TakeDamage(new DamageInfo(DamageType.Blast, 2, 8)));
            Assert.AreEqual(DamageOutcome.Damaged, box.TakeDamage(new DamageInfo(DamageType.Explosion, 3, 1)));
            Assert.AreEqual(DamageOutcome.Cleared, box.TakeDamage(new DamageInfo(DamageType.Explosion, 3, 1)));
        }

        [Test]
        public void RemovingAChaliceBoxFreesAllFourCells()
        {
            var board = ChaliceBoxBoard();

            board.Remove(board.GetItem(At(1, 1)));

            Assert.IsTrue(board.IsEmpty(At(1, 1)));
            Assert.IsTrue(board.IsEmpty(At(2, 1)));
            Assert.IsTrue(board.IsEmpty(At(1, 2)));
            Assert.IsTrue(board.IsEmpty(At(2, 2)));
        }
    }
}
