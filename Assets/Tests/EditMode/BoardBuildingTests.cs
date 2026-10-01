using System;
using System.Linq;
using DreamCase.Core;
using DreamCase.Core.Items;
using NUnit.Framework;
using UnityEngine;

namespace DreamCase.Tests
{
    public class BoardBuildingTests
    {
        private const int LevelCount = 10;

        private BoardBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new BoardBuilder(new ItemFactory(new System.Random(0)));
        }

        private static LevelData[] LoadAllLevels() =>
            Resources.LoadAll<TextAsset>("Levels")
                .Select(asset => JsonUtility.FromJson<LevelData>(asset.text))
                .ToArray();

        private static LevelData LoadLevel(int number) =>
            LoadAllLevels().Single(level => level.level_number == number);

        [Test]
        public void AllLevelFilesArePresent()
        {
            var numbers = LoadAllLevels().Select(level => level.level_number).OrderBy(n => n);
            CollectionAssert.AreEqual(Enumerable.Range(1, LevelCount), numbers);
        }

        [Test]
        public void EveryLevelBuildsWithItsDeclaredSize()
        {
            foreach (var level in LoadAllLevels())
            {
                var board = builder.Build(level);
                Assert.AreEqual(level.grid_width, board.Width, $"Level {level.level_number}");
                Assert.AreEqual(level.grid_height, board.Height, $"Level {level.level_number}");
            }
        }

        [Test]
        public void GridIsReadFromTheBottomLeft()
        {
            // Level 1: the two bottom rows are stones, the third row starts with six blue cubes.
            var board = builder.Build(LoadLevel(1));

            Assert.IsInstanceOf<Stone>(board.GetItem(new GridPosition(0, 0)));
            Assert.AreEqual(CubeColor.Blue, ((Cube)board.GetItem(new GridPosition(0, 2))).Color);
            Assert.AreEqual(CubeColor.Green, ((Cube)board.GetItem(new GridPosition(9, 2))).Color);
        }

        [Test]
        public void ChaliceBoxIsOneObjectCoveringFourCells()
        {
            // Level 4: the chalice box's bottom-left corner is at (0, 2).
            var board = builder.Build(LoadLevel(4));
            var box = board.GetItem(new GridPosition(0, 2));

            Assert.IsInstanceOf<ChaliceBox>(box);
            Assert.AreEqual(new GridPosition(0, 2), box.Position);
            Assert.AreSame(box, board.GetItem(new GridPosition(1, 2)));
            Assert.AreSame(box, board.GetItem(new GridPosition(0, 3)));
            Assert.AreSame(box, board.GetItem(new GridPosition(1, 3)));
            Assert.AreEqual(1, board.Items.OfType<ChaliceBox>().Count());
        }

        [Test]
        public void RandomCodeAlwaysCreatesACube()
        {
            var factory = new ItemFactory(new System.Random(0));
            for (int i = 0; i < 100; i++)
                Assert.IsInstanceOf<Cube>(factory.Create("rand"));
        }

        [Test]
        public void UnknownCodeThrows()
        {
            var factory = new ItemFactory(new System.Random(0));
            Assert.Throws<ArgumentException>(() => factory.Create("unknown"));
        }
    }
}
