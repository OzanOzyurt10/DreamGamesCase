namespace DreamCase.Core.Items
{
    /// <summary>A 2x2 obstacle: one object referenced by four board cells.</summary>
    public sealed class ChaliceBox : Obstacle
    {
        public override int Width => 2;
        public override int Height => 2;
    }
}
