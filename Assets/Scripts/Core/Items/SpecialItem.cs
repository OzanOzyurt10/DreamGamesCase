namespace DreamCase.Core.Items
{
    /// <summary>Items that explode when tapped or hit by another explosion: rockets and TNT.</summary>
    public abstract class SpecialItem : Item
    {
        private const int MinComboSize = 2;

        /// <summary>A special item can always be tapped.</summary>
        internal override bool CanBeTapped(Board board) => true;

        /// <summary>Explodes alone, or as a combo with every special item connected to it.</summary>
        internal override TapResult Tap(TapContext context)
        {
            var explosions = new ExplosionResolver(context.Board, context.SourceIds);
            var group = FloodFill.Find<SpecialItem>(context.Board, this, _ => true);
            var result = group.Count >= MinComboSize
                ? explosions.DetonateCombo(group, Position)
                : explosions.Detonate(this);
            return TapResult.FromExplosion(result);
        }

        // Only another explosion triggers a special item; a neighbouring blast doesn't.
        internal override DamageOutcome TakeDamage(DamageInfo damage) =>
            damage.Type == DamageType.Explosion ? DamageOutcome.Cleared : DamageOutcome.None;

        internal override void OnCleared(ExplosionResolver explosions, int tick) => Detonate(explosions, tick);

        /// <summary>Starts this item's own explosion shape from its position.</summary>
        internal abstract void Detonate(ExplosionResolver explosions, int tick);
    }
}
