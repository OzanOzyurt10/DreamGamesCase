using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>The whole timeline of an explosion and its chain reactions, for the view to replay.</summary>
    public sealed class ExplosionResult
    {
        /// <summary>Set when the tapped item exploded as a combo; the timeline starts after the merge.</summary>
        public ComboMerge Combo { get; }

        public IReadOnlyList<ClearedItem> Cleared { get; }
        public IReadOnlyList<RocketLaunch> Rockets { get; }

        public ExplosionResult(ComboMerge combo, IReadOnlyList<ClearedItem> cleared, IReadOnlyList<RocketLaunch> rockets)
        {
            Combo = combo;
            Cleared = cleared;
            Rockets = rockets;
        }
    }
}
