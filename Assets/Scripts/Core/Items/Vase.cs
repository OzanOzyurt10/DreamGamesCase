using System.Collections.Generic;

namespace DreamCase.Core.Items
{
    /// <summary>Damaged by adjacent blasts and explosions; cleared after two damages, at most one per source. Falls.</summary>
    public sealed class Vase : Obstacle
    {
        public const int MaxHealth = 2;

        private readonly HashSet<int> damagingSources = new HashSet<int>();

        public int Health { get; private set; } = MaxHealth;

        internal override DamageOutcome TakeDamage(DamageInfo damage)
        {
            if (!damagingSources.Add(damage.SourceId))
                return DamageOutcome.None;

            Health--;
            return Health > 0 ? DamageOutcome.Damaged : DamageOutcome.Cleared;
        }
    }
}
