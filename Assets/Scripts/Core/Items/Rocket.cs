namespace DreamCase.Core.Items
{
    public sealed class Rocket : SpecialItem
    {
        public RocketDirection Direction { get; }

        public Rocket(RocketDirection direction)
        {
            Direction = direction;
        }

        internal override void Detonate(ExplosionResolver explosions, int tick) =>
            explosions.FireRocket(Position, Direction, tick, explosions.NewSource());
    }
}
