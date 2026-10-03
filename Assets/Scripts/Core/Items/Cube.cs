namespace DreamCase.Core.Items
{
    public sealed class Cube : Item
    {
        public CubeColor Color { get; }

        public Cube(CubeColor color)
        {
            Color = color;
        }

        /// <summary>A cube can be tapped when it is part of a group of at least two same-coloured cubes.</summary>
        internal override bool CanBeTapped(Board board) =>
            CubeGroupFinder.FindGroup(board, this).Count >= CubeBlast.MinGroupSize;

        internal override TapResult Tap(TapContext context) => TapResult.FromBlast(CubeBlast.Resolve(context, this));

        // A neighbouring blast doesn't affect cubes; only explosions clear them.
        internal override DamageOutcome TakeDamage(DamageInfo damage) =>
            damage.Type == DamageType.Explosion ? DamageOutcome.Cleared : DamageOutcome.None;
    }
}
