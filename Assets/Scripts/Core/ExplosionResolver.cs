using System.Collections.Generic;
using System.Linq;
using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>
    /// Runs an explosion and every chain reaction it causes on a tick-based timeline. Each explosion
    /// schedules hits on future ticks; hits are applied in tick order, and a special item cleared by a hit
    /// detonates and schedules hits of its own. Everything is resolved at once; the view replays the result.
    /// </summary>
    internal sealed class ExplosionResolver
    {
        private readonly struct Hit
        {
            public readonly GridPosition Cell;
            public readonly int SourceId;

            public Hit(GridPosition cell, int sourceId)
            {
                Cell = cell;
                SourceId = sourceId;
            }
        }

        private readonly Board board;
        private readonly DamageSourceIds sourceIds;
        private readonly SortedDictionary<int, List<Hit>> hitsByTick = new SortedDictionary<int, List<Hit>>();
        private readonly List<ClearedItem> cleared = new List<ClearedItem>();
        private readonly List<RocketLaunch> rockets = new List<RocketLaunch>();

        public ExplosionResolver(Board board, DamageSourceIds sourceIds)
        {
            this.board = board;
            this.sourceIds = sourceIds;
        }

        public int NewSource() => sourceIds.Next();

        /// <summary>Clears a tapped special item at tick 0 and resolves everything that follows.</summary>
        public ExplosionResult Detonate(SpecialItem tapped)
        {
            Clear(tapped, 0);
            return RunTimeline(null);
        }

        /// <summary>
        /// Merges adjacent special items into the tapped cell and explodes them once, as a combo. The members
        /// leave the board without their own explosions; anything the combo hits still reacts normally.
        /// </summary>
        public ExplosionResult DetonateCombo(IReadOnlyList<SpecialItem> members, GridPosition center)
        {
            foreach (var member in members)
                board.Remove(member);

            ComboRules.Detonate(members, center, this, NewSource());
            return RunTimeline(new ComboMerge(center, members));
        }

        /// <summary>
        /// Two halves fly from the origin in opposite directions, one cell per tick, to the board's edge.
        /// The origin is hit too: empty for a single rocket, but combo rockets can start on occupied cells.
        /// </summary>
        public void FireRocket(GridPosition origin, RocketDirection direction, int tick, int sourceId)
        {
            if (!board.IsInside(origin))
                return;

            rockets.Add(new RocketLaunch(origin, direction, tick));
            Schedule(tick + 1, origin, sourceId);

            foreach (var step in direction.HalfDirections())
            {
                var cell = origin + step;
                for (int hitTick = tick + 1; board.IsInside(cell); hitTick++, cell += step)
                    Schedule(hitTick, cell, sourceId);
            }
        }

        /// <summary>Hits every cell within <paramref name="radius"/> of the center on the next tick.</summary>
        public void ExplodeArea(GridPosition center, int radius, int tick, int sourceId)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var cell = new GridPosition(center.X + dx, center.Y + dy);
                    if (board.IsInside(cell))
                        Schedule(tick + 1, cell, sourceId);
                }
            }
        }

        private ExplosionResult RunTimeline(ComboMerge combo)
        {
            while (hitsByTick.Count > 0)
            {
                int tick = hitsByTick.Keys.First();
                var hits = hitsByTick[tick];
                hitsByTick.Remove(tick);

                // Hits are only ever scheduled for later ticks, so this list can't grow while we go through it.
                foreach (var hit in hits)
                    Apply(hit, tick);
            }
            return new ExplosionResult(combo, cleared, rockets);
        }

        private void Schedule(int tick, GridPosition cell, int sourceId)
        {
            if (!hitsByTick.TryGetValue(tick, out var hits))
            {
                hits = new List<Hit>();
                hitsByTick.Add(tick, hits);
            }
            hits.Add(new Hit(cell, sourceId));
        }

        private void Apply(Hit hit, int tick)
        {
            var item = board.GetItem(hit.Cell);
            if (item != null && item.TakeDamage(new DamageInfo(DamageType.Explosion, hit.SourceId)))
                Clear(item, tick);
        }

        private void Clear(Item item, int tick)
        {
            board.Remove(item);
            cleared.Add(new ClearedItem(item, tick));
            item.OnCleared(this, tick);
        }
    }
}
