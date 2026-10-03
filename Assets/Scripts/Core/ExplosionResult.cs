using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>The whole timeline of an explosion and its chain reactions, for the view to replay.</summary>
    public sealed class ExplosionResult
    {
        /// <summary>Set when the tapped item exploded as a combo; the timeline starts after the merge.</summary>
        public ComboMerge Combo { get; }

        /// <summary>Items removed from the board, each on the tick it was hit.</summary>
        public IReadOnlyList<TimedItem> Cleared { get; }

        /// <summary>Items that took damage but stayed on the board.</summary>
        public IReadOnlyList<TimedItem> Damaged { get; }

        public IReadOnlyList<RocketLaunch> Rockets { get; }

        public ExplosionResult(
            ComboMerge combo,
            IReadOnlyList<TimedItem> cleared,
            IReadOnlyList<TimedItem> damaged,
            IReadOnlyList<RocketLaunch> rockets)
        {
            Combo = combo;
            Cleared = cleared;
            Damaged = damaged;
            Rockets = rockets;
        }
    }
}
