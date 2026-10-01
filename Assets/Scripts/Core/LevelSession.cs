using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The state of a level being played: its board and the moves left.</summary>
    public sealed class LevelSession
    {
        private const int MinBlastGroupSize = 2;

        private readonly ItemFactory itemFactory;

        public Board Board { get; }
        public int MovesLeft { get; private set; }

        public LevelSession(Board board, int moveCount, ItemFactory itemFactory)
        {
            Board = board;
            MovesLeft = moveCount;
            this.itemFactory = itemFactory;
        }

        /// <summary>
        /// Applies a tap to the board. Returns null if the tap is not a valid move; then nothing changes
        /// and no move is spent.
        /// </summary>
        public BlastResult TryTap(GridPosition cell)
        {
            if (MovesLeft <= 0 || !Board.IsInside(cell) || !(Board.GetItem(cell) is Cube tappedCube))
                return null;

            var group = CubeGroupFinder.FindGroup(Board, tappedCube);
            if (group.Count < MinBlastGroupSize)
                return null;

            foreach (var cube in group)
                Board.Remove(cube);

            MovesLeft--;
            return new BlastResult(group);
        }

        /// <summary>Lets items fall into the gaps, then fills the columns from the top with new cubes.</summary>
        public FallResult Settle()
        {
            var falls = Gravity.Apply(Board);
            var spawns = Refill.Apply(Board, itemFactory);
            return new FallResult(falls, spawns);
        }
    }
}
