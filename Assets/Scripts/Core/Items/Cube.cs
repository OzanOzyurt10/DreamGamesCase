namespace DreamCase.Core.Items
{
    public sealed class Cube : Item
    {
        public CubeColor Color { get; }

        public Cube(CubeColor color)
        {
            Color = color;
        }

        // A neighbouring blast doesn't affect cubes; only explosions clear them.
        internal override DamageOutcome TakeDamage(DamageInfo damage) =>
            damage.Type == DamageType.Explosion ? DamageOutcome.Cleared : DamageOutcome.None;
    }
}
