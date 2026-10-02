using DreamCase.Core.Items;

namespace DreamCase.Core
{
    /// <summary>A rocket splitting at <see cref="Origin"/>. Each half reaches the k-th cell from it at Tick + k.</summary>
    public readonly struct RocketLaunch
    {
        public GridPosition Origin { get; }
        public RocketDirection Direction { get; }
        public int Tick { get; }

        public RocketLaunch(GridPosition origin, RocketDirection direction, int tick)
        {
            Origin = origin;
            Direction = direction;
            Tick = tick;
        }
    }
}
