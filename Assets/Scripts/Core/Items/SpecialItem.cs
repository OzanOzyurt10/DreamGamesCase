namespace DreamCase.Core.Items
{
    /// <summary>Items that explode when tapped or hit by another explosion: rockets and TNT.</summary>
    public abstract class SpecialItem : Item
    {
        internal override bool TakeDamage(DamageInfo damage) => damage.Type == DamageType.Explosion;

        internal override void OnCleared(ExplosionResolver explosions, int tick) => Detonate(explosions, tick);

        /// <summary>Starts this item's own explosion shape from its position.</summary>
        internal abstract void Detonate(ExplosionResolver explosions, int tick);
    }
}
