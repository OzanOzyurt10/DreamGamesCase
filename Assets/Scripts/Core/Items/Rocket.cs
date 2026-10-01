namespace DreamCase.Core.Items
{
    public sealed class Rocket : SpecialItem
    {
        public RocketDirection Direction { get; }

        public Rocket(RocketDirection direction)
        {
            Direction = direction;
        }
    }
}
