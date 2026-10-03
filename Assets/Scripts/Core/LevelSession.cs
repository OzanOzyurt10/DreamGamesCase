using System;
using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>The state of a level being played: its board and the moves left.</summary>
    public sealed class LevelSession
    {
        private const int MinBlastGroupSize = 2;
        private const int MinComboSize = 2;

        private readonly ItemFactory itemFactory;
        private readonly DamageSourceIds sourceIds = new DamageSourceIds();

        public Board Board { get; }
        public int MovesLeft { get; private set; }

        /// <summary>Every obstacle is cleared. Only meaningful once the board has settled.</summary>
        public bool IsWon => !Board.Items.OfType<Obstacle>().Any();

        /// <summary>No moves left and obstacles remain. Only meaningful once the board has settled.</summary>
        public bool IsLost => MovesLeft == 0 && !IsWon;

        public LevelSession(Board board, int moveCount, ItemFactory itemFactory)
        {
            Board = board;
            MovesLeft = moveCount;
            this.itemFactory = itemFactory;
        }

        /// <summary>How many obstacles of each type are still on the board. Cleared types are left out.</summary>
        public IReadOnlyDictionary<Type, int> RemainingObstacles() =>
            Board.Items.OfType<Obstacle>()
                .GroupBy(obstacle => obstacle.GetType())
                .ToDictionary(group => group.Key, group => group.Count());

        /// <summary>
        /// Applies a tap to the board. Returns null if the tap is not a valid move; then nothing changes
        /// and no move is spent.
        /// </summary>
        public TapResult TryTap(GridPosition cell)
        {
            if (MovesLeft <= 0 || !Board.IsInside(cell))
                return null;

            TapResult result;
            switch (Board.GetItem(cell))
            {
                case Cube cube:
                    var blast = TryBlast(cube, cell);
                    result = blast == null ? null : TapResult.FromBlast(blast);
                    break;
                case SpecialItem special:
                    result = TapResult.FromExplosion(Detonate(special));
                    break;
                default:
                    result = null;
                    break;
            }

            if (result != null)
                MovesLeft--;
            return result;
        }

        /// <summary>Lets items fall into the gaps, then fills the columns from the top with new cubes.</summary>
        public FallResult Settle()
        {
            var falls = Gravity.Apply(Board);
            var spawns = Refill.Apply(Board, itemFactory);
            return new FallResult(falls, spawns);
        }

        /// <summary>A tapped special item explodes alone, or as a combo with the special items connected to it.</summary>
        private ExplosionResult Detonate(SpecialItem tapped)
        {
            var explosions = new ExplosionResolver(Board, sourceIds);
            var group = FloodFill.Find(Board, tapped, _ => true);
            return group.Count >= MinComboSize
                ? explosions.DetonateCombo(group, tapped.Position)
                : explosions.Detonate(tapped);
        }

        private BlastResult TryBlast(Cube tappedCube, GridPosition cell)
        {
            var group = CubeGroupFinder.FindGroup(Board, tappedCube);
            if (group.Count < MinBlastGroupSize)
                return null;

            foreach (var cube in group)
                Board.Remove(cube);

            // Neighbours are damaged before the special item appears, so it can't take part in its own blast.
            var (damagedObstacles, clearedObstacles) = BlastDamage.Apply(Board, group, sourceIds.Next());

            SpecialItem createdItem = null;
            var reward = BlastRewards.ForGroupSize(group.Count);
            if (reward != BlastReward.None)
            {
                createdItem = itemFactory.CreateReward(reward);
                Board.Place(createdItem, cell);
            }

            return new BlastResult(cell, group, createdItem, damagedObstacles, clearedObstacles);
        }
    }
}
