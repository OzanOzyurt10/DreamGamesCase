namespace DreamCase.Core.Items
{
    public sealed class Tnt : SpecialItem
    {
        /// <summary>Cells reached in every direction: radius 2 is a 5x5 area.</summary>
        private const int Radius = 2;

        internal override void Detonate(ExplosionResolver explosions, int tick) =>
            explosions.ExplodeArea(Position, Radius, tick, explosions.NewSource());
    }
}
