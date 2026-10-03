namespace DreamCase.Core.Items
{
    /// <summary>Items that explode when tapped or hit by another explosion: rockets and TNT.</summary>
    public abstract class SpecialItem : Item
    {
        // Only another explosion triggers a special item; a neighbouring blast doesn't.
        internal override DamageOutcome TakeDamage(DamageInfo damage) =>
            damage.Type == DamageType.Explosion ? DamageOutcome.Cleared : DamageOutcome.None;

        internal override void OnCleared(ExplosionResolver explosions, int tick) => Detonate(explosions, tick);

        /// <summary>Starts this item's own explosion shape from its position.</summary>
        internal abstract void Detonate(ExplosionResolver explosions, int tick);
    }
}
