using System.Collections.Generic;

namespace DreamCase.Core
{
    /// <summary>The whole timeline of an explosion and its chain reactions, for the view to replay.</summary>
    public sealed class ExplosionResult
    {
        public IReadOnlyList<ClearedItem> Cleared { get; }
        public IReadOnlyList<RocketLaunch> Rockets { get; }

        public ExplosionResult(IReadOnlyList<ClearedItem> cleared, IReadOnlyList<RocketLaunch> rockets)
        {
            Cleared = cleared;
            Rockets = rockets;
        }
    }
}
