namespace DreamCase.Core.Items
{
    /// <summary>Only explosions damage it, and one is enough. Doesn't fall.</summary>
    public sealed class Stone : Obstacle
    {
        public override bool CanFall => false;

        internal override DamageOutcome TakeDamage(DamageInfo damage) =>
            damage.Type == DamageType.Explosion ? DamageOutcome.Cleared : DamageOutcome.None;
    }
}
