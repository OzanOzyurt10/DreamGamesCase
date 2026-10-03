using System;
using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The state of a level being played: its board and the moves left.</summary>
    public sealed class LevelSession
    {
        private readonly ItemFactory itemFactory;
        private readonly Random random;
        private readonly DamageSourceIds sourceIds = new DamageSourceIds();

        public Board Board { get; }
        public int MovesLeft { get; private set; }

        /// <summary>Every obstacle is cleared. Only meaningful once the board has settled.</summary>
        public bool IsWon => !Board.Items.OfType<Obstacle>().Any();

        /// <summary>
        /// Obstacles remain and the player can't go on: no moves left, or no item can be tapped even after a shuffle.
        /// Only meaningful once the board has settled.
        /// </summary>
        public bool IsLost => !IsWon && (MovesLeft == 0 || !MoveFinder.HasAvailableMove(Board));

        public LevelSession(Board board, int moveCount, ItemFactory itemFactory, Random random)
        {
            Board = board;
            MovesLeft = moveCount;
            this.itemFactory = itemFactory;
            this.random = random;
        }

        /// <summary>How many obstacles of each type are still on the board. Cleared types are left out.</summary>
        public IReadOnlyDictionary<Type, int> RemainingObstacles() =>
            Board.Items.OfType<Obstacle>()
                .GroupBy(obstacle => obstacle.GetType())
                .ToDictionary(group => group.Key, group => group.Count());

        /// <summary>
        /// Applies a tap to the board. Returns null if the tap is not a valid move; then nothing changes
        /// and no move is spent. What a tap does is up to the tapped item.
        /// </summary>
        public TapResult TryTap(GridPosition cell)
        {
            if (MovesLeft <= 0 || !Board.IsInside(cell))
                return null;

            var item = Board.GetItem(cell);
            if (item == null || !item.CanBeTapped(Board))
                return null;

            MovesLeft--;
            return item.Tap(new TapContext(Board, itemFactory, sourceIds));
        }

        /// <summary>Lets items fall into the gaps, then fills the columns from the top with new cubes.</summary>
        public FallResult Settle()
        {
            var falls = Gravity.Apply(Board);
            var spawns = Refill.Apply(Board, itemFactory);
            return new FallResult(falls, spawns);
        }

        /// <summary>
        /// Shuffles the cubes if the level is still being played but no item can be tapped.
        /// Returns false, with no moves, when no shuffle was needed.
        /// </summary>
        public bool TryShuffle(out IReadOnlyList<ItemMove> moves)
        {
            if (IsWon || MovesLeft == 0 || MoveFinder.HasAvailableMove(Board))
            {
                moves = Array.Empty<ItemMove>();
                return false;
            }

            moves = Shuffler.Shuffle(Board, random);
            return true;
        }
    }
}
