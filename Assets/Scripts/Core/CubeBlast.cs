using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>Blasting a tapped cube's group: the group leaves, its neighbours take blast damage, a special item may appear.</summary>
    internal static class CubeBlast
    {
        public const int MinGroupSize = 2;

        public static BlastResult Resolve(TapContext context, Cube tappedCube)
        {
            var board = context.Board;
            var cell = tappedCube.Position;
            var group = CubeGroupFinder.FindGroup(board, tappedCube);

            foreach (var cube in group)
                board.Remove(cube);

            // Neighbours are damaged before the special item appears, so it can't take part in its own blast.
            var (damagedObstacles, clearedObstacles) = BlastDamage.Apply(board, group, context.SourceIds.Next());

            SpecialItem createdItem = null;
            var reward = BlastRewards.ForGroupSize(group.Count);
            if (reward != BlastReward.None)
            {
                createdItem = context.ItemFactory.CreateReward(reward);
                board.Place(createdItem, cell);
            }

            return new BlastResult(cell, group, createdItem, damagedObstacles, clearedObstacles);
        }
    }
}
